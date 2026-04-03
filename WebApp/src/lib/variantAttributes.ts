import type {
  ProductAttributeDefinitionModel,
  ProductVariantAttributeModel,
  ProductVariantAttributeValueRequest,
} from '../types/contracts';

export function cloneVariantAttributeValues(
  attributes: readonly ProductVariantAttributeValueRequest[],
): ProductVariantAttributeValueRequest[] {
  return attributes.map((attribute) => ({
    attributeId: attribute.attributeId,
    value: attribute.value,
  }));
}

export function mapVariantAttributesToValues(
  attributes: readonly ProductVariantAttributeModel[],
): ProductVariantAttributeValueRequest[] {
  return attributes.map((attribute) => ({
    attributeId: attribute.attributeId,
    value: attribute.value,
  }));
}

export function normalizeVariantAttributeValues(
  attributes: readonly ProductVariantAttributeValueRequest[],
): ProductVariantAttributeValueRequest[] {
  return attributes
    .map((attribute) => ({
      attributeId: attribute.attributeId,
      value: attribute.value.trim(),
    }))
    .sort((left, right) => left.attributeId.localeCompare(right.attributeId));
}

export function summarizeVariantAttributes(
  attributes: readonly ProductVariantAttributeModel[],
  limit = 3,
): string {
  const items = attributes.map((attribute) => `${attribute.name}: ${attribute.value}`);
  if (items.length <= limit) {
    return items.join(' • ');
  }

  return `${items.slice(0, limit).join(' • ')} +${items.length - limit}`;
}

export function resolveAttributeName(
  attributeId: string,
  definitions: readonly ProductAttributeDefinitionModel[],
): string {
  return definitions.find((definition) => definition.id === attributeId)?.name ?? 'Select attribute';
}
