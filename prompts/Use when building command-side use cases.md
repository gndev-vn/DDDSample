Follow repository instructions.

Task:
Implement a command and handler for <business action>.

Requirements:
- command name must reflect business intent
- handler must be focused on one use case
- keep domain invariants protected
- use FluentValidation
- use cancellation tokens
- use the existing Mediator implementation, not MediatR
- use repositories/ports/contracts already used by the application layer
- if persistence is involved, keep provider details in Infrastructure
- add unit tests for handler behavior and business rules
- document assumptions and edge cases