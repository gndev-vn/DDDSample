import { computed, reactive, ref, watch } from 'vue';
import { identityApi } from '../api/identity';
import { orderingApi } from '../api/ordering';
import { appPermissions } from '../lib/permissions';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type { CreateUserRequest, CustomerModel, RoleModel, UpdateUserRequest, UserProfile } from '../types/contracts';

const customerRoleName = 'Customer';

export function useUsersAdmin() {
  const authStore = useAuthStore();
  const uiStore = useUiStore();

  const createUserForm = reactive<CreateUserRequest>({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    isActive: true,
    roles: ['User'],
    customerId: null,
  });

  const editUserForm = reactive<UpdateUserRequest>({
    id: '',
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    isActive: true,
    customerId: null,
  });

  const assignedRoles = ref<string[]>([]);
  const selectedUserId = ref('');
  const adminLoading = ref(false);
  const creatingUser = ref(false);
  const savingUser = ref(false);
  const deletingUserId = ref<string | null>(null);
  const feedback = ref<string | null>(null);
  const errorMessage = ref<string | null>(null);
  const users = ref<UserProfile[]>([]);
  const roles = ref<RoleModel[]>([]);
  const customers = ref<CustomerModel[]>([]);
  const isUserDialogOpen = ref(false);
  const isCreateUserDialogOpen = ref(false);
  const userSearch = ref('');

  const selectedUser = computed(() => users.value.find((user) => user.id === selectedUserId.value) ?? null);
  const roleNames = computed(() => roles.value.map((role) => role.name));
  const customerOptions = computed(() => customers.value.map((customer) => ({
    value: customer.id,
    label: customer.displayName,
    description: [customer.email, customer.phoneNumber].filter(Boolean).join(' · '),
    keywords: [customer.email, customer.phoneNumber ?? '', customer.id],
  })));
  const customerLabels = computed(() => new Map(customers.value.map((customer) => [customer.id, `${customer.displayName} · ${customer.email}`])));
  const canViewUsers = computed(() => authStore.hasPermission(appPermissions.users.view));
  const canCreateUsers = computed(() => authStore.hasPermission(appPermissions.users.create));
  const canUpdateUsers = computed(() => authStore.hasPermission(appPermissions.users.update));
  const canDeleteUsers = computed(() => authStore.hasPermission(appPermissions.users.delete));
  const canViewCustomers = computed(() => authStore.hasPermission(appPermissions.customers.view));
  const canManageUsers = computed(() => canCreateUsers.value || canUpdateUsers.value || canDeleteUsers.value);

  const hasUserChanges = computed(() => {
    if (!selectedUser.value) {
      return false;
    }

    const normalizedAssignedRoles = [...assignedRoles.value].sort();
    const normalizedUserRoles = [...selectedUser.value.roles].sort();

    return JSON.stringify({
      username: editUserForm.username.trim(),
      email: editUserForm.email.trim(),
      firstName: editUserForm.firstName.trim(),
      lastName: editUserForm.lastName.trim(),
      isActive: editUserForm.isActive,
      customerId: normalizeCustomerId(editUserForm.customerId),
      roles: normalizedAssignedRoles,
    }) !== JSON.stringify({
      username: selectedUser.value.username.trim(),
      email: selectedUser.value.email.trim(),
      firstName: selectedUser.value.firstName.trim(),
      lastName: selectedUser.value.lastName.trim(),
      isActive: selectedUser.value.isActive,
      customerId: normalizeCustomerId(selectedUser.value.customerId),
      roles: normalizedUserRoles,
    });
  });

  const filteredUsers = computed(() => {
    const search = userSearch.value.trim().toLowerCase();
    if (!search) {
      return users.value;
    }

    return users.value.filter((user) =>
      [user.firstName, user.lastName, user.email, user.username, user.customerId ?? '', describeLinkedCustomer(user), ...user.roles]
        .join(' ')
        .toLowerCase()
        .includes(search),
    );
  });

  const activeUserCount = computed(() => users.value.filter((user) => user.isActive).length);
  const adminUserCount = computed(() =>
    users.value.filter((user) => user.roles.some((role) => role.toLowerCase() === 'admin')).length,
  );
  const customerAccountCount = computed(() => users.value.filter((user) => Boolean(normalizeCustomerId(user.customerId))).length);

  function setOutcome(message: string | null, error: string | null = null) {
    feedback.value = message;
    errorMessage.value = error;
  }

  function normalizeCustomerId(value: string | null | undefined) {
    const normalized = value?.trim();
    return normalized ? normalized : null;
  }

  function syncCustomerRole(target: string[], customerId: string | null | undefined) {
    const normalizedCustomerId = normalizeCustomerId(customerId);
    const customerRoleIndex = target.findIndex((role) => role.toLowerCase() === customerRoleName.toLowerCase());

    if (normalizedCustomerId) {
      if (customerRoleIndex < 0 && roleNames.value.includes(customerRoleName)) {
        target.push(customerRoleName);
      }

      return;
    }

    if (customerRoleIndex >= 0) {
      target.splice(customerRoleIndex, 1);
    }
  }

  function describeLinkedCustomer(user: UserProfile) {
    const customerId = normalizeCustomerId(user.customerId);
    if (!customerId) {
      return 'No linked customer';
    }

    return customerLabels.value.get(customerId) ?? `Linked customer ${customerId}`;
  }

  function applySelectedUser(user: UserProfile | null) {
    if (!user) {
      editUserForm.id = '';
      editUserForm.username = '';
      editUserForm.email = '';
      editUserForm.firstName = '';
      editUserForm.lastName = '';
      editUserForm.isActive = true;
      editUserForm.customerId = null;
      assignedRoles.value = [];
      return;
    }

    editUserForm.id = user.id;
    editUserForm.username = user.username;
    editUserForm.email = user.email;
    editUserForm.firstName = user.firstName;
    editUserForm.lastName = user.lastName;
    editUserForm.isActive = user.isActive;
    editUserForm.customerId = normalizeCustomerId(user.customerId);
    assignedRoles.value = [...user.roles];
  }

  function resetCreateUserForm() {
    createUserForm.username = '';
    createUserForm.email = '';
    createUserForm.password = '';
    createUserForm.firstName = '';
    createUserForm.lastName = '';
    createUserForm.isActive = true;
    createUserForm.customerId = null;
    createUserForm.roles = roleNames.value.includes('User') ? ['User'] : roleNames.value.slice(0, 1);
  }

  function openUserDialog(user: UserProfile) {
    selectedUserId.value = user.id;
    isUserDialogOpen.value = true;
  }

  function openCreateUserDialog() {
    resetCreateUserForm();
    isCreateUserDialogOpen.value = true;
  }

  function toggleRoleSelection(target: string[], roleName: string) {
    const existingIndex = target.findIndex((role) => role === roleName);
    if (existingIndex >= 0) {
      target.splice(existingIndex, 1);
      return;
    }

    target.push(roleName);
  }

  async function refreshAdminData(preserveSelection = true) {
    if (!authStore.token || !canViewUsers.value) {
      users.value = [];
      roles.value = [];
      customers.value = [];
      selectedUserId.value = '';
      applySelectedUser(null);
      adminLoading.value = false;
      return;
    }

    adminLoading.value = true;
    setOutcome(null, null);

    try {
      const [userResult, roleResult, customerResult] = await Promise.all([
        identityApi.getUsers(authStore.token),
        identityApi.getRoles(authStore.token),
        canViewCustomers.value ? orderingApi.getCustomers(authStore.token) : Promise.resolve([]),
      ]);

      users.value = userResult;
      roles.value = roleResult;
      customers.value = customerResult;

      if (!preserveSelection || !userResult.some((user) => user.id === selectedUserId.value)) {
        selectedUserId.value = userResult[0]?.id ?? '';
      }

      applySelectedUser(userResult.find((user) => user.id === selectedUserId.value) ?? null);

      if (!createUserForm.roles.length && roleNames.value.length > 0) {
        resetCreateUserForm();
      }
    } catch (error) {
      setOutcome(null, error instanceof Error ? error.message : 'Unable to load user administration data.');
    } finally {
      adminLoading.value = false;
    }
  }

  async function handleCreateUser() {
    if (!authStore.token || !canCreateUsers.value) {
      setOutcome(null, 'User create permission is required to create users.');
      return;
    }

    creatingUser.value = true;
    setOutcome(null, null);

    try {
      syncCustomerRole(createUserForm.roles, createUserForm.customerId);
      const createdUser = await identityApi.createUser(authStore.token, {
        ...createUserForm,
        roles: [...createUserForm.roles],
        customerId: normalizeCustomerId(createUserForm.customerId),
      });

      resetCreateUserForm();
      isCreateUserDialogOpen.value = false;
      await refreshAdminData(false);

      if (createdUser) {
        selectedUserId.value = createdUser.id;
      }

      uiStore.pushToast('User created successfully.', 'success');
      setOutcome('User created successfully.');
    } catch (error) {
      setOutcome(null, error instanceof Error ? error.message : 'User creation failed.');
    } finally {
      creatingUser.value = false;
    }
  }

  async function handleSaveUser() {
    if (!authStore.token || !editUserForm.id || !canUpdateUsers.value) {
      setOutcome(null, 'User update permission is required to save user changes.');
      return;
    }

    savingUser.value = true;
    setOutcome(null, null);

    try {
      syncCustomerRole(assignedRoles.value, editUserForm.customerId);
      await identityApi.updateUser(authStore.token, {
        ...editUserForm,
        customerId: normalizeCustomerId(editUserForm.customerId),
      });
      await identityApi.assignRoles(authStore.token, {
        userId: editUserForm.id,
        roles: [...assignedRoles.value],
      });

      await refreshAdminData();
      isUserDialogOpen.value = false;
      uiStore.pushToast('User details saved.', 'success');
      setOutcome('User details and roles updated successfully.');
    } catch (error) {
      setOutcome(null, error instanceof Error ? error.message : 'User update failed.');
    } finally {
      savingUser.value = false;
    }
  }

  async function handleDeleteUser(user: UserProfile) {
    if (!authStore.token || !canDeleteUsers.value) {
      setOutcome(null, 'User delete permission is required to delete users.');
      return;
    }

    const confirmed = await uiStore.confirm({
      title: 'Delete user',
      message: `Delete ${user.email}? This permanently removes the identity account.`,
      confirmLabel: 'Delete user',
      tone: 'danger',
    });
    if (!confirmed) {
      return;
    }

    deletingUserId.value = user.id;
    setOutcome(null, null);

    try {
      await identityApi.deleteUser(authStore.token, user.id);
      if (selectedUserId.value === user.id) {
        selectedUserId.value = '';
        applySelectedUser(null);
        isUserDialogOpen.value = false;
      }

      await refreshAdminData(false);
      uiStore.pushToast('User deleted.', 'success');
      setOutcome('User deleted successfully.');
    } catch (error) {
      setOutcome(null, error instanceof Error ? error.message : 'User deletion failed.');
    } finally {
      deletingUserId.value = null;
    }
  }

  watch(selectedUser, (user) => {
    applySelectedUser(user);
  });

  watch(() => createUserForm.customerId, (customerId) => {
    syncCustomerRole(createUserForm.roles, customerId);
  });

  watch(() => editUserForm.customerId, (customerId) => {
    syncCustomerRole(assignedRoles.value, customerId);
  });

  watch(
    () => [authStore.token, authStore.user?.id, canViewUsers.value] as const,
    async ([token, , canView]) => {
      if (token && canView) {
        await refreshAdminData();
        return;
      }

      users.value = [];
      roles.value = [];
      customers.value = [];
      selectedUserId.value = '';
      applySelectedUser(null);
    },
    { immediate: true },
  );

  return {
    adminLoading,
    creatingUser,
    savingUser,
    deletingUserId,
    feedback,
    errorMessage,
    users,
    roleNames,
    customerOptions,
    isUserDialogOpen,
    isCreateUserDialogOpen,
    userSearch,
    createUserForm,
    editUserForm,
    assignedRoles,
    selectedUser,
    canCreateUsers,
    canUpdateUsers,
    canDeleteUsers,
    canManageUsers,
    canViewCustomers,
    hasUserChanges,
    filteredUsers,
    activeUserCount,
    adminUserCount,
    customerAccountCount,
    openUserDialog,
    openCreateUserDialog,
    toggleRoleSelection,
    refreshAdminData,
    handleCreateUser,
    handleSaveUser,
    handleDeleteUser,
    describeLinkedCustomer,
  };
}
