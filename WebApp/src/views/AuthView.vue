<script setup lang="ts">
import EntityDialog from '../components/ui/EntityDialog.vue';
import { formatDate } from '../lib/formatters';
import { useUsersAdmin } from '../composables/useUsersAdmin';

const {
  adminLoading,
  creatingUser,
  savingUser,
  deletingUserId,
  feedback,
  errorMessage,
  users,
  roleNames,
  isUserDialogOpen,
  isCreateUserDialogOpen,
  userSearch,
  createUserForm,
  editUserForm,
  assignedRoles,
  selectedUser,
  canManageUsers,
  hasUserChanges,
  filteredUsers,
  activeUserCount,
  adminUserCount,
  openUserDialog,
  openCreateUserDialog,
  toggleRoleSelection,
  refreshAdminData,
  handleCreateUser,
  handleSaveUser,
  handleDeleteUser,
} = useUsersAdmin();
</script>

<template>
  <section class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-end md:justify-between">
      <div>
        <h3 class="section-title">Users</h3>
        <p class="mt-2 text-sm leading-6 text-slate-600">
          Manage operator accounts, activation state, and role assignments from one production-focused workspace.
        </p>
      </div>

      <div class="flex w-full flex-wrap gap-3 md:w-auto md:justify-end">
        <input v-model="userSearch" class="toolbar-search" placeholder="Search users by name, email, username, role..." />
        <button class="btn-secondary" :disabled="adminLoading" @click="refreshAdminData()">
          <span v-if="adminLoading" class="button-spinner" aria-hidden="true" />
          <span v-else class="button-icon" aria-hidden="true">↻</span>
          <span>{{ adminLoading ? 'Refreshing...' : 'Reload users' }}</span>
        </button>
      </div>
    </div>

    <div v-if="feedback" class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
      {{ feedback }}
    </div>
    <div v-if="errorMessage" class="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ errorMessage }}
    </div>
    <div v-if="!canManageUsers" class="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      The current account does not have permission to manage users.
    </div>

    <div class="flex flex-wrap gap-3">
      <article class="summary-tile min-w-[180px] flex-1"><p class="workspace-label">Users</p><p class="mt-3 display-value">{{ users.length }}</p></article>
      <article class="summary-tile min-w-[180px] flex-1"><p class="workspace-label">Active users</p><p class="mt-3 display-value">{{ activeUserCount }}</p></article>
      <article class="summary-tile min-w-[180px] flex-1"><p class="workspace-label">Admins</p><p class="mt-3 display-value">{{ adminUserCount }}</p></article>
    </div>

    <article class="section-panel">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h4 class="section-title">User directory</h4>
          <p class="mt-2 text-sm text-slate-600">{{ filteredUsers.length }} of {{ users.length }} user account(s).</p>
        </div>
        <button class="btn-primary" :disabled="!canManageUsers" @click="openCreateUserDialog"><span class="button-icon" aria-hidden="true">＋</span><span>New user</span></button>
      </div>

      <div class="mt-5 table-shell">
        <table class="data-table">
          <thead>
            <tr>
              <th class="px-5 py-4">User</th>
              <th class="px-5 py-4">Username</th>
              <th class="px-5 py-4">Roles</th>
              <th class="px-5 py-4">Status</th>
              <th class="px-5 py-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id">
              <td class="px-5 py-4">
                <button class="min-w-0 text-left" @click="openUserDialog(user)">
                  <p class="font-semibold text-slate-900">{{ user.firstName }} {{ user.lastName }}</p>
                  <p class="mt-1 text-sm text-slate-600">{{ user.email }}</p>
                </button>
              </td>
              <td class="px-5 py-4 align-top text-slate-700">{{ user.username }}</td>
              <td class="px-5 py-4 align-top">
                <div class="flex flex-wrap gap-2">
                  <span v-for="roleName in user.roles" :key="`${user.id}-${roleName}`" class="rounded-full bg-white px-3 py-1 text-xs font-medium text-slate-700">
                    {{ roleName }}
                  </span>
                </div>
              </td>
              <td class="px-5 py-4 align-top text-slate-700">{{ user.isActive ? 'Active' : 'Inactive' }}</td>
              <td class="px-5 py-4 align-top">
                <div class="flex flex-wrap gap-2">
                  <button class="icon-button" title="Details" aria-label="View user details" @click="openUserDialog(user)"><span class="icon-glyph">👁</span></button>
                  <button class="icon-button icon-button-danger" title="Delete" aria-label="Delete user" :disabled="deletingUserId === user.id || !canManageUsers" @click="handleDeleteUser(user)"><span v-if="deletingUserId === user.id" class="button-spinner" aria-hidden="true" /><span v-else class="icon-glyph">🗑</span></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <p v-if="!filteredUsers.length && !adminLoading" class="px-5 py-4 text-sm text-slate-500">
          {{ users.length ? 'No users match the current search.' : 'No users were returned by the Identity API.' }}
        </p>
      </div>
    </article>

    <EntityDialog :open="isUserDialogOpen" title="User details" description="Update user profile fields, status, and role assignment." width-class="max-w-4xl" @close="isUserDialogOpen = false">
      <div v-if="selectedUser" class="grid gap-4">
        <div class="grid gap-4 md:grid-cols-2">
          <label><span class="field-label">Username</span><input v-model="editUserForm.username" class="text-input" /></label>
          <label><span class="field-label">Email</span><input v-model="editUserForm.email" class="text-input" type="email" /></label>
          <label><span class="field-label">First name</span><input v-model="editUserForm.firstName" class="text-input" /></label>
          <label><span class="field-label">Last name</span><input v-model="editUserForm.lastName" class="text-input" /></label>
        </div>

        <div class="grid gap-4 md:grid-cols-3">
          <div class="subtle-panel"><p class="workspace-label">Created</p><p class="mt-2 text-sm font-medium text-slate-900">{{ formatDate(selectedUser.createdAt) }}</p></div>
          <div class="subtle-panel"><p class="workspace-label">Last updated</p><p class="mt-2 text-sm font-medium text-slate-900">{{ formatDate(selectedUser.updatedAt) }}</p></div>
          <label class="flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700"><input v-model="editUserForm.isActive" type="checkbox" />Keep account active</label>
        </div>

        <div>
          <span class="field-label">Assigned roles</span>
          <div class="grid gap-3 md:grid-cols-2">
            <label v-for="roleName in roleNames" :key="`edit-${roleName}`" class="flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
              <input :checked="assignedRoles.includes(roleName)" type="checkbox" @change="toggleRoleSelection(assignedRoles, roleName)" />
              <span>{{ roleName }}</span>
            </label>
          </div>
        </div>

        <div class="flex flex-wrap gap-2">
          <button class="btn-primary" :disabled="savingUser || !assignedRoles.length || !canManageUsers || !hasUserChanges" @click="handleSaveUser"><span v-if="savingUser" class="button-spinner" aria-hidden="true" /><span v-else class="button-icon" aria-hidden="true">💾</span><span>{{ savingUser ? 'Saving changes...' : 'Save user changes' }}</span></button>
          <button class="btn-danger" :disabled="deletingUserId === selectedUser.id || !canManageUsers" @click="handleDeleteUser(selectedUser)"><span v-if="deletingUserId === selectedUser.id" class="button-spinner" aria-hidden="true" /><span v-else class="button-icon" aria-hidden="true">🗑</span><span>{{ deletingUserId === selectedUser.id ? 'Deleting...' : 'Delete user' }}</span></button>
        </div>
      </div>
    </EntityDialog>

    <EntityDialog :open="isCreateUserDialogOpen" title="New user" description="Create an operator account and assign initial roles." width-class="max-w-4xl" @close="isCreateUserDialogOpen = false">
      <div class="grid gap-4 md:grid-cols-2">
        <label><span class="field-label">Username</span><input v-model="createUserForm.username" class="text-input" /></label>
        <label><span class="field-label">Email</span><input v-model="createUserForm.email" class="text-input" type="email" /></label>
        <label><span class="field-label">First name</span><input v-model="createUserForm.firstName" class="text-input" /></label>
        <label><span class="field-label">Last name</span><input v-model="createUserForm.lastName" class="text-input" /></label>
        <label class="md:col-span-2"><span class="field-label">Temporary password</span><input v-model="createUserForm.password" class="text-input" type="password" autocomplete="new-password" /></label>
        <label class="md:col-span-2 flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700"><input v-model="createUserForm.isActive" type="checkbox" />Create account as active</label>
        <div class="md:col-span-2">
          <span class="field-label">Initial roles</span>
          <div class="grid gap-3 md:grid-cols-2">
            <label v-for="roleName in roleNames" :key="`create-${roleName}`" class="flex items-center gap-3 rounded-2xl bg-[var(--color-surface-low)] px-4 py-3 text-sm text-slate-700">
              <input :checked="createUserForm.roles.includes(roleName)" type="checkbox" @change="toggleRoleSelection(createUserForm.roles, roleName)" />
              <span>{{ roleName }}</span>
            </label>
          </div>
        </div>
        <button class="btn-primary md:col-span-2" :disabled="creatingUser || !canManageUsers" @click="handleCreateUser"><span v-if="creatingUser" class="button-spinner" aria-hidden="true" /><span v-else class="button-icon" aria-hidden="true">＋</span><span>{{ creatingUser ? 'Creating user...' : 'Create user' }}</span></button>
      </div>
    </EntityDialog>
  </section>
</template>



