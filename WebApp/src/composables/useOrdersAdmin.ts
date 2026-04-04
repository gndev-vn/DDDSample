import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { catalogApi } from '../api/catalog';
import { orderingApi } from '../api/ordering';
import { orderStatusLabel } from '../lib/formatters';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type {
  AddressInput,
  CreateOrderRequest,
  CustomerModel,
  OrderLineModel,
  OrderModel,
  ProductResponse,
  ProductVariantResponse,
} from '../types/contracts';

type CreateOrderLineDraft = {
  id: string;
  variantId: string;
  quantity: number;
};

type SearchableOption = {
  value: string;
  label: string;
  description?: string;
  keywords?: string[];
};

export function useOrdersAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();
  const route = useRoute();
  const router = useRouter();

  const loading = ref(true);
  const submitting = ref(false);
  const saving = ref(false);
  const deletingOrderId = ref<string | null>(null);
  const payingOrderId = ref<string | null>(null);
  const cancelingOrderId = ref<string | null>(null);
  const error = ref<string | null>(null);
  const success = ref<string | null>(null);
  const orders = ref<OrderModel[]>([]);
  const products = ref<ProductResponse[]>([]);
  const variants = ref<ProductVariantResponse[]>([]);
  const customers = ref<CustomerModel[]>([]);
  const selectedOrderId = ref('');
  const isOrderDialogOpen = ref(false);
  const isCreateOrderDialogOpen = ref(false);
  const orderSearch = ref('');

  const createForm = reactive({
    customerId: '',
    lines: [] as CreateOrderLineDraft[],
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

  const selectedOrder = computed(() => orders.value.find((order) => order.id === selectedOrderId.value) ?? null);
  const customerFilterId = computed(() => (typeof route.query.customerId === 'string' ? route.query.customerId : '').trim());
  const customerFilter = computed(() => customers.value.find((customer) => customer.id === customerFilterId.value) ?? null);
  const canViewOrders = computed(() => authStore.hasPermission(appPermissions.orders.view));
  const canCreateOrders = computed(() => authStore.hasPermission(appPermissions.orders.create));
  const canUpdateOrders = computed(() => authStore.hasPermission(appPermissions.orders.update));
  const canDeleteOrders = computed(() => authStore.hasPermission(appPermissions.orders.delete));
  const canManageOrders = computed(() => canCreateOrders.value || canUpdateOrders.value || canDeleteOrders.value);

  const activeVariants = computed(() => variants.value.filter((variant) => variant.isActive));

  const customerOptions = computed<SearchableOption[]>(() => {
    const optionMap = new Map<string, SearchableOption>();

    for (const customer of customers.value) {
      if (!customer.isActive && customer.id !== editForm.customerId && customer.id !== createForm.customerId) {
        continue;
      }

      optionMap.set(customer.id, {
        value: customer.id,
        label: customer.displayName,
        description: [customer.email, customer.phoneNumber, customer.address, customer.isActive ? null : 'Inactive']
          .filter(Boolean)
          .join(' · '),
        keywords: [customer.email, customer.phoneNumber ?? '', customer.address ?? ''],
      });
    }

    for (const order of orders.value) {
      if (optionMap.has(order.customerId)) {
        continue;
      }

      optionMap.set(order.customerId, {
        value: order.customerId,
        label: order.customerName || `Customer ${order.customerId.slice(0, 8)}`,
        description: [order.customerEmail, order.customerPhone].filter(Boolean).join(' · '),
        keywords: [order.customerEmail, order.customerPhone ?? '', order.customerId],
      });
    }

    return [...optionMap.values()].sort((left, right) => left.label.localeCompare(right.label));
  });

  const variantOptions = computed<SearchableOption[]>(() =>
    activeVariants.value.map((variant) => ({
      value: variant.id,
      label: variant.name,
      description: `${variant.sku} · ${variantPriceLabel(variant)}`,
      keywords: [variant.sku, products.value.find((product) => product.id === variant.parentId)?.name ?? ''],
    })),
  );

  const filteredOrders = computed(() => {
    const search = orderSearch.value.trim().toLowerCase();
    if (!search) {
      return orders.value;
    }

    return orders.value.filter((order) =>
      [
        order.id,
        order.customerId,
        order.customerName,
        order.customerEmail,
        order.customerPhone ?? '',
        orderStatusLabel(order.status),
        order.lines.map((line) => `${line.name} ${line.sku} ${line.productId}`).join(' '),
      ]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  const createOrderCanSubmit = computed(() => {
    if (!createForm.customerId.trim() || createForm.lines.length === 0) {
      return false;
    }

    return createForm.lines.every((line) => line.variantId.trim() && Number.isFinite(line.quantity) && line.quantity > 0);
  });

  const hasOrderChanges = computed(() => {
    if (!selectedOrder.value) {
      return false;
    }

    return JSON.stringify({
      customerId: editForm.customerId.trim(),
      shippingAddress: normalizeAddress(editForm.shippingAddress),
    }) !== JSON.stringify({
      customerId: selectedOrder.value.customerId.trim(),
      shippingAddress: normalizeAddress(selectedOrder.value.shippingAddress),
    });
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

  function normalizeAddress(address?: AddressInput | null) {
    return {
      line1: address?.line1?.trim() ?? '',
      line2: address?.line2?.trim() ?? '',
      city: address?.city?.trim() ?? '',
      province: address?.province?.trim() ?? '',
      district: address?.district?.trim() ?? '',
      ward: address?.ward?.trim() ?? '',
    };
  }

  function cloneLines(lines: OrderLineModel[]) {
    return lines.map((line) => ({ ...line }));
  }

  function createDraftLine(variantId = activeVariants.value[0]?.id ?? ''): CreateOrderLineDraft {
    return {
      id: crypto.randomUUID(),
      variantId,
      quantity: 1,
    };
  }

  function variantForCreateLine(variantId: string) {
    return activeVariants.value.find((variant) => variant.id === variantId) ?? null;
  }

  function productForVariant(variant: ProductVariantResponse | null) {
    if (!variant) {
      return null;
    }

    return products.value.find((product) => product.id === variant.parentId) ?? null;
  }

  function variantUnitPrice(variant: ProductVariantResponse | null) {
    if (!variant) {
      return 0;
    }

    return variant.overridePrice ?? productForVariant(variant)?.basePrice ?? 0;
  }

  function variantCurrency(variant: ProductVariantResponse | null) {
    if (!variant) {
      return 'USD';
    }

    return variant.currency || productForVariant(variant)?.currency || 'USD';
  }

  function variantPriceLabel(variant: ProductVariantResponse | null) {
    if (!variant) {
      return 'USD 0.00';
    }

    const price = variantUnitPrice(variant).toFixed(2);
    if (variant.overridePrice == null) {
      return `${variantCurrency(variant)} ${price} · uses product price`;
    }

    return `${variantCurrency(variant)} ${price}`;
  }

  function lineTotal(line: OrderLineModel) {
    return line.unitPrice * line.quantity;
  }

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function resetCreateForm() {
    createForm.customerId = customers.value.find((customer) => customer.isActive)?.id ?? '';
    createForm.lines = [createDraftLine()];
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

  async function clearCustomerFilter() {
    await router.push({ name: 'orders' });
  }

  function addCreateLine() {
    createForm.lines.push(createDraftLine());
  }

  function removeCreateLine(lineId: string) {
    if (createForm.lines.length === 1) {
      createForm.lines = [createDraftLine()];
      return;
    }

    createForm.lines = createForm.lines.filter((line) => line.id !== lineId);
  }

  function selectCreateLineVariant(lineId: string, variantId: string) {
    createForm.lines = createForm.lines.map((line) => (line.id === lineId ? { ...line, variantId } : line));
  }

  async function refresh() {
    if (!authStore.token || !canViewOrders.value) {
      orders.value = [];
      products.value = [];
      variants.value = [];
      customers.value = [];
      selectedOrderId.value = '';
      applySelectedOrder(null);
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      const [orderResult, productResult, variantResult, customerResult] = await Promise.all([
        orderingApi.getOrders(authStore.token, customerFilterId.value || undefined),
        catalogApi.getProducts(authStore.token),
        catalogApi.getProductVariants(authStore.token),
        orderingApi.getCustomers(authStore.token),
      ]);

      orders.value = orderResult;
      products.value = productResult;
      variants.value = variantResult.filter((variant) => variant.isActive);
      customers.value = customerResult;

      if (createForm.lines.length === 0) {
        resetCreateForm();
      }

      if (selectedOrderId.value && !orders.value.some((order) => order.id === selectedOrderId.value)) {
        selectedOrderId.value = '';
      }

      applySelectedOrder(orders.value.find((order) => order.id === selectedOrderId.value) ?? null);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load ordering data.');
    } finally {
      loading.value = false;
    }
  }

  async function submitOrder() {
    if (!authStore.token || !canCreateOrders.value) {
      setOutcome(null, 'Order create permission is required to create orders.');
      return;
    }

    if (!createOrderCanSubmit.value) {
      setOutcome(null, 'Select a customer and add at least one valid variant line before creating an order.');
      return;
    }

    submitting.value = true;
    setOutcome(null, null);

    try {
      const billingAddress = createForm.billingSameAsShipping
        ? cloneAddress(createForm.shippingAddress)
        : cloneAddress(createForm.billingAddress);

      const request: CreateOrderRequest = {
        customerId: createForm.customerId.trim(),
        shippingAddress: cloneAddress(createForm.shippingAddress),
        billingAddress,
        lines: createForm.lines.map((line) => {
          const variant = variantForCreateLine(line.variantId);
          if (!variant) {
            throw new Error('Every order line must reference a valid product variant.');
          }

          return {
            productId: variant.id,
            name: variant.name,
            sku: variant.sku,
            quantity: line.quantity,
            unitPrice: variantUnitPrice(variant),
            currency: variantCurrency(variant),
          };
        }),
      };

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
    if (!authStore.token || !editForm.id || !canUpdateOrders.value) {
      setOutcome(null, 'Order update permission is required to save order changes.');
      return;
    }

    saving.value = true;
    setOutcome(null, null);

    try {
      await orderingApi.updateOrder(authStore.token, {
        id: editForm.id,
        customerId: editForm.customerId,
        shippingAddress: cloneAddress(editForm.shippingAddress),
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
    if (!authStore.token || !canUpdateOrders.value) {
      setOutcome(null, 'Order update permission is required to change order state.');
      return;
    }

    if (action === 'pay') {
      payingOrderId.value = orderId;
    } else {
      cancelingOrderId.value = orderId;
    }

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
      if (action === 'pay') {
        payingOrderId.value = null;
      } else {
        cancelingOrderId.value = null;
      }
    }
  }

  async function deleteOrder(orderId: string) {
    if (!authStore.token || !canDeleteOrders.value) {
      setOutcome(null, 'Order delete permission is required to delete orders.');
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

  watch(selectedOrder, (order) => {
    applySelectedOrder(order);
  });

  watch(
    () => [authStore.token, authStore.user?.id, canViewOrders.value, customerFilterId.value] as const,
    async ([token, , canView]) => {
      if (token && canView) {
        await refresh();
        return;
      }

      orders.value = [];
      products.value = [];
      variants.value = [];
      customers.value = [];
      selectedOrderId.value = '';
      applySelectedOrder(null);
      loading.value = false;
    },
    { immediate: true },
  );

  onMounted(() => {
    void refresh();
  });

  return {
    loading,
    submitting,
    saving,
    deletingOrderId,
    payingOrderId,
    cancelingOrderId,
    error,
    success,
    orders,
    customers,
    selectedOrder,
    isOrderDialogOpen,
    isCreateOrderDialogOpen,
    orderSearch,
    customerFilter,
    createForm,
    editForm,
    customerOptions,
    variantOptions,
    filteredOrders,
    canManageOrders,
    canCreateOrders,
    canUpdateOrders,
    canDeleteOrders,
    createOrderCanSubmit,
    hasOrderChanges,
    variantForCreateLine,
    variantUnitPrice,
    variantCurrency,
    lineTotal,
    openOrderDialog,
    openCreateOrderDialog,
    clearCustomerFilter,
    addCreateLine,
    removeCreateLine,
    selectCreateLineVariant,
    refresh,
    submitOrder,
    saveOrder,
    runOrderAction,
    deleteOrder,
  };
}
