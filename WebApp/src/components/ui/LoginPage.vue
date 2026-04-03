<script setup lang="ts">
import { reactive, ref } from 'vue';
import { ApiError } from '../../lib/http';
import { useAuthStore } from '../../stores/auth';
import { useUiStore } from '../../stores/ui';

const authStore = useAuthStore();
const uiStore = useUiStore();

const form = reactive({
  email: 'admin@example.com',
  password: 'admin123',
});

const errorMessage = ref<string | null>(null);

async function signIn() {
  errorMessage.value = null;

  try {
    await authStore.login({ ...form });
    form.password = '';
    uiStore.pushToast('Signed in successfully.', 'success');
  } catch (error) {
    errorMessage.value = error instanceof ApiError ? error.message : error instanceof Error ? error.message : 'Sign-in failed.';
  }
}
</script>

<template>
  <div class="min-h-screen bg-[var(--color-page)] px-4 py-8 lg:px-8">
    <div class="mx-auto grid min-h-[calc(100vh-4rem)] max-w-6xl items-center gap-8 lg:grid-cols-[minmax(0,1.1fr)_460px]">
      <section class="hidden rounded-[32px] bg-gradient-to-br from-brand-700 via-brand-600 to-[#221c6b] p-10 text-white shadow-[0_30px_90px_rgba(51,38,188,0.34)] lg:block">
        <p class="workspace-label text-white/70">DDDSample</p>
        <h1 class="mt-4 text-4xl font-semibold" style="font-family: Manrope, Inter, ui-sans-serif, system-ui, sans-serif;">
          Business admin workspace
        </h1>
        <p class="mt-5 max-w-xl text-sm leading-7 text-white/80">
          Secure back-office access for user administration, catalog maintenance, order operations, and payment review.
        </p>

        <div class="mt-10 grid gap-4">
          <div class="rounded-[24px] bg-white/10 p-5 backdrop-blur-sm">
            <p class="text-sm font-semibold">Users and roles</p>
            <p class="mt-2 text-sm text-white/75">Manage operator accounts, roles, and access without mixing authentication into the workspace.</p>
          </div>
          <div class="rounded-[24px] bg-white/10 p-5 backdrop-blur-sm">
            <p class="text-sm font-semibold">Product operations</p>
            <p class="mt-2 text-sm text-white/75">Navigate directly to categories, products, and variants from the entity sidebar.</p>
          </div>
          <div class="rounded-[24px] bg-white/10 p-5 backdrop-blur-sm">
            <p class="text-sm font-semibold">Operational queues</p>
            <p class="mt-2 text-sm text-white/75">Review orders and payments in focused tables designed for real daily usage.</p>
          </div>
        </div>
      </section>

      <section class="card p-8 lg:p-10">
        <p class="workspace-label text-brand-700">Operator sign-in</p>
        <h2 class="mt-3 text-3xl font-semibold text-[var(--color-ink)]" style="font-family: Manrope, Inter, ui-sans-serif, system-ui, sans-serif;">
          Welcome back
        </h2>
        <p class="mt-3 text-sm leading-6 text-[var(--color-ink-muted)]">
          Sign in to access the internal DDDSample admin workspace.
        </p>

        <div v-if="errorMessage" class="mt-6 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {{ errorMessage }}
        </div>

        <div class="mt-8 grid gap-5">
          <label>
            <span class="field-label">Email</span>
            <input v-model="form.email" class="text-input" type="email" autocomplete="email" />
          </label>
          <label>
            <span class="field-label">Password</span>
            <input v-model="form.password" class="text-input" type="password" autocomplete="current-password" @keyup.enter="signIn" />
          </label>
        </div>

        <div class="mt-8 flex flex-wrap justify-end gap-3">
          <button class="btn-primary" :disabled="authStore.loading || !form.email || !form.password" @click="signIn">
            <span v-if="authStore.loading" class="button-spinner" aria-hidden="true" />
            <span v-else class="button-icon" aria-hidden="true">→</span>
            <span>{{ authStore.loading ? 'Signing in...' : 'Sign in' }}</span>
          </button>
        </div>
      </section>
    </div>
  </div>
</template>
