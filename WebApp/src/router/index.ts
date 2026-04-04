import { createRouter, createWebHistory } from 'vue-router';
import { useUiStore } from '../stores/ui';
import AuthView from '../views/AuthView.vue';
import CategoriesView from '../views/CategoriesView.vue';
import CustomersView from '../views/CustomersView.vue';
import DashboardView from '../views/DashboardView.vue';
import OrdersView from '../views/OrdersView.vue';
import PaymentsView from '../views/PaymentsView.vue';
import ProductsView from '../views/ProductsView.vue';
import RolesView from '../views/RolesView.vue';
import VariantsView from '../views/VariantsView.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: { name: 'overview' } },
    { path: '/overview', name: 'overview', component: DashboardView, meta: { title: 'Overview' } },
    { path: '/users', name: 'users', component: AuthView, meta: { title: 'Users' } },
    { path: '/roles', name: 'roles', component: RolesView, meta: { title: 'Roles' } },
    { path: '/categories', name: 'categories', component: CategoriesView, meta: { title: 'Categories' } },
    { path: '/products', name: 'products', component: ProductsView, meta: { title: 'Products' } },
    { path: '/variants', name: 'variants', component: VariantsView, meta: { title: 'Product variants' } },
    { path: '/catalog', redirect: { name: 'products' } },
    { path: '/identity', redirect: { name: 'users' } },
    { path: '/customers', name: 'customers', component: CustomersView, meta: { title: 'Customers' } },
    { path: '/orders', name: 'orders', component: OrdersView, meta: { title: 'Orders' } },
    { path: '/payments', name: 'payments', component: PaymentsView, meta: { title: 'Payments' } },
  ],
});

router.beforeEach((to, from, next) => {
  if (to.fullPath !== from.fullPath) {
    useUiStore().startPageLoading();
  }

  next();
});

router.afterEach((to) => {
  const title = typeof to.meta.title === 'string' ? to.meta.title : 'DDDSample';
  document.title = `${title} · DDDSample Admin`;

  window.setTimeout(() => {
    useUiStore().finishPageLoading();
  }, 150);
});

router.onError(() => {
  useUiStore().finishPageLoading();
});

export default router;
