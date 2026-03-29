# CatalogAPI business logic

## Purpose

`CatalogAPI` manages the catalog domain for categories, products, and product variants. It exposes REST, gRPC, and GraphQL read/write surfaces, but the business behavior is centered on the domain entities and Mediator command/query handlers.

## Domain model

### Category

A category represents a navigable grouping for products.

Business rules:

- `Name` is required.
- `Slug` is required.
- `Description` is optional and normalized to an empty string when omitted.
- A category may have a parent category.
- A category cannot be its own parent.
- Category hierarchies cannot contain cycles.
- A category cannot be deleted while child categories still exist.
- A category cannot be deleted while products still reference it.

Domain behaviors:

- Creating a category raises `CategoryCreatedDomainEvent`.
- Updating category details raises `CategoryUpdatedDomainEvent` with the current state payload.
- Deleting a category raises `CategoryDeletedDomainEvent` before persistence removal.
- Activating/deactivating a category changes visibility because EF Core applies an `IsActive` query filter.

### Product

A product is the aggregate root for product variants.

Business rules:

- `Name` is required.
- `Slug` is required.
- `BasePrice` is required.
- A product must reference an existing category when created or updated through the application layer.
- Product variants are managed through the owning product aggregate.

Domain behaviors:

- Creating a product raises `ProductCreatedDomainEvent`.
- Updating product details raises `ProductUpdatedDomainEvent` with effective pricing and identity fields.
- Deleting a product raises `ProductDeletedDomainEvent` before the aggregate is removed.

### Product variant

A product variant is a child entity inside the `Product` aggregate.

Business rules:

- `Name` is required.
- `Sku` is required.
- SKU must be unique within a single product.
- `OverridePrice` is optional.
- When no override price is present, the effective variant price is the product base price.
- Attribute names must be unique within a variant create request.

Domain behaviors:

- Creating a variant through `Product.AddVariant` raises `ProductVariantCreatedDomainEvent`.
- Updating a variant through `Product.UpdateVariant` raises `ProductVariantUpdatedDomainEvent`.
- Deleting a variant through `Product.RemoveVariant` raises `ProductVariantDeletedDomainEvent`.

## Application layer

### Category commands

#### `CreateCategoryCommand`

- Validates that an optional parent category exists.
- Creates the category aggregate.
- Persists it with EF Core.

#### `UpdateCategoryCommand`

- Loads the category, including inactive ones via `IgnoreQueryFilters()`.
- Normalizes `Guid.Empty` parent values to `null`.
- Verifies that any requested parent exists and that the new parent chain does not create a cycle.
- Applies the update through the category aggregate, preserving domain-event publication.

#### `DeleteCategoryCommand`

- Loads the category, including inactive ones.
- Rejects deletion when child categories exist.
- Rejects deletion when products still reference the category.
- Raises a delete domain event and removes the category from persistence.

### Product commands

#### `CreateProductCommand`

- Resolves the referenced category.
- Creates the product aggregate with a normalized `Money` value object.
- Persists the product.

#### `UpdateProductCommand`

- Loads the product.
- Resolves the referenced category.
- Applies changes through `Product.UpdateDetails`.
- Persists the updated aggregate.

#### `DeleteProductCommand`

- Loads the product aggregate.
- Raises a delete domain event.
- Removes the product from persistence.

### Product variant commands

#### `CreateProductVariantCommand`

- Loads the owning product aggregate.
- Creates the variant through `Product.AddVariant`.
- Persists the new child entity and aggregate changes.

#### `UpdateProductVariantCommand`

- Loads the owning product aggregate.
- Updates the variant through `Product.UpdateVariant`.
- Persists the aggregate changes.

#### `DeleteProductVariantCommand`

- Resolves the owning product by variant id.
- Loads the aggregate.
- Deletes the variant through `Product.RemoveVariant`.
- Removes the variant row and persists the aggregate changes.

## Query logic

### Category queries

- `GetCategoriesQuery` applies optional `Name` and `Slug` filters only when they are supplied.
- Results are ordered by `Name` by default, or by `Slug` when slug search is present.
- The category global query filter returns active categories only.

### Product queries

- `GetProductsQuery` applies optional `Name` and `Slug` filters only when supplied.
- Results are ordered by `Name` by default, or by `Slug` when slug search is present.
- Products are projected to response DTOs rather than returning entities directly.

### Variant queries

- Variants can be listed globally, by id, or by owning product id.
- Variant reads project directly to `ProductVariantResponse` DTOs.

## Validation rules

FluentValidation is used on transport request contracts before command handlers execute.

### Categories

- `Name` required, max 200.
- `Slug` required, max 200.
- `Description` max 1000.

### Products

- `Name` required, max 200.
- `Slug` required, max 200.
- `CategoryId` required.
- `BasePrice` must be `>= 0`.
- `Currency` required, length 3.

### Product variants

- `Name` required, max 200.
- `Sku` required, max 100.
- `Description` max 4000.
- `ParentId` required.
- `OverridePrice` must be `>= 0`.
- `Currency` required, length 3.
- Create requests require a non-null attribute list and unique attribute names.

## Messaging and domain events

Domain events are captured through `EntityWithEvents` and published by the shared `DomainEventInterceptor` via Wolverine.

### Category events

- `CategoryCreatedDomainEvent` -> `catalog.category.created`
- `CategoryUpdatedDomainEvent` -> `catalog.category.updated`
- `CategoryDeletedDomainEvent` -> `catalog.category.deleted`

### Product events

- `ProductCreatedDomainEvent` -> `catalog.product.created`
- `ProductUpdatedDomainEvent` -> `catalog.product.updated`
- `ProductDeletedDomainEvent` -> `catalog.product.deleted`

### Product variant events

- `ProductVariantCreatedDomainEvent` -> `catalog.product-variant.created`
- `ProductVariantUpdatedDomainEvent` -> `catalog.product-variant.updated`
- `ProductVariantDeletedDomainEvent` -> `catalog.product-variant.deleted`

## Transport surfaces

### REST controllers

- `ProductsController` and `ProductVariantsController` delegate writes to Mediator commands.
- `CategoriesController` currently performs direct EF Core reads for simple GET endpoints and uses commands for writes.
- Controllers wrap responses with `ApiResponse` DTOs.

### GraphQL

- Current GraphQL support exposes category reads and category creation.
- GraphQL delegates to Mediator rather than embedding domain logic.

### gRPC

- Product and category gRPC services delegate most write/read behavior to Mediator handlers.
- `GetProductsByIds` uses EF Core projection directly for bulk product lookup.
- gRPC exceptions are translated through shared RPC exception helpers.

## Persistence rules

- Categories use a global query filter for `IsActive`.
- Category-to-category deletes are restricted in the database.
- Category-to-product deletes are restricted in the database.
- Product-to-variant deletes are cascaded in the database.
- Variant attribute values are stored as owned value objects.
- Money values are stored as owned value objects for products and optional variant override prices.

## Cache, Azure, deployment, and CI impact

- No Redis cache behavior is involved in Catalog business logic.
- No cache keys, TTLs, or invalidation strategies were added or changed.
- No Azure-specific behavior changed.
- No deployment or CI workflow updates were required for these business-logic fixes.

## Known behavioral assumptions

- Inactive categories are hidden from normal category queries because of the EF query filter.
- Category re-parenting allows moving a category within the hierarchy as long as the new parent exists and does not create a cycle.
- Product deletion publishes a product delete event, but variant delete events are only published when variants are removed explicitly through the variant command path.
