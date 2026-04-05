<script setup lang="ts">
import { computed } from 'vue';
import { useCategoryAdmin } from '../composables/useCategoryAdmin';
import EntityDialog from '../components/ui/EntityDialog.vue';

const admin = useCategoryAdmin();

const summaryCards = computed(() => [
  { label: 'Categories', value: admin.categories.value.length },
  {
    label: 'Root categories',
    value: admin.categories.value.filter((category) => !category.parentId || category.parentId === admin.emptyGuid).length,
  },
  { label: 'Active categories', value: admin.categories.value.filter((category) => category.isActive).length },
]);
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Categories</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">Manage the catalog taxonomy and parent-child category structure.</p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="admin.catalogSearch.value" class="toolbar-search" placeholder="Search categories..." />
        <button class="btn-secondary self-start" :disabled="admin.loading.value" @click="admin.refresh">
          <span v-if="admin.loading.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ admin.loading.value ? 'Refreshing...' : 'Reload data' }}</span>
        </button>
      </div>
    </div>

    <div v-if="!admin.canViewCurrentPage.value" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account does not have permission to view this catalog workspace.
    </div>

    <div v-else-if="!admin.canManageCurrentPage.value" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account can review this workspace, but cannot create, update, or delete records here.
    </div>

    <div v-if="admin.success.value" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ admin.success.value }}
    </div>

    <div v-if="admin.error.value" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ admin.error.value }}
    </div>

    <div class="flex flex-wrap gap-3">
      <article v-for="card in summaryCards" :key="card.label" class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">{{ card.label }}</p>
        <p class="mt-3 display-value">{{ card.value }}</p>
      </article>
    </div>

    <article class="section-panel">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <p class="workspace-label">Category workspace</p>
        </div>

        <button class="btn-primary" :disabled="!admin.canCreateCategories.value" @click="admin.openCategoryDialog()">
          <span class="button-icon" aria-hidden="true">＋</span>
          <span>New category</span>
        </button>
      </div>

      <div class="mt-4 text-sm text-slate-600">
        {{ admin.filteredCategories.value.length }} of {{ admin.categories.value.length }} category record(s).
      </div>

      <div class="mt-6 table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-4 py-4">Name</th>
              <th class="px-4 py-4">Slug</th>
              <th class="px-4 py-4">Parent</th>
              <th class="px-4 py-4">Status</th>
              <th class="px-4 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="category in admin.filteredCategories.value" :key="category.id">
              <td class="px-4 py-4">
                <button class="text-left font-semibold text-slate-900" @click="admin.openCategoryDialog(category)">{{ category.name }}</button>
              </td>
              <td class="px-4 py-4 text-xs uppercase tracking-[0.16em] text-slate-500">{{ category.slug }}</td>
              <td class="px-4 py-4 text-slate-600">{{ admin.categories.value.find((item) => item.id === category.parentId)?.name ?? 'Root' }}</td>
              <td class="px-4 py-4 text-slate-600">{{ category.isActive ? 'Active' : 'Inactive' }}</td>
              <td class="px-4 py-4">
                <div class="flex flex-wrap gap-2">
                  <button class="icon-button" title="Details" aria-label="View category details" @click="admin.openCategoryDialog(category)"><span class="icon-glyph">👁</span></button>
                  <button class="icon-button icon-button-danger" :disabled="admin.deleting.value === category.id || !admin.canDeleteCategories.value" title="Delete" aria-label="Delete category" @click="admin.deleteCategory(category)"><span v-if="admin.deleting.value === category.id" class="button-spinner" aria-hidden="true" /><span v-else class="icon-glyph">✕</span></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!admin.filteredCategories.value.length && !admin.loading.value" class="px-4 py-4 text-sm text-slate-500">
          {{ admin.categories.value.length ? 'No categories match the current search.' : 'No categories returned by the API yet.' }}
        </p>
      </div>
    </article>

    <EntityDialog :open="admin.isCategoryDialogOpen.value" title="Category details" description="View and update category information in a focused dialog." width-class="max-w-3xl" @close="admin.isCategoryDialogOpen.value = false">
      <div class="grid gap-4">
        <label>
          <span class="field-label">Name</span>
          <input v-model="admin.categoryForm.name" class="text-input" />
        </label>
        <label>
          <span class="field-label">Slug</span>
          <input v-model="admin.categoryForm.slug" class="text-input" />
        </label>
        <label>
          <span class="field-label">Description</span>
          <textarea v-model="admin.categoryForm.description" class="text-area" />
        </label>
        <label>
          <span class="field-label">Parent category</span>
          <select v-model="admin.categoryForm.parentId" class="select-input">
            <option value="">No parent</option>
            <option v-for="category in admin.categories.value.filter((item) => item.id !== admin.categoryForm.id)" :key="category.id" :value="category.id">
              {{ category.name }}
            </option>
          </select>
        </label>
        <label class="flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
          <input v-model="admin.categoryForm.isActive" type="checkbox" />
          Active?
        </label>
        <div class="flex flex-wrap gap-2">
          <button class="btn-primary" :disabled="admin.saving.value || !(admin.categoryForm.id ? admin.canUpdateCategories.value : admin.canCreateCategories.value) || !admin.categoryCanSave.value" @click="admin.saveCategory">
            <span v-if="admin.saving.value" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">✓</span>
            <span>{{ admin.saving.value ? 'Saving...' : admin.categoryForm.id ? 'Save category' : 'Create category' }}</span>
          </button>
          <button v-if="admin.selectedCategory.value" class="btn-danger" :disabled="admin.deleting.value === admin.selectedCategory.value.id || !admin.canDeleteCategories.value" @click="admin.deleteCategory(admin.selectedCategory.value)">
            Delete category
          </button>
        </div>
      </div>
    </EntityDialog>
  </section>
</template>




