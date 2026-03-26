---
applyTo: '**'
---

# Agent Execution Rules for This Repository

## Mission
Deliver production-grade changes for a .NET 10 Clean Architecture / DDD application with strong correctness, maintainability, observability, and documentation.

## Querying Microsoft Documentation

You have access to MCP tools called `microsoft_docs_search`, `microsoft_docs_fetch`, and `microsoft_code_sample_search` - these tools allow you to search through and fetch Microsoft's latest official documentation and code samples, and that information might be more detailed or newer than what's in your training data set.

When handling questions around how to work with native Microsoft technologies, such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use these tools for research purposes when dealing with specific / narrowly defined questions that may occur.

## Step 1: Understand before changing
Before writing code:
- read the relevant feature area end-to-end
- identify the affected layers: API, Application, Domain, Infrastructure
- identify storage dependencies: EF Core, PostgreSQL, SQL Server, Cosmos, Redis/HybridCache
- identify whether the change is command-side, query-side, or both
- inspect existing patterns and naming conventions
- inspect existing validators, handlers, mappings, tests, and docs

## Step 2: Plan first for non-trivial tasks
For non-trivial work, produce a short plan:
- problem summary
- assumptions
- design approach
- files/layers to change
- tests to add/update
- docs to update
- risks

## Step 3: Implement with minimal correct scope
While implementing:
- preserve Clean Architecture boundaries
- keep domain logic in domain/application, not controllers
- keep handlers cohesive
- keep infrastructure concerns isolated
- use DTOs for transport contracts
- use cancellation tokens for public async flows
- update dependency injection registrations if needed
- keep code style aligned with existing repo

## Step 4: Validate quality
Before finishing:
- re-check nullability
- re-check async correctness
- re-check auth/authz if relevant
- re-check validation coverage
- re-check query efficiency
- re-check cache invalidation/TTL if relevant
- re-check logging safety
- re-check exception handling
- re-check Docker/Kubernetes/Azure/CI impact if relevant

## Step 5: Test
- add/update unit tests
- add/update integration tests where behavior crosses boundaries
- avoid brittle mocking
- verify critical edge cases
- verify failures are handled intentionally

## Step 6: Document
If behavior/config/contracts/operations changed:
- update technical docs
- update API docs/OpenAPI metadata
- update migration/config/deployment notes if needed
- add Mermaid diagrams when architecture/flow changes are significant

## Final response expectations
When completing a task, include:
- summary of change
- assumptions
- key implementation details
- tests added/updated
- documentation updated
- risks or follow-up notes