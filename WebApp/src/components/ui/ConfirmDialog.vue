<script setup lang="ts">
import { storeToRefs } from 'pinia';
import EntityDialog from './EntityDialog.vue';
import { useUiStore } from '../../stores/ui';

const uiStore = useUiStore();
const { confirmOpen, confirmTitle, confirmMessage, confirmLabel, confirmTone } = storeToRefs(uiStore);
</script>

<template>
  <EntityDialog
    :open="confirmOpen"
    :title="confirmTitle"
    :description="confirmMessage"
    width-class="max-w-xl"
    @close="uiStore.resolveConfirm(false)"
  >
    <div class="flex flex-wrap justify-end gap-3">
      <button class="btn-secondary" @click="uiStore.resolveConfirm(false)">Cancel</button>
      <button :class="confirmTone === 'danger' ? 'btn-danger' : 'btn-primary'" @click="uiStore.resolveConfirm(true)">
        {{ confirmLabel }}
      </button>
    </div>
  </EntityDialog>
</template>
