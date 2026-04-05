<script setup lang="ts">
import EntityDialog from '../components/ui/EntityDialog.vue';
import StatusBadge from '../components/ui/StatusBadge.vue';
import { useCustomersAdmin } from '../composables/useCustomersAdmin';

const {
  loading,
  creatingCustomer,
  savingCustomer,
  deletingCustomerId,
  success,
  error,
  customers,
  selectedCustomer,
  customerSearch,
  createCustomerForm,
  editCustomerForm,
  isCustomerDialogOpen,
  isCreateCustomerDialogOpen,
  canCreateCustomers,
  canUpdateCustomers,
  canDeleteCustomers,
  canManageCustomers,
  filteredCustomers,
  activeCustomerCount,
  inactiveCustomerCount,
  hasCustomerChanges,
  openCustomerDialog,
  openCreateCustomerDialog,
  viewCustomerOrders,
  refreshCustomers,
  handleCreateCustomer,
  handleSaveCustomer,
  handleDeleteCustomer,
} = useCustomersAdmin();
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Customer administration</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Manage customer records used by order creation and order ownership.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="customerSearch" class="toolbar-search" placeholder="Search customers by name, email, phone, or address" />
        <button class="btn-primary" :disabled="!canCreateCustomers" @click="openCreateCustomerDialog">
          <span class="button-icon" aria-hidden="true">＋</span>
          <span>New customer</span>
        </button>
        <button class="btn-secondary" :disabled="loading" @click="refreshCustomers">
          <span v-if="loading" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ loading ? 'Refreshing...' : 'Reload customers' }}</span>
        </button>
      </div>
    </div>

    <div v-if="!canManageCustomers" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account can review customers, but create, update, and delete actions depend on separate permissions.
    </div>
    <div v-if="success" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ success }}
    </div>
    <div v-if="error" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <div class="flex flex-wrap gap-4">
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">Customers</p>
        <p class="mt-2 text-3xl font-semibold text-slate-950">{{ customers.length }}</p>
      </article>
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">Active</p>
        <p class="mt-2 text-3xl font-semibold text-slate-950">{{ activeCustomerCount }}</p>
      </article>
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">Inactive</p>
        <p class="mt-2 text-3xl font-semibold text-slate-950">{{ inactiveCustomerCount }}</p>
      </article>
    </div>

    <article class="section-panel">
      <div class="mb-4 text-sm text-slate-600">
        {{ filteredCustomers.length }} of {{ customers.length }} customer record(s).
      </div>

      <div class="table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-5 py-4">Customer</th>
              <th class="px-5 py-4">Contact</th>
              <th class="px-5 py-4">Address</th>
              <th class="px-5 py-4">Status</th>
              <th class="px-5 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="customer in filteredCustomers" :key="customer.id">
              <td class="px-5 py-4">
                <button class="text-left font-semibold text-slate-900" @click="openCustomerDialog(customer)">{{ customer.displayName }}</button>
                <p class="mt-1 text-xs text-slate-500">{{ customer.id }}</p>
              </td>
              <td class="px-5 py-4 text-slate-600">
                <div>{{ customer.email }}</div>
                <div v-if="customer.phoneNumber" class="mt-1 text-xs text-slate-500">{{ customer.phoneNumber }}</div>
              </td>
              <td class="px-5 py-4 text-slate-600">{{ customer.address || 'No address saved' }}</td>
              <td class="px-5 py-4">
                <StatusBadge :label="customer.isActive ? 'Active' : 'Inactive'" :tone="customer.isActive ? 'success' : 'warning'" />
              </td>
              <td class="px-5 py-4">
                <div class="flex flex-wrap gap-2">
                  <button class="icon-button" aria-label="Edit customer" title="Edit customer" @click="openCustomerDialog(customer)">
                    <span class="icon-glyph">✎</span>
                  </button>
                  <button class="icon-button icon-button-danger" :disabled="deletingCustomerId === customer.id || !canDeleteCustomers" aria-label="Delete customer" title="Delete customer" @click="handleDeleteCustomer(customer)">
                    <span v-if="deletingCustomerId === customer.id" class="button-spinner" aria-hidden="true" />
                    <span v-else class="icon-glyph">✕</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!filteredCustomers.length && !loading" class="px-4 py-4 text-sm text-slate-500">
          {{ customers.length ? 'No customers match the current search.' : 'No customers are available yet.' }}
        </p>
      </div>
    </article>

    <EntityDialog :open="isCustomerDialogOpen" title="Customer details" description="Review and update the selected customer." @close="isCustomerDialogOpen = false">
      <div class="grid gap-4 md:grid-cols-2">
        <label class="md:col-span-2"><span class="field-label">Display name</span><input v-model="editCustomerForm.displayName" class="text-input" /></label>
        <label><span class="field-label">Email</span><input v-model="editCustomerForm.email" class="text-input" type="email" /></label>
        <label><span class="field-label">Phone number</span><input v-model="editCustomerForm.phoneNumber" class="text-input" /></label>
        <label class="md:col-span-2"><span class="field-label">Address</span><input v-model="editCustomerForm.address" class="text-input" /></label>
        <label class="md:col-span-2 flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
          <input v-model="editCustomerForm.isActive" type="checkbox" />
          Active?
        </label>
        <div class="md:col-span-2 flex flex-wrap justify-end gap-3">
          <button v-if="selectedCustomer" class="btn-secondary" @click="viewCustomerOrders(selectedCustomer.id)">
            <span class="button-icon" aria-hidden="true">↗</span>
            <span>View orders</span>
          </button>
          <button class="btn-primary" :disabled="savingCustomer || !canUpdateCustomers || !hasCustomerChanges" @click="handleSaveCustomer">
            <span v-if="savingCustomer" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">✓</span>
            <span>{{ savingCustomer ? 'Saving customer...' : 'Save customer changes' }}</span>
          </button>
          <button v-if="selectedCustomer" class="btn-danger" :disabled="deletingCustomerId === selectedCustomer.id || !canDeleteCustomers" @click="handleDeleteCustomer(selectedCustomer)">
            <span v-if="deletingCustomerId === selectedCustomer.id" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">✕</span>
            <span>Delete customer</span>
          </button>
        </div>
      </div>
    </EntityDialog>

    <EntityDialog :open="isCreateCustomerDialogOpen" title="New customer" description="Create a customer record for ordering flows." @close="isCreateCustomerDialogOpen = false">
      <div class="grid gap-4 md:grid-cols-2">
        <label class="md:col-span-2"><span class="field-label">Display name</span><input v-model="createCustomerForm.displayName" class="text-input" /></label>
        <label><span class="field-label">Email</span><input v-model="createCustomerForm.email" class="text-input" type="email" /></label>
        <label><span class="field-label">Phone number</span><input v-model="createCustomerForm.phoneNumber" class="text-input" /></label>
        <label class="md:col-span-2"><span class="field-label">Address</span><input v-model="createCustomerForm.address" class="text-input" /></label>
        <label class="md:col-span-2 flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
          <input v-model="createCustomerForm.isActive" type="checkbox" />
          Active?
        </label>
        <div class="md:col-span-2 flex justify-end">
          <button class="btn-primary" :disabled="creatingCustomer || !canCreateCustomers" @click="handleCreateCustomer">
            <span v-if="creatingCustomer" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">✓</span>
            <span>{{ creatingCustomer ? 'Creating customer...' : 'Create customer' }}</span>
          </button>
        </div>
      </div>
    </EntityDialog>
  </section>
</template>

