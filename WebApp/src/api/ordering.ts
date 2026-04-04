import { apiRequest } from '../lib/http';
import type {
  CreateCustomerRequest,
  CreateOrderRequest,
  CustomerModel,
  OrderModel,
  ProductCacheModel,
  UpdateCustomerRequest,
  UpdateOrderRequest,
} from '../types/contracts';

export const orderingApi = {
  async getCustomers(token: string, search?: string): Promise<CustomerModel[]> {
    const query = search ? `?search=${encodeURIComponent(search)}` : '';
    const response = await apiRequest<CustomerModel[]>('ordering', '/api/Customers' + query, { token });
    return response.data ?? [];
  },

  async createCustomer(token: string, request: CreateCustomerRequest): Promise<CustomerModel | null> {
    const response = await apiRequest<CustomerModel>('ordering', '/api/Customers', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateCustomer(token: string, request: UpdateCustomerRequest): Promise<CustomerModel | null> {
    const response = await apiRequest<CustomerModel>('ordering', '/api/Customers/' + request.id, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async deleteCustomer(token: string, customerId: string): Promise<void> {
    await apiRequest<null>('ordering', '/api/Customers/' + customerId, {
      method: 'DELETE',
      token,
    });
  },

  async getOrders(token?: string | null, customerId?: string): Promise<OrderModel[]> {
    const query = customerId ? `?customerId=${encodeURIComponent(customerId)}` : '';
    const response = await apiRequest<OrderModel[]>('ordering', '/api/Orders' + query, { token });
    return response.data ?? [];
  },

  async createOrder(token: string, request: CreateOrderRequest): Promise<OrderModel | null> {
    const response = await apiRequest<OrderModel>('ordering', '/api/Orders', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateOrder(token: string, request: UpdateOrderRequest): Promise<OrderModel | null> {
    const response = await apiRequest<OrderModel>('ordering', '/api/Orders/' + request.id, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async payOrder(token: string, orderId: string): Promise<boolean> {
    const response = await apiRequest<boolean>('ordering', '/api/Orders/' + orderId + '/pay', {
      method: 'POST',
      token,
    });

    return response.data ?? false;
  },

  async cancelOrder(token: string, orderId: string): Promise<boolean> {
    const response = await apiRequest<boolean>('ordering', '/api/Orders/' + orderId + '/cancel', {
      method: 'POST',
      token,
    });

    return response.data ?? false;
  },

  async deleteOrder(token: string, orderId: string): Promise<void> {
    await apiRequest<null>('ordering', '/api/Orders/' + orderId, {
      method: 'DELETE',
      token,
    });
  },

  async getProductsInOrder(orderId: string, token?: string | null): Promise<ProductCacheModel[]> {
    const response = await apiRequest<ProductCacheModel[]>('ordering', '/api/ProductsCache/orders/' + orderId, { token });
    return response.data ?? [];
  },
};
