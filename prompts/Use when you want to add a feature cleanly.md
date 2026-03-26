Follow all repository custom instructions and agent rules before making changes.

Task:
Implement <feature name> in this .NET 10 Clean Architecture / DDD solution.

Requirements:
- preserve Clean Architecture boundaries
- use CQRS with the existing Mediator implementation, not MediatR
- use FluentValidation for request/command validation
- use EF Core / PostgreSQL / SQL Server / Cosmos / Redis only where appropriate to existing architecture
- keep controllers/endpoints thin
- keep business logic out of controllers
- use DTOs for transport contracts
- add/update xUnit tests
- use Moq only for true external boundaries
- update OpenAPI/Swagger metadata
- update technical documentation
- if cache is involved, define key, TTL, and invalidation strategy
- if Azure/deployment/CI impact exists, document it

Process:
1. inspect existing patterns and relevant files first
2. summarize assumptions
3. produce a short implementation plan
4. implement the smallest complete solution
5. add/update tests
6. update docs
7. summarize changed files and risks

Output expectations:
- concise plan first
- then implementation
- then tests/docs summary