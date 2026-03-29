# OrderingAPI performance approach

## Recommendation summary

For a DDD-oriented microservice architecture, Ordering should not call Catalog synchronously on every order read. The recommended approach is:

- keep Ordering reads local
- maintain a local catalog read model inside Ordering
- use catalog events to keep the local read model fresh
- use gRPC only as an explicit fallback path, not the default hot path

## Current performance-oriented design

### Order reads

`GetOrderByIdQuery` and `GetOrdersQuery` read from Ordering's own SQL database only.

They do not require a synchronous CatalogAPI call to render orders.

### Local catalog read model

Ordering stores:

- `ProductCache`
- `CategoryCache`

These tables are part of Ordering's local read model. They allow the service to enrich and validate order operations without coupling every request to CatalogAPI availability or network latency.

### Event-driven synchronization

Ordering listens to catalog events and updates local cache state for:

- product created
- product updated
- product deleted
- category created
- category updated
- category deleted

This keeps the local read model aligned with Catalog over time while preserving service autonomy.

### Write-path optimization

Order creation and update resolve product metadata from Ordering's local `ProductCache` by SKU.

When the product exists in the local cache:

- `ProductId` is recorded on the order line
- price and currency are taken from the local catalog cache, not from untrusted client input
- order responses can include product names without a Catalog roundtrip

When the product is not yet present in the local cache, Ordering currently falls back to the request values for compatibility.

## Why cache is better than gRPC on the hot path

Using gRPC for every order read would:

- add network latency to every request
- couple Ordering availability to Catalog availability
- create fan-out pressure under read load
- make order rendering sensitive to transient cross-service failures

Using a local event-driven read model instead:

- keeps reads fast and local
- reduces cross-service latency and failure propagation
- fits CQRS and DDD bounded-context patterns
- scales better under high read volume

## When gRPC is still useful

gRPC remains useful for:

- explicit fallback or repair workflows
- administrative resync jobs
- point-in-time validation when local cache is known to be cold
- non-hot-path bulk synchronization

It should not be the default strategy for end-user order viewing.

## Future improvement

The strongest DDD option is to promote product display data to an immutable order-line snapshot at order creation time, so historical orders remain fully self-contained even if the catalog changes later.
