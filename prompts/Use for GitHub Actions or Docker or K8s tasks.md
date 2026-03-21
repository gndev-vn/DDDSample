Follow repository instructions.

Task:
Implement/update CI/CD or deployment configuration for <area>.

Requirements:
- keep workflows deterministic and readable
- align build/test/publish steps with the .NET 10 solution
- do not expose secrets
- account for Docker/Kubernetes/Azure deployment needs
- document environment variables, required secrets, and operational assumptions
- if changing container behavior, consider readiness/liveness and graceful shutdown
- if changing GitHub Actions, explain why each workflow/job change is needed