<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import EntityDialog from '../components/ui/EntityDialog.vue';
import { identityApi } from '../api/identity';
import { appPermissions, formatPermissionAction, permissionGroups, summarizePermissions } from '../lib/permissions';
import { formatDate } from '../lib/formatters';
import { useAuthStore } from '../stores/auth';
import { useUiStore } from '../stores/ui';
import type { CreateRoleRequest, RoleModel } from '../types/contracts';

const authStore = useAuthStore();
const uiStore = useUiStore();

const roles = ref<RoleModel[]>([]);
const loading = ref(false);
const savingRole = ref(false);
const errorMessage = ref<string | null>(null);
const feedback = ref<string | null>(null);
const roleSearch = ref('');
const isRoleDialogOpen = ref(false);
const editingRoleId = ref<string | null>(null);
const initialRoleSnapshot = ref('');

const canViewRoles = computed(() => authStore.hasPermission(appPermissions.roles.view));
const canCreateRoles = computed(() => authStore.hasPermission(appPermissions.roles.create));
const canUpdateRoles = computed(() => authStore.hasPermission(appPermissions.roles.update));
const canDeleteRoles = computed(() => authStore.hasPermission(appPermissions.roles.delete));
const canManageRoles = computed(() => canCreateRoles.value || canUpdateRoles.value || canDeleteRoles.value);

const roleForm = reactive<CreateRoleRequest>({ name: '', description: '', permissions: [] });

const filteredRoles = computed(() => {
  const search = roleSearch.value.trim().toLowerCase();
  if (!search) {
    return roles.value;
  }

  return roles.value.filter((role) =>
    [role.name, role.description, ...role.permissions].join(' ').toLowerCase().includes(search),
  );
});

const selectedPermissionCount = computed(() => roleForm.permissions.length);
const roleSnapshot = computed(() =>
  JSON.stringify({
    name: roleForm.name.trim(),
    description: roleForm.description.trim(),
    permissions: [...roleForm.permissions].sort(),
  }),
);
const roleHasChanges = computed(() => roleSnapshot.value !== initialRoleSnapshot.value);
const canSaveRole = computed(() => {
  if (savingRole.value || !roleForm.name.trim()) {
    return false;
  }

  if (editingRoleId.value) {
    return canUpdateRoles.value && roleHasChanges.value;
  }

  return canCreateRoles.value;
});

function setOutcome(message: string | null, error: string | null = null) {
  feedback.value = message;
  errorMessage.value = error;
}

function updateInitialRoleSnapshot() {
  initialRoleSnapshot.value = JSON.stringify({
    name: roleForm.name.trim(),
    description: roleForm.description.trim(),
    permissions: [...roleForm.permissions].sort(),
  });
}

function resetRoleForm() {
  editingRoleId.value = null;
  roleForm.name = '';
  roleForm.description = '';
  roleForm.permissions = [];
  updateInitialRoleSnapshot();
}

function openRoleDialog(role?: RoleModel) {
  if (role) {
    editingRoleId.value = role.id;
    roleForm.name = role.name;
    roleForm.description = role.description;
    roleForm.permissions = [...role.permissions].sort();
  } else {
    resetRoleForm();
  }

  updateInitialRoleSnapshot();
  isRoleDialogOpen.value = true;
}

function togglePermission(permission: string) {
  if (roleForm.permissions.includes(permission)) {
    roleForm.permissions = roleForm.permissions.filter((item) => item !== permission);
    return;
  }

  roleForm.permissions = [...roleForm.permissions, permission].sort();
}

function permissionBadges(role: RoleModel) {
  return summarizePermissions(role.permissions);
}

async function refreshRoles() {
  if (!authStore.token || !canViewRoles.value) {
    roles.value = [];
    return;
  }

  loading.value = true;
  setOutcome(null, null);
  try {
    roles.value = await identityApi.getRoles(authStore.token);
  } catch (error) {
    setOutcome(null, error instanceof Error ? error.message : 'Unable to load roles.');
  } finally {
    loading.value = false;
  }
}

async function saveRole() {
  if (!authStore.token) {
    setOutcome(null, 'Sign in before saving roles.');
    return;
  }

  if (editingRoleId.value && !canUpdateRoles.value) {
    setOutcome(null, 'Role update permission is required to save role permissions.');
    return;
  }

  if (!editingRoleId.value && !canCreateRoles.value) {
    setOutcome(null, 'Role create permission is required to create roles.');
    return;
  }

  if (!canSaveRole.value) {
    return;
  }

  savingRole.value = true;
  setOutcome(null, null);
  try {
    if (editingRoleId.value) {
      await identityApi.updateRolePermissions(authStore.token, editingRoleId.value, {
        permissions: roleForm.permissions,
      });
      uiStore.pushToast('Role permissions updated successfully.', 'success');
      setOutcome('Role permissions updated successfully.');
    } else {
      await identityApi.createRole(authStore.token, { ...roleForm, permissions: [...roleForm.permissions] });
      uiStore.pushToast('Role created successfully.', 'success');
      setOutcome('Role created successfully.');
    }

    isRoleDialogOpen.value = false;
    await refreshRoles();
  } catch (error) {
    setOutcome(null, error instanceof Error ? error.message : 'Role save failed.');
  } finally {
    savingRole.value = false;
  }
}

onMounted(() => {
  void refreshRoles();
});
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Roles</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">Manage roles and the permissions each role carries for every admin entity.</p>
      </div>
      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="roleSearch" class="toolbar-search" placeholder="Search roles or permissions..." />
        <button class="btn-secondary" :disabled="loading" @click="refreshRoles">
          <span v-if="loading" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">&#8635;</span>
          <span>{{ loading ? 'Refreshing...' : 'Reload roles' }}</span>
        </button>
      </div>
    </div>

    <div v-if="feedback" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{{ feedback }}</div>
    <div v-if="errorMessage" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ errorMessage }}</div>
    <div v-if="!canViewRoles" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">The current account does not have permission to view roles.</div>
    <div v-else-if="!canManageRoles" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">The current account can review roles, but create and update actions require separate permissions.</div>

    <article class="section-panel">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h4 class="section-title">Role definitions</h4>
          <p class="mt-2 text-sm text-slate-600">{{ filteredRoles.length }} of {{ roles.length }} role definition(s).</p>
        </div>
        <button class="btn-primary" :disabled="!canCreateRoles" @click="openRoleDialog()">
          <span class="button-icon" aria-hidden="true">+</span>
          <span>New role</span>
        </button>
      </div>

      <div class="mt-5 table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-5 py-4">Role</th>
              <th class="px-5 py-4">Description</th>
              <th class="px-5 py-4">Permissions</th>
              <th class="px-5 py-4">Created</th>
              <th class="px-5 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="role in filteredRoles" :key="role.id">
              <td class="px-5 py-4 font-medium text-slate-900">{{ role.name }}</td>
              <td class="px-5 py-4 text-slate-600">{{ role.description || 'No description provided.' }}</td>
              <td class="px-5 py-4 text-slate-600">
                <div class="flex flex-wrap gap-2">
                  <span v-for="permission in permissionBadges(role)" :key="role.id + '-' + permission" class="permission-badge">
                    {{ permission }}
                  </span>
                </div>
              </td>
              <td class="px-5 py-4 text-slate-600">{{ formatDate(role.createdAt) }}</td>
              <td class="px-5 py-4">
                <button class="icon-button" :disabled="!canUpdateRoles" title="Edit permissions" aria-label="Edit role permissions" @click="openRoleDialog(role)">
                  <span class="icon-glyph">&#9998;</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!filteredRoles.length && !loading" class="px-5 py-4 text-sm text-slate-500">{{ roles.length ? 'No roles match the current search.' : 'No roles were returned by the Identity API.' }}</p>
      </div>
    </article>

    <EntityDialog :open="isRoleDialogOpen" :title="editingRoleId ? 'Edit role permissions' : 'New role'" description="Assign permissions to control exactly what the role can do on each entity." width-class="max-w-4xl" @close="isRoleDialogOpen = false">
      <div class="grid gap-6">
        <div class="grid gap-4 md:grid-cols-2">
          <label>
            <span class="field-label">Role name</span>
            <input v-model="roleForm.name" class="text-input" placeholder="Manager" :disabled="Boolean(editingRoleId)" />
          </label>
          <label>
            <span class="field-label">Description</span>
            <input v-model="roleForm.description" class="text-input" placeholder="Can manage daily operations" :disabled="Boolean(editingRoleId)" />
          </label>
        </div>

        <div class="rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
          <span class="field-label">Selected permissions</span>
          <p class="mt-2 font-medium text-slate-900">{{ selectedPermissionCount }} permission(s) selected</p>
        </div>

        <div class="grid gap-3">
          <div v-for="group in permissionGroups" :key="group.label" class="subtle-panel">
            <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <div>
                <p class="font-semibold text-slate-900">{{ group.label }}</p>
                <p class="mt-1 text-sm text-slate-500">Choose the actions available for this entity.</p>
              </div>
              <div class="flex flex-wrap gap-2">
                <button
                  v-for="permission in group.permissions"
                  :key="permission"
                  class="permission-toggle"
                  :class="roleForm.permissions.includes(permission) ? 'permission-toggle-active' : ''"
                  type="button"
                  @click="togglePermission(permission)"
                >
                  {{ formatPermissionAction(permission) }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <button class="btn-primary" :disabled="!canSaveRole" @click="saveRole">
          <span v-if="savingRole" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">&#10003;</span>
          <span>{{ savingRole ? 'Saving role...' : editingRoleId ? 'Save permissions' : 'Create role' }}</span>
        </button>
      </div>
    </EntityDialog>
  </section>
</template>


