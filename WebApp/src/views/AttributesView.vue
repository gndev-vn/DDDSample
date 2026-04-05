<script setup lang="ts">
import { computed } from 'vue';
import { useVariantAdmin } from '../composables/useVariantAdmin';

const admin = useVariantAdmin();

const filteredDefinitions = computed(() => {
  const search = admin.newAttributeName.value.trim().toLowerCase();
  if (!search) {
    return admin.attributeDefinitions.value;
  }

  return admin.attributeDefinitions.value.filter((definition) =>
    definition.name.toLowerCase().includes(search),
  );
});

const canCreateFromSearch = computed(() =>
  admin.canCreateVariants.value &&
  admin.newAttributeName.value.trim().length > 0 &&
  !admin.attributeDefinitions.value.some(
    (definition) => definition.name.toLowerCase() === admin.newAttributeName.value.trim().toLowerCase(),
  ),
);
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Attribute library</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Manage reusable variant attributes in one focused workspace before assigning them to products and variants.
        </p>
      </div>
      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="admin.newAttributeName.value" class="toolbar-search" placeholder="Search or create an attribute..." />
        <button class="btn-secondary" :disabled="admin.loading.value" @click="admin.refresh">
          <span v-if="admin.loading.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ admin.loading.value ? 'Refreshing...' : 'Reload attributes' }}</span>
        </button>
      </div>
    </div>

    <div v-if="!admin.canViewCurrentPage.value" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account does not have permission to view this catalog workspace.
    </div>
    <div v-else-if="!admin.canManageCurrentPage.value" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account can review this workspace, but cannot create, update, or delete records here.
    </div>
    <div v-if="admin.success.value" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ admin.success.value }}
    </div>
    <div v-if="admin.error.value" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ admin.error.value }}
    </div>

    <div class="flex flex-wrap gap-3">
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">Definitions</p>
        <p class="mt-3 display-value">{{ admin.attributeDefinitions.value.length }}</p>
      </article>
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">In use</p>
        <p class="mt-3 display-value">{{ admin.attributeDefinitions.value.filter((definition) => definition.usageCount > 0).length }}</p>
      </article>
      <article class="summary-tile min-w-[180px] flex-1">
        <p class="workspace-label">Unused</p>
        <p class="mt-3 display-value">{{ admin.attributeDefinitions.value.filter((definition) => definition.usageCount === 0).length }}</p>
      </article>
    </div>

    <article class="section-panel space-y-5">
      <div class="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p class="workspace-label">Definition management</p>
          <h4 class="mt-2 section-title">Reusable catalog attributes</h4>
          <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
            Search the existing library or create a new definition when merchandising needs a new attribute.
          </p>
        </div>

        <button v-if="canCreateFromSearch" class="btn-primary" :disabled="admin.creatingAttribute.value" @click="admin.createAttributeDefinition">
          <span v-if="admin.creatingAttribute.value" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">＋</span>
          <span>{{ admin.creatingAttribute.value ? 'Creating...' : `Create "${admin.newAttributeName.value.trim()}"` }}</span>
        </button>
      </div>

      <div class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        <article v-for="definition in filteredDefinitions" :key="definition.id" class="attribute-library-item bg-white shadow-[0_10px_24px_rgba(15,23,42,0.05)]">
          <div class="flex items-start justify-between gap-3">
            <div>
              <h5 class="text-base font-semibold text-slate-900">{{ definition.name }}</h5>
              <p class="mt-2 text-sm text-[var(--color-ink-muted)]">
                Used on {{ definition.usageCount }} variant{{ definition.usageCount === 1 ? '' : 's' }}.
              </p>
            </div>
            <span class="attribute-library-badge">{{ definition.usageCount }}</span>
          </div>
        </article>
      </div>

      <p v-if="!filteredDefinitions.length" class="rounded-2xl bg-[var(--color-surface-low)] px-4 py-4 text-sm text-[var(--color-ink-muted)]">
        {{ admin.attributeDefinitions.value.length ? 'No attributes match the current search.' : 'No attribute definitions have been created yet.' }}
      </p>
    </article>
  </section>
</template>
