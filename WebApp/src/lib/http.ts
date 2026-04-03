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
  if (Array.isArray(record.errors)) {
    return record.errors.filter((item): item is string => typeof item === 'string');
  }

  if (!record.errors || typeof record.errors !== 'object') {
    return [];
  }

  return Object.values(record.errors as Record<string, unknown>).flatMap((value) => {
    if (Array.isArray(value)) {
      return value.filter((item): item is string => typeof item === 'string');
    }

    return typeof value === 'string' ? [value] : [];
  });
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
    const message =
      (payload as { message?: string; title?: string } | undefined)?.message ??
      (payload as { title?: string } | undefined)?.title ??
      `Request failed with status ${response.status}`;

    if (response.status === 401 && options.token) {
      window.dispatchEvent(new CustomEvent('dddsample:auth-expired'));
    }

    throw new ApiError(message, response.status, flattenErrorBag(payload));
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
