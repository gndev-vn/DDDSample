import { defineStore } from 'pinia';

export type ToastTone = 'success' | 'error' | 'info';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  tone?: 'default' | 'danger';
}

interface ToastMessage {
  id: string;
  message: string;
  tone: ToastTone;
}

let pendingResolver: ((value: boolean) => void) | null = null;

export const useUiStore = defineStore('ui', {
  state: () => ({
    toasts: [] as ToastMessage[],
    confirmOpen: false,
    confirmTitle: '',
    confirmMessage: '',
    confirmLabel: 'Confirm',
    confirmTone: 'default' as 'default' | 'danger',
    pageLoadingCount: 0,
  }),
  getters: {
    pageLoading: (state) => state.pageLoadingCount > 0,
  },
  actions: {
    pushToast(message: string, tone: ToastTone = 'info') {
      const id = crypto.randomUUID();
      this.toasts = [...this.toasts, { id, message, tone }];

      window.setTimeout(() => {
        this.dismissToast(id);
      }, 4000);
    },

    dismissToast(id: string) {
      this.toasts = this.toasts.filter((toast) => toast.id !== id);
    },

    startPageLoading() {
      this.pageLoadingCount += 1;
    },

    finishPageLoading() {
      this.pageLoadingCount = Math.max(0, this.pageLoadingCount - 1);
    },

    async confirm(options: ConfirmOptions) {
      this.confirmTitle = options.title;
      this.confirmMessage = options.message;
      this.confirmLabel = options.confirmLabel ?? 'Confirm';
      this.confirmTone = options.tone ?? 'default';
      this.confirmOpen = true;

      return await new Promise<boolean>((resolve) => {
        pendingResolver = resolve;
      });
    },

    resolveConfirm(value: boolean) {
      this.confirmOpen = false;
      const resolver = pendingResolver;
      pendingResolver = null;
      resolver?.(value);
    },
  },
});
