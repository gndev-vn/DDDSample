export interface CategoryModel {
  id: string;
  isActive: boolean;
  slug: string;
  name: string;
  description: string;
  parentId?: string | null;
}

export interface ProductResponse {
  id: string;
  categoryId: string;
  basePrice: number;
  currency: string;
  isActive: boolean;
  imageUrl?: string | null;
  slug: string;
  name: string;
  description: string;
}

export interface ProductAttributeDefinitionModel {
  id: string;
  name: string;
  usageCount: number;
}

export interface ProductVariantAttributeModel {
  attributeId: string;
  name: string;
  value: string;
}

export interface ProductVariantAttributeValueRequest {
  attributeId: string;
  value: string;
}

export interface ProductVariantResponse {
  id: string;
  parentId: string;
  name: string;
  sku: string;
  description: string;
  currency: string;
  overridePrice: number | null;
  isActive: boolean;
  attributes: ProductVariantAttributeModel[];
}

export interface CategoryCreateRequest {
  name: string;
  slug: string;
  description: string;
  parentId?: string | null;
}

export interface CategoryUpdateRequest {
  id: string;
  name: string;
  slug: string;
  description: string;
  parentId: string;
  isActive?: boolean | null;
}

export interface ProductCreateRequest {
  name: string;
  slug: string;
  description: string;
  categoryId: string;
  basePrice: number;
  currency: string;
}

export interface ProductUpdateRequest {
  id: string;
  name: string;
  slug: string;
  description: string;
  categoryId: string;
  basePrice: number;
  currency: string;
}

export interface ProductAttributeCreateRequest {
  name: string;
}

export interface ProductVariantCreateRequest {
  name: string;
  sku: string;
  description: string;
  parentId: string;
  overridePrice: number | null;
  currency: string;
  attributes: ProductVariantAttributeValueRequest[];
}

export interface ProductVariantUpdateRequest {
  id: string;
  name: string;
  sku: string;
  description: string;
  parentId: string;
  overridePrice: number | null;
  currency: string;
  attributes: ProductVariantAttributeValueRequest[];
}
