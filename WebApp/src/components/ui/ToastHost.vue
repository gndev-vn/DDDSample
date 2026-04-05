<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { useUiStore } from '../../stores/ui';

const uiStore = useUiStore();
const { toasts } = storeToRefs(uiStore);
</script>

<template>
  <Teleport to="body">
    <div class="pointer-events-none fixed bottom-4 right-4 z-[70] flex w-full justify-end px-4 sm:bottom-6 sm:right-6">
      <TransitionGroup name="toast-list" tag="div" class="flex w-full max-w-md flex-col gap-3">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          class="pointer-events-auto flex items-start justify-between gap-4 rounded-[22px] border px-4 py-4 shadow-[0_18px_40px_rgba(15,23,42,0.18)] backdrop-blur-sm"
          :class="{
            'border-emerald-300 bg-emerald-50/95 text-emerald-950': toast.tone === 'success',
            'border-rose-300 bg-rose-50/95 text-rose-950': toast.tone === 'error',
            'border-[var(--color-border-strong)] bg-white/96 text-[var(--color-ink)]': toast.tone === 'info',
          }"
        >
          <p class="text-sm font-medium leading-6">{{ toast.message }}</p>
          <button class="text-xs font-semibold uppercase tracking-[0.16em] text-current opacity-70 transition hover:opacity-100" @click="uiStore.dismissToast(toast.id)">
            Close
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-list-enter-active,
.toast-list-leave-active {
  transition: all 0.24s ease;
}

.toast-list-enter-from,
.toast-list-leave-to {
  opacity: 0;
  transform: translateY(10px) scale(0.98);
}

.toast-list-move {
  transition: transform 0.24s ease;
}
</style>
