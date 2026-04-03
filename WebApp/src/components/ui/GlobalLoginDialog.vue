<script setup lang="ts">
import { reactive, ref } from 'vue';
import { ApiError } from '../../lib/http';
import { useAuthStore } from '../../stores/auth';
import { useUiStore } from '../../stores/ui';

const authStore = useAuthStore();
const uiStore = useUiStore();

const form = reactive({
  email: authStore.user?.email ?? '',
  password: '',
});

const errorMessage = ref<string | null>(null);

async function signIn() {
  errorMessage.value = null;

  try {
    await authStore.login({ ...form });
    form.password = '';
    uiStore.pushToast('Session restored successfully.', 'success');
  } catch (error) {
    errorMessage.value = error instanceof ApiError ? error.message : error instanceof Error ? error.message : 'Sign-in failed.';
  }
}

async function signOut() {
  await authStore.logout();
  uiStore.pushToast('Signed out.', 'info');
}
</script>

<template>
  <Teleport to="body">
    <div v-if="authStore.reauthRequired" class="fixed inset-0 z-[80] flex items-center justify-center bg-slate-950/55 px-4 py-8">
      <div class="max-h-[90vh] w-full max-w-2xl overflow-hidden rounded-[28px] bg-white shadow-[0_30px_80px_rgba(15,23,42,0.24)]">
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 px-6 py-5">
          <div>
            <h3 class="text-xl font-semibold text-slate-950">Session expired</h3>
            <p class="mt-2 text-sm text-slate-600">Sign in again to continue your work without leaving the current screen.</p>
          </div>
          <button class="dialog-close-button" type="button" title="Close dialog" aria-label="Close dialog" @click="signOut">
            <span class="icon-glyph">✕</span>
          </button>
        </div>

        <div class="grid gap-5 px-6 py-6">
          <div v-if="errorMessage" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {{ errorMessage }}
          </div>

          <label>
            <span class="field-label">Email</span>
            <input v-model="form.email" class="text-input" type="email" autocomplete="email" />
          </label>

          <label>
            <span class="field-label">Password</span>
            <input v-model="form.password" class="text-input" type="password" autocomplete="current-password" @keyup.enter="signIn" />
          </label>

          <div class="flex flex-wrap justify-end gap-3">
            <button class="btn-secondary" :disabled="authStore.loading" @click="signOut">
              <span class="button-icon" aria-hidden="true">⇠</span>
              <span>Sign out</span>
            </button>
            <button class="btn-primary" :disabled="authStore.loading || !form.email || !form.password" @click="signIn">
              <span v-if="authStore.loading" class="button-spinner" aria-hidden="true" />
              <span v-else class="button-icon" aria-hidden="true">↺</span>
              <span>{{ authStore.loading ? 'Signing in...' : 'Continue session' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>
