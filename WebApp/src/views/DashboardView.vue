<script setup lang="ts">
import { computed, onMounted, reactive } from 'vue';
import { RouterLink } from 'vue-router';
import { catalogApi } from '../api/catalog';
import { orderingApi } from '../api/ordering';
import { paymentApi } from '../api/payment';
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();

const state = reactive({
  loading: true,
  error: null as string | null,
  metrics: { categories: 0, products: 0, variants: 0, orders: 0, payments: 0 },
  orderStatuses: new Map<number, number>(),
  paymentStatuses: new Map<number, number>(),
});

const summaryCards = computed(() => [
  { title: 'Categories', value: state.metrics.categories, subtitle: 'Taxonomy nodes in the catalog' },
  { title: 'Products', value: state.metrics.products, subtitle: 'Managed product records' },
  { title: 'Variants', value: state.metrics.variants, subtitle: 'Orderable SKU records' },
  { title: 'Orders', value: state.metrics.orders, subtitle: 'Orders in the operational queue' },
  { title: 'Payments', value: state.metrics.payments, subtitle: 'Tracked payment transactions' },
]);

const alertCards = computed(() => {
  const pendingPayments = state.paymentStatuses.get(1) ?? 0;
  const submittedOrders = state.orderStatuses.get(0) ?? 0;
  const cancelledOrders = state.orderStatuses.get(5) ?? 0;

  return [
    { title: 'Orders awaiting action', value: submittedOrders, subtitle: 'Submitted orders that may need fulfillment or payment updates', to: { name: 'orders' } },
    { title: 'Payments pending review', value: pendingPayments, subtitle: 'Transactions waiting for approval or failure handling', to: { name: 'payments' } },
    { title: 'Cancelled orders', value: cancelledOrders, subtitle: 'Orders cancelled and ready for operational follow-up', to: { name: 'orders' } },
  ].filter((item) => item.value > 0);
});

async function refresh() {
  state.loading = true;
  state.error = null;

  try {
    const [categories, products, variants, orders, payments] = await Promise.all([
      catalogApi.getCategories(authStore.token),
      catalogApi.getProducts(authStore.token),
      catalogApi.getProductVariants(authStore.token),
      orderingApi.getOrders(authStore.token),
      paymentApi.getPayments(authStore.token),
    ]);

    state.metrics.categories = categories.length;
    state.metrics.products = products.length;
    state.metrics.variants = variants.length;
    state.metrics.orders = orders.length;
    state.metrics.payments = payments.length;
    state.orderStatuses = orders.reduce((counts, order) => counts.set(order.status, (counts.get(order.status) ?? 0) + 1), new Map<number, number>());
    state.paymentStatuses = payments.reduce((counts, payment) => counts.set(payment.status, (counts.get(payment.status) ?? 0) + 1), new Map<number, number>());
  } catch (error) {
    state.error = error instanceof Error ? error.message : 'Unable to load the overview workspace.';
  } finally {
    state.loading = false;
  }
}

onMounted(() => {
  void refresh();
});
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Overview</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">Daily operating summary for the entities and operational queues that matter most.</p>
      </div>
      <button class="btn-secondary self-start" :disabled="state.loading" @click="refresh">
        <span v-if="state.loading" class="button-spinner" aria-hidden="true" />
        <span v-else class="button-icon" aria-hidden="true">↻</span>
        <span>{{ state.loading ? 'Refreshing...' : 'Refresh overview' }}</span>
      </button>
    </div>

    <div v-if="state.error" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ state.error }}</div>

    <div class="flex flex-wrap gap-3">
      <article v-for="item in summaryCards" :key="item.title" class="summary-tile min-w-[220px] flex-1">
        <p class="workspace-label">{{ item.title }}</p>
        <p class="mt-3 display-value">{{ item.value }}</p>
        <p class="mt-2 text-sm text-slate-600">{{ item.subtitle }}</p>
      </article>
    </div>

    <article class="section-panel">
      <div class="flex flex-col gap-2 md:flex-row md:items-end md:justify-between">
        <div>
          <h4 class="section-title">Needs attention</h4>
          <p class="mt-2 text-sm text-slate-600">Only urgent operational queues stay here. Navigation belongs in the sidebar.</p>
        </div>
        <p v-if="!alertCards.length && !state.loading" class="text-sm text-slate-500">No urgent queues are above zero right now.</p>
      </div>
      <div v-if="alertCards.length" class="mt-5 grid gap-3 md:grid-cols-3">
        <RouterLink v-for="item in alertCards" :key="item.title" :to="item.to" class="subtle-panel transition hover:bg-white">
          <p class="workspace-label">{{ item.title }}</p>
          <p class="mt-3 text-3xl font-semibold text-slate-900">{{ item.value }}</p>
          <p class="mt-3 text-sm text-slate-600">{{ item.subtitle }}</p>
        </RouterLink>
      </div>
    </article>
  </section>
</template>
