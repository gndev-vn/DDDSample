import { computed, onMounted, reactive, ref } from 'vue';
import { paymentApi } from '../api/payment';
import { paymentStatusLabel } from '../lib/formatters';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type { PaymentModel } from '../types/contracts';

export function usePaymentsAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();

  const loading = ref(true);
  const workingPaymentId = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);
  const payments = ref<PaymentModel[]>([]);
  const selectedPaymentId = ref('');
  const isPaymentDialogOpen = ref(false);
  const paymentSearch = ref('');
  const forms = reactive<Record<string, { transactionReference: string; failureReason: string }>>({});

  const selectedPayment = computed(() =>
    payments.value.find((payment) => payment.id === selectedPaymentId.value) ?? null,
  );

  const canViewPayments = computed(() => authStore.hasPermission(appPermissions.payments.view));
  const canManagePayments = computed(() => authStore.hasPermission(appPermissions.payments.manage));

  const filteredPayments = computed(() => {
    const search = paymentSearch.value.trim().toLowerCase();
    if (!search) {
      return payments.value;
    }

    return payments.value.filter((payment) =>
      [
        payment.id,
        payment.orderId,
        payment.transactionReference ?? '',
        payment.failureReason ?? '',
        paymentStatusLabel(payment.status),
      ]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  function stateFor(paymentId: string) {
    if (!forms[paymentId]) {
      forms[paymentId] = {
        transactionReference: `TX-${paymentId.slice(0, 8).toUpperCase()}`,
        failureReason: '',
      };
    }

    return forms[paymentId];
  }

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function openPaymentDialog(payment: PaymentModel) {
    selectedPaymentId.value = payment.id;
    isPaymentDialogOpen.value = true;
  }

  async function refresh() {
    if (!authStore.token || !canViewPayments.value) {
      payments.value = [];
      selectedPaymentId.value = '';
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      payments.value = await paymentApi.getPayments(authStore.token);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load payments.');
    } finally {
      loading.value = false;
    }
  }

  async function completePayment(payment: PaymentModel) {
    if (!authStore.token || !canManagePayments.value) {
      setOutcome(null, 'Payment management permission is required to complete payments.');
      return;
    }

    const form = stateFor(payment.id);
    workingPaymentId.value = payment.id;
    setOutcome(null, null);

    try {
      const confirmed = await uiStore.confirm({
        title: 'Approve payment',
        message: `Approve payment ${payment.id} and mark it as completed?`,
        confirmLabel: 'Approve payment',
      });
      if (!confirmed) {
        return;
      }

      await paymentApi.completePayment(authStore.token, payment.id, {
        transactionReference: form.transactionReference,
      });
      uiStore.pushToast('Payment marked as completed.', 'success');
      setOutcome('Payment marked as completed.');
      await refresh();
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to complete payment.');
    } finally {
      workingPaymentId.value = null;
    }
  }

  async function failPayment(payment: PaymentModel) {
    if (!authStore.token || !canManagePayments.value) {
      setOutcome(null, 'Payment management permission is required to fail payments.');
      return;
    }

    const form = stateFor(payment.id);
    workingPaymentId.value = payment.id;
    setOutcome(null, null);

    try {
      const confirmed = await uiStore.confirm({
        title: 'Mark payment as failed',
        message: `Mark payment ${payment.id} as failed?`,
        confirmLabel: 'Mark failed',
        tone: 'danger',
      });
      if (!confirmed) {
        return;
      }

      await paymentApi.failPayment(authStore.token, payment.id, {
        reason: form.failureReason,
      });
      uiStore.pushToast('Payment marked as failed.', 'success');
      setOutcome('Payment marked as failed.');
      await refresh();
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to fail payment.');
    } finally {
      workingPaymentId.value = null;
    }
  }

  onMounted(() => {
    void refresh();
  });

  return {
    loading,
    workingPaymentId,
    error,
    success,
    payments,
    selectedPaymentId,
    isPaymentDialogOpen,
    paymentSearch,
    selectedPayment,
    canManagePayments,
    filteredPayments,
    stateFor,
    openPaymentDialog,
    refresh,
    completePayment,
    failPayment,
  };
}

