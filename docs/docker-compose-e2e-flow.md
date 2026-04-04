# Docker Compose E2E flow

## Purpose

This document is a demo-ready walkthrough for the current Docker Compose stack.

It shows, step by step:

- which HTTP request to send
- the exact request body to use
- which domain/integration events are emitted
- which service consumes those events
- what state change you should observe in the next service

The latest fully verified live run used sample suffix `0330083922`.

## Services and ports

- `CatalogAPI`: `http://localhost:9080`
- `OrderingAPI`: `http://localhost:9084`
- `IdentityAPI`: `http://localhost:9088`
- `PaymentAPI`: `http://localhost:9092`

## Seeded accounts for demo

These are now created automatically by `IdentityAPI` on startup.

- Admin user: `admin@example.com` / `admin123`
- Normal user: `user@example.com` / `user123`

## High-level event flow

```text
CatalogAPI
  Category/Product/ProductVariant create/update
    -> publishes catalog events
    -> OrderingAPI consumes category/product events and updates read caches

OrderingAPI
  order create
    -> publishes OrderCreatedEvent
    -> PaymentAPI consumes and creates pending payment

PaymentAPI
  payment complete
    -> publishes PaymentCompletedEvent
    -> OrderingAPI consumes and marks order as Paid

PaymentAPI
  payment fail
    -> publishes PaymentFailedEvent
    -> currently no Ordering state transition is applied from this event
```

## Recommended demo order

1. Login as seeded admin and seeded user.
2. Create categories.
3. Create products.
4. Create variants.
5. Create orders.
6. Show automatic payment creation.
7. Update catalog data and show Ordering read-model updates.
8. Complete one payment.
9. Fail one payment.
10. Show final state in APIs and logs.

---

## 1. Verify the stack is running

```bash
docker compose ps
```

Expected:

- all APIs are `Up`
- `mongodb` is healthy
- `sqlserver` is healthy

---

## 2. Login with seeded accounts

### 2.1 Login as admin

```http
POST http://localhost:9088/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}
```

Expected response highlights:

- `200 OK`
- `data.user.email = admin@example.com`
- `data.user.roles = ["Admin"]`
- JWT contains the `Admin` role claim

Observed in the verified run:

- admin id: `e8100330-b702-4259-b019-9323b3f43f7f`

### 2.2 Login as normal user

```http
POST http://localhost:9088/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "user123"
}
```

Expected response highlights:

- `200 OK`
- `data.user.email = user@example.com`
- `data.user.roles = ["User"]`

Observed in the verified run:

- user id: `2c741169-98b8-4664-925c-a6f6e2666c3d`

What to say in the demo:

- Identity is now bootstrapped on startup.
- No manual role creation is needed anymore.
- The admin token is used for protected Catalog write endpoints.

---

## 3. Create categories in Catalog

Use the admin JWT from step 2.1.

### 3.1 Create Electronics category

```http
POST http://localhost:9080/api/categories
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Electronics 0330083922",
  "slug": "electronics-0330083922",
  "description": "Electronics sample category 0330083922",
  "parentId": null
}
```

Observed id:

- `9552ddca-a871-4333-85d5-9481ce5e8a52`

### 3.2 Create Apparel category

```http
POST http://localhost:9080/api/categories
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Apparel 0330083922",
  "slug": "apparel-0330083922",
  "description": "Apparel sample category 0330083922",
  "parentId": null
}
```

Observed id:

- `06985f20-2401-47fe-bcab-68e5b62af3f7`

### 3.3 Create Accessories category

```http
POST http://localhost:9080/api/categories
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Accessories 0330083922",
  "slug": "accessories-0330083922",
  "description": "Accessories sample category 0330083922",
  "parentId": null
}
```

Observed id:

- `7c0f8743-813d-418c-82a2-e05f95f14403`

### Events for category creation

For each category:

- `CatalogAPI` raises `CategoryCreatedDomainEvent`
- `CatalogAPI` publishes `CategoryCreatedEvent`
- `OrderingAPI` consumes `CategoryCreatedEvent`
- `OrderingAPI` upserts `CategoryCaches`

Observed log examples:

```text
[CatalogAPI] Publishing CategoryCreatedEvent for category 9552ddca-a871-4333-85d5-9481ce5e8a52 (Electronics 0330083922)
[OrderingAPI] Consuming CategoryCreatedEvent for category 9552ddca-a871-4333-85d5-9481ce5e8a52 (Electronics 0330083922)
```

What to observe:

- Catalog returns created categories immediately.
- Ordering does not expose categories directly by API, so the read-side effect is mainly visible in logs or SQL cache tables.

---

## 4. Create products in Catalog

### 4.1 Create Phone product

```http
POST http://localhost:9080/api/products
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Phone 0330083922",
  "slug": "phone-0330083922",
  "description": "Phone sample product 0330083922",
  "categoryId": "9552ddca-a871-4333-85d5-9481ce5e8a52",
  "basePrice": 699.99,
  "currency": "USD"
}
```

Observed id:

- `4a896162-b757-48b3-8cb5-2fc0986229da`

### 4.2 Create Laptop product

```http
POST http://localhost:9080/api/products
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Laptop 0330083922",
  "slug": "laptop-0330083922",
  "description": "Laptop sample product 0330083922",
  "categoryId": "9552ddca-a871-4333-85d5-9481ce5e8a52",
  "basePrice": 1499.50,
  "currency": "USD"
}
```

Observed id:

- `75e5a23b-44ff-48d5-a2fe-6b05c1f316ec`

### 4.3 Create TShirt product

```http
POST http://localhost:9080/api/products
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "TShirt 0330083922",
  "slug": "tshirt-0330083922",
  "description": "TShirt sample product 0330083922",
  "categoryId": "06985f20-2401-47fe-bcab-68e5b62af3f7",
  "basePrice": 29.95,
  "currency": "USD"
}
```

Observed id:

- `4e26d931-65b8-46f7-b649-dcf27623b7ec`

### 4.4 Create Cap product

```http
POST http://localhost:9080/api/products
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Cap 0330083922",
  "slug": "cap-0330083922",
  "description": "Cap sample product 0330083922",
  "categoryId": "7c0f8743-813d-418c-82a2-e05f95f14403",
  "basePrice": 19.99,
  "currency": "USD"
}
```

Observed id:

- `26c05a58-9ee7-49dd-ae37-4a8ce2d6fa0f`

### Events for product creation

For each product:

- `CatalogAPI` raises `ProductCreatedDomainEvent`
- `CatalogAPI` publishes `ProductCreatedEvent`
- `OrderingAPI` consumes `ProductCreatedEvent`
- `OrderingAPI` upserts `ProductCaches`

Observed log examples:

```text
[CatalogAPI] Publishing ProductCreatedEvent for product 4a896162-b757-48b3-8cb5-2fc0986229da (phone-0330083922)
[OrderingAPI] Consuming ProductCreatedEvent for product 4a896162-b757-48b3-8cb5-2fc0986229da (phone-0330083922)
```

What to observe:

- Products become available in Ordering's local cache.
- Ordering later uses this cache to resolve order line names and prices.

---

## 5. Create product variants in Catalog

These are useful for demo completeness, even though Ordering does not currently project variant data.

### 5.1 Create Phone Black 128GB

```http
POST http://localhost:9080/api/productvariants
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Black 128GB 0330083922",
  "sku": "PHONE-BLK-128-0330083922",
  "description": "Phone black variant 0330083922",
  "parentId": "4a896162-b757-48b3-8cb5-2fc0986229da",
  "overridePrice": 729.99,
  "currency": "USD",
  "attributes": [
    { "name": "Color", "value": "Black" },
    { "name": "Storage", "value": "128GB" }
  ]
}
```

Observed id:

- `d99a1482-338a-4643-86e6-93348dd90235`

### 5.2 Create Phone Blue 256GB

```http
POST http://localhost:9080/api/productvariants
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Blue 256GB 0330083922",
  "sku": "PHONE-BLU-256-0330083922",
  "description": "Phone blue variant 0330083922",
  "parentId": "4a896162-b757-48b3-8cb5-2fc0986229da",
  "overridePrice": 779.99,
  "currency": "USD",
  "attributes": [
    { "name": "Color", "value": "Blue" },
    { "name": "Storage", "value": "256GB" }
  ]
}
```

Observed id:

- `b7cb8f16-1aa8-4160-9311-b98f104b1fcd`

### 5.3 Create Laptop 16GB RAM

```http
POST http://localhost:9080/api/productvariants
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "16GB RAM 0330083922",
  "sku": "LAPTOP-16GB-0330083922",
  "description": "Laptop variant 0330083922",
  "parentId": "75e5a23b-44ff-48d5-a2fe-6b05c1f316ec",
  "overridePrice": 1599.50,
  "currency": "USD",
  "attributes": [
    { "name": "RAM", "value": "16GB" },
    { "name": "Storage", "value": "512GB SSD" }
  ]
}
```

Observed id:

- `60ede201-de4c-41f4-bf3f-18a59e52d0ef`

### 5.4 Create TShirt Large

```http
POST http://localhost:9080/api/productvariants
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Large 0330083922",
  "sku": "TSHIRT-L-0330083922",
  "description": "TShirt variant 0330083922",
  "parentId": "4e26d931-65b8-46f7-b649-dcf27623b7ec",
  "overridePrice": 34.95,
  "currency": "USD",
  "attributes": [
    { "name": "Size", "value": "L" },
    { "name": "Color", "value": "White" }
  ]
}
```

Observed id:

- `80abfeb0-7891-43af-a3e7-7c5eb21f6efa`

### 5.5 Create Cap Adjustable

```http
POST http://localhost:9080/api/productvariants
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "name": "Adjustable 0330083922",
  "sku": "CAP-ADJ-0330083922",
  "description": "Cap variant 0330083922",
  "parentId": "26c05a58-9ee7-49dd-ae37-4a8ce2d6fa0f",
  "overridePrice": 24.99,
  "currency": "USD",
  "attributes": [
    { "name": "Size", "value": "Adjustable" },
    { "name": "Color", "value": "Black" }
  ]
}
```

Observed id:

- `9fbf8ef8-c274-4df6-9967-6f15271cbcd0`

### Events for variant creation

For each variant:

- `CatalogAPI` raises `ProductVariantCreatedDomainEvent`
- `CatalogAPI` publishes `ProductVariantCreatedEvent`
- no Ordering consumer currently projects variant data into Ordering read models

What to observe:

- variants are created successfully in Catalog
- variant events appear in Catalog logs
- variant data is not directly surfaced in Ordering today

---

## 6. Create orders in Ordering

Important behavior:

- Ordering resolves order line name and price from its own `ProductCaches`
- the `sku` to send in the order line must be the product slug known by Ordering cache
- the posted `name` and `unitPrice` are effectively ignored when cache data exists

### 6.1 Create order for normal user

```http
POST http://localhost:9084/api/orders
Content-Type: application/json

{
  "customerId": "2c741169-98b8-4664-925c-a6f6e2666c3d",
  "shippingAddress": {
    "line1": "100 Flow Street 0330083922",
    "line2": "Suite 200",
    "city": "Ho Chi Minh City",
    "province": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ben Nghe"
  },
  "billingAddress": {
    "line1": "100 Flow Street 0330083922",
    "line2": "Suite 200",
    "city": "Ho Chi Minh City",
    "province": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ben Nghe"
  },
  "lines": [
    {
      "productId": "4a896162-b757-48b3-8cb5-2fc0986229da",
      "name": "ignored",
      "sku": "phone-0330083922",
      "quantity": 2,
      "unitPrice": 9999,
      "currency": "USD"
    },
    {
      "productId": "4e26d931-65b8-46f7-b649-dcf27623b7ec",
      "name": "ignored",
      "sku": "tshirt-0330083922",
      "quantity": 3,
      "unitPrice": 9999,
      "currency": "USD"
    }
  ]
}
```

Observed order id:

- `97808bce-d0a3-4d72-aa82-50bdcd05ed47`

Observed response highlights:

- line name resolved to `Phone 0330083922`
- line unit price resolved to `699.99`
- second line resolved to `TShirt 0330083922` at `29.95`
- order status initially `0` (`Submitted` / pending state in API response)

### 6.2 Create order for admin user

```http
POST http://localhost:9084/api/orders
Content-Type: application/json

{
  "customerId": "e8100330-b702-4259-b019-9323b3f43f7f",
  "shippingAddress": {
    "line1": "100 Flow Street 0330083922",
    "line2": "Suite 200",
    "city": "Ho Chi Minh City",
    "province": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ben Nghe"
  },
  "billingAddress": {
    "line1": "100 Flow Street 0330083922",
    "line2": "Suite 200",
    "city": "Ho Chi Minh City",
    "province": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ben Nghe"
  },
  "lines": [
    {
      "productId": "75e5a23b-44ff-48d5-a2fe-6b05c1f316ec",
      "name": "ignored",
      "sku": "laptop-0330083922",
      "quantity": 1,
      "unitPrice": 9999,
      "currency": "USD"
    },
    {
      "productId": "26c05a58-9ee7-49dd-ae37-4a8ce2d6fa0f",
      "name": "ignored",
      "sku": "cap-0330083922",
      "quantity": 2,
      "unitPrice": 9999,
      "currency": "USD"
    }
  ]
}
```

Observed order id:

- `cadf46c1-ff23-4b29-9cd9-9948bbdf8e61`

Observed response highlights:

- line names resolved to `Laptop 0330083922` and `Cap 0330083922`
- line prices resolved to `1499.50` and `19.99`
- order status initially `0`

### Events for order creation

For each order:

- `OrderingAPI` raises `OrderCreatedDomainEvent`
- `OrderingAPI` publishes `OrderCreatedEvent`
- `PaymentAPI` consumes `OrderCreatedEvent`
- `PaymentAPI` creates a pending payment
- `PaymentAPI` raises `PaymentCreatedDomainEvent`
- `PaymentAPI` publishes `PaymentCreatedEvent`

Observed log examples:

```text
[OrderingAPI] Publishing OrderCreatedEvent for order 97808bce-d0a3-4d72-aa82-50bdcd05ed47 with total 729.94 USD
[PaymentAPI] Consuming OrderCreatedEvent for order 97808bce-d0a3-4d72-aa82-50bdcd05ed47
[PaymentAPI] Publishing PaymentCreatedEvent for payment 31fad328-a879-4539-aa36-2c39e5cd9bc8 and order 97808bce-d0a3-4d72-aa82-50bdcd05ed47
```

What to observe:

- order totals are computed from Ordering data, not the fake posted line price
- a payment is created automatically without calling PaymentAPI directly

---

## 7. Verify payments were auto-created

### 7.1 Get payment for order 1

```http
GET http://localhost:9092/api/payments/orders/97808bce-d0a3-4d72-aa82-50bdcd05ed47
```

Observed response highlights:

- payment id: `31fad328-a879-4539-aa36-2c39e5cd9bc8`
- amount: `729.94`
- status: `1` (`Pending`)

### 7.2 Get payment for order 2

```http
GET http://localhost:9092/api/payments/orders/cadf46c1-ff23-4b29-9cd9-9948bbdf8e61
```

Observed response highlights:

- payment id: `e680f882-c220-413e-a1f7-cdd11c9c5bed`
- amount: `1519.49`
- status: `1` (`Pending`)

What to observe:

- Payment records appear only after the asynchronous `OrderCreatedEvent` is consumed.
- This is a good place in the demo to show eventual consistency between services.

---

## 8. Update categories, products, and variants

This step is useful to demonstrate read-model propagation and current system limitations.

### 8.1 Update categories

Important current contract detail:

- `CategoryUpdateRequest.ParentId` is not nullable
- for root categories, use `00000000-0000-0000-0000-000000000000`

Example update:

```http
PUT http://localhost:9080/api/categories/9552ddca-a871-4333-85d5-9481ce5e8a52
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "id": "9552ddca-a871-4333-85d5-9481ce5e8a52",
  "name": "Electronics 0330083922 Updated",
  "slug": "electronics-0330083922-updated",
  "description": "Electronics sample category 0330083922 updated",
  "parentId": "00000000-0000-0000-0000-000000000000",
  "isActive": true
}
```

Repeat the same pattern for:

- `06985f20-2401-47fe-bcab-68e5b62af3f7`
- `7c0f8743-813d-418c-82a2-e05f95f14403`

### 8.2 Update products

Example update for Phone:

```http
PUT http://localhost:9080/api/products/4a896162-b757-48b3-8cb5-2fc0986229da
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "id": "4a896162-b757-48b3-8cb5-2fc0986229da",
  "name": "Phone 0330083922 Updated",
  "slug": "phone-0330083922-updated",
  "description": "Phone sample product 0330083922 updated",
  "categoryId": "9552ddca-a871-4333-85d5-9481ce5e8a52",
  "basePrice": 710.24,
  "currency": "USD"
}
```

Other verified product updates:

- Laptop -> `1520.00`
- TShirt -> `60.70`
- Cap -> `60.99`

### 8.3 Update variants

Example update for Phone Black 128GB:

```http
PUT http://localhost:9080/api/productvariants/d99a1482-338a-4643-86e6-93348dd90235
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "id": "d99a1482-338a-4643-86e6-93348dd90235",
  "name": "Black 128GB 0330083922 Updated",
  "sku": "PHONE-BLK-128-0330083922-upd",
  "description": "Phone black variant 0330083922 updated",
  "parentId": "4a896162-b757-48b3-8cb5-2fc0986229da",
  "overridePrice": 735.49,
  "currency": "USD"
}
```

### Events for catalog updates

Category updates:

- `CatalogAPI` publishes `CategoryUpdatedEvent`
- `OrderingAPI` consumes `CategoryUpdatedEvent`
- `OrderingAPI` updates `CategoryCaches`

Product updates:

- `CatalogAPI` publishes `ProductUpdatedEvent`
- `OrderingAPI` consumes `ProductUpdatedEvent`
- `OrderingAPI` updates `ProductCaches`

Variant updates:

- `CatalogAPI` publishes `ProductVariantUpdatedEvent`
- no Ordering consumer currently projects variant updates

What to observe after propagation:

```http
GET http://localhost:9084/api/orders/97808bce-d0a3-4d72-aa82-50bdcd05ed47
GET http://localhost:9084/api/orders/cadf46c1-ff23-4b29-9cd9-9948bbdf8e61
```

Observed result:

- order line names changed to updated product names
- order line prices stayed at original order-time values

Observed examples:

- `Phone 0330083922` -> `Phone 0330083922 Updated`
- `TShirt 0330083922` -> `TShirt 0330083922 Updated`
- unit prices stayed `699.99` and `29.95`

What to say in the demo:

- this is deliberate read-model behavior for historical orders
- names are refreshed from cache for display
- prices remain the order-time values
- variant events exist, but Ordering does not project them yet

---

## 9. Complete one payment and fail one payment

### 9.1 Complete the first payment

```http
POST http://localhost:9092/api/payments/31fad328-a879-4539-aa36-2c39e5cd9bc8/complete
Content-Type: application/json

{
  "transactionReference": "TXN-0330083922-1"
}
```

Observed result:

- payment status changed to `2` (`Completed`)
- transaction reference persisted

### 9.2 Fail the second payment

```http
POST http://localhost:9092/api/payments/e680f882-c220-413e-a1f7-cdd11c9c5bed/fail
Content-Type: application/json

{
  "reason": "Intentional failure for flow 0330083922"
}
```

Observed result:

- payment status changed to `3` (`Failed`)
- failure reason persisted

### Events for payment transitions

Payment complete:

- `PaymentAPI` raises `PaymentCompletedDomainEvent`
- `PaymentAPI` publishes `PaymentCompletedEvent`
- `OrderingAPI` consumes `PaymentCompletedEvent`
- `OrderingAPI` marks the order as `Paid`
- `OrderingAPI` raises and publishes `OrderPaidEvent`

Payment fail:

- `PaymentAPI` raises `PaymentFailedDomainEvent`
- `PaymentAPI` publishes `PaymentFailedEvent`
- no Ordering state transition is currently applied from this event

Observed log examples:

```text
[PaymentAPI] Publishing PaymentCompletedEvent for payment 31fad328-a879-4539-aa36-2c39e5cd9bc8 and order 97808bce-d0a3-4d72-aa82-50bdcd05ed47
[OrderingAPI] Consuming PaymentCompletedEvent for order 97808bce-d0a3-4d72-aa82-50bdcd05ed47 and payment 31fad328-a879-4539-aa36-2c39e5cd9bc8
[OrderingAPI] Publishing OrderPaidEvent for order 97808bce-d0a3-4d72-aa82-50bdcd05ed47

[PaymentAPI] Publishing PaymentFailedEvent for payment e680f882-c220-413e-a1f7-cdd11c9c5bed and order cadf46c1-ff23-4b29-9cd9-9948bbdf8e61: Intentional failure for flow 0330083922
```

---

## 10. Verify final state

### 10.1 Verify final order state

```http
GET http://localhost:9084/api/orders/97808bce-d0a3-4d72-aa82-50bdcd05ed47
GET http://localhost:9084/api/orders/cadf46c1-ff23-4b29-9cd9-9948bbdf8e61
```

Observed final state:

- order `97808bce-d0a3-4d72-aa82-50bdcd05ed47` -> status `1` (`Paid`)
- order `cadf46c1-ff23-4b29-9cd9-9948bbdf8e61` -> status `0` (`Submitted`)

### 10.2 Verify final payment state

```http
GET http://localhost:9092/api/payments/orders/97808bce-d0a3-4d72-aa82-50bdcd05ed47
GET http://localhost:9092/api/payments/orders/cadf46c1-ff23-4b29-9cd9-9948bbdf8e61
```

Observed final state:

- payment `31fad328-a879-4539-aa36-2c39e5cd9bc8` -> `Completed`
- payment `e680f882-c220-413e-a1f7-cdd11c9c5bed` -> `Failed`

---

## 11. SQL verification used in the live run

### Catalog

```sql
SELECT Id, Name, Slug, Description
FROM CatalogAPI.dbo.Categories
WHERE Id IN (
  '9552ddca-a871-4333-85d5-9481ce5e8a52',
  '06985f20-2401-47fe-bcab-68e5b62af3f7',
  '7c0f8743-813d-418c-82a2-e05f95f14403'
);

SELECT Id, Name, Slug, BasePrice
FROM CatalogAPI.dbo.Products
WHERE Id IN (
  '4a896162-b757-48b3-8cb5-2fc0986229da',
  '75e5a23b-44ff-48d5-a2fe-6b05c1f316ec',
  '4e26d931-65b8-46f7-b649-dcf27623b7ec',
  '26c05a58-9ee7-49dd-ae37-4a8ce2d6fa0f'
);

SELECT Id, Name, Sku
FROM CatalogAPI.dbo.ProductVariants
WHERE Id IN (
  'd99a1482-338a-4643-86e6-93348dd90235',
  'b7cb8f16-1aa8-4160-9311-b98f104b1fcd',
  '60ede201-de4c-41f4-bf3f-18a59e52d0ef',
  '80abfeb0-7891-43af-a3e7-7c5eb21f6efa',
  '9fbf8ef8-c274-4df6-9967-6f15271cbcd0'
);
```

Observed highlights:

- categories persisted with updated names/slugs/descriptions
- products persisted with updated names/slugs/prices
- variants persisted with updated names/SKUs

### Ordering

```sql
SELECT Id, Status, CustomerId
FROM OrderingAPI.dbo.Orders
WHERE Id IN (
  '97808bce-d0a3-4d72-aa82-50bdcd05ed47',
  'cadf46c1-ff23-4b29-9cd9-9948bbdf8e61'
);
```

Observed highlights:

- order `97808bce...` = `Paid`
- order `cadf46c1...` = `Submitted`

### Payment

```sql
SELECT Id, OrderId, Status, Amount, TransactionReference, FailureReason
FROM PaymentAPI.dbo.Payments
WHERE Id IN (
  '31fad328-a879-4539-aa36-2c39e5cd9bc8',
  'e680f882-c220-413e-a1f7-cdd11c9c5bed'
);
```

Observed highlights:

- payment `31fad328...` = `Completed`, `TXN-0330083922-1`
- payment `e680f882...` = `Failed`, `Intentional failure for flow 0330083922`

---

## 12. Key demo talking points

### What is working well

- seeded identity bootstrap for a clean startup experience
- protected admin writes in Catalog
- asynchronous cross-service propagation through Wolverine and Kafka
- Ordering read model decoupled from Catalog live queries
- automatic payment creation from order events
- payment completion flowing back into Ordering
- consistent publish/consume log wording across services

### Important current limitations to mention honestly

- Ordering does not project product variant events yet
- product variant attribute updates are not replaced through the current update contract
- category propagation is mainly visible through cache/SQL, not through order DTOs
- order line display names can refresh from cache, but line prices remain historical order-time values
- `CategoryUpdateRequest.ParentId` still uses `Guid.Empty` for root updates instead of nullable semantics

### Short architecture explanation for interview

- `CatalogAPI` is the source of truth for catalog entities
- `OrderingAPI` keeps its own read-side caches for categories/products
- `OrderingAPI` creates orders from cached product data, not direct catalog calls
- `PaymentAPI` reacts to order creation and manages payment lifecycle
- event-driven consistency keeps service boundaries clean while allowing eventual consistency

## 13. Recommended demo script summary

If you need the shortest live demo path tomorrow, do this:

1. login as seeded admin and seeded user
2. create one category, one product, one variant
3. create one order using the product slug as line SKU
4. show automatic payment creation in `PaymentAPI`
5. update the product name in `CatalogAPI`
6. show the order line name updated in `OrderingAPI`
7. complete the payment
8. show the order status changed to `Paid`
9. point at the event logs for `OrderCreatedEvent` and `PaymentCompletedEvent`

That gives you auth, write model, read model, eventing, eventual consistency, and payment workflow in a compact demo.


## Customer login seed
- Customer user: `alex.nguyen@example.com` / `customer123`
- Linked Ordering customer: `Alex Nguyen` (`11111111-1111-1111-1111-111111111111`)

