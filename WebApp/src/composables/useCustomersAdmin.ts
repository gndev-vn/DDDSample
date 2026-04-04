import { computed, reactive, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { orderingApi } from '../api/ordering';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type { CreateCustomerRequest, CustomerModel, UpdateCustomerRequest } from '../types/contracts';

export function useCustomersAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();
  const router = useRouter();

  const loading = ref(false);
  const creatingCustomer = ref(false);
  const savingCustomer = ref(false);
  const deletingCustomerId = ref<string | null>(null);
  const success = ref<string | null>(null);
  const error = ref<string | null>(null);
  const customers = ref<CustomerModel[]>([]);
  const selectedCustomerId = ref('');
  const isCustomerDialogOpen = ref(false);
  const isCreateCustomerDialogOpen = ref(false);
  const customerSearch = ref('');

  const createCustomerForm = reactive<CreateCustomerRequest>({
    displayName: '',
    email: '',
    phoneNumber: '',
    address: '',
    isActive: true,
  });

  const editCustomerForm = reactive<UpdateCustomerRequest>({
    id: '',
    displayName: '',
    email: '',
    phoneNumber: '',
    address: '',
    isActive: true,
  });

  const selectedCustomer = computed(() => customers.value.find((customer) => customer.id === selectedCustomerId.value) ?? null);
  const canViewCustomers = computed(() => authStore.hasPermission(appPermissions.customers.view));
  const canCreateCustomers = computed(() => authStore.hasPermission(appPermissions.customers.create));
  const canUpdateCustomers = computed(() => authStore.hasPermission(appPermissions.customers.update));
  const canDeleteCustomers = computed(() => authStore.hasPermission(appPermissions.customers.delete));
  const canManageCustomers = computed(() => canCreateCustomers.value || canUpdateCustomers.value || canDeleteCustomers.value);

  const filteredCustomers = computed(() => {
    const search = customerSearch.value.trim().toLowerCase();
    if (!search) {
      return customers.value;
    }

    return customers.value.filter((customer) =>
      [customer.displayName, customer.email, customer.phoneNumber ?? '', customer.address ?? '', customer.isActive ? 'active' : 'inactive']
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  const activeCustomerCount = computed(() => customers.value.filter((customer) => customer.isActive).length);
  const inactiveCustomerCount = computed(() => customers.value.filter((customer) => !customer.isActive).length);

  const hasCustomerChanges = computed(() => {
    if (!selectedCustomer.value) {
      return false;
    }

    return JSON.stringify({
      displayName: editCustomerForm.displayName.trim(),
      email: editCustomerForm.email.trim(),
      phoneNumber: editCustomerForm.phoneNumber?.trim() ?? '',
      address: editCustomerForm.address?.trim() ?? '',
      isActive: editCustomerForm.isActive,
    }) !== JSON.stringify({
      displayName: selectedCustomer.value.displayName.trim(),
      email: selectedCustomer.value.email.trim(),
      phoneNumber: selectedCustomer.value.phoneNumber?.trim() ?? '',
      address: selectedCustomer.value.address?.trim() ?? '',
      isActive: selectedCustomer.value.isActive,
    });
  });

  function setOutcome(message: string | null, failure: string | null = null) {
    success.value = message;
    error.value = failure;
  }

  function applySelectedCustomer(customer: CustomerModel | null) {
    if (!customer) {
      editCustomerForm.id = '';
      editCustomerForm.displayName = '';
      editCustomerForm.email = '';
      editCustomerForm.phoneNumber = '';
      editCustomerForm.address = '';
      editCustomerForm.isActive = true;
      return;
    }

    editCustomerForm.id = customer.id;
    editCustomerForm.displayName = customer.displayName;
    editCustomerForm.email = customer.email;
    editCustomerForm.phoneNumber = customer.phoneNumber ?? '';
    editCustomerForm.address = customer.address ?? '';
    editCustomerForm.isActive = customer.isActive;
  }

  function resetCreateCustomerForm() {
    createCustomerForm.displayName = '';
    createCustomerForm.email = '';
    createCustomerForm.phoneNumber = '';
    createCustomerForm.address = '';
    createCustomerForm.isActive = true;
  }

  function openCustomerDialog(customer: CustomerModel) {
    selectedCustomerId.value = customer.id;
    isCustomerDialogOpen.value = true;
  }

  function openCreateCustomerDialog() {
    resetCreateCustomerForm();
    isCreateCustomerDialogOpen.value = true;
  }

  async function viewCustomerOrders(customerId: string) {
    if (!customerId) {
      return;
    }

    isCustomerDialogOpen.value = false;
    await router.push({ name: 'orders', query: { customerId } });
  }

  async function refreshCustomers(preserveSelection = true) {
    if (!authStore.token || !canViewCustomers.value) {
      customers.value = [];
      selectedCustomerId.value = '';
      applySelectedCustomer(null);
      loading.value = false;
      return;
    }

    loading.value = true;
    setOutcome(null, null);

    try {
      const result = await orderingApi.getCustomers(authStore.token);
      customers.value = result;

      if (!preserveSelection || !result.some((customer) => customer.id === selectedCustomerId.value)) {
        selectedCustomerId.value = result[0]?.id ?? '';
      }

      applySelectedCustomer(result.find((customer) => customer.id === selectedCustomerId.value) ?? null);
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Unable to load customers.');
    } finally {
      loading.value = false;
    }
  }

  async function handleCreateCustomer() {
    if (!authStore.token || !canCreateCustomers.value) {
      setOutcome(null, 'Customer create permission is required to create customers.');
      return;
    }

    creatingCustomer.value = true;
    setOutcome(null, null);

    try {
      const createdCustomer = await orderingApi.createCustomer(authStore.token, {
        displayName: createCustomerForm.displayName,
        email: createCustomerForm.email,
        phoneNumber: createCustomerForm.phoneNumber,
        address: createCustomerForm.address,
        isActive: createCustomerForm.isActive,
      });

      resetCreateCustomerForm();
      isCreateCustomerDialogOpen.value = false;
      await refreshCustomers(false);

      if (createdCustomer) {
        selectedCustomerId.value = createdCustomer.id;
      }

      uiStore.pushToast('Customer created successfully.', 'success');
      setOutcome('Customer created successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Customer creation failed.');
    } finally {
      creatingCustomer.value = false;
    }
  }

  async function handleSaveCustomer() {
    if (!authStore.token || !editCustomerForm.id || !canUpdateCustomers.value) {
      setOutcome(null, 'Customer update permission is required to save customer changes.');
      return;
    }

    savingCustomer.value = true;
    setOutcome(null, null);

    try {
      await orderingApi.updateCustomer(authStore.token, {
        id: editCustomerForm.id,
        displayName: editCustomerForm.displayName,
        email: editCustomerForm.email,
        phoneNumber: editCustomerForm.phoneNumber,
        address: editCustomerForm.address,
        isActive: editCustomerForm.isActive,
      });

      await refreshCustomers();
      uiStore.pushToast('Customer details saved.', 'success');
      setOutcome('Customer updated successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Customer update failed.');
    } finally {
      savingCustomer.value = false;
    }
  }

  async function handleDeleteCustomer(customer: CustomerModel) {
    if (!authStore.token || !canDeleteCustomers.value) {
      setOutcome(null, 'Customer delete permission is required to delete customers.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete customer',
      message: `Delete ${customer.displayName}? Customers with existing orders cannot be deleted.`,
      confirmLabel: 'Delete customer',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deletingCustomerId.value = customer.id;
    setOutcome(null, null);

    try {
      await orderingApi.deleteCustomer(authStore.token, customer.id);
      if (selectedCustomerId.value === customer.id) {
        selectedCustomerId.value = '';
        applySelectedCustomer(null);
        isCustomerDialogOpen.value = false;
      }

      await refreshCustomers(false);
      uiStore.pushToast('Customer deleted.', 'success');
      setOutcome('Customer deleted successfully.');
    } catch (cause) {
      setOutcome(null, cause instanceof Error ? cause.message : 'Customer deletion failed.');
    } finally {
      deletingCustomerId.value = null;
    }
  }

  watch(selectedCustomer, (customer) => {
    applySelectedCustomer(customer);
  });

  watch(
    () => [authStore.token, authStore.user?.id, canViewCustomers.value] as const,
    async ([token, , canView]) => {
      if (token && canView) {
        await refreshCustomers();
        return;
      }

      customers.value = [];
      selectedCustomerId.value = '';
      applySelectedCustomer(null);
    },
    { immediate: true },
  );

  return {
    loading,
    creatingCustomer,
    savingCustomer,
    deletingCustomerId,
    success,
    error,
    customers,
    selectedCustomer,
    customerSearch,
    createCustomerForm,
    editCustomerForm,
    isCustomerDialogOpen,
    isCreateCustomerDialogOpen,
    canCreateCustomers,
    canUpdateCustomers,
    canDeleteCustomers,
    canManageCustomers,
    filteredCustomers,
    activeCustomerCount,
    inactiveCustomerCount,
    hasCustomerChanges,
    openCustomerDialog,
    openCreateCustomerDialog,
    viewCustomerOrders,
    refreshCustomers,
    handleCreateCustomer,
    handleSaveCustomer,
    handleDeleteCustomer,
  };
}
