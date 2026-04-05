<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';

type SearchableSelectOption = {
  value: string;
  label: string;
  description?: string;
  keywords?: string[];
};

const props = withDefaults(defineProps<{
  modelValue: string;
  options: SearchableSelectOption[];
  placeholder?: string;
  emptyLabel?: string;
  disabled?: boolean;
  inputClass?: string;
}>(), {
  placeholder: 'Search...',
  emptyLabel: 'No results found.',
  disabled: false,
  inputClass: 'text-input',
});

const emit = defineEmits<{
  'update:modelValue': [string];
  select: [string];
}>();

const root = ref<HTMLElement | null>(null);
const isOpen = ref(false);
const query = ref('');

const selectedOption = computed(() => props.options.find((option) => option.value === props.modelValue) ?? null);
const filteredOptions = computed(() => {
  const search = query.value.trim().toLowerCase();
  if (!search) {
    return props.options;
  }

  return props.options.filter((option) =>
    [option.label, option.description ?? '', ...(option.keywords ?? [])].join(' ').toLowerCase().includes(search),
  );
});

function syncQueryToSelection() {
  query.value = selectedOption.value?.label ?? '';
}

function handleDocumentPointer(event: MouseEvent) {
  if (!root.value?.contains(event.target as Node)) {
    isOpen.value = false;
    syncQueryToSelection();
  }
}

function handleFocus() {
  if (!props.disabled) {
    isOpen.value = true;
  }
}

function handleInput(event: Event) {
  query.value = (event.target as HTMLInputElement).value;
  isOpen.value = true;
}

function chooseOption(option: SearchableSelectOption) {
  emit('update:modelValue', option.value);
  emit('select', option.value);
  query.value = option.label;
  isOpen.value = false;
}

watch(() => props.modelValue, () => {
  if (!isOpen.value) {
    syncQueryToSelection();
  }
}, { immediate: true });

watch(() => props.options, () => {
  if (!isOpen.value) {
    syncQueryToSelection();
  }
}, { deep: true });

onMounted(() => {
  document.addEventListener('mousedown', handleDocumentPointer);
});

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleDocumentPointer);
});
</script>

<template>
  <div ref="root" class="searchable-select">
    <input
      :class="['searchable-select-input', inputClass]"
      :value="query"
      :placeholder="placeholder"
      :disabled="disabled"
      @focus="handleFocus"
      @input="handleInput"
    />

    <Transition name="select-menu">
      <div
        v-if="isOpen && !disabled"
        class="select-menu absolute z-30 mt-2 max-h-72 w-full overflow-y-auto rounded-2xl border border-[var(--color-border)] bg-white p-2 shadow-[0_18px_40px_rgba(15,23,42,0.16)]"
      >
        <button
          v-for="option in filteredOptions"
          :key="option.value"
          class="flex w-full flex-col rounded-2xl px-3 py-2 text-left transition hover:bg-[var(--color-surface-low)]"
          type="button"
          @click="chooseOption(option)"
        >
          <span class="text-sm font-medium text-slate-950">{{ option.label }}</span>
          <span v-if="option.description" class="mt-1 text-xs text-slate-600">{{ option.description }}</span>
        </button>

        <p v-if="!filteredOptions.length" class="px-3 py-2 text-sm text-slate-600">{{ emptyLabel }}</p>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.searchable-select {
  position: relative;
  width: 100%;
}

.searchable-select-input {
  width: 100%;
}

.select-menu-enter-active,
.select-menu-leave-active {
  transition: opacity 0.18s ease, transform 0.18s ease;
}

.select-menu-enter-from,
.select-menu-leave-to {
  opacity: 0;
  transform: translateY(-4px) scale(0.98);
}
</style>
