<script setup lang="ts">
import { computed, onBeforeUnmount, watch } from 'vue';

const props = withDefaults(defineProps<{
  open: boolean;
  title: string;
  description?: string;
  widthClass?: string;
  closable?: boolean;
}>(), {
  closable: true,
});

const emit = defineEmits<{
  close: [];
}>();

const isClosable = computed(() => props.closable !== false);

function closeDialog() {
  if (isClosable.value) {
    emit('close');
  }
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.open && isClosable.value) {
    emit('close');
  }
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      window.addEventListener('keydown', handleKeydown);
      return;
    }

    window.removeEventListener('keydown', handleKeydown);
  },
  { immediate: true },
);

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeydown);
});
</script>

<template>
  <Teleport to="body">
    <Transition name="dialog-fade" appear>
      <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/48 px-4 py-8 backdrop-blur-[2px]" @click.self="closeDialog">
        <div
          class="dialog-panel max-h-[90vh] w-full overflow-hidden rounded-[28px] border border-[var(--color-border)] bg-white shadow-[0_30px_80px_rgba(15,23,42,0.24)]"
          :class="widthClass ?? 'max-w-5xl'"
        >
          <div class="flex items-start justify-between gap-4 border-b border-[var(--color-border)] px-6 py-5">
            <div>
              <h3 class="text-xl font-semibold text-slate-950">{{ title }}</h3>
              <p v-if="description" class="mt-2 text-sm text-slate-600">{{ description }}</p>
            </div>
            <button
              v-if="isClosable"
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
    </Transition>
  </Teleport>
</template>

<style scoped>
.dialog-fade-enter-active,
.dialog-fade-leave-active {
  transition: opacity 0.22s ease;
}

.dialog-fade-enter-from,
.dialog-fade-leave-to {
  opacity: 0;
}

.dialog-fade-enter-active .dialog-panel,
.dialog-fade-leave-active .dialog-panel {
  transition: transform 0.22s ease, opacity 0.22s ease;
}

.dialog-fade-enter-from .dialog-panel,
.dialog-fade-leave-to .dialog-panel {
  opacity: 0;
  transform: translateY(8px) scale(0.98);
}
</style>
