export const appPermissions = {
  roles: {
    view: 'identity.roles.view',
    create: 'identity.roles.create',
    update: 'identity.roles.update',
    delete: 'identity.roles.delete',
  },
  users: {
    view: 'identity.users.view',
    create: 'identity.users.create',
    update: 'identity.users.update',
    delete: 'identity.users.delete',
  },
  categories: {
    view: 'catalog.categories.view',
    create: 'catalog.categories.create',
    update: 'catalog.categories.update',
    delete: 'catalog.categories.delete',
  },
  products: {
    view: 'catalog.products.view',
    create: 'catalog.products.create',
    update: 'catalog.products.update',
    delete: 'catalog.products.delete',
  },
  variants: {
    view: 'catalog.variants.view',
    create: 'catalog.variants.create',
    update: 'catalog.variants.update',
    delete: 'catalog.variants.delete',
  },
  customers: {
    view: 'ordering.customers.view',
    create: 'ordering.customers.create',
    update: 'ordering.customers.update',
    delete: 'ordering.customers.delete',
  },
  orders: {
    view: 'ordering.orders.view',
    create: 'ordering.orders.create',
    update: 'ordering.orders.update',
    delete: 'ordering.orders.delete',
  },
  payments: {
    view: 'payment.payments.view',
    create: 'payment.payments.create',
    update: 'payment.payments.update',
    delete: 'payment.payments.delete',
  },
} as const;

export const permissionGroups = [
  { label: 'Roles', permissions: [appPermissions.roles.view, appPermissions.roles.create, appPermissions.roles.update] },
  { label: 'Users', permissions: [appPermissions.users.view, appPermissions.users.create, appPermissions.users.update, appPermissions.users.delete] },
  { label: 'Categories', permissions: [appPermissions.categories.view, appPermissions.categories.create, appPermissions.categories.update, appPermissions.categories.delete] },
  { label: 'Products', permissions: [appPermissions.products.view, appPermissions.products.create, appPermissions.products.update, appPermissions.products.delete] },
  { label: 'Variants', permissions: [appPermissions.variants.view, appPermissions.variants.create, appPermissions.variants.update, appPermissions.variants.delete] },
  { label: 'Customers', permissions: [appPermissions.customers.view, appPermissions.customers.create, appPermissions.customers.update, appPermissions.customers.delete] },
  { label: 'Orders', permissions: [appPermissions.orders.view, appPermissions.orders.create, appPermissions.orders.update, appPermissions.orders.delete] },
  { label: 'Payments', permissions: [appPermissions.payments.view, appPermissions.payments.update] },
] as const;

const permissionActionOrder = ['view', 'create', 'update', 'delete'] as const;

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
