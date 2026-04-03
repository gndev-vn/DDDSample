<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink, useRoute } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';

const authStore = useAuthStore();
const uiStore = useUiStore();
const route = useRoute();

const navigationGroups = [
  {
    label: 'Overview',
    items: [{ name: 'overview', label: 'Overview', description: 'Alerts, activity, and business summary' }],
  },
  {
    label: 'Administration',
    items: [
      { name: 'users', label: 'Users', description: 'Operator accounts and access assignments' },
      { name: 'roles', label: 'Roles', description: 'Role definitions for access control' },
    ],
  },
  {
    label: 'Catalog',
    items: [
      { name: 'categories', label: 'Categories', description: 'Taxonomy and product grouping' },
      { name: 'products', label: 'Products', description: 'Core product records' },
      { name: 'variants', label: 'Variants', description: 'Sellable SKU-level variants' },
    ],
  },
  {
    label: 'Operations',
    items: [
      { name: 'orders', label: 'Orders', description: 'Order queue and lifecycle actions' },
      { name: 'payments', label: 'Payments', description: 'Transaction review and resolution' },
    ],
  },
];

const mobileNavigation = navigationGroups.flatMap((group) => group.items);

const authCaption = computed(() => {
  if (!authStore.user) {
    return 'No active session';
  }

  return `${authStore.user.email} · ${authStore.user.roles.join(', ')}`;
});

async function refreshSession() {
  try {
    await authStore.refreshProfile();
    uiStore.pushToast('Session verified.', 'success');
  } catch (error) {
    uiStore.pushToast(error instanceof Error ? error.message : 'Unable to verify the session.', 'error');
  }
}

async function signOut() {
  await authStore.logout();
  uiStore.pushToast('Signed out.', 'info');
}
</script>

<template>
  <div class="min-h-screen bg-[var(--color-page)]">
    <div class="mx-auto flex min-h-screen max-w-[1680px] gap-6 px-4 py-6">
      <aside class="hidden w-[320px] shrink-0 rounded-[32px] bg-[var(--color-surface)] p-6 shadow-[0_18px_50px_rgba(44,52,55,0.06)] xl:flex xl:flex-col">
        <div>
          <p class="workspace-label text-brand-700">DDDSample</p>
          <h1 class="mt-3 text-3xl font-semibold text-[var(--color-ink)]" style="font-family: Manrope, Inter, ui-sans-serif, system-ui, sans-serif;">
            Business Admin
          </h1>
          <p class="mt-3 text-sm leading-6 text-[var(--color-ink-muted)]">
            Domain-oriented workspace for identity, catalog entities, orders, and payments.
          </p>
        </div>

        <div class="mt-8 space-y-6">
          <section v-for="group in navigationGroups" :key="group.label">
            <p class="workspace-label">{{ group.label }}</p>
            <nav class="mt-3 flex flex-col gap-2">
              <RouterLink
                v-for="item in group.items"
                :key="item.name"
                :to="{ name: item.name }"
                class="sidebar-link"
                :class="route.name === item.name ? 'sidebar-link-active' : ''"
              >
                <span v-if="route.name === item.name" class="absolute left-0 top-1/2 h-10 w-1 -translate-y-1/2 rounded-full bg-brand-500" />
                <div class="pl-2">
                  <div class="font-semibold">{{ item.label }}</div>
                  <div class="mt-1 text-xs text-[var(--color-ink-muted)]">{{ item.description }}</div>
                </div>
              </RouterLink>
            </nav>
          </section>
        </div>

        <div class="mt-8 subtle-panel">
          <p class="workspace-label">Session</p>
          <p class="mt-3 text-sm font-medium text-[var(--color-ink)]">{{ authCaption }}</p>
          <div class="mt-4 flex flex-wrap gap-2">
            <button
              class="btn-secondary"
              :disabled="authStore.loading"
              title="Re-check the current token and operator profile without leaving the page."
              @click="refreshSession"
            >
              <span v-if="authStore.loading" class="button-spinner" aria-hidden="true" />
              <span v-else class="button-icon" aria-hidden="true">↻</span>
              <span>{{ authStore.loading ? 'Verifying...' : 'Verify session' }}</span>
            </button>
            <button class="btn-danger" :disabled="authStore.loading" @click="signOut">
              <span class="button-icon" aria-hidden="true">⇠</span>
              <span>Sign out</span>
            </button>
          </div>
        </div>
      </aside>

      <div class="min-w-0 flex-1">
        <header class="space-y-4 xl:hidden">
          <div class="card p-5">
            <p class="workspace-label text-brand-700">DDDSample</p>
            <h1 class="mt-2 text-2xl font-semibold text-[var(--color-ink)]" style="font-family: Manrope, Inter, ui-sans-serif, system-ui, sans-serif;">
              Business Admin
            </h1>
            <p class="mt-2 text-sm text-[var(--color-ink-muted)]">{{ authCaption }}</p>
          </div>

          <div class="card p-4">
            <div class="flex flex-wrap gap-2">
              <RouterLink
                v-for="item in mobileNavigation"
                :key="item.name"
                :to="{ name: item.name }"
                class="rounded-2xl px-4 py-2 text-sm font-semibold transition"
                :class="route.name === item.name ? 'bg-brand-50 text-brand-700' : 'bg-[var(--color-surface-low)] text-[var(--color-ink-muted)]'"
              >
                {{ item.label }}
              </RouterLink>
            </div>
          </div>
        </header>

        <main>
          <section class="card px-6 py-6 lg:px-8 lg:py-8">
            <div class="mb-6 flex flex-wrap items-center justify-between gap-3 border-b border-slate-200/70 pb-5">
              <div class="rounded-[20px] bg-[var(--color-surface-low)] px-4 py-3 text-sm text-[var(--color-ink-muted)]">
                {{ authCaption }}
              </div>
              <div class="flex flex-wrap gap-2">
                <button
                  class="btn-secondary"
                  :disabled="authStore.loading"
                  title="Re-check the current token and operator profile without leaving the page."
                  @click="refreshSession"
                >
                  <span v-if="authStore.loading" class="button-spinner" aria-hidden="true" />
                  <span v-else class="button-icon" aria-hidden="true">↻</span>
                  <span>{{ authStore.loading ? 'Verifying...' : 'Verify session' }}</span>
                </button>
                <button class="btn-danger" :disabled="authStore.loading" @click="signOut">
                  <span class="button-icon" aria-hidden="true">⇠</span>
                  <span>Sign out</span>
                </button>
              </div>
            </div>

            <slot />
          </section>
        </main>
      </div>
    </div>
  </div>
</template>
