import { computed, onMounted, reactive, ref, watch } from 'vue';
import { catalogApi } from '../api/catalog';
import { appPermissions } from '../lib/permissions';
import {
  cloneVariantAttributeValues,
  mapVariantAttributesToValues,
  normalizeVariantAttributeValues,
} from '../lib/variantAttributes';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type {
  CategoryModel,
  ProductAttributeDefinitionModel,
  ProductResponse,
  ProductVariantAttributeValueRequest,
  ProductVariantResponse,
} from '../types/contracts';

type PendingVariantDraft = {
  id: string;
  name: string;
  sku: string;
  description: string;
  overridePrice: number;
  currency: string;
  attributes: ProductVariantAttributeValueRequest[];
};

function areAttributesEqual(
  left: readonly ProductVariantAttributeValueRequest[],
  right: readonly ProductVariantAttributeValueRequest[],
) {
  return JSON.stringify(normalizeVariantAttributeValues(left)) === JSON.stringify(normalizeVariantAttributeValues(right));
}

export function useProductAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();

  const loading = ref(true);
  const saving = ref(false);
  const deleting = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);
  const creatingAttribute = ref(false);

  const categories = ref<CategoryModel[]>([]);
  const products = ref<ProductResponse[]>([]);
  const variants = ref<ProductVariantResponse[]>([]);
  const attributeDefinitions = ref<ProductAttributeDefinitionModel[]>([]);

  const isProductDialogOpen = ref(false);
  const isVariantDialogOpen = ref(false);
  const expandedProductIds = ref<string[]>([]);
  const editingDraftVariantId = ref<string | null>(null);

  const catalogSearch = ref('');
  const selectedProductId = ref('');
  const selectedVariantId = ref('');
  const newAttributeName = ref('');
  const pendingVariants = ref<PendingVariantDraft[]>([]);

  const productForm = reactive({
    id: '',
    name: '',
    slug: '',
    description: '',
    categoryId: '',
    basePrice: 0,
    currency: 'USD',
  });

  const variantForm = reactive({
    id: '',
    parentId: '',
    name: '',
    sku: '',
    description: '',
    overridePrice: 0,
    currency: 'USD',
    attributes: [] as ProductVariantAttributeValueRequest[],
  });

  const canViewProducts = computed(() => authStore.hasPermission(appPermissions.products.view));
  const canManageProducts = computed(() => authStore.hasPermission(appPermissions.products.manage));
  const canManageVariants = computed(() => authStore.hasPermission(appPermissions.variants.manage));
  const canViewCurrentPage = computed(() => canViewProducts.value);
  const canManageCurrentPage = computed(() => canManageProducts.value);

  const selectedProduct = computed(() =>
    products.value.find((product) => product.id === selectedProductId.value) ?? null,
  );

  const selectedVariant = computed(() =>
    variants.value.find((variant) => variant.id === selectedVariantId.value) ?? null,
  );

  const selectedProductVariants = computed(() =>
    variants.value.filter((variant) => variant.parentId === selectedProductId.value),
  );

  const filteredProducts = computed(() => {
    const search = catalogSearch.value.trim().toLowerCase();
    if (!search) {
      return products.value;
    }

    return products.value.filter((product) => {
      const relatedVariants = variants.value
        .filter((variant) => variant.parentId === product.id)
        .map((variant) => `${variant.name} ${variant.sku} ${variant.attributes.map((attribute) => `${attribute.name} ${attribute.value}`).join(' ')}`);

      return [
        product.name,
        product.slug,
        product.description,
        categories.value.find((category) => category.id === product.categoryId)?.name ?? '',
        ...relatedVariants,
      ]
        .join(' ')
        .toLowerCase()
        .includes(search);
    });
  });

  const productCanSave = computed(() => {
    const hasRequiredFields = Boolean(
      productForm.name.trim() &&
      productForm.slug.trim() &&
      productForm.categoryId.trim() &&
      productForm.currency.trim(),
    );

    if (!productForm.id) {
      return hasRequiredFields;
    }

    if (!selectedProduct.value || !hasRequiredFields) {
      return false;
    }

    return JSON.stringify({
      name: productForm.name.trim(),
      slug: productForm.slug.trim(),
      description: productForm.description.trim(),
      categoryId: productForm.categoryId,
      basePrice: productForm.basePrice,
      currency: productForm.currency.trim(),
    }) !== JSON.stringify({
      name: selectedProduct.value.name.trim(),
      slug: selectedProduct.value.slug.trim(),
      description: selectedProduct.value.description.trim(),
      categoryId: selectedProduct.value.categoryId,
      basePrice: selectedProduct.value.basePrice,
      currency: selectedProduct.value.currency.trim(),
    });
  });

  const variantCanSave = computed(() => {
    const hasValues = variantForm.attributes.every((attribute) => attribute.attributeId.trim() && attribute.value.trim());
    const baseIsValid = Boolean(
      variantForm.name.trim() &&
      variantForm.sku.trim() &&
      variantForm.currency.trim() &&
      hasValues,
    );
    const targetProductId = variantForm.parentId || productForm.id || selectedProductId.value;

    if (editingDraftVariantId.value) {
      const draft = pendingVariants.value.find((item) => item.id === editingDraftVariantId.value);
      if (!draft || !baseIsValid) {
        return false;
      }

      return JSON.stringify({
        name: variantForm.name.trim(),
        sku: variantForm.sku.trim(),
        description: variantForm.description.trim(),
        overridePrice: variantForm.overridePrice,
        currency: variantForm.currency.trim(),
      }) !== JSON.stringify({
        name: draft.name.trim(),
        sku: draft.sku.trim(),
        description: draft.description.trim(),
        overridePrice: draft.overridePrice,
        currency: draft.currency.trim(),
      }) || !areAttributesEqual(variantForm.attributes, draft.attributes);
    }

    if (variantForm.id) {
      if (!selectedVariant.value || !baseIsValid || !targetProductId) {
        return false;
      }

      return JSON.stringify({
        parentId: targetProductId,
        name: variantForm.name.trim(),
        sku: variantForm.sku.trim(),
        description: variantForm.description.trim(),
        overridePrice: variantForm.overridePrice,
        currency: variantForm.currency.trim(),
      }) !== JSON.stringify({
        parentId: selectedVariant.value.parentId,
        name: selectedVariant.value.name.trim(),
        sku: selectedVariant.value.sku.trim(),
        description: selectedVariant.value.description.trim(),
        overridePrice: selectedVariant.value.overridePrice,
        currency: selectedVariant.value.currency.trim(),
      }) || !areAttributesEqual(variantForm.attributes, mapVariantAttributesToValues(selectedVariant.value.attributes));
    }

    if (!targetProductId) {
      return baseIsValid;
    }

    return baseIsValid && Boolean(targetProductId);
  });

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function resetProductForm() {
    productForm.id = '';
    productForm.name = '';
    productForm.slug = '';
    productForm.description = '';
    productForm.categoryId = categories.value[0]?.id ?? '';
    productForm.basePrice = 0;
    productForm.currency = 'USD';
  }

  function resetVariantForm(parentId = selectedProductId.value || productForm.id) {
    variantForm.id = '';
    variantForm.parentId = parentId || '';
    variantForm.name = '';
    variantForm.sku = '';
    variantForm.description = '';
    variantForm.overridePrice = 0;
    variantForm.currency = 'USD';
    variantForm.attributes = [];
    editingDraftVariantId.value = null;
  }

  async function createAttributeDefinition() {
    if (!authStore.token || !canManageVariants.value || !newAttributeName.value.trim()) {
      return;
    }

    creatingAttribute.value = true;
    setOutcome(null, null);

    try {
      const created = await catalogApi.createProductAttribute(authStore.token, {
        name: newAttributeName.value.trim(),
      });
      if (!created) {
        throw new Error('Product attribute creation did not return the created definition.');
      }

      attributeDefinitions.value = [...attributeDefinitions.value, created].sort((left, right) => left.name.localeCompare(right.name));
      newAttributeName.value = '';
      uiStore.pushToast('Attribute definition created successfully.', 'success');
      setOutcome('Attribute definition created successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Attribute definition creation failed.');
    } finally {
      creatingAttribute.value = false;
    }
  }

  function openProductDialog(product?: ProductResponse) {
    selectedVariantId.value = '';
    pendingVariants.value = [];

    if (product) {
      selectedProductId.value = product.id;
    } else {
      selectedProductId.value = '';
      resetProductForm();
    }

    resetVariantForm(product?.id);
    isProductDialogOpen.value = true;
  }

  function openVariantDialog(product?: ProductResponse) {
    const productId = product?.id ?? selectedProductId.value ?? productForm.id;
    if (productId) {
      selectedProductId.value = productId;
    }

    selectedVariantId.value = '';
    resetVariantForm(productId);
    isVariantDialogOpen.value = true;
  }

  function openExistingVariantDialog(variant: ProductVariantResponse) {
    selectedProductId.value = variant.parentId;
    selectedVariantId.value = variant.id;
    editingDraftVariantId.value = null;
    isVariantDialogOpen.value = true;
  }

  function openDraftVariantDialog(draftId: string) {
    const draft = pendingVariants.value.find((item) => item.id === draftId);
    if (!draft) {
      return;
    }

    selectedVariantId.value = '';
    editingDraftVariantId.value = draft.id;
    variantForm.id = '';
    variantForm.parentId = '';
    variantForm.name = draft.name;
    variantForm.sku = draft.sku;
    variantForm.description = draft.description;
    variantForm.overridePrice = draft.overridePrice;
    variantForm.currency = draft.currency;
    variantForm.attributes = cloneVariantAttributeValues(draft.attributes);
    isVariantDialogOpen.value = true;
  }

  function removePendingVariant(draftId: string) {
    pendingVariants.value = pendingVariants.value.filter((item) => item.id !== draftId);
  }

  function toggleProductExpansion(productId: string) {
    if (expandedProductIds.value.includes(productId)) {
      expandedProductIds.value = expandedProductIds.value.filter((id) => id !== productId);
      return;
    }

    expandedProductIds.value = [...expandedProductIds.value, productId];
  }

  function isProductExpanded(productId: string) {
    return expandedProductIds.value.includes(productId);
  }

  function variantsForProduct(productId: string) {
    const search = catalogSearch.value.trim().toLowerCase();
    const productVariants = variants.value.filter((variant) => variant.parentId === productId);
    if (!search) {
      return productVariants;
    }

    return productVariants.filter((variant) =>
      [
        variant.name,
        variant.sku,
        variant.description,
        ...variant.attributes.map((attribute) => `${attribute.name} ${attribute.value}`),
      ]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  }

  async function refresh() {
    if (!authStore.token || !canViewProducts.value) {
      categories.value = [];
      products.value = [];
      variants.value = [];
      attributeDefinitions.value = [];
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      const [categoryResult, productResult, variantResult, attributeResult] = await Promise.all([
        catalogApi.getCategories(authStore.token),
        catalogApi.getProducts(authStore.token),
        catalogApi.getProductVariants(authStore.token),
        catalogApi.getProductAttributes(authStore.token),
      ]);

      categories.value = categoryResult;
      products.value = productResult;
      variants.value = variantResult;
      attributeDefinitions.value = attributeResult;

      if (!productForm.categoryId) {
        productForm.categoryId = categories.value[0]?.id ?? '';
      }
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load product data.');
    } finally {
      loading.value = false;
    }
  }

  async function saveProduct() {
    if (!authStore.token || !canManageProducts.value || !productCanSave.value) {
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      let currentProductId = productForm.id;

      if (productForm.id) {
        const updatedProduct = await catalogApi.updateProduct(authStore.token, { ...productForm });
        currentProductId = updatedProduct?.id ?? productForm.id;
        uiStore.pushToast('Product updated successfully.', 'success');
        setOutcome('Product updated successfully.');
      } else {
        const createdProduct = await catalogApi.createProduct(authStore.token, { ...productForm });
        if (!createdProduct) {
          throw new Error('Product creation did not return the created product.');
        }

        currentProductId = createdProduct.id;
        selectedProductId.value = createdProduct.id;

        for (const draft of pendingVariants.value) {
          await catalogApi.createProductVariant(authStore.token, {
            parentId: createdProduct.id,
            name: draft.name,
            sku: draft.sku,
            description: draft.description,
            overridePrice: draft.overridePrice,
            currency: draft.currency,
            attributes: cloneVariantAttributeValues(draft.attributes),
          });
        }

        pendingVariants.value = [];
        uiStore.pushToast('Product created successfully.', 'success');
        setOutcome('Product created successfully.');
      }

      selectedProductId.value = currentProductId;
      await refresh();

      const currentProduct = products.value.find((product) => product.id === currentProductId) ?? null;
      if (currentProduct) {
        productForm.id = currentProduct.id;
        productForm.name = currentProduct.name;
        productForm.slug = currentProduct.slug;
        productForm.description = currentProduct.description;
        productForm.categoryId = currentProduct.categoryId;
        productForm.basePrice = currentProduct.basePrice;
        productForm.currency = currentProduct.currency;
        resetVariantForm(currentProduct.id);
      }
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Product save failed.');
    } finally {
      saving.value = false;
    }
  }

  async function saveVariant() {
    if (!authStore.token || !canManageVariants.value || !variantCanSave.value) {
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      const targetProductId = variantForm.parentId || productForm.id || selectedProductId.value;
      const attributes = cloneVariantAttributeValues(variantForm.attributes);

      if (!targetProductId) {
        if (editingDraftVariantId.value) {
          pendingVariants.value = pendingVariants.value.map((draft) =>
            draft.id === editingDraftVariantId.value
              ? {
                  ...draft,
                  name: variantForm.name,
                  sku: variantForm.sku,
                  description: variantForm.description,
                  overridePrice: variantForm.overridePrice,
                  currency: variantForm.currency,
                  attributes,
                }
              : draft,
          );
          uiStore.pushToast('Variant draft updated.', 'success');
          setOutcome('Variant draft updated.');
        } else {
          pendingVariants.value = [
            ...pendingVariants.value,
            {
              id: crypto.randomUUID(),
              name: variantForm.name,
              sku: variantForm.sku,
              description: variantForm.description,
              overridePrice: variantForm.overridePrice,
              currency: variantForm.currency,
              attributes,
            },
          ];
          uiStore.pushToast('Variant added to the draft product.', 'success');
          setOutcome('Variant added to the draft product.');
        }

        resetVariantForm();
        isVariantDialogOpen.value = false;
        return;
      }

      if (variantForm.id) {
        await catalogApi.updateProductVariant(authStore.token, {
          id: variantForm.id,
          parentId: targetProductId,
          name: variantForm.name,
          sku: variantForm.sku,
          description: variantForm.description,
          overridePrice: variantForm.overridePrice,
          currency: variantForm.currency,
          attributes,
        });
        uiStore.pushToast('Variant updated successfully.', 'success');
        setOutcome('Variant updated successfully.');
      } else {
        await catalogApi.createProductVariant(authStore.token, {
          parentId: targetProductId,
          name: variantForm.name,
          sku: variantForm.sku,
          description: variantForm.description,
          overridePrice: variantForm.overridePrice,
          currency: variantForm.currency,
          attributes,
        });
        uiStore.pushToast('Variant created successfully.', 'success');
        setOutcome('Variant created successfully.');
      }

      await refresh();
      resetVariantForm(targetProductId);
      selectedVariantId.value = '';
      isVariantDialogOpen.value = false;
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Variant save failed.');
    } finally {
      saving.value = false;
    }
  }

  async function deleteProduct(product: ProductResponse) {
    if (!authStore.token || !canManageProducts.value) {
      setOutcome(null, 'Product management permission is required to manage products.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete product',
      message: `Delete "${product.name}" and its management entry?`,
      confirmLabel: 'Delete product',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deleting.value = product.id;
    setOutcome(null, null);

    try {
      await catalogApi.deleteProduct(authStore.token, product.id);
      if (selectedProductId.value === product.id) {
        selectedProductId.value = '';
        resetProductForm();
        resetVariantForm();
      }

      await refresh();
      uiStore.pushToast('Product deleted successfully.', 'success');
      setOutcome('Product deleted successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Product deletion failed.');
    } finally {
      deleting.value = null;
    }
  }

  async function deleteVariant(variant: ProductVariantResponse) {
    if (!authStore.token || !canManageVariants.value) {
      setOutcome(null, 'Variant management permission is required to manage variants.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete variant',
      message: `Delete variant "${variant.name}"?`,
      confirmLabel: 'Delete variant',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deleting.value = variant.id;
    setOutcome(null, null);

    try {
      await catalogApi.deleteProductVariant(authStore.token, variant.id);
      if (selectedVariantId.value === variant.id) {
        selectedVariantId.value = '';
        resetVariantForm(variant.parentId);
      }

      await refresh();
      uiStore.pushToast('Variant deleted successfully.', 'success');
      setOutcome('Variant deleted successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Variant deletion failed.');
    } finally {
      deleting.value = null;
    }
  }

  watch(selectedProduct, (product) => {
    if (!product) {
      return;
    }

    productForm.id = product.id;
    productForm.name = product.name;
    productForm.slug = product.slug;
    productForm.description = product.description;
    productForm.categoryId = product.categoryId;
    productForm.basePrice = product.basePrice;
    productForm.currency = product.currency;
  });

  watch(selectedVariant, (variant) => {
    if (!variant) {
      return;
    }

    variantForm.id = variant.id;
    variantForm.parentId = variant.parentId;
    variantForm.name = variant.name;
    variantForm.sku = variant.sku;
    variantForm.description = variant.description;
    variantForm.overridePrice = variant.overridePrice;
    variantForm.currency = variant.currency;
    variantForm.attributes = mapVariantAttributesToValues(variant.attributes);
  });

  onMounted(() => {
    void refresh();
  });

  return {
    loading,
    saving,
    deleting,
    error,
    success,
    categories,
    products,
    variants,
    attributeDefinitions,
    isProductDialogOpen,
    isVariantDialogOpen,
    catalogSearch,
    selectedProduct,
    selectedVariant,
    selectedProductVariants,
    productForm,
    variantForm,
    pendingVariants,
    editingDraftVariantId,
    newAttributeName,
    creatingAttribute,
    canViewCurrentPage,
    canManageCurrentPage,
    canManageProducts,
    canManageVariants,
    filteredProducts,
    productCanSave,
    variantCanSave,
    createAttributeDefinition,
    openProductDialog,
    openVariantDialog,
    openExistingVariantDialog,
    openDraftVariantDialog,
    removePendingVariant,
    toggleProductExpansion,
    isProductExpanded,
    variantsForProduct,
    refresh,
    saveProduct,
    saveVariant,
    deleteProduct,
    deleteVariant,
  };
}
