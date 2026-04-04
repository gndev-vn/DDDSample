<script setup lang="ts">
import SearchableSelect from '../components/ui/SearchableSelect.vue';
import EntityDialog from '../components/ui/EntityDialog.vue';
import StatusBadge from '../components/ui/StatusBadge.vue';
import { useOrdersAdmin } from '../composables/useOrdersAdmin';
import { formatCurrency, orderStatusLabel, toneForStatus } from '../lib/formatters';

const {
  loading,
  submitting,
  saving,
  deletingOrderId,
  payingOrderId,
  cancelingOrderId,
  error,
  success,
  orders,
  selectedOrder,
  isOrderDialogOpen,
  isCreateOrderDialogOpen,
  orderSearch,
  customerFilter,
  createForm,
  editForm,
  customerOptions,
  variantOptions,
  filteredOrders,
  canManageOrders,
  canCreateOrders,
  canUpdateOrders,
  canDeleteOrders,
  createOrderCanSubmit,
  hasOrderChanges,
  variantForCreateLine,
  variantUnitPrice,
  variantCurrency,
  lineTotal,
  openOrderDialog,
  openCreateOrderDialog,
  clearCustomerFilter,
  addCreateLine,
  removeCreateLine,
  selectCreateLineVariant,
  refresh,
  submitOrder,
  saveOrder,
  runOrderAction,
  deleteOrder,
} = useOrdersAdmin();
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Orders</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Review order state, update shipping details, and create new orders from sellable variants.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="orderSearch" class="toolbar-search" placeholder="Search orders, customers, SKUs, or product ids" />
        <button class="btn-primary" :disabled="!canCreateOrders" @click="openCreateOrderDialog">
          <span class="button-icon" aria-hidden="true">＋</span>
          <span>New order</span>
        </button>
        <button class="btn-secondary" :disabled="loading" @click="refresh">
          <span v-if="loading" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ loading ? 'Refreshing...' : 'Reload orders' }}</span>
        </button>
      </div>
    </div>

    <div v-if="customerFilter" class="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-brand-200 bg-brand-50 px-4 py-3 text-sm text-brand-900">
      <div>
        Showing orders for <span class="font-semibold">{{ customerFilter.displayName }}</span>
        <span class="text-brand-700">· {{ customerFilter.email }}</span>
      </div>
      <button class="btn-secondary" @click="clearCustomerFilter">Show all orders</button>
    </div>

    <div v-if="!canManageOrders" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account can review orders, but create, update, and delete actions depend on separate permissions.
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
              <th class="px-4 py-4">Order</th>
              <th class="px-4 py-4">Customer</th>
              <th class="px-4 py-4">Items</th>
              <th class="px-4 py-4">Status</th>
              <th class="px-4 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in filteredOrders" :key="order.id">
              <td class="px-4 py-4">
                <div class="font-medium text-slate-900">{{ order.id }}</div>
              </td>
              <td class="px-4 py-4">
                <div class="font-medium text-slate-900">{{ order.customerName }}</div>
                <div class="mt-1 text-xs text-slate-500">{{ order.customerEmail }}</div>
              </td>
              <td class="px-4 py-4 text-slate-500">{{ order.lines.length }} line item(s)</td>
              <td class="px-4 py-4">
                <StatusBadge :label="orderStatusLabel(order.status)" :tone="toneForStatus(order.status, 'order')" />
              </td>
              <td class="px-4 py-4">
                <div class="flex flex-wrap gap-2">
                  <button class="icon-button" title="Details" aria-label="View order details" @click="openOrderDialog(order)">
                    <span class="icon-glyph">✎</span>
                  </button>
                  <button class="icon-button" title="Mark paid" aria-label="Mark order as paid" :disabled="payingOrderId === order.id || !canUpdateOrders" @click="runOrderAction(order.id, 'pay')">
                    <span v-if="payingOrderId === order.id" class="button-spinner" aria-hidden="true" />
                    <span v-else class="icon-glyph">✓</span>
                  </button>
                  <button class="icon-button" title="Cancel" aria-label="Cancel order" :disabled="cancelingOrderId === order.id || !canUpdateOrders" @click="runOrderAction(order.id, 'cancel')">
                    <span v-if="cancelingOrderId === order.id" class="button-spinner" aria-hidden="true" />
                    <span v-else class="icon-glyph">✕</span>
                  </button>
                  <button class="icon-button icon-button-danger" title="Delete" aria-label="Delete order" :disabled="deletingOrderId === order.id || !canDeleteOrders" @click="deleteOrder(order.id)">
                    <span v-if="deletingOrderId === order.id" class="button-spinner" aria-hidden="true" />
                    <span v-else class="icon-glyph">✕</span>
                  </button>
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

    <EntityDialog :open="isOrderDialogOpen" title="Order details" description="Review and update the selected order." @close="isOrderDialogOpen = false">
      <div v-if="selectedOrder" class="space-y-6">
        <label>
          <span class="field-label">Customer</span>
          <SearchableSelect
            :model-value="editForm.customerId"
            :options="customerOptions"
            placeholder="Search customers by name, email, phone, or address"
            empty-label="No matching customers."
            :disabled="!canUpdateOrders"
            @update:model-value="editForm.customerId = $event"
          />
        </label>

        <div class="rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-600">
          Snapshot on this order: <span class="font-semibold text-slate-900">{{ selectedOrder.customerName }}</span>
          <span class="mx-2 text-slate-300">·</span>
          <span>{{ selectedOrder.customerEmail }}</span>
        </div>

        <div class="grid gap-4 md:grid-cols-2">
          <label class="md:col-span-2"><span class="field-label">Shipping line 1</span><input v-model="editForm.shippingAddress.line1" class="text-input" /></label>
          <label><span class="field-label">Shipping line 2</span><input v-model="editForm.shippingAddress.line2" class="text-input" /></label>
          <label><span class="field-label">City</span><input v-model="editForm.shippingAddress.city" class="text-input" /></label>
          <label><span class="field-label">Province</span><input v-model="editForm.shippingAddress.province" class="text-input" /></label>
          <label><span class="field-label">District</span><input v-model="editForm.shippingAddress.district" class="text-input" /></label>
          <label><span class="field-label">Ward</span><input v-model="editForm.shippingAddress.ward" class="text-input" /></label>
        </div>

        <div class="space-y-3">
          <div class="flex items-center justify-between gap-3">
            <span class="field-label mb-0">Line items</span>
            <span class="text-sm text-slate-500">Read-only order snapshot</span>
          </div>

          <div v-if="editForm.lines.length" class="space-y-3">
            <article v-for="line in editForm.lines" :key="line.id" class="subtle-panel space-y-4">
              <div class="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h5 class="text-base font-semibold text-slate-900">{{ line.name || 'Unnamed line item' }}</h5>
                  <p class="mt-1 font-mono text-xs uppercase tracking-[0.16em] text-slate-500">{{ line.sku }}</p>
                </div>
                <StatusBadge label="Locked" tone="neutral" />
              </div>

              <div class="grid gap-4 md:grid-cols-4">
                <div>
                  <span class="field-label">Product</span>
                  <p class="mt-2 text-sm font-medium text-slate-900">{{ line.name }}</p>
                </div>
                <div>
                  <span class="field-label">Quantity</span>
                  <p class="mt-2 text-sm font-medium text-slate-900">{{ line.quantity }}</p>
                </div>
                <div>
                  <span class="field-label">Unit price</span>
                  <p class="mt-2 text-sm font-medium text-slate-900">{{ formatCurrency(line.unitPrice, line.currency) }}</p>
                </div>
                <div>
                  <span class="field-label">Line total</span>
                  <p class="mt-2 text-sm font-medium text-slate-900">{{ formatCurrency(lineTotal(line), line.currency) }}</p>
                </div>
              </div>
            </article>
          </div>

          <p v-else class="rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-500">
            No line items are stored on this order.
          </p>
        </div>

        <div class="flex justify-end">
          <button class="btn-primary" :disabled="saving || !canUpdateOrders || !hasOrderChanges" @click="saveOrder">
            <span v-if="saving" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">✓</span>
            <span>{{ saving ? 'Saving order...' : 'Save order changes' }}</span>
          </button>
        </div>
      </div>
    </EntityDialog>

    <EntityDialog :open="isCreateOrderDialogOpen" title="New order" description="Create a new order with one or more product variants." width-class="max-w-6xl" @close="isCreateOrderDialogOpen = false">
      <div class="grid gap-6 xl:grid-cols-[minmax(0,1.15fr)_360px]">
        <div class="space-y-4">
          <label>
            <span class="field-label">Customer</span>
            <SearchableSelect
              :model-value="createForm.customerId"
              :options="customerOptions"
              placeholder="Search customers by name, email, phone, or address"
              empty-label="No matching customers."
              :disabled="!canCreateOrders"
              @update:model-value="createForm.customerId = $event"
            />
          </label>

          <div class="rounded-[24px] bg-[var(--color-surface-low)] p-5">
            <div class="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p class="workspace-label">Order lines</p>
                <h4 class="mt-2 section-title">Variants in this order</h4>
              </div>
              <button class="btn-secondary" :disabled="!canCreateOrders" @click="addCreateLine">
                <span class="button-icon" aria-hidden="true">＋</span>
                <span>Add variant</span>
              </button>
            </div>

            <div class="mt-4 space-y-3">
              <article v-for="line in createForm.lines" :key="line.id" class="rounded-2xl bg-white p-4 shadow-sm ring-1 ring-slate-100">
                <div class="grid gap-4 md:grid-cols-[minmax(0,1fr)_120px_auto] md:items-end">
                  <label>
                    <span class="field-label">Variant</span>
                    <SearchableSelect
                      :model-value="line.variantId"
                      :options="variantOptions"
                      placeholder="Search variants by name or SKU"
                      empty-label="No matching variants."
                      :disabled="!canCreateOrders"
                      @update:model-value="selectCreateLineVariant(line.id, $event)"
                    />
                  </label>
                  <label>
                    <span class="field-label">Quantity</span>
                    <input v-model.number="line.quantity" class="text-input" min="1" type="number" />
                  </label>
                  <button class="btn-danger" :disabled="!canCreateOrders" @click="removeCreateLine(line.id)">
                    <span class="button-icon" aria-hidden="true">−</span>
                    <span>Remove</span>
                  </button>
                </div>

                <div v-if="variantForCreateLine(line.variantId)" class="mt-4 grid gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700 md:grid-cols-3">
                  <div>
                    <span class="field-label">Variant</span>
                    <p class="mt-2 font-medium text-slate-900">{{ variantForCreateLine(line.variantId)?.name }}</p>
                  </div>
                  <div>
                    <span class="field-label">SKU</span>
                    <p class="mt-2 font-medium text-slate-900">{{ variantForCreateLine(line.variantId)?.sku }}</p>
                  </div>
                  <div>
                    <span class="field-label">Unit price</span>
                    <p class="mt-2 font-medium text-brand-700">
                      {{ formatCurrency(variantUnitPrice(variantForCreateLine(line.variantId)), variantCurrency(variantForCreateLine(line.variantId))) }}
                    </p>
                  </div>
                </div>
              </article>
            </div>
          </div>
        </div>

        <div class="space-y-4">
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

          <div class="flex justify-end">
            <button class="btn-primary" :disabled="submitting || !canCreateOrders || !createOrderCanSubmit" @click="submitOrder">
              <span v-if="submitting" class="button-spinner" aria-hidden="true" />
              <span v-else class="button-icon" aria-hidden="true">✓</span>
              <span>{{ submitting ? 'Creating order...' : 'Create order' }}</span>
            </button>
          </div>
        </div>
      </div>
    </EntityDialog>
  </section>
</template>
