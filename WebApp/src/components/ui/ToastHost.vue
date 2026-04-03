<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { useUiStore } from '../../stores/ui';

const uiStore = useUiStore();
const { toasts } = storeToRefs(uiStore);
</script>

<template>
  <Teleport to="body">
    <div class="pointer-events-none fixed inset-x-0 top-4 z-[70] flex justify-center px-4">
      <div class="flex w-full max-w-xl flex-col gap-3">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          class="pointer-events-auto flex items-start justify-between gap-4 rounded-[22px] border px-4 py-4 shadow-[0_20px_60px_rgba(15,23,42,0.16)]"
          :class="{
            'border-emerald-200 bg-emerald-50 text-emerald-800': toast.tone === 'success',
            'border-rose-200 bg-rose-50 text-rose-800': toast.tone === 'error',
            'border-slate-200 bg-white text-slate-800': toast.tone === 'info',
          }"
        >
          <p class="text-sm font-medium leading-6">{{ toast.message }}</p>
          <button class="text-xs font-semibold uppercase tracking-[0.16em] text-current opacity-70 transition hover:opacity-100" @click="uiStore.dismissToast(toast.id)">
            Close
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
