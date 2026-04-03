<script setup lang="ts">
defineProps<{
  open: boolean;
  title: string;
  description?: string;
  widthClass?: string;
  closable?: boolean;
}>();

const emit = defineEmits<{
  close: [];
}>();
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/40 px-4 py-8">
      <div
        class="max-h-[90vh] w-full overflow-hidden rounded-[28px] bg-white shadow-[0_30px_80px_rgba(15,23,42,0.24)]"
        :class="widthClass ?? 'max-w-5xl'"
      >
        <div class="flex items-start justify-between gap-4 border-b border-slate-200 px-6 py-5">
          <div>
            <h3 class="text-xl font-semibold text-slate-950">{{ title }}</h3>
            <p v-if="description" class="mt-2 text-sm text-slate-600">{{ description }}</p>
          </div>
          <button
            v-if="true"
            class="dialog-close-button"
            type="button"
            title="Close dialog"
            aria-label="Close dialog"
            @click="emit('close')"
          >
            <span class="icon-glyph">✕</span>
          </button>
        </div>
        <div class="max-h-[calc(90vh-96px)] overflow-y-auto px-6 py-6">
          <slot />
        </div>
      </div>
    </div>
  </Teleport>
</template>
