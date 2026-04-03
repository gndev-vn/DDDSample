<script setup lang="ts">
import { RouterView } from 'vue-router';
import AppShell from './components/AppShell.vue';
import ConfirmDialog from './components/ui/ConfirmDialog.vue';
import GlobalLoginDialog from './components/ui/GlobalLoginDialog.vue';
import LoginPage from './components/ui/LoginPage.vue';
import ToastHost from './components/ui/ToastHost.vue';
import { useAuthStore } from './stores/auth';
import { useUiStore } from './stores/ui';

const authStore = useAuthStore();
const uiStore = useUiStore();
</script>

<template>
  <div v-if="!authStore.initialized" class="flex min-h-screen items-center justify-center bg-[var(--color-page)] px-4">
    <div class="rounded-[28px] bg-white px-8 py-8 text-center shadow-[0_30px_80px_rgba(15,23,42,0.24)]">
      <p class="workspace-label text-brand-700">DDDSample</p>
      <h2 class="mt-3 text-2xl font-semibold text-slate-950">Restoring session</h2>
      <p class="mt-3 text-sm text-slate-600">Loading your operator workspace...</p>
      <div class="mt-5 flex justify-center">
        <span class="button-spinner" aria-hidden="true" />
      </div>
    </div>
  </div>

  <template v-else-if="authStore.isAuthenticated">
    <AppShell>
      <RouterView />
    </AppShell>
    <div v-if="uiStore.pageLoading" class="page-loading-overlay" aria-hidden="true">
      <div class="page-loading-bar" />
    </div>
    <GlobalLoginDialog />
    <ConfirmDialog />
    <ToastHost />
  </template>

  <template v-else>
    <LoginPage />
    <ToastHost />
  </template>
</template>
