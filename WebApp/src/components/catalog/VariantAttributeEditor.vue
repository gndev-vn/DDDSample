<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import type {
  ProductAttributeDefinitionModel,
  ProductVariantAttributeValueRequest,
} from '../../types/contracts';
import { resolveAttributeName } from '../../lib/variantAttributes';

const props = withDefaults(defineProps<{
  modelValue: ProductVariantAttributeValueRequest[];
  definitions: ProductAttributeDefinitionModel[];
  newDefinitionName: string;
  disabled?: boolean;
  creatingDefinition?: boolean;
  canManageDefinitions?: boolean;
}>(), {
  disabled: false,
  creatingDefinition: false,
  canManageDefinitions: false,
});

const emit = defineEmits<{
  'update:modelValue': [ProductVariantAttributeValueRequest[]];
  'update:newDefinitionName': [string];
  'create-definition': [];
}>();

const root = ref<HTMLElement | null>(null);
const isSearchOpen = ref(false);

const availableDefinitions = computed(() =>
  props.definitions.filter((definition) =>
    !props.modelValue.some((attribute) => attribute.attributeId === definition.id),
  ),
);

const searchedDefinitions = computed(() => {
  const search = props.newDefinitionName.trim().toLowerCase();
  if (!search) {
    return [];
  }

  return availableDefinitions.value.filter((definition) => definition.name.toLowerCase().includes(search));
});

const canCreateDefinitionFromSearch = computed(() =>
  props.canManageDefinitions &&
  props.newDefinitionName.trim().length > 0 &&
  !props.definitions.some((definition) => definition.name.toLowerCase() === props.newDefinitionName.trim().toLowerCase()),
);

const attributePreview = computed(() =>
  props.modelValue
    .filter((attribute) => attribute.attributeId && attribute.value.trim())
    .map((attribute) => `${resolveAttributeName(attribute.attributeId, props.definitions)}: ${attribute.value.trim()}`),
);

function updateRows(next: ProductVariantAttributeValueRequest[]) {
  emit('update:modelValue', next);
}

function addAttributeRow(attributeId: string) {
  updateRows([
    ...props.modelValue,
    { attributeId, value: '' },
  ]);
  emit('update:newDefinitionName', '');
  isSearchOpen.value = false;
}

function updateAttribute(index: number, patch: Partial<ProductVariantAttributeValueRequest>) {
  updateRows(props.modelValue.map((attribute, currentIndex) =>
    currentIndex === index
      ? {
          attributeId: patch.attributeId ?? attribute.attributeId,
          value: patch.value ?? attribute.value,
        }
      : attribute,
  ));
}

function removeAttribute(index: number) {
  updateRows(props.modelValue.filter((_, currentIndex) => currentIndex !== index));
}

function handleDefinitionNameInput(event: Event) {
  emit('update:newDefinitionName', (event.target as HTMLInputElement).value);
  isSearchOpen.value = true;
}

function handleAttributeValueInput(index: number, event: Event) {
  updateAttribute(index, { value: (event.target as HTMLInputElement).value });
}

function handleDocumentPointer(event: MouseEvent) {
  if (!root.value?.contains(event.target as Node)) {
    isSearchOpen.value = false;
  }
}

function openSearch() {
  if (!props.disabled) {
    isSearchOpen.value = true;
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleDocumentPointer);
});

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleDocumentPointer);
});
</script>

<template>
  <section ref="root" class="attribute-editor space-y-4">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div>
        <h4 class="section-title">Attribute assignments</h4>
        <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
          Search the reusable attribute library, click a result to add it, then fill in the value.
        </p>
      </div>
      <div class="attribute-library-badge">
        {{ definitions.length }} definition{{ definitions.length === 1 ? '' : 's' }}
      </div>
    </div>

    <div class="attribute-library-panel space-y-3">
      <div class="searchable-select">
        <input
          :value="newDefinitionName"
          class="text-input"
          :disabled="disabled"
          placeholder="Search or create an attribute, e.g. Material"
          @focus="openSearch"
          @input="handleDefinitionNameInput"
        />

        <Transition name="select-menu">
          <div
            v-if="isSearchOpen && !disabled"
            class="select-menu absolute z-30 mt-2 max-h-72 w-full overflow-y-auto rounded-2xl border border-[var(--color-border)] bg-white p-2 shadow-[0_18px_40px_rgba(15,23,42,0.16)]"
          >
            <div v-if="searchedDefinitions.length" class="space-y-2">
              <button
                v-for="definition in searchedDefinitions"
                :key="definition.id"
                class="flex w-full items-center justify-between rounded-2xl px-3 py-3 text-left transition hover:bg-[var(--color-surface-low)]"
                :disabled="disabled"
                type="button"
                @click="addAttributeRow(definition.id)"
              >
                <div>
                  <div class="font-medium text-slate-900">{{ definition.name }}</div>
                  <div class="mt-1 text-xs text-slate-500">
                    Used on {{ definition.usageCount }} variant{{ definition.usageCount === 1 ? '' : 's' }}
                  </div>
                </div>
                <span class="text-xs font-semibold uppercase tracking-[0.16em] text-brand-700">Add</span>
              </button>
            </div>

            <button
              v-else-if="canCreateDefinitionFromSearch"
              class="flex w-full items-center justify-between rounded-2xl px-3 py-3 text-left transition hover:bg-[var(--color-surface-low)]"
              :disabled="creatingDefinition || !canManageDefinitions"
              type="button"
              @click="emit('create-definition')"
            >
              <div>
                <div class="font-medium text-slate-900">Create {{ newDefinitionName.trim() }}</div>
                <div class="mt-1 text-xs text-slate-500">No matching attribute definition exists yet.</div>
              </div>
              <span v-if="creatingDefinition" class="button-spinner" aria-hidden="true" />
              <span v-else class="text-xs font-semibold uppercase tracking-[0.16em] text-brand-700">Create</span>
            </button>

            <p v-else-if="newDefinitionName.trim().length" class="px-3 py-2 text-sm text-[var(--color-ink-muted)]">
              That attribute is already assigned to this variant.
            </p>

            <p v-else class="px-3 py-2 text-sm text-[var(--color-ink-muted)]">
              Search the attribute library to add an assignment.
            </p>
          </div>
        </Transition>
      </div>

      <p v-if="!definitions.length" class="text-sm text-[var(--color-ink-muted)]">
        Start the library with the core attributes you reuse across many variants.
      </p>
    </div>

    <div v-if="definitions.length && modelValue.length" class="space-y-3">
      <div v-for="(attribute, index) in modelValue" :key="index" class="attribute-row">
        <div class="attribute-pill">
          {{ resolveAttributeName(attribute.attributeId, definitions) }}
        </div>
        <input
          class="text-input"
          :disabled="disabled"
          :value="attribute.value"
          placeholder="Value"
          @input="handleAttributeValueInput(index, $event)"
        />
        <button class="icon-button" :disabled="disabled" title="Remove attribute" aria-label="Remove attribute" @click="removeAttribute(index)">
          <span class="icon-glyph">✕</span>
        </button>
      </div>
    </div>

    <div v-if="attributePreview.length" class="flex flex-wrap gap-2">
      <span v-for="item in attributePreview" :key="item" class="attribute-chip">
        {{ item }}
      </span>
    </div>
  </section>
</template>

<style scoped>
.searchable-select {
  position: relative;
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
