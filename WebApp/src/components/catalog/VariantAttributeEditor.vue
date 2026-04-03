<script setup lang="ts">
import { computed } from 'vue';
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

const canAddAttribute = computed(() =>
  props.definitions.some((definition) =>
    !props.modelValue.some((attribute) => attribute.attributeId === definition.id),
  ),
);

const attributePreview = computed(() =>
  props.modelValue
    .filter((attribute) => attribute.attributeId && attribute.value.trim())
    .map((attribute) => `${resolveAttributeName(attribute.attributeId, props.definitions)}: ${attribute.value.trim()}`),
);

function updateRows(next: ProductVariantAttributeValueRequest[]) {
  emit('update:modelValue', next);
}

function addAttributeRow() {
  const available = props.definitions.find((definition) =>
    !props.modelValue.some((attribute) => attribute.attributeId === definition.id),
  );
  if (!available) {
    return;
  }

  updateRows([
    ...props.modelValue,
    { attributeId: available.id, value: '' },
  ]);
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

function optionsFor(index: number) {
  const usedIds = new Set(
    props.modelValue
      .filter((_, currentIndex) => currentIndex !== index)
      .map((attribute) => attribute.attributeId),
  );

  return props.definitions.filter((definition) =>
    !usedIds.has(definition.id) || definition.id === props.modelValue[index]?.attributeId,
  );
}

function handleDefinitionNameInput(event: Event) {
  emit('update:newDefinitionName', (event.target as HTMLInputElement).value);
}

function handleAttributeDefinitionChange(index: number, event: Event) {
  updateAttribute(index, { attributeId: (event.target as HTMLSelectElement).value });
}

function handleAttributeValueInput(index: number, event: Event) {
  updateAttribute(index, { value: (event.target as HTMLInputElement).value });
}
</script>

<template>
  <section class="attribute-editor space-y-4">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div>
        <h4 class="section-title">Attribute assignments</h4>
        <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
          Build each variant from reusable attribute definitions so operators stay consistent.
        </p>
      </div>
      <div class="attribute-library-badge">
        {{ definitions.length }} definition{{ definitions.length === 1 ? '' : 's' }}
      </div>
    </div>

    <div class="attribute-library-panel space-y-3">
      <div class="flex flex-col gap-3 md:flex-row">
        <input
          :value="newDefinitionName"
          class="text-input"
          :disabled="disabled || !canManageDefinitions"
          placeholder="Create a new reusable attribute, e.g. Material"
          @input="handleDefinitionNameInput"
        />
        <button
          class="btn-secondary"
          :disabled="disabled || !canManageDefinitions || creatingDefinition || !newDefinitionName.trim()"
          @click="emit('create-definition')"
        >
          <span v-if="creatingDefinition" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">＋</span>
          <span>{{ creatingDefinition ? 'Creating...' : 'Add attribute definition' }}</span>
        </button>
      </div>

      <div v-if="definitions.length" class="flex flex-wrap gap-2">
        <span v-for="definition in definitions" :key="definition.id" class="attribute-chip attribute-chip-neutral">
          {{ definition.name }}
          <span class="text-xs text-slate-500">{{ definition.usageCount }}</span>
        </span>
      </div>
      <p v-else class="text-sm text-[var(--color-ink-muted)]">
        Start the library with the core attributes you reuse across many variants.
      </p>
    </div>

    <div v-if="definitions.length" class="space-y-3">
      <div v-if="modelValue.length" class="space-y-3">
        <div v-for="(attribute, index) in modelValue" :key="index" class="attribute-row">
          <select
            class="select-input"
            :disabled="disabled"
            :value="attribute.attributeId"
            @change="handleAttributeDefinitionChange(index, $event)"
          >
            <option v-for="definition in optionsFor(index)" :key="definition.id" :value="definition.id">
              {{ definition.name }}
            </option>
          </select>
          <input
            class="text-input"
            :disabled="disabled"
            :value="attribute.value"
            placeholder="Value"
            @input="handleAttributeValueInput(index, $event)"
          />
          <button class="icon-button" :disabled="disabled" title="Remove attribute" aria-label="Remove attribute" @click="removeAttribute(index)">
            <span class="icon-glyph">✕</span>
            <span>Remove</span>
          </button>
        </div>
      </div>

      <div class="flex flex-wrap items-center justify-between gap-3">
        <button class="btn-secondary" :disabled="disabled || !canAddAttribute" @click="addAttributeRow">
          <span class="button-icon" aria-hidden="true">＋</span>
          <span>Add attribute</span>
        </button>
        <p class="text-sm text-[var(--color-ink-muted)]">
          {{ modelValue.length }} assigned / {{ definitions.length }} available
        </p>
      </div>
    </div>

    <div v-if="attributePreview.length" class="flex flex-wrap gap-2">
      <span v-for="item in attributePreview" :key="item" class="attribute-chip">
        {{ item }}
      </span>
    </div>
  </section>
</template>
