<script setup lang="ts">
defineProps<{
  title: string;
  value: string | number;
  subtitle: string;
}>();
</script>

<template>
  <article class="summary-tile">
    <p class="workspace-label">{{ title }}</p>
    <p class="mt-4 display-value">{{ value }}</p>
    <p class="mt-2 text-sm text-[var(--color-ink-muted)]">{{ subtitle }}</p>
  </article>
</template>
