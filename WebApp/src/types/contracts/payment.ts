export interface PaymentModel {
  id: string;
  orderId: string;
  amount: number;
  currency: string;
  status: number;
  transactionReference?: string | null;
  failureReason?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  completedAtUtc?: string | null;
}

export interface CompletePaymentRequest {
  transactionReference: string;
}

export interface FailPaymentRequest {
  reason: string;
}
