<script setup lang="ts">
import { computed } from 'vue';
import VariantAttributeEditor from '../components/catalog/VariantAttributeEditor.vue';
import EntityDialog from '../components/ui/EntityDialog.vue';
import { useProductAdmin } from '../composables/useProductAdmin';
import { formatCurrency } from '../lib/formatters';
import { summarizeVariantAttributes } from '../lib/variantAttributes';

const admin = useProductAdmin();

const summaryCards = computed(() => [
  { label: 'Products', value: admin.products.value.length },
  { label: 'Variants', value: admin.variants.value.length },
  { label: 'Active variants', value: admin.variants.value.filter((variant) => variant.isActive).length },
  { label: 'Attributes', value: admin.attributeDefinitions.value.length },
]);

const selectedProductVariantCount = computed(() => admin.selectedProductVariants.value.length);
const selectedProductActiveVariantCount = computed(
  () => admin.selectedProductVariants.value.filter((variant) => variant.isActive).length,
);
const selectedProductAttributeCount = computed(() => {
  const attributeIds = new Set(
    admin.selectedProductVariants.value.flatMap((variant) => variant.attributes.map((attribute) => attribute.attributeId)),
  );
  return attributeIds.size;
});
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Products</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Shape the product catalog around reusable variant attributes and a cleaner merchandising workflow.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input
          v-model="admin.catalogSearch.value"
          class="toolbar-search"
          placeholder="Search products, variants, SKU, attributes..."
        />
        <button class="btn-secondary self-start" :disabled="admin.loading.value" @click="admin.refresh">
          <span v-if="admin.loading.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ admin.loading.value ? 'Refreshing...' : 'Reload data' }}</span>
        </button>
        <button class="btn-primary" :disabled="!admin.canManageCurrentPage.value" @click="admin.openProductDialog()">
          <span class="button-icon" aria-hidden="true">＋</span>
          <span>New product</span>
        </button>
      </div>
    </div>

    <div
      v-if="!admin.canViewCurrentPage.value"
      class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800"
    >
      The current account does not have permission to view this catalog workspace.
    </div>

    <div
      v-else-if="!admin.canManageCurrentPage.value"
      class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800"
    >
      The current account can review this workspace, but cannot create, update, or delete records here.
    </div>

    <div
      v-if="admin.success.value"
      class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
    >
      {{ admin.success.value }}
    </div>

    <div
      v-if="admin.error.value"
      class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
    >
      {{ admin.error.value }}
    </div>

    <div class="flex flex-wrap gap-3">
      <article v-for="card in summaryCards" :key="card.label" class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">{{ card.label }}</p>
        <p class="mt-3 display-value">{{ card.value }}</p>
      </article>
    </div>

    <article class="section-panel space-y-6">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <p class="workspace-label">Product workspace</p>
          <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
            Keep each product focused, then manage its sellable variants inline with reusable attribute assignments.
          </p>
        </div>
      </div>

      <div class="text-sm text-slate-600">
        {{ admin.filteredProducts.value.length }} of {{ admin.products.value.length }} product record(s).
      </div>

      <div class="table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="w-14 px-4 py-4"></th>
              <th class="px-4 py-4">Product</th>
              <th class="px-4 py-4">Category</th>
              <th class="px-4 py-4">Base price</th>
              <th class="px-4 py-4">Variants</th>
              <th class="px-4 py-4">Status</th>
              <th class="px-4 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="product in admin.filteredProducts.value" :key="product.id">
              <tr class="cursor-pointer hover:bg-white/60" @click="admin.toggleProductExpansion(product.id)">
                <td class="px-4 py-4">
                  <button
                    class="icon-button row-toggle-button"
                    :title="admin.isProductExpanded(product.id) ? 'Collapse variants' : 'Expand variants'"
                    :aria-label="admin.isProductExpanded(product.id) ? 'Collapse variants' : 'Expand variants'"
                    @click.stop="admin.toggleProductExpansion(product.id)"
                  >
                    <span class="icon-glyph">{{ admin.isProductExpanded(product.id) ? '▾' : '▸' }}</span>
                  </button>
                </td>
                <td class="px-4 py-4">
                  <button class="min-w-0 text-left" @click.stop="admin.openProductDialog(product)">
                    <div class="font-semibold text-[var(--color-ink)]">{{ product.name }}</div>
                    <div class="mt-1 line-clamp-2 text-sm text-[var(--color-ink-muted)]">
                      {{ product.description || 'No product description yet.' }}
                    </div>
                  </button>
                </td>
                <td class="px-4 py-4 text-slate-600">
                  {{ admin.categories.value.find((category) => category.id === product.categoryId)?.name ?? 'Unassigned' }}
                </td>
                <td class="px-4 py-4 text-brand-700">{{ formatCurrency(product.basePrice, product.currency) }}</td>
                <td class="px-4 py-4 text-slate-600">{{ admin.variantsForProduct(product.id).length }}</td>
                <td class="px-4 py-4 text-slate-600">{{ product.isActive ? 'Active' : 'Inactive' }}</td>
                <td class="px-4 py-4">
                  <div class="flex flex-wrap justify-end gap-2">
                    <button
                      class="icon-button"
                      :disabled="!admin.canCreateVariants.value"
                      title="Add variant"
                      aria-label="Add variant"
                      @click.stop="admin.openVariantDialog(product)"
                    >
                      <span class="icon-glyph">＋</span>
                    </button>
                    <button
                      class="icon-button"
                      title="Details"
                      aria-label="View product details"
                      @click.stop="admin.openProductDialog(product)"
                    >
                      <span class="icon-glyph">👁</span>
                    </button>
                    <button
                      class="icon-button icon-button-danger"
                      :disabled="admin.deleting.value === product.id || !admin.canManageProducts.value"
                      title="Delete"
                      aria-label="Delete product"
                      @click.stop="admin.deleteProduct(product)"
                    >
                      <span v-if="admin.deleting.value === product.id" class="button-spinner" aria-hidden="true" />
                      <span v-else class="icon-glyph">✕</span>
                    </button>
                  </div>
                </td>
              </tr>

              <template v-if="admin.isProductExpanded(product.id)">
                <tr
                  v-for="variant in admin.variantsForProduct(product.id)"
                  :key="`${product.id}-${variant.id}`"
                  class="bg-white/70"
                >
                  <td class="px-4 py-4"></td>
                  <td class="px-4 py-4">
                    <div class="pl-2">
                      <div class="text-xs uppercase tracking-[0.16em] text-slate-500">{{ variant.sku }}</div>
                      <div class="mt-1 font-medium text-slate-900">{{ variant.name }}</div>
                      <div class="mt-2 text-sm text-slate-500">{{ variant.description || 'No variant description yet.' }}</div>
                    </div>
                  </td>
                  <td class="px-4 py-4 text-slate-500">Variant of {{ product.name }}</td>
                  <td class="px-4 py-4 text-brand-700">{{ formatCurrency(admin.effectiveVariantPrice(variant), variant.currency) }}</td>
                  <td class="px-4 py-4 text-slate-600">
                    {{ variant.attributes.length ? summarizeVariantAttributes(variant.attributes, 2) : 'No attributes yet' }}
                  </td>
                  <td class="px-4 py-4 text-slate-600">{{ variant.isActive ? 'Active' : 'Inactive' }}</td>
                  <td class="px-4 py-4">
                    <div class="flex flex-wrap justify-end gap-2">
                      <button
                        class="icon-button"
                        title="Edit variant"
                        aria-label="Edit variant"
                        @click.stop="admin.openExistingVariantDialog(variant)"
                      >
                        <span class="icon-glyph">✎</span>
                      </button>
                      <button
                        class="icon-button icon-button-danger"
                        :disabled="admin.deleting.value === variant.id || !admin.canDeleteVariants.value"
                        title="Delete variant"
                        aria-label="Delete variant"
                        @click.stop="admin.deleteVariant(variant)"
                      >
                        <span v-if="admin.deleting.value === variant.id" class="button-spinner" aria-hidden="true" />
                        <span v-else class="icon-glyph">✕</span>
                      </button>
                    </div>
                  </td>
                </tr>

                <tr v-if="!admin.variantsForProduct(product.id).length" class="bg-white/70">
                  <td class="px-4 py-4"></td>
                  <td colspan="6" class="px-4 py-4 text-sm text-slate-500">No variants for this product yet.</td>
                </tr>
              </template>
            </template>
          </tbody>
        </table>

        <p v-if="!admin.filteredProducts.value.length && !admin.loading.value" class="px-4 py-4 text-sm text-slate-500">
          {{ admin.products.value.length ? 'No products match the current search.' : 'No products returned by the API yet.' }}
        </p>
      </div>
    </article>

    <EntityDialog
      :open="admin.isProductDialogOpen.value"
      title="Product details"
      description="Define the product, then stage or review the variants that belong to it."
      width-class="max-w-7xl"
      @close="admin.isProductDialogOpen.value = false"
    >
      <div class="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_380px]">
        <div class="space-y-6">
          <div class="grid gap-4 md:grid-cols-2">
            <label class="md:col-span-2">
              <span class="field-label">Name</span>
              <input v-model="admin.productForm.name" class="text-input" />
            </label>
            <label>
              <span class="field-label">Slug</span>
              <input v-model="admin.productForm.slug" class="text-input" />
            </label>
            <label>
              <span class="field-label">Category</span>
              <select v-model="admin.productForm.categoryId" class="select-input">
                <option value="" disabled>Select category</option>
                <option v-for="category in admin.categories.value" :key="category.id" :value="category.id">
                  {{ category.name }}
                </option>
              </select>
            </label>
            <label>
              <span class="field-label">Base price</span>
              <input v-model.number="admin.productForm.basePrice" class="text-input" type="number" min="0" step="0.01" />
            </label>
            <label>
              <span class="field-label">Currency</span>
              <input v-model="admin.productForm.currency" class="text-input" />
            </label>
            <label class="md:col-span-2">
              <span class="field-label">Description</span>
              <textarea v-model="admin.productForm.description" class="text-area" />
            </label>
          </div>

          <div class="rounded-[24px] bg-[var(--color-surface-low)] p-5">
            <div class="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p class="workspace-label">Variant lineup</p>
                <h4 class="mt-2 section-title">Variants for this product</h4>
              </div>
              <button
                class="btn-secondary"
                :disabled="!admin.canCreateVariants.value"
                @click="admin.openVariantDialog(admin.selectedProduct.value ?? undefined)"
              >
                <span class="button-icon" aria-hidden="true">＋</span>
                <span>Add variant</span>
              </button>
            </div>

            <p v-if="!admin.productForm.id" class="mt-3 text-sm text-slate-500">
              Variants added here stay in draft until the product is created, then they are published together.
            </p>

            <div class="mt-4 overflow-x-auto rounded-[20px] bg-white">
              <table class="min-w-full text-left text-sm">
                <thead class="text-xs font-semibold uppercase tracking-[0.16em] text-[var(--color-ink-muted)]">
                  <tr>
                    <th class="px-4 py-4">SKU</th>
                    <th class="px-4 py-4">Variant</th>
                    <th class="px-4 py-4">Attributes</th>
                    <th class="px-4 py-4">Price</th>
                    <th class="px-4 py-4">Actions</th>
                  </tr>
                </thead>
                <tbody v-if="admin.productForm.id">
                  <tr v-for="variant in admin.selectedProductVariants.value" :key="variant.id" class="border-t border-slate-100">
                    <td class="px-4 py-4 font-mono text-xs uppercase tracking-[0.16em] text-slate-500">{{ variant.sku }}</td>
                    <td class="px-4 py-4">
                      <div class="font-medium text-slate-900">{{ variant.name }}</div>
                      <div class="mt-1 text-xs text-slate-500">{{ variant.description || 'No description yet.' }}</div>
                    </td>
                    <td class="px-4 py-4 text-slate-600">
                      {{ variant.attributes.length ? summarizeVariantAttributes(variant.attributes, 3) : 'No attributes yet' }}
                    </td>
                    <td class="px-4 py-4 text-brand-700">{{ formatCurrency(admin.effectiveVariantPrice(variant), variant.currency) }}</td>
                    <td class="px-4 py-4">
                      <div class="flex flex-wrap justify-end gap-2">
                        <button class="icon-button" title="Edit variant" aria-label="Edit variant" @click="admin.openExistingVariantDialog(variant)">
                          <span class="icon-glyph">✎</span>
                        </button>
                        <button
                          class="icon-button icon-button-danger"
                          :disabled="admin.deleting.value === variant.id || !admin.canDeleteVariants.value"
                          title="Delete variant"
                          aria-label="Delete variant"
                          @click="admin.deleteVariant(variant)"
                        >
                          <span v-if="admin.deleting.value === variant.id" class="button-spinner" aria-hidden="true" />
                          <span v-else class="icon-glyph">✕</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                  <tr v-if="!admin.selectedProductVariants.value.length">
                    <td colspan="5" class="px-4 py-4 text-sm text-slate-500">No variants added to this product yet.</td>
                  </tr>
                </tbody>
                <tbody v-else>
                  <tr v-for="draft in admin.pendingVariants.value" :key="draft.id" class="border-t border-slate-100">
                    <td class="px-4 py-4 font-mono text-xs uppercase tracking-[0.16em] text-slate-500">{{ draft.sku }}</td>
                    <td class="px-4 py-4">
                      <div class="font-medium text-slate-900">{{ draft.name }}</div>
                      <div class="mt-1 text-xs text-slate-500">{{ draft.description || 'No description yet.' }}</div>
                    </td>
                    <td class="px-4 py-4 text-slate-600">
                      {{ draft.attributes.length ? draft.attributes.length + ' attribute assignment(s)' : 'No attributes yet' }}
                    </td>
                    <td class="px-4 py-4 text-brand-700">{{ formatCurrency(draft.overridePrice ?? admin.productForm.basePrice, draft.currency || admin.productForm.currency) }}</td>
                    <td class="px-4 py-4">
                      <div class="flex flex-wrap justify-end gap-2">
                        <button class="icon-button" title="Edit draft" aria-label="Edit draft variant" @click="admin.openDraftVariantDialog(draft.id)">
                          <span class="icon-glyph">✎</span>
                        </button>
                        <button class="icon-button icon-button-danger" title="Remove draft" aria-label="Remove draft variant" @click="admin.removePendingVariant(draft.id)">
                          <span class="icon-glyph">✕</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                  <tr v-if="!admin.pendingVariants.value.length">
                    <td colspan="5" class="px-4 py-4 text-sm text-slate-500">No variants staged for this new product yet.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="flex flex-wrap justify-end gap-2">
            <button
              class="btn-primary"
              :disabled="admin.saving.value || !(admin.productForm.id ? admin.canUpdateProducts.value : admin.canCreateProducts.value) || !admin.productCanSave.value"
              @click="admin.saveProduct"
            >
              <span v-if="admin.saving.value" class="button-spinner" aria-hidden="true" />
              <span v-else class="button-icon" aria-hidden="true">✓</span>
              <span>{{ admin.saving.value ? 'Saving...' : admin.productForm.id ? 'Save product' : 'Create product' }}</span>
            </button>
            <button
              v-if="admin.selectedProduct.value"
              class="btn-danger"
              :disabled="admin.deleting.value === admin.selectedProduct.value.id || !admin.canDeleteProducts.value"
              @click="admin.deleteProduct(admin.selectedProduct.value)"
            >
              Delete product
            </button>
          </div>
        </div>

        <aside class="section-panel space-y-4">
          <div>
            <p class="workspace-label">Merchandising snapshot</p>
            <h4 class="mt-3 section-title">Product coverage</h4>
            <p class="mt-2 text-sm leading-6 text-[var(--color-ink-muted)]">
              Use these signals to keep the selected product ready for selling, with enough variant coverage and clear attribute data.
            </p>
          </div>

          <div class="grid gap-3">
            <article class="summary-tile">
              <p class="workspace-label">Variants</p>
              <p class="mt-3 display-value">{{ admin.productForm.id ? selectedProductVariantCount : admin.pendingVariants.value.length }}</p>
            </article>
            <article class="summary-tile">
              <p class="workspace-label">Active variants</p>
              <p class="mt-3 display-value">{{ admin.productForm.id ? selectedProductActiveVariantCount : admin.pendingVariants.value.length }}</p>
            </article>
            <article class="summary-tile">
              <p class="workspace-label">Attribute coverage</p>
              <p class="mt-3 display-value">{{ admin.productForm.id ? selectedProductAttributeCount : admin.attributeDefinitions.value.length }}</p>
            </article>
          </div>

          <div class="space-y-3">
            <h5 class="text-sm font-semibold text-slate-900">Attribute library</h5>
            <div class="space-y-2">
              <article
                v-for="definition in admin.attributeDefinitions.value.slice(0, 6)"
                :key="definition.id"
                class="attribute-library-item"
              >
                <div>
                  <div class="font-medium text-slate-900">{{ definition.name }}</div>
                  <div class="mt-1 text-xs text-slate-500">
                    Used on {{ definition.usageCount }} variant{{ definition.usageCount === 1 ? '' : 's' }}
                  </div>
                </div>
              </article>
              <p v-if="!admin.attributeDefinitions.value.length" class="text-sm text-slate-500">
                No reusable attributes yet. Add a few common definitions before scaling variant coverage.
              </p>
            </div>
          </div>
        </aside>
      </div>
    </EntityDialog>

    <EntityDialog
      :open="admin.isVariantDialogOpen.value"
      title="Variant details"
      description="Create or update a sellable variant with reusable attribute assignments."
      width-class="max-w-6xl"
      @close="admin.isVariantDialogOpen.value = false"
    >
      <div class="grid gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(340px,0.95fr)]">
        <div class="space-y-4">
          <div v-if="admin.selectedProduct.value || admin.productForm.name" class="subtle-panel">
            <span class="field-label">Assigned to current product</span>
            <p class="mt-2 font-medium text-slate-900">
              {{ admin.selectedProduct.value?.name || admin.productForm.name || 'Current draft product' }}
            </p>
            <p class="mt-2 text-sm text-slate-600">
              This variant stays attached to the product you are creating or editing here.
            </p>
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
              <input :value="admin.variantForm.overridePrice ?? ''" class="text-input" type="number" min="0" step="0.01" placeholder="Leave empty to use product price" @input="admin.setVariantOverridePrice($event.target.value)" />
            </label>
            <label>
              <span class="field-label">Currency</span>
              <input :value="admin.currentVariantCurrency()" class="text-input" disabled />
            </label>
          </div>
          <p class="-mt-2 text-sm text-slate-500">
            <span v-if="admin.variantForm.overridePrice == null">This variant will use the current product price.</span>
            <span v-else>Set an override only when this variant must sell at a different price.</span>
          </p>
          <label>
            <span class="field-label">Description</span>
            <textarea v-model="admin.variantForm.description" class="text-area" />
          </label>
        </div>

        <VariantAttributeEditor
          v-model="admin.variantForm.attributes"
          v-model:newDefinitionName="admin.newAttributeName.value"
          :definitions="admin.attributeDefinitions.value"
          :disabled="admin.saving.value || !(admin.variantForm.id ? admin.canUpdateVariants.value : admin.canCreateVariants.value)"
          :creating-definition="admin.creatingAttribute.value"
          :can-manage-definitions="admin.canCreateVariants.value"
          @create-definition="admin.createAttributeDefinition"
        />
      </div>

      <div class="mt-6 flex flex-wrap gap-2">
        <button
          class="btn-primary"
          :disabled="admin.saving.value || !(admin.variantForm.id ? admin.canUpdateVariants.value : admin.canCreateVariants.value) || !admin.variantCanSave.value"
          @click="admin.saveVariant"
        >
          <span v-if="admin.saving.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">✓</span>
          <span>
            {{ admin.saving.value ? 'Saving...' : admin.variantForm.id || admin.editingDraftVariantId.value ? 'Save variant' : 'Add variant' }}
          </span>
        </button>
        <button
          v-if="admin.selectedVariant.value"
          class="btn-danger"
          :disabled="admin.deleting.value === admin.selectedVariant.value.id || !admin.canDeleteVariants.value"
          @click="admin.deleteVariant(admin.selectedVariant.value)"
        >
          Delete variant
        </button>
      </div>
    </EntityDialog>
  </section>
</template>






