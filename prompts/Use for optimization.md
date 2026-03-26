Follow repository instructions.

Task:
Review and improve the performance of <feature/query/path>.

Requirements:
- identify the actual bottleneck or likely inefficiencies first
- avoid premature optimization
- consider EF Core query efficiency, indexing, over-fetching, N+1, memory usage, and async behavior
- consider Redis/HybridCache only if it clearly adds value
- if Cosmos is involved, consider partitioning and RU cost
- preserve correctness and maintainability
- add/update tests if behavior changes
- document trade-offs and operational impact