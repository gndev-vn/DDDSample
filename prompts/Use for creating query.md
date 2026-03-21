Follow repository instructions.

Task:
Implement a query and handler for <read use case>.

Requirements:
- query must not mutate state
- return DTOs, not domain entities
- optimize read path appropriately
- use AsNoTracking for EF Core read paths where appropriate
- consider pagination/filtering/sorting if relevant
- if using Cosmos, be mindful of partition key and cross-partition costs
- if using cache, define cache key, TTL, and invalidation assumptions
- add tests for happy path, empty results, and edge cases
- update documentation