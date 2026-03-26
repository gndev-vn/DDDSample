### PROMPT 1: FULL FEATURE
Follow all repository custom instructions and agent rules before making changes.

Task:
Implement Google login for this ASP.NET Core Web API that already has JWT authentication.

Requirements:
- inspect existing auth flow first
- preserve Clean Architecture / DDD boundaries
- use existing CQRS + Mediator pattern
- add Google login endpoint
- validate Google identity on the backend
- reuse existing JWT issuing logic
- map Google login to existing user by email if appropriate
- create a local user on first login if consistent with current auth rules
- use FluentValidation
- add/update xUnit tests
- update Swagger/OpenAPI and technical docs

Start by inspecting the existing auth implementation and give me a short plan before coding.

### PROMPT 2: AFTER IT GIVES A PLAN
Proceed with the implementation.

Additional constraints:
- use configuration options for Google client settings
- do not hardcode secrets
- return the same auth response DTO currently used by password login
- if account linking rules are ambiguous, choose the safest option and state the assumption
- add regression tests for invalid token and existing-user login

### PROMPT 3: IF YOU WANT STRICTER SECURITY REVIEW
Before finalizing, do a security review of the Google login flow.

Specifically verify:
- token is verified server-side
- client-supplied email is not trusted unless obtained from verified token payload
- audience/client id validation is enforced
- issuer validation is enforced if appropriate
- existing auth and authorization flows are not bypassed
- sensitive token data is not logged

### PROMPT 4: IF YOU WANT DOCS AND DIAGRAMS
Now update the technical documentation for the new Google login flow.

Include:
- flow summary
- configuration required
- security considerations
- Mermaid sequence diagram
- API request/response example
