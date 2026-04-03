const orderStatuses = ['Submitted', 'Paid', 'Packed', 'Shipped', 'Completed', 'Cancelled'];
const paymentStatuses = ['Unknown', 'Pending', 'Completed', 'Failed'];

export function formatCurrency(amount: number, currency: string): string {
  try {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
      maximumFractionDigits: currency === 'VND' ? 0 : 2,
    }).format(amount);
  } catch {
    return `${amount.toFixed(2)} ${currency}`;
  }
}

export function formatDate(value?: string | null): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

export function orderStatusLabel(status: number): string {
  return orderStatuses[status] ?? `Unknown (${status})`;
}

export function paymentStatusLabel(status: number): string {
  return paymentStatuses[status] ?? `Unknown (${status})`;
}

export function toneForStatus(status: number, type: 'order' | 'payment'): 'neutral' | 'success' | 'warning' | 'danger' {
  if (type === 'order') {
    if (status === 1 || status === 4) {
      return 'success';
    }

    if (status === 5) {
      return 'danger';
    }

    return 'warning';
  }

  if (status === 2) {
    return 'success';
  }

  if (status === 3) {
    return 'danger';
  }

  return 'warning';
}
