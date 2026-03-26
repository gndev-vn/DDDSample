# API and Application Standards

## API route conventions
- Use plural resource names unless existing conventions differ
- Use noun-based routes
- Avoid verbs in routes except for intentional action endpoints
- Keep routes stable and predictable

## API contract rules
- Use request/response DTOs
- Separate create/update/read DTOs when appropriate
- Do not expose persistence-only fields unless explicitly required
- Use consistent error response formatting
- Prefer ProblemDetails or repository-standard equivalent

## HTTP semantics
- GET = read
- POST = create or action when necessary
- PUT = full update when appropriate
- PATCH = partial update when supported
- DELETE = delete

## API documentation
- Add endpoint summary/description
- Document response codes
- Document validation/auth requirements where useful
- Keep OpenAPI aligned with implementation

## CQRS application rules
- Commands change state
- Queries do not change state
- Command/query names must reflect business intent
- Handlers should be small, focused, and testable

## Persistence boundaries
- API never directly depends on EF/Cosmos/Redis models
- Application uses contracts/interfaces
- Infrastructure owns provider-specific details

## Cache standards
- Define cache key structure explicitly
- Define TTL intentionally
- Document invalidation/update strategy
- Do not cache sensitive or user-specific data carelessly

## Documentation standards
When adding a feature, document:
- purpose
- key design decisions
- storage choices
- cache behavior
- failure/edge behavior
- configuration/deployment impact