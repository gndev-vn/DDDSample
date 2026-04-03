import { apiRequest } from '../lib/http';
import type { CreateOrderRequest, OrderModel, ProductCacheModel, UpdateOrderRequest } from '../types/contracts';

export const orderingApi = {
  async getOrders(token?: string | null): Promise<OrderModel[]> {
    const response = await apiRequest<OrderModel[]>('ordering', '/api/Orders', { token });
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
    const response = await apiRequest<OrderModel>('ordering', `/api/Orders/${request.id}`, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async payOrder(token: string, orderId: string): Promise<boolean> {
    const response = await apiRequest<boolean>('ordering', `/api/Orders/${orderId}/pay`, {
      method: 'POST',
      token,
    });

    return response.data ?? false;
  },

  async cancelOrder(token: string, orderId: string): Promise<boolean> {
    const response = await apiRequest<boolean>('ordering', `/api/Orders/${orderId}/cancel`, {
      method: 'POST',
      token,
    });

    return response.data ?? false;
  },

  async deleteOrder(token: string, orderId: string): Promise<void> {
    await apiRequest<null>('ordering', `/api/Orders/${orderId}`, {
      method: 'DELETE',
      token,
    });
  },

  async getProductsInOrder(orderId: string, token?: string | null): Promise<ProductCacheModel[]> {
    const response = await apiRequest<ProductCacheModel[]>(
      'ordering',
      `/api/ProductsCache/orders/${orderId}`,
      { token },
    );

    return response.data ?? [];
  },
};
