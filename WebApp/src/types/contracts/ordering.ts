export interface AddressInput {
  line1: string;
  line2?: string | null;
  city: string;
  province: string;
  district: string;
  ward: string;
}

export interface CustomerModel {
  id: string;
  displayName: string;
  email: string;
  phoneNumber?: string | null;
  address?: string | null;
  isActive: boolean;
  orderCount: number;
}

export interface CreateCustomerRequest {
  displayName: string;
  email: string;
  phoneNumber?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface UpdateCustomerRequest {
  id: string;
  displayName: string;
  email: string;
  phoneNumber?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface OrderLineInput {
  productId: string;
  name: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

export interface CreateOrderRequest {
  customerId: string;
  shippingAddress: AddressInput;
  billingAddress?: AddressInput | null;
  lines: OrderLineInput[];
}

export interface OrderLineModel {
  id: string;
  productId: string;
  name: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

export interface OrderModel {
  id: string;
  status: number;
  shippingAddress?: AddressInput | null;
  lines: OrderLineModel[];
  customerId: string;
  customerName: string;
  customerEmail: string;
  customerPhone?: string | null;
}

export interface ProductCacheModel {
  id: string;
  sku: string;
  name: string;
  currentPrice: number;
  currency: string;
  lastUpdatedUtc: string;
  imageUrl: string;
  isActive: boolean;
}

export interface UpdateOrderRequest {
  id: string;
  customerId: string;
  shippingAddress: AddressInput;
  lines: OrderLineModel[];
}
