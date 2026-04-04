import type { ApiResponse } from '../types/contracts';

export type BackendService = 'catalog' | 'identity' | 'ordering' | 'payment';

const serviceBasePaths: Record<BackendService, string> = {
  catalog: '/backend/catalog',
  identity: '/backend/identity',
  ordering: '/backend/ordering',
  payment: '/backend/payment',
};

export class ApiError extends Error {
  public readonly status: number;
  public readonly details: string[];

  public constructor(message: string, status: number, details: string[] = []) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

interface ApiRequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  body?: unknown;
  token?: string | null;
  headers?: HeadersInit;
}

function flattenErrorBag(payload: unknown): string[] {
  if (!payload || typeof payload !== 'object') {
    return [];
  }

  const record = payload as Record<string, unknown>;
  const details = new Set<string>();

  if (typeof record.detail === 'string' && record.detail.trim()) {
    details.add(record.detail.trim());
  }

  if (Array.isArray(record.errors)) {
    for (const item of record.errors) {
      if (typeof item === 'string' && item.trim()) {
        details.add(item.trim());
      }
    }

    return [...details];
  }

  if (!record.errors || typeof record.errors !== 'object') {
    return [...details];
  }

  for (const value of Object.values(record.errors as Record<string, unknown>)) {
    if (Array.isArray(value)) {
      for (const item of value) {
        if (typeof item === 'string' && item.trim()) {
          details.add(item.trim());
        }
      }

      continue;
    }

    if (typeof value === 'string' && value.trim()) {
      details.add(value.trim());
    }
  }

  return [...details];
}

function isGenericMessage(message: string) {
  const normalized = message.trim().toLowerCase();
  return normalized === 'operation failed'
    || normalized === 'invalid request'
    || normalized === 'an error occurred while processing your request';
}

function buildErrorMessage(payload: unknown, status: number, details: string[]) {
  const record = payload as { message?: string; title?: string; detail?: string } | undefined;
  const message = record?.message ?? record?.title ?? record?.detail ?? `Request failed with status ${status}`;

  if (!details.length) {
    return message;
  }

  if (isGenericMessage(message) || message === `Request failed with status ${status}`) {
    return details.join(' ');
  }

  if (details.includes(message)) {
    return message;
  }

  return `${message}: ${details[0]}`;
}

export async function apiRequest<T>(
  service: BackendService,
  path: string,
  options: ApiRequestOptions = {},
): Promise<ApiResponse<T>> {
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');

  const hasBody = options.body !== undefined;
  if (hasBody && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (options.token) {
    headers.set('Authorization', `Bearer ${options.token}`);
  }

  const response = await fetch(`${serviceBasePaths[service]}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: hasBody ? JSON.stringify(options.body) : undefined,
  });

  const contentType = response.headers.get('content-type') ?? '';
  const payload = contentType.includes('application/json')
    ? ((await response.json()) as ApiResponse<T> | Record<string, unknown>)
    : undefined;

  if (!response.ok) {
    const details = flattenErrorBag(payload);
    const message = buildErrorMessage(payload, response.status, details);

    if (response.status === 401 && options.token) {
      window.dispatchEvent(new CustomEvent('dddsample:auth-expired'));
    }

    throw new ApiError(message, response.status, details);
  }

  if (payload) {
    return payload as ApiResponse<T>;
  }

  return {
    success: true,
    data: null,
    message: null,
    errors: null,
    timestamp: new Date().toISOString(),
  };
}
