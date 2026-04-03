import { computed, onMounted, reactive, ref, watch } from 'vue';
import { catalogApi } from '../api/catalog';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type { CategoryModel } from '../types/contracts';

export function useCategoryAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();
  const emptyGuid = '00000000-0000-0000-0000-000000000000';

  const loading = ref(true);
  const saving = ref(false);
  const deleting = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);

  const categories = ref<CategoryModel[]>([]);
  const isCategoryDialogOpen = ref(false);
  const catalogSearch = ref('');
  const selectedCategoryId = ref('');

  const categoryForm = reactive({
    id: '',
    name: '',
    slug: '',
    description: '',
    parentId: '',
    isActive: true,
  });

  const canViewCategories = computed(() => authStore.hasPermission(appPermissions.categories.view));
  const canManageCategories = computed(() => authStore.hasPermission(appPermissions.categories.manage));
  const canViewCurrentPage = computed(() => canViewCategories.value);
  const canManageCurrentPage = computed(() => canManageCategories.value);

  const selectedCategory = computed(() =>
    categories.value.find((category) => category.id === selectedCategoryId.value) ?? null,
  );

  const filteredCategories = computed(() => {
    const search = catalogSearch.value.trim().toLowerCase();
    if (!search) {
      return categories.value;
    }

    return categories.value.filter((category) =>
      [
        category.name,
        category.slug,
        category.description,
        categories.value.find((item) => item.id === category.parentId)?.name ?? '',
      ]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  const categoryCanSave = computed(() => {
    const hasRequiredFields = Boolean(categoryForm.name.trim() && categoryForm.slug.trim());
    if (!categoryForm.id) {
      return hasRequiredFields;
    }

    if (!selectedCategory.value || !hasRequiredFields) {
      return false;
    }

    return JSON.stringify({
      name: categoryForm.name.trim(),
      slug: categoryForm.slug.trim(),
      description: categoryForm.description.trim(),
      parentId: categoryForm.parentId || '',
      isActive: categoryForm.isActive,
    }) !== JSON.stringify({
      name: selectedCategory.value.name.trim(),
      slug: selectedCategory.value.slug.trim(),
      description: selectedCategory.value.description.trim(),
      parentId: selectedCategory.value.parentId ?? '',
      isActive: selectedCategory.value.isActive,
    });
  });

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function resetCategoryForm() {
    categoryForm.id = '';
    categoryForm.name = '';
    categoryForm.slug = '';
    categoryForm.description = '';
    categoryForm.parentId = '';
    categoryForm.isActive = true;
  }

  function openCategoryDialog(category?: CategoryModel) {
    if (category) {
      selectedCategoryId.value = category.id;
    } else {
      selectedCategoryId.value = '';
      resetCategoryForm();
    }

    isCategoryDialogOpen.value = true;
  }

  async function refresh() {
    if (!authStore.token || !canViewCategories.value) {
      categories.value = [];
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      categories.value = await catalogApi.getCategories(authStore.token);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load category data.');
    } finally {
      loading.value = false;
    }
  }

  async function saveCategory() {
    if (!authStore.token || !canManageCategories.value || !categoryCanSave.value) {
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      if (categoryForm.id) {
        await catalogApi.updateCategory(authStore.token, {
          id: categoryForm.id,
          name: categoryForm.name,
          slug: categoryForm.slug,
          description: categoryForm.description,
          parentId: categoryForm.parentId || emptyGuid,
          isActive: categoryForm.isActive,
        });
        uiStore.pushToast('Category updated successfully.', 'success');
        setOutcome('Category updated successfully.');
      } else {
        await catalogApi.createCategory(authStore.token, {
          name: categoryForm.name,
          slug: categoryForm.slug,
          description: categoryForm.description,
          parentId: categoryForm.parentId || null,
        });
        uiStore.pushToast('Category created successfully.', 'success');
        setOutcome('Category created successfully.');
      }

      resetCategoryForm();
      isCategoryDialogOpen.value = false;
      await refresh();
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Category save failed.');
    } finally {
      saving.value = false;
    }
  }

  async function deleteCategory(category: CategoryModel) {
    if (!authStore.token || !canManageCategories.value) {
      setOutcome(null, 'Category management permission is required to manage categories.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete category',
      message: `Delete "${category.name}"?`,
      confirmLabel: 'Delete category',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deleting.value = category.id;
    setOutcome(null, null);

    try {
      await catalogApi.deleteCategory(authStore.token, category.id);
      if (selectedCategoryId.value === category.id) {
        selectedCategoryId.value = '';
        resetCategoryForm();
      }

      await refresh();
      uiStore.pushToast('Category deleted successfully.', 'success');
      setOutcome('Category deleted successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Category deletion failed.');
    } finally {
      deleting.value = null;
    }
  }

  watch(selectedCategory, (category) => {
    if (!category) {
      return;
    }

    categoryForm.id = category.id;
    categoryForm.name = category.name;
    categoryForm.slug = category.slug;
    categoryForm.description = category.description;
    categoryForm.parentId = category.parentId ?? '';
    categoryForm.isActive = category.isActive;
  });

  onMounted(() => {
    void refresh();
  });

  return {
    emptyGuid,
    loading,
    saving,
    deleting,
    error,
    success,
    categories,
    isCategoryDialogOpen,
    catalogSearch,
    selectedCategory,
    categoryForm,
    canViewCurrentPage,
    canManageCurrentPage,
    canManageCategories,
    filteredCategories,
    categoryCanSave,
    openCategoryDialog,
    refresh,
    saveCategory,
    deleteCategory,
  };
}
