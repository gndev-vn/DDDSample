<script setup lang="ts">
import { computed } from 'vue';
import VariantAttributeEditor from '../components/catalog/VariantAttributeEditor.vue';
import EntityDialog from '../components/ui/EntityDialog.vue';
import { useVariantAdmin } from '../composables/useVariantAdmin';
import { formatCurrency } from '../lib/formatters';
import { summarizeVariantAttributes } from '../lib/variantAttributes';

const admin = useVariantAdmin();

const summaryCards = computed(() => [
  { label: 'Variants', value: admin.variants.value.length },
  { label: 'Products with variants', value: new Set(admin.variants.value.map((variant) => variant.parentId)).size },
  { label: 'Active variants', value: admin.variants.value.filter((variant) => variant.isActive).length },
  { label: 'Attributes', value: admin.attributeDefinitions.value.length },
]);
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Product variants</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Manage SKU-level variants with a reusable attribute library and clearer merchandising context.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="admin.catalogSearch.value" class="toolbar-search" placeholder="Search variants, products, attributes..." />
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

    <div class="grid gap-6 xl:grid-cols-[minmax(0,1.25fr)_380px]">
      <article class="section-panel space-y-6">
        <div class="flex flex-wrap items-center justify-between gap-4">
          <div>
            <p class="workspace-label">Variant workspace</p>
            <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
              Keep sellable variants clean by pairing each one with a controlled set of reusable attributes.
            </p>
          </div>

          <button class="btn-primary" :disabled="!admin.canManageCurrentPage.value" @click="admin.openVariantDialog()">
            <span class="button-icon" aria-hidden="true">＋</span>
            <span>New variant</span>
          </button>
        </div>

        <div class="text-sm text-slate-600">
          {{ admin.filteredVariants.value.length }} of {{ admin.variants.value.length }} variant record(s).
        </div>

        <div class="table-shell">
          <table class="data-table">
            <thead>
              <tr>
                <th class="px-4 py-4">SKU</th>
                <th class="px-4 py-4">Variant</th>
                <th class="px-4 py-4">Product</th>
                <th class="px-4 py-4">Attributes</th>
                <th class="px-4 py-4">Price</th>
                <th class="px-4 py-4">Status</th>
                <th class="px-4 py-4">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="variant in admin.filteredVariants.value" :key="variant.id">
                <td class="px-4 py-4 font-mono text-xs uppercase tracking-[0.16em] text-slate-500">{{ variant.sku }}</td>
                <td class="px-4 py-4">
                  <button class="text-left font-medium text-slate-900" @click="admin.openExistingVariantDialog(variant)">{{ variant.name }}</button>
                </td>
                <td class="px-4 py-4 text-slate-600">{{ admin.products.value.find((product) => product.id === variant.parentId)?.name ?? 'Unknown product' }}</td>
                <td class="px-4 py-4 text-slate-600">
                  {{ variant.attributes.length ? summarizeVariantAttributes(variant.attributes, 2) : 'No attributes yet' }}
                </td>
                <td class="px-4 py-4 text-brand-700">{{ formatCurrency(variant.overridePrice, variant.currency) }}</td>
                <td class="px-4 py-4 text-slate-600">{{ variant.isActive ? 'Active' : 'Inactive' }}</td>
                <td class="px-4 py-4">
                  <div class="flex flex-wrap gap-2">
                    <button class="icon-button" title="Details" aria-label="View variant details" @click="admin.openExistingVariantDialog(variant)">
                      <span class="icon-glyph">👁</span>
                      <span>Details</span>
                    </button>
                    <button class="icon-button icon-button-danger" :disabled="admin.deleting.value === variant.id || !admin.canManageVariants.value" title="Delete" aria-label="Delete variant" @click="admin.deleteVariant(variant)">
                      <span v-if="admin.deleting.value === variant.id" class="button-spinner" aria-hidden="true" />
                      <span v-else class="icon-glyph">🗑</span>
                      <span>{{ admin.deleting.value === variant.id ? 'Deleting...' : 'Delete' }}</span>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>

          <p v-if="!admin.filteredVariants.value.length && !admin.loading.value" class="px-4 py-4 text-sm text-slate-500">
            {{ admin.variants.value.length ? 'No variants match the current search.' : 'No variants returned by the API yet.' }}
          </p>
        </div>
      </article>

      <aside class="section-panel space-y-4">
        <div>
          <p class="workspace-label">Attribute library</p>
          <h4 class="mt-3 section-title">Reusable variant attributes</h4>
          <p class="mt-2 text-sm leading-6 text-[var(--color-ink-muted)]">
            Create the core attributes once, then assign them consistently across all sellable variants.
          </p>
        </div>

        <div class="space-y-3">
          <input v-model="admin.newAttributeName.value" class="text-input" :disabled="admin.creatingAttribute.value || !admin.canManageVariants.value" placeholder="New attribute definition" />
          <button class="btn-secondary w-full" :disabled="admin.creatingAttribute.value || !admin.canManageVariants.value || !admin.newAttributeName.value.trim()" @click="admin.createAttributeDefinition">
            <span v-if="admin.creatingAttribute.value" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">＋</span>
            <span>{{ admin.creatingAttribute.value ? 'Creating...' : 'Add definition' }}</span>
          </button>
        </div>

        <div class="space-y-2">
          <article v-for="definition in admin.attributeDefinitions.value" :key="definition.id" class="attribute-library-item">
            <div>
              <div class="font-medium text-slate-900">{{ definition.name }}</div>
              <div class="mt-1 text-xs text-slate-500">Used on {{ definition.usageCount }} variant{{ definition.usageCount === 1 ? '' : 's' }}</div>
            </div>
          </article>
          <p v-if="!admin.attributeDefinitions.value.length" class="text-sm text-slate-500">
            No attribute definitions yet. Create a few common ones like Color, Size, or Material to speed up variant authoring.
          </p>
        </div>
      </aside>
    </div>

    <EntityDialog :open="admin.isVariantDialogOpen.value" title="Variant details" description="Create or update a sellable variant with reusable attribute assignments." width-class="max-w-6xl" @close="admin.isVariantDialogOpen.value = false">
      <div class="grid gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(340px,0.95fr)]">
        <div class="space-y-4">
          <label>
            <span class="field-label">Parent product</span>
            <select v-model="admin.variantForm.parentId" class="select-input">
              <option value="">Select a product</option>
              <option v-for="product in admin.products.value" :key="product.id" :value="product.id">{{ product.name }}</option>
            </select>
          </label>
          <div class="rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
            <span class="field-label">Product context</span>
            <p class="mt-2 font-medium text-slate-900">{{ admin.products.value.find((product) => product.id === admin.variantForm.parentId)?.name || 'Select a product' }}</p>
          </div>
          <label>
            <span class="field-label">Name</span>
            <input v-model="admin.variantForm.name" class="text-input" />
          </label>
          <label>
            <span class="field-label">SKU</span>
            <input v-model="admin.variantForm.sku" class="text-input" />
          </label>
          <div class="grid gap-4 md:grid-cols-2">
            <label>
              <span class="field-label">Override price</span>
              <input v-model.number="admin.variantForm.overridePrice" class="text-input" type="number" min="0" step="0.01" />
            </label>
            <label>
              <span class="field-label">Currency</span>
              <input v-model="admin.variantForm.currency" class="text-input" />
            </label>
          </div>
          <label>
            <span class="field-label">Description</span>
            <textarea v-model="admin.variantForm.description" class="text-area" />
          </label>
        </div>

        <VariantAttributeEditor
          v-model="admin.variantForm.attributes"
          v-model:newDefinitionName="admin.newAttributeName.value"
          :definitions="admin.attributeDefinitions.value"
          :disabled="admin.saving.value || !admin.canManageVariants.value"
          :creating-definition="admin.creatingAttribute.value"
          :can-manage-definitions="admin.canManageVariants.value"
          @create-definition="admin.createAttributeDefinition"
        />
      </div>

      <div class="mt-6 flex flex-wrap gap-2">
        <button class="btn-primary" :disabled="admin.saving.value || !admin.canManageVariants.value || !admin.variantCanSave.value" @click="admin.saveVariant">
          <span v-if="admin.saving.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">💾</span>
          <span>{{ admin.saving.value ? 'Saving...' : admin.variantForm.id ? 'Save variant' : 'Add variant' }}</span>
        </button>
        <button v-if="admin.selectedVariant.value" class="btn-danger" :disabled="admin.deleting.value === admin.selectedVariant.value.id || !admin.canManageVariants.value" @click="admin.deleteVariant(admin.selectedVariant.value)">
          Delete variant
        </button>
      </div>
    </EntityDialog>
  </section>
</template>


