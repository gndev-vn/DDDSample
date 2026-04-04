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
  ProductAttributeDefinitionModel,
  ProductVariantAttributeValueRequest,
  ProductResponse,
  ProductVariantResponse,
} from '../types/contracts';

function areAttributesEqual(
  left: readonly ProductVariantAttributeValueRequest[],
  right: readonly ProductVariantAttributeValueRequest[],
) {
  return JSON.stringify(normalizeVariantAttributeValues(left)) === JSON.stringify(normalizeVariantAttributeValues(right));
}

export function useVariantAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();

  const loading = ref(true);
  const saving = ref(false);
  const deleting = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);
  const creatingAttribute = ref(false);
  const products = ref<ProductResponse[]>([]);
  const variants = ref<ProductVariantResponse[]>([]);
  const attributeDefinitions = ref<ProductAttributeDefinitionModel[]>([]);
  const isVariantDialogOpen = ref(false);
  const catalogSearch = ref('');
  const selectedVariantId = ref('');
  const newAttributeName = ref('');

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

  const canViewVariants = computed(() => authStore.hasPermission(appPermissions.variants.view));
  const canCreateVariants = computed(() => authStore.hasPermission(appPermissions.variants.create));
  const canUpdateVariants = computed(() => authStore.hasPermission(appPermissions.variants.update));
  const canDeleteVariants = computed(() => authStore.hasPermission(appPermissions.variants.delete));
  const canManageVariants = computed(() => canCreateVariants.value || canUpdateVariants.value || canDeleteVariants.value);
  const canViewCurrentPage = computed(() => canViewVariants.value);
  const canManageCurrentPage = computed(() => canManageVariants.value);

  const selectedVariant = computed(() =>
    variants.value.find((variant) => variant.id === selectedVariantId.value) ?? null,
  );

  const filteredVariants = computed(() => {
    const search = catalogSearch.value.trim().toLowerCase();
    if (!search) {
      return variants.value;
    }

    return variants.value.filter((variant) => {
      const productName = products.value.find((product) => product.id === variant.parentId)?.name ?? '';

      return [
        variant.name,
        variant.sku,
        variant.description,
        productName,
        ...variant.attributes.map((attribute) => `${attribute.name} ${attribute.value}`),
      ]
        .join(' ')
        .toLowerCase()
        .includes(search);
    });
  });

  const variantCanSave = computed(() => {
    const hasValues = variantForm.attributes.every((attribute) => attribute.attributeId.trim() && attribute.value.trim());
    const baseIsValid = Boolean(
      variantForm.parentId.trim() &&
      variantForm.name.trim() &&
      variantForm.sku.trim() &&
      variantForm.currency.trim() &&
      hasValues,
    );
    if (!variantForm.id) {
      return baseIsValid;
    }

    if (!selectedVariant.value || !baseIsValid) {
      return false;
    }

    return JSON.stringify({
      parentId: variantForm.parentId,
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
  });

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function resetVariantForm(parentId = '') {
    variantForm.id = '';
    variantForm.parentId = parentId;
    variantForm.name = '';
    variantForm.sku = '';
    variantForm.description = '';
    variantForm.overridePrice = 0;
    variantForm.currency = 'USD';
    variantForm.attributes = [];
  }

  async function createAttributeDefinition() {
    if (!authStore.token || !canCreateVariants.value || !newAttributeName.value.trim()) {
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

  function openVariantDialog() {
    selectedVariantId.value = '';
    resetVariantForm();
    isVariantDialogOpen.value = true;
  }

  function openExistingVariantDialog(variant: ProductVariantResponse) {
    selectedVariantId.value = variant.id;
    isVariantDialogOpen.value = true;
  }

  async function refresh() {
    if (!authStore.token || !canViewVariants.value) {
      products.value = [];
      variants.value = [];
      attributeDefinitions.value = [];
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      const [productResult, variantResult, attributeResult] = await Promise.all([
        catalogApi.getProducts(authStore.token),
        catalogApi.getProductVariants(authStore.token),
        catalogApi.getProductAttributes(authStore.token),
      ]);

      products.value = productResult;
      variants.value = variantResult;
      attributeDefinitions.value = attributeResult;
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load variant data.');
    } finally {
      loading.value = false;
    }
  }

  async function saveVariant() {
    if (!authStore.token || !variantCanSave.value) {
      return;
    }

    if (variantForm.id && !canUpdateVariants.value) {
      setOutcome(null, 'Variant update permission is required to save variant changes.');
      return;
    }

    if (!variantForm.id && !canCreateVariants.value) {
      setOutcome(null, 'Variant create permission is required to create variants.');
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      const attributes = cloneVariantAttributeValues(variantForm.attributes);

      if (variantForm.id) {
        await catalogApi.updateProductVariant(authStore.token, {
          id: variantForm.id,
          parentId: variantForm.parentId,
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
          parentId: variantForm.parentId,
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
      resetVariantForm();
      selectedVariantId.value = '';
      isVariantDialogOpen.value = false;
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Variant save failed.');
    } finally {
      saving.value = false;
    }
  }

  async function deleteVariant(variant: ProductVariantResponse) {
    if (!authStore.token || !canDeleteVariants.value) {
      setOutcome(null, 'Variant delete permission is required to delete variants.');
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
        resetVariantForm();
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
    products,
    variants,
    attributeDefinitions,
    isVariantDialogOpen,
    catalogSearch,
    selectedVariant,
    variantForm,
    newAttributeName,
    creatingAttribute,
    canViewCurrentPage,
    canManageCurrentPage,
    canCreateVariants,
    canUpdateVariants,
    canDeleteVariants,
    canManageVariants,
    filteredVariants,
    variantCanSave,
    createAttributeDefinition,
    openVariantDialog,
    openExistingVariantDialog,
    refresh,
    saveVariant,
    deleteVariant,
  };
}
