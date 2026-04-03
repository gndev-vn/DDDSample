<script setup lang="ts">
import EntityDialog from '../components/ui/EntityDialog.vue';
import StatusBadge from '../components/ui/StatusBadge.vue';
import { useOrdersAdmin } from '../composables/useOrdersAdmin';
import { formatCurrency, orderStatusLabel, toneForStatus } from '../lib/formatters';

const {
  loading,
  submitting,
  saving,
  deletingOrderId,
  actionOrderId,
  error,
  success,
  orders,
  variants,
  cachedProducts,
  selectedOrderId,
  isOrderDialogOpen,
  isCreateOrderDialogOpen,
  orderSearch,
  createForm,
  editForm,
  selectedVariant,
  selectedOrder,
  hasOrderChanges,
  canManageOrders,
  filteredOrders,
  openOrderDialog,
  openCreateOrderDialog,
  refresh,
  submitOrder,
  saveOrder,
  runOrderAction,
  deleteOrder,
  addLine,
  removeLine,
} = useOrdersAdmin();
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Order administration</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Production order operations with a single table, detail dialog, and creation dialog.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="orderSearch" class="toolbar-search" placeholder="Search orders, customer, SKU..." />
        <button class="btn-primary" :disabled="!canManageOrders" @click="openCreateOrderDialog"><span class="button-icon" aria-hidden="true">＋</span><span>New order</span></button>
        <button class="btn-secondary" :disabled="loading" @click="refresh">
          {{ loading ? 'Refreshing...' : 'Reload orders' }}
        </button>
      </div>
    </div>

    <div v-if="!canManageOrders" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account does not have permission to manage orders.
    </div>

    <div v-if="success" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ success }}
    </div>

    <div v-if="error" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <article class="section-panel">
      <div class="mb-4 text-sm text-slate-600">
        {{ filteredOrders.length }} of {{ orders.length }} order record(s).
      </div>
      <div class="table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-4 py-4">Order ID</th>
              <th class="px-4 py-4">Customer</th>
              <th class="px-4 py-4">Lines</th>
              <th class="px-4 py-4">Status</th>
              <th class="px-4 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in filteredOrders" :key="order.id">
              <td class="px-4 py-4">
                <button class="text-left font-semibold text-slate-900" @click="openOrderDialog(order)">{{ order.id }}</button>
              </td>
              <td class="px-4 py-4 text-slate-600">{{ order.customerId }}</td>
              <td class="px-4 py-4 text-slate-500">{{ order.lines.length }} line item(s)</td>
              <td class="px-4 py-4"><StatusBadge :label="orderStatusLabel(order.status)" :tone="toneForStatus(order.status, 'order')" /></td>
              <td class="px-4 py-4">
                <div class="flex flex-wrap gap-2">
                  <button class="icon-button" title="Details" aria-label="View order details" @click="openOrderDialog(order)"><span class="icon-glyph">👁</span></button>
                  <button class="icon-button" title="Mark paid" aria-label="Mark order as paid" :disabled="actionOrderId === order.id || !canManageOrders" @click="runOrderAction(order.id, 'pay')"><span v-if="actionOrderId === order.id" class="button-spinner" aria-hidden="true" /><span v-else class="icon-glyph">✓</span></button>
                  <button class="icon-button" title="Cancel" aria-label="Cancel order" :disabled="actionOrderId === order.id || !canManageOrders" @click="runOrderAction(order.id, 'cancel')"><span v-if="actionOrderId === order.id" class="button-spinner" aria-hidden="true" /><span v-else class="icon-glyph">✕</span></button>
                  <button class="icon-button icon-button-danger" title="Delete" aria-label="Delete order" :disabled="deletingOrderId === order.id || !canManageOrders" @click="deleteOrder(order.id)"><span v-if="deletingOrderId === order.id" class="button-spinner" aria-hidden="true" /><span v-else class="icon-glyph">🗑</span></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!filteredOrders.length && !loading" class="px-4 py-4 text-sm text-slate-500">
          {{ orders.length ? 'No orders match the current search.' : 'No orders returned by the Ordering API yet.' }}
        </p>
      </div>
    </article>

    <EntityDialog
      :open="isOrderDialogOpen"
      title="Order details"
      description="Review and update the selected order."
      @close="isOrderDialogOpen = false"
    >
      <div v-if="selectedOrder" class="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]">
        <div class="grid gap-4">
          <label><span class="field-label">Customer ID</span><input v-model="editForm.customerId" class="text-input" /></label>
          <div class="grid gap-4 md:grid-cols-2">
            <label class="md:col-span-2"><span class="field-label">Shipping line 1</span><input v-model="editForm.shippingAddress.line1" class="text-input" /></label>
            <label><span class="field-label">Shipping line 2</span><input v-model="editForm.shippingAddress.line2" class="text-input" /></label>
            <label><span class="field-label">City</span><input v-model="editForm.shippingAddress.city" class="text-input" /></label>
            <label><span class="field-label">Province</span><input v-model="editForm.shippingAddress.province" class="text-input" /></label>
            <label><span class="field-label">District</span><input v-model="editForm.shippingAddress.district" class="text-input" /></label>
            <label><span class="field-label">Ward</span><input v-model="editForm.shippingAddress.ward" class="text-input" /></label>
          </div>

          <div>
            <div class="mb-3 flex items-center justify-between gap-3">
              <span class="field-label mb-0">Line items</span>
              <button class="btn-secondary" :disabled="!canManageOrders" @click="addLine"><span class="button-icon" aria-hidden="true">＋</span><span>Add line</span></button>
            </div>
            <div class="space-y-3">
              <div v-for="line in editForm.lines" :key="line.id" class="subtle-panel">
                <div class="grid gap-4 md:grid-cols-2">
                  <label><span class="field-label">Product ID</span><input v-model="line.productId" class="text-input" /></label>
                  <label><span class="field-label">Name</span><input v-model="line.name" class="text-input" /></label>
                  <label><span class="field-label">SKU</span><input v-model="line.sku" class="text-input" /></label>
                  <label><span class="field-label">Currency</span><input v-model="line.currency" class="text-input" /></label>
                  <label><span class="field-label">Quantity</span><input v-model.number="line.quantity" class="text-input" type="number" min="1" /></label>
                  <label><span class="field-label">Unit price</span><input v-model.number="line.unitPrice" class="text-input" type="number" min="0" step="0.01" /></label>
                </div>
                <button class="btn-danger mt-4" :disabled="!canManageOrders" @click="removeLine(line.id)"><span class="button-icon" aria-hidden="true">−</span><span>Remove line</span></button>
              </div>
            </div>
          </div>

          <button class="btn-primary" :disabled="saving || !canManageOrders || !hasOrderChanges" @click="saveOrder">
            <span v-if="saving" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">💾</span>
            <span>{{ saving ? 'Saving order...' : 'Save order changes' }}</span>
          </button>
        </div>

        <aside class="subtle-panel">
          <h5 class="text-sm font-semibold text-slate-900">Cached product snapshot</h5>
          <div v-if="selectedOrderId && cachedProducts[selectedOrderId]?.length" class="mt-4 space-y-3">
            <div v-for="product in cachedProducts[selectedOrderId]" :key="product.id" class="rounded-2xl bg-white p-4">
              <div class="flex flex-wrap items-center justify-between gap-3">
                <p class="font-semibold text-slate-900">{{ product.name }}</p>
                <p class="text-sm text-slate-700">{{ formatCurrency(product.currentPrice, product.currency) }}</p>
              </div>
              <p class="mt-2 text-xs uppercase tracking-[0.16em] text-slate-500">{{ product.sku }}</p>
            </div>
          </div>
          <p v-else class="mt-4 text-sm text-slate-500">No cached product snapshot is available for this order yet.</p>
        </aside>
      </div>
    </EntityDialog>

    <EntityDialog
      :open="isCreateOrderDialogOpen"
      title="New order"
      description="Create a new order from the available product variants."
      @close="isCreateOrderDialogOpen = false"
    >
      <div class="grid gap-6 xl:grid-cols-2">
        <div class="grid gap-4">
          <label><span class="field-label">Customer ID</span><input v-model="createForm.customerId" class="text-input" /></label>
          <div class="grid gap-4 md:grid-cols-[minmax(0,1fr)_140px]">
            <label>
              <span class="field-label">Variant</span>
              <select v-model="createForm.selectedVariantId" class="select-input">
                <option value="" disabled>Select a variant</option>
                <option v-for="variant in variants" :key="variant.id" :value="variant.id">
                  {{ variant.name }} · {{ variant.sku }}
                </option>
              </select>
            </label>
            <label><span class="field-label">Quantity</span><input v-model.number="createForm.quantity" class="text-input" min="1" type="number" /></label>
          </div>

          <div v-if="selectedVariant" class="subtle-panel text-sm text-slate-700">
            <p class="font-medium text-slate-900">{{ selectedVariant.name }}</p>
            <p class="mt-1 text-slate-500">{{ selectedVariant.sku }}</p>
            <p class="mt-3 text-brand-700">{{ formatCurrency(selectedVariant.overridePrice, selectedVariant.currency) }}</p>
          </div>
        </div>

        <div class="grid gap-4">
          <div class="grid gap-4 md:grid-cols-2">
            <label class="md:col-span-2"><span class="field-label">Shipping line 1</span><input v-model="createForm.shippingAddress.line1" class="text-input" /></label>
            <label><span class="field-label">Shipping line 2</span><input v-model="createForm.shippingAddress.line2" class="text-input" /></label>
            <label><span class="field-label">City</span><input v-model="createForm.shippingAddress.city" class="text-input" /></label>
            <label><span class="field-label">Province</span><input v-model="createForm.shippingAddress.province" class="text-input" /></label>
            <label><span class="field-label">District</span><input v-model="createForm.shippingAddress.district" class="text-input" /></label>
            <label><span class="field-label">Ward</span><input v-model="createForm.shippingAddress.ward" class="text-input" /></label>
          </div>

          <label class="flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
            <input v-model="createForm.billingSameAsShipping" type="checkbox" />
            Billing address is the same as shipping
          </label>

          <div v-if="!createForm.billingSameAsShipping" class="grid gap-4 md:grid-cols-2">
            <label class="md:col-span-2"><span class="field-label">Billing line 1</span><input v-model="createForm.billingAddress.line1" class="text-input" /></label>
            <label><span class="field-label">Billing line 2</span><input v-model="createForm.billingAddress.line2" class="text-input" /></label>
            <label><span class="field-label">City</span><input v-model="createForm.billingAddress.city" class="text-input" /></label>
            <label><span class="field-label">Province</span><input v-model="createForm.billingAddress.province" class="text-input" /></label>
            <label><span class="field-label">District</span><input v-model="createForm.billingAddress.district" class="text-input" /></label>
            <label><span class="field-label">Ward</span><input v-model="createForm.billingAddress.ward" class="text-input" /></label>
          </div>

          <button class="btn-primary" :disabled="submitting || !canManageOrders" @click="submitOrder">
            <span v-if="submitting" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">＋</span>
            <span>{{ submitting ? 'Creating order...' : 'Create order' }}</span>
          </button>
        </div>
      </div>
    </EntityDialog>
  </section>
</template>


