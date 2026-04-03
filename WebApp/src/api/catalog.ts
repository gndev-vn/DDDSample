import { apiRequest } from '../lib/http';
import type {
  CategoryCreateRequest,
  CategoryModel,
  CategoryUpdateRequest,
  ProductAttributeCreateRequest,
  ProductAttributeDefinitionModel,
  ProductCreateRequest,
  ProductResponse,
  ProductUpdateRequest,
  ProductVariantCreateRequest,
  ProductVariantResponse,
  ProductVariantUpdateRequest,
} from '../types/contracts';

export const catalogApi = {
  async getCategories(token?: string | null): Promise<CategoryModel[]> {
    const response = await apiRequest<CategoryModel[]>('catalog', '/api/Categories', { token });
    return response.data ?? [];
  },

  async getProducts(token?: string | null): Promise<ProductResponse[]> {
    const response = await apiRequest<ProductResponse[]>('catalog', '/api/Products', { token });
    return response.data ?? [];
  },

  async getProductVariants(token?: string | null): Promise<ProductVariantResponse[]> {
    const response = await apiRequest<ProductVariantResponse[]>('catalog', '/api/ProductVariants', { token });
    return response.data ?? [];
  },

  async getProductAttributes(token?: string | null): Promise<ProductAttributeDefinitionModel[]> {
    const response = await apiRequest<ProductAttributeDefinitionModel[]>('catalog', '/api/ProductAttributes', { token });
    return response.data ?? [];
  },

  async createCategory(token: string, request: CategoryCreateRequest): Promise<CategoryModel | null> {
    const response = await apiRequest<CategoryModel>('catalog', '/api/Categories', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateCategory(token: string, request: CategoryUpdateRequest): Promise<CategoryModel | null> {
    const response = await apiRequest<CategoryModel>('catalog', `/api/Categories/${request.id}`, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async deleteCategory(token: string, categoryId: string): Promise<void> {
    await apiRequest<null>('catalog', `/api/Categories/${categoryId}`, {
      method: 'DELETE',
      token,
    });
  },

  async createProduct(token: string, request: ProductCreateRequest): Promise<ProductResponse | null> {
    const response = await apiRequest<ProductResponse>('catalog', '/api/Products', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateProduct(token: string, request: ProductUpdateRequest): Promise<ProductResponse | null> {
    const response = await apiRequest<ProductResponse>('catalog', `/api/Products/${request.id}`, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async deleteProduct(token: string, productId: string): Promise<void> {
    await apiRequest<null>('catalog', `/api/Products/${productId}`, {
      method: 'DELETE',
      token,
    });
  },

  async createProductAttribute(token: string, request: ProductAttributeCreateRequest): Promise<ProductAttributeDefinitionModel | null> {
    const response = await apiRequest<ProductAttributeDefinitionModel>('catalog', '/api/ProductAttributes', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async createProductVariant(token: string, request: ProductVariantCreateRequest): Promise<ProductVariantResponse | null> {
    const response = await apiRequest<ProductVariantResponse>('catalog', '/api/ProductVariants', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateProductVariant(token: string, request: ProductVariantUpdateRequest): Promise<ProductVariantResponse | null> {
    const response = await apiRequest<ProductVariantResponse>('catalog', `/api/ProductVariants/${request.id}`, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async deleteProductVariant(token: string, variantId: string): Promise<void> {
    await apiRequest<null>('catalog', `/api/ProductVariants/${variantId}`, {
      method: 'DELETE',
      token,
    });
  },
};
