# DDDSample WebApp

Vue 3 + TypeScript + Tailwind frontend for the DDDSample Aspire solution.

## Features

- Identity workflows: register, login, logout, current-user profile, manual Google token handoff
- Catalog views: categories, products, product variants
- Ordering workflows: create orders, pay/cancel orders, inspect cached products per order
- Payment workflows: list payments, lookup by order, complete/fail pending payments

## Run locally

```powershell
npm install
npm run dev
```

The app calls relative `/backend/*` paths. In local development, Vite proxies those routes to the backend APIs using environment variables that AppHost provides.
