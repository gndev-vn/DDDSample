import { computed, onMounted, reactive, ref, watch } from 'vue';
import { catalogApi } from '../api/catalog';
import { orderingApi } from '../api/ordering';
import { orderStatusLabel } from '../lib/formatters';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type {
  AddressInput,
  CreateOrderRequest,
  OrderLineModel,
  OrderModel,
  ProductCacheModel,
  ProductVariantResponse,
} from '../types/contracts';

export function useOrdersAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();

  const loading = ref(true);
  const submitting = ref(false);
  const saving = ref(false);
  const deletingOrderId = ref<string | null>(null);
  const actionOrderId = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);
  const orders = ref<OrderModel[]>([]);
  const variants = ref<ProductVariantResponse[]>([]);
  const cachedProducts = reactive<Record<string, ProductCacheModel[]>>({});
  const selectedOrderId = ref('');
  const isOrderDialogOpen = ref(false);
  const isCreateOrderDialogOpen = ref(false);
  const orderSearch = ref('');

  const createForm = reactive({
    customerId: authStore.user?.id ?? crypto.randomUUID(),
    selectedVariantId: '',
    quantity: 1,
    shippingAddress: {
      line1: '',
      line2: '',
      city: '',
      province: '',
      district: '',
      ward: '',
    },
    billingSameAsShipping: true,
    billingAddress: {
      line1: '',
      line2: '',
      city: '',
      province: '',
      district: '',
      ward: '',
    },
  });

  const editForm = reactive<{
    id: string;
    customerId: string;
    shippingAddress: AddressInput;
    lines: OrderLineModel[];
  }>({
    id: '',
    customerId: '',
    shippingAddress: {
      line1: '',
      line2: '',
      city: '',
      province: '',
      district: '',
      ward: '',
    },
    lines: [],
  });

  const selectedVariant = computed(() =>
    variants.value.find((variant) => variant.id === createForm.selectedVariantId) ?? null,
  );

  const selectedOrder = computed(() =>
    orders.value.find((order) => order.id === selectedOrderId.value) ?? null,
  );

  const hasOrderChanges = computed(() => {
    if (!selectedOrder.value) {
      return false;
    }

    return JSON.stringify({
      customerId: editForm.customerId.trim(),
      shippingAddress: {
        line1: editForm.shippingAddress.line1.trim(),
        line2: editForm.shippingAddress.line2?.trim() ?? '',
        city: editForm.shippingAddress.city.trim(),
        province: editForm.shippingAddress.province.trim(),
        district: editForm.shippingAddress.district.trim(),
        ward: editForm.shippingAddress.ward.trim(),
      },
      lines: editForm.lines.map((line) => ({
        id: line.id,
        productId: line.productId,
        name: line.name.trim(),
        sku: line.sku.trim(),
        quantity: line.quantity,
        unitPrice: line.unitPrice,
        currency: line.currency.trim(),
      })),
    }) !== JSON.stringify({
      customerId: selectedOrder.value.customerId.trim(),
      shippingAddress: {
        line1: selectedOrder.value.shippingAddress?.line1?.trim() ?? '',
        line2: selectedOrder.value.shippingAddress?.line2?.trim() ?? '',
        city: selectedOrder.value.shippingAddress?.city?.trim() ?? '',
        province: selectedOrder.value.shippingAddress?.province?.trim() ?? '',
        district: selectedOrder.value.shippingAddress?.district?.trim() ?? '',
        ward: selectedOrder.value.shippingAddress?.ward?.trim() ?? '',
      },
      lines: selectedOrder.value.lines.map((line) => ({
        id: line.id,
        productId: line.productId,
        name: line.name.trim(),
        sku: line.sku.trim(),
        quantity: line.quantity,
        unitPrice: line.unitPrice,
        currency: line.currency.trim(),
      })),
    });
  });

  const canViewOrders = computed(() => authStore.hasPermission(appPermissions.orders.view));
  const canManageOrders = computed(() => authStore.hasPermission(appPermissions.orders.manage));

  const filteredOrders = computed(() => {
    const search = orderSearch.value.trim().toLowerCase();
    if (!search) {
      return orders.value;
    }

    return orders.value.filter((order) =>
      [
        order.id,
        order.customerId,
        orderStatusLabel(order.status),
        order.lines.map((line) => `${line.name} ${line.sku} ${line.productId}`).join(' '),
      ]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  function cloneAddress(address?: AddressInput | null): AddressInput {
    return {
      line1: address?.line1 ?? '',
      line2: address?.line2 ?? '',
      city: address?.city ?? '',
      province: address?.province ?? '',
      district: address?.district ?? '',
      ward: address?.ward ?? '',
    };
  }

  function cloneLines(lines: OrderLineModel[]): OrderLineModel[] {
    return lines.map((line) => ({ ...line }));
  }

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function resetCreateForm() {
    createForm.customerId = authStore.user?.id ?? crypto.randomUUID();
    createForm.selectedVariantId = variants.value[0]?.id ?? '';
    createForm.quantity = 1;
    createForm.shippingAddress = cloneAddress();
    createForm.billingSameAsShipping = true;
    createForm.billingAddress = cloneAddress();
  }

  function applySelectedOrder(order: OrderModel | null) {
    if (!order) {
      editForm.id = '';
      editForm.customerId = '';
      editForm.shippingAddress = cloneAddress();
      editForm.lines = [];
      return;
    }

    editForm.id = order.id;
    editForm.customerId = order.customerId;
    editForm.shippingAddress = cloneAddress(order.shippingAddress);
    editForm.lines = cloneLines(order.lines);
  }

  function openOrderDialog(order: OrderModel) {
    selectedOrderId.value = order.id;
    isOrderDialogOpen.value = true;
  }

  function openCreateOrderDialog() {
    resetCreateForm();
    isCreateOrderDialogOpen.value = true;
  }

  async function refresh() {
    if (!authStore.token || !canViewOrders.value) {
      orders.value = [];
      variants.value = [];
      selectedOrderId.value = '';
      applySelectedOrder(null);
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      const [orderResult, variantResult] = await Promise.all([
        orderingApi.getOrders(authStore.token),
        catalogApi.getProductVariants(authStore.token),
      ]);

      orders.value = orderResult;
      variants.value = variantResult.filter((variant) => variant.isActive);

      if (!createForm.selectedVariantId && variants.value.length > 0) {
        createForm.selectedVariantId = variants.value[0].id;
      }

      if (!selectedOrderId.value && orders.value.length > 0) {
        selectedOrderId.value = orders.value[0].id;
      }

      applySelectedOrder(orders.value.find((order) => order.id === selectedOrderId.value) ?? null);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load ordering data.');
    } finally {
      loading.value = false;
    }
  }

  async function submitOrder() {
    if (!authStore.token || !canManageOrders.value) {
      setOutcome(null, 'Order management permission is required to create orders.');
      return;
    }

    if (!selectedVariant.value) {
      setOutcome(null, 'Select a product variant before creating an order.');
      return;
    }

    submitting.value = true;
    setOutcome(null, null);

    const billingAddress = createForm.billingSameAsShipping
      ? { ...createForm.shippingAddress }
      : { ...createForm.billingAddress };

    const request: CreateOrderRequest = {
      customerId: createForm.customerId,
      shippingAddress: { ...createForm.shippingAddress },
      billingAddress,
      lines: [
        {
          productId: selectedVariant.value.id,
          name: selectedVariant.value.name,
          sku: selectedVariant.value.sku,
          quantity: createForm.quantity,
          unitPrice: selectedVariant.value.overridePrice,
          currency: selectedVariant.value.currency,
        },
      ],
    };

    try {
      const createdOrder = await orderingApi.createOrder(authStore.token, request);
      resetCreateForm();
      isCreateOrderDialogOpen.value = false;
      await refresh();
      if (createdOrder) {
        selectedOrderId.value = createdOrder.id;
      }
      uiStore.pushToast('Order created successfully.', 'success');
      setOutcome('Order created successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Order creation failed.');
    } finally {
      submitting.value = false;
    }
  }

  async function saveOrder() {
    if (!authStore.token || !canManageOrders.value || !editForm.id) {
      setOutcome(null, 'Select an order and sign in as an administrator before saving.');
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      await orderingApi.updateOrder(authStore.token, {
        id: editForm.id,
        customerId: editForm.customerId,
        shippingAddress: { ...editForm.shippingAddress },
        lines: editForm.lines.map((line) => ({ ...line })),
      });

      await refresh();
      uiStore.pushToast('Order updated successfully.', 'success');
      setOutcome('Order updated successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Order update failed.');
    } finally {
      saving.value = false;
    }
  }

  async function runOrderAction(orderId: string, action: 'pay' | 'cancel') {
    if (!authStore.token || !canManageOrders.value) {
      setOutcome(null, 'Order management permission is required to manage order lifecycle.');
      return;
    }

    actionOrderId.value = orderId;
    setOutcome(null, null);

    try {
      if (action === 'pay') {
        const confirmed = await uiStore.confirm({
          title: 'Mark order as paid',
          message: 'Confirm that this order has been paid and should move to the paid state.',
          confirmLabel: 'Mark as paid',
        });
        if (!confirmed) {
          return;
        }

        await orderingApi.payOrder(authStore.token, orderId);
        uiStore.pushToast('Order marked as paid.', 'success');
        setOutcome('Order marked as paid.');
      } else {
        const confirmed = await uiStore.confirm({
          title: 'Cancel order',
          message: 'Cancel this order? This should be used only when the order should no longer proceed.',
          confirmLabel: 'Cancel order',
          tone: 'danger',
        });
        if (!confirmed) {
          return;
        }

        await orderingApi.cancelOrder(authStore.token, orderId);
        uiStore.pushToast('Order cancelled successfully.', 'success');
        setOutcome('Order cancelled successfully.');
      }

      await refresh();
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Order action failed.');
    } finally {
      actionOrderId.value = null;
    }
  }

  async function deleteOrder(orderId: string) {
    if (!authStore.token || !canManageOrders.value) {
      setOutcome(null, 'Order management permission is required to delete orders.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete order',
      message: 'Delete this order permanently?',
      confirmLabel: 'Delete order',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deletingOrderId.value = orderId;
    setOutcome(null, null);

    try {
      await orderingApi.deleteOrder(authStore.token, orderId);
      if (selectedOrderId.value === orderId) {
        selectedOrderId.value = '';
        applySelectedOrder(null);
        isOrderDialogOpen.value = false;
      }
      await refresh();
      uiStore.pushToast('Order deleted successfully.', 'success');
      setOutcome('Order deleted successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Order deletion failed.');
    } finally {
      deletingOrderId.value = null;
    }
  }

  async function loadOrderCache(orderId: string) {
    if (!authStore.token || cachedProducts[orderId]) {
      return;
    }

    try {
      cachedProducts[orderId] = await orderingApi.getProductsInOrder(orderId, authStore.token);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Could not load cached products for the order.');
    }
  }

  function addLine() {
    editForm.lines.push({
      id: crypto.randomUUID(),
      productId: '',
      name: '',
      sku: '',
      quantity: 1,
      unitPrice: 0,
      currency: 'USD',
    });
  }

  function removeLine(lineId: string) {
    editForm.lines = editForm.lines.filter((line) => line.id !== lineId);
  }

  watch(selectedOrder, (order) => {
    applySelectedOrder(order);
    if (order) {
      void loadOrderCache(order.id);
    }
  });

  onMounted(() => {
    void refresh();
  });

  return {
    loading,
    submitting,
    saving,
    deletingOrderId,
    actionOrderId,
    error,
    success,
    orders,
    variants,
    cachedProducts,
    selectedOrderId,
    isOrderDialogOpen,
    isCreateOrderDialogOpen,
    orderSearch,
    createForm,
    editForm,
    selectedVariant,
    selectedOrder,
    hasOrderChanges,
    canManageOrders,
    filteredOrders,
    openOrderDialog,
    openCreateOrderDialog,
    refresh,
    submitOrder,
    saveOrder,
    runOrderAction,
    deleteOrder,
    addLine,
    removeLine,
  };
}

