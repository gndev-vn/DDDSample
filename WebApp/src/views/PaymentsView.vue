<script setup lang="ts">
import EntityDialog from '../components/ui/EntityDialog.vue';
import StatusBadge from '../components/ui/StatusBadge.vue';
import { usePaymentsAdmin } from '../composables/usePaymentsAdmin';
import { formatCurrency, formatDate, paymentStatusLabel, toneForStatus } from '../lib/formatters';

const {
  loading,
  workingPaymentId,
  error,
  success,
  payments,
  isPaymentDialogOpen,
  paymentSearch,
  selectedPayment,
  canManagePayments,
  filteredPayments,
  stateFor,
  openPaymentDialog,
  refresh,
  completePayment,
  failPayment,
} = usePaymentsAdmin();
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Payment administration</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Production payment review with a single transaction table and a focused detail dialog.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="paymentSearch" class="toolbar-search" placeholder="Search transactions, orders, status..." />
        <button class="btn-secondary self-start" :disabled="loading" @click="refresh">
          <span v-if="loading" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ loading ? 'Refreshing...' : 'Reload payments' }}</span>
        </button>
      </div>
    </div>

    <div v-if="!canManagePayments" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account does not have permission to manage payments.
    </div>

    <div v-if="success" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ success }}
    </div>

    <div v-if="error" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <article class="section-panel">
      <div class="mb-4 text-sm text-slate-600">
        {{ filteredPayments.length }} of {{ payments.length }} payment record(s).
      </div>
      <div class="table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-5 py-4">Transaction ID</th>
              <th class="px-5 py-4">Order</th>
              <th class="px-5 py-4">Amount</th>
              <th class="px-5 py-4">Method</th>
              <th class="px-5 py-4">Status</th>
              <th class="px-5 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="payment in filteredPayments" :key="payment.id">
              <td class="px-5 py-5">
                <button class="text-left font-semibold text-[var(--color-ink)]" @click="openPaymentDialog(payment)">{{ payment.id }}</button>
                <p class="mt-1 text-xs text-[var(--color-ink-muted)]">{{ formatDate(payment.createdAtUtc) }}</p>
              </td>
              <td class="px-5 py-5">
                <p class="text-sm font-medium text-[var(--color-ink)]">{{ payment.orderId }}</p>
                <p class="mt-1 text-xs text-[var(--color-ink-muted)]">Updated {{ formatDate(payment.updatedAtUtc) }}</p>
              </td>
              <td class="px-5 py-5 font-medium text-[var(--color-ink)]">{{ formatCurrency(payment.amount, payment.currency) }}</td>
              <td class="px-5 py-5 text-[var(--color-ink-muted)]">{{ payment.transactionReference ? 'Card / bank' : 'Awaiting confirmation' }}</td>
              <td class="px-5 py-5">
                <StatusBadge :label="paymentStatusLabel(payment.status)" :tone="toneForStatus(payment.status, 'payment')" />
              </td>
              <td class="px-5 py-5">
                <button class="icon-button" title="Details" aria-label="View payment details" @click="openPaymentDialog(payment)"><span class="icon-glyph">👁</span></button>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!filteredPayments.length && !loading" class="px-5 py-4 text-sm text-slate-500">
          {{ payments.length ? 'No payments match the current search.' : 'No payments returned by the API yet.' }}
        </p>
      </div>
    </article>

    <EntityDialog
      :open="isPaymentDialogOpen"
      title="Payment details"
      description="Review payment state and complete operator actions from one focused dialog."
      width-class="max-w-4xl"
      @close="isPaymentDialogOpen = false"
    >
      <div v-if="selectedPayment" class="grid gap-6">
        <div class="grid gap-4 md:grid-cols-2">
          <div class="subtle-panel">
            <p class="workspace-label">Transaction</p>
            <p class="mt-2 text-sm font-medium text-slate-900">{{ selectedPayment.id }}</p>
            <p class="mt-2 text-xs text-slate-500">{{ formatDate(selectedPayment.createdAtUtc) }}</p>
          </div>
          <div class="subtle-panel">
            <p class="workspace-label">Order</p>
            <p class="mt-2 text-sm font-medium text-slate-900">{{ selectedPayment.orderId }}</p>
            <p class="mt-2 text-sm text-brand-700">{{ formatCurrency(selectedPayment.amount, selectedPayment.currency) }}</p>
          </div>
        </div>

        <div class="subtle-panel">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <p class="font-semibold text-slate-900">Payment status</p>
            <StatusBadge :label="paymentStatusLabel(selectedPayment.status)" :tone="toneForStatus(selectedPayment.status, 'payment')" />
          </div>

          <div v-if="selectedPayment.status === 1" class="mt-4 grid gap-4 md:grid-cols-2">
            <label>
              <span class="field-label">Transaction reference</span>
              <input v-model="stateFor(selectedPayment.id).transactionReference" class="text-input" />
            </label>
            <label>
              <span class="field-label">Failure reason</span>
              <input v-model="stateFor(selectedPayment.id).failureReason" class="text-input" placeholder="Insufficient funds" />
            </label>
            <div class="md:col-span-2 flex flex-wrap gap-2">
              <button class="btn-primary" :disabled="workingPaymentId === selectedPayment.id || !canManagePayments" @click="completePayment(selectedPayment)">
                <span v-if="workingPaymentId === selectedPayment.id" class="button-spinner" aria-hidden="true" />
                <span v-else class="button-icon" aria-hidden="true">✓</span>
                <span>{{ workingPaymentId === selectedPayment.id ? 'Working...' : 'Approve payment' }}</span>
              </button>
              <button class="btn-danger" :disabled="workingPaymentId === selectedPayment.id || !stateFor(selectedPayment.id).failureReason.trim() || !canManagePayments" @click="failPayment(selectedPayment)">
                <span v-if="workingPaymentId === selectedPayment.id" class="button-spinner" aria-hidden="true" />
                <span v-else class="button-icon" aria-hidden="true">✕</span>
                <span>{{ workingPaymentId === selectedPayment.id ? 'Working...' : 'Mark failed' }}</span>
              </button>
            </div>
          </div>

          <div v-else class="mt-4 text-sm text-slate-600">
            <p>Transaction reference: {{ selectedPayment.transactionReference || '—' }}</p>
            <p class="mt-2">Failure reason: {{ selectedPayment.failureReason || '—' }}</p>
          </div>
        </div>
      </div>
    </EntityDialog>
  </section>
</template>


