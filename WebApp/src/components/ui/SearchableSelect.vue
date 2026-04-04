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

    <div
      v-if="isOpen && !disabled"
      class="absolute z-30 mt-2 max-h-72 w-full overflow-y-auto rounded-2xl border border-slate-200 bg-white p-2 shadow-xl"
    >
      <button
        v-for="option in filteredOptions"
        :key="option.value"
        class="flex w-full flex-col rounded-2xl px-3 py-2 text-left transition hover:bg-slate-50"
        type="button"
        @click="chooseOption(option)"
      >
        <span class="text-sm font-medium text-slate-900">{{ option.label }}</span>
        <span v-if="option.description" class="mt-1 text-xs text-slate-500">{{ option.description }}</span>
      </button>

      <p v-if="!filteredOptions.length" class="px-3 py-2 text-sm text-slate-500">{{ emptyLabel }}</p>
    </div>
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
</style>
