export const appPermissions = {
  roles: {
    view: 'identity.roles.view',
    manage: 'identity.roles.manage',
  },
  users: {
    view: 'identity.users.view',
    manage: 'identity.users.manage',
  },
  categories: {
    view: 'catalog.categories.view',
    manage: 'catalog.categories.manage',
  },
  products: {
    view: 'catalog.products.view',
    manage: 'catalog.products.manage',
  },
  variants: {
    view: 'catalog.variants.view',
    manage: 'catalog.variants.manage',
  },
  orders: {
    view: 'ordering.orders.view',
    manage: 'ordering.orders.manage',
  },
  payments: {
    view: 'payment.payments.view',
    manage: 'payment.payments.manage',
  },
} as const;

export const permissionGroups = [
  { label: 'Roles', permissions: [appPermissions.roles.view, appPermissions.roles.manage] },
  { label: 'Users', permissions: [appPermissions.users.view, appPermissions.users.manage] },
  { label: 'Categories', permissions: [appPermissions.categories.view, appPermissions.categories.manage] },
  { label: 'Products', permissions: [appPermissions.products.view, appPermissions.products.manage] },
  { label: 'Variants', permissions: [appPermissions.variants.view, appPermissions.variants.manage] },
  { label: 'Orders', permissions: [appPermissions.orders.view, appPermissions.orders.manage] },
  { label: 'Payments', permissions: [appPermissions.payments.view, appPermissions.payments.manage] },
] as const;

const permissionActionOrder = ['view', 'manage'] as const;

export function formatPermission(permission: string) {
  const parts = permission.split('.');
  return parts.map((part) => part.charAt(0).toUpperCase() + part.slice(1)).join(' / ');
}

export function formatPermissionAction(permission: string) {
  const action = permission.split('.').at(-1) ?? permission;
  return action.charAt(0).toUpperCase() + action.slice(1);
}

export function summarizePermissions(permissions: string[]) {
  const actionsByEntity = new Map<string, Set<string>>();

  for (const permission of permissions) {
    const parts = permission.split('.');
    if (parts.length < 3) {
      continue;
    }

    const entity = parts[1];
    const action = parts[2];
    const currentActions = actionsByEntity.get(entity) ?? new Set<string>();
    currentActions.add(action);
    actionsByEntity.set(entity, currentActions);
  }

  return permissionGroups
    .map((group) => {
      const entityKey = group.permissions[0].split('.')[1];
      const actions = actionsByEntity.get(entityKey);
      if (!actions?.size) {
        return null;
      }

      const orderedActions = permissionActionOrder
        .filter((action) => actions.has(action))
        .map((action) => action.charAt(0).toUpperCase() + action.slice(1));

      return `${group.label}/${orderedActions.join('+')}`;
    })
    .filter((item): item is string => item !== null);
}
