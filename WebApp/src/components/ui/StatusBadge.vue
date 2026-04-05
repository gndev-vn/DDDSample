<script setup lang="ts">
const props = defineProps<{
  label: string;
  tone?: 'neutral' | 'success' | 'warning' | 'danger';
}>();

const toneClassMap: Record<NonNullable<typeof props.tone>, string> = {
  neutral: 'border border-[var(--color-border)] bg-white text-[var(--color-ink-muted)]',
  success: 'border border-emerald-300 bg-emerald-50 text-emerald-900',
  warning: 'border border-amber-300 bg-amber-50 text-amber-900',
  danger: 'border border-rose-300 bg-rose-50 text-rose-900',
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
