# Storage Decision Guidelines

## Use relational storage (EF Core + PostgreSQL/SQL Server) when:
- strong consistency is required
- relational joins/constraints matter
- transactional integrity is required
- data is core business source of truth

## Use Cosmos DB when:
- document-oriented access patterns dominate
- partition-friendly scale matters
- denormalized read/write patterns fit well
- eventual consistency trade-offs are acceptable

## Use Redis / HybridCache when:
- data is derived, transient, or performance-oriented
- source of truth exists elsewhere
- TTL and invalidation strategy are well-defined

## Important rules
- always document source of truth
- always document consistency expectations
- always document cache invalidation strategy
- never assume distributed transaction semantics across stores