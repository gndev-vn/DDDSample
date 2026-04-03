import { apiRequest } from '../lib/http';
import type { CompletePaymentRequest, FailPaymentRequest, PaymentModel } from '../types/contracts';

export const paymentApi = {
  async getPayments(token?: string | null): Promise<PaymentModel[]> {
    const response = await apiRequest<PaymentModel[]>('payment', '/api/Payments', { token });
    return response.data ?? [];
  },

  async getPaymentByOrderId(orderId: string, token?: string | null): Promise<PaymentModel | null> {
    const response = await apiRequest<PaymentModel>('payment', `/api/Payments/orders/${orderId}`, { token });
    return response.data ?? null;
  },

  async completePayment(token: string, paymentId: string, request: CompletePaymentRequest): Promise<PaymentModel | null> {
    const response = await apiRequest<PaymentModel>('payment', `/api/Payments/${paymentId}/complete`, {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async failPayment(token: string, paymentId: string, request: FailPaymentRequest): Promise<PaymentModel | null> {
    const response = await apiRequest<PaymentModel>('payment', `/api/Payments/${paymentId}/fail`, {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },
};
