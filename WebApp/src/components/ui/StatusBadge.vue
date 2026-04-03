<script setup lang="ts">
const props = defineProps<{
  label: string;
  tone?: 'neutral' | 'success' | 'warning' | 'danger';
}>();

const toneClassMap: Record<NonNullable<typeof props.tone>, string> = {
  neutral: 'bg-[var(--color-surface-high)] text-[var(--color-ink-muted)]',
  success: 'bg-[#d3e4fe] text-[#314055]',
  warning: 'bg-amber-100 text-amber-800',
  danger: 'bg-[#f97386] text-[#6e0523]',
};
</script>

<template>
  <span
    class="inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em]"
    :class="toneClassMap[tone ?? 'neutral']"
  >
    {{ label }}
  </span>
</template>
