# Package Upgrade Plan

## Overview

**Target**: DDDSample.sln — 6 projects (Shared, GrpcShared, CatalogAPI, IdentityAPI, OrderingAPI, Tests)
**Scope**: Update all NuGet packages to latest compatible versions. No TFM change (staying on net10.0). All 38 packages have open-source licenses (MIT/Apache/BSD) — no paid-license packages present.

### Selected Strategy
**All-at-Once** — All projects upgraded simultaneously, split by risk level.
**Rationale**: 6 projects, all net10.0, package-only changes. Dependency depth of 2 with no circular deps.

---

## Tasks

### 01-patch-packages: Low-risk package updates

Update packages with minor/patch version bumps and no expected breaking changes across all projects. Covers: FluentValidation (→12.1.1), Grpc.AspNetCore/Core.Api/Tools (→2.76.0/2.76.0/2.78.0), Google.Protobuf (→3.34.1), Google.Apis.Auth (→1.73.0), HotChocolate.AspNetCore (→15.1.12), Mediator.Abstractions/SourceGenerator (→3.0.2), MongoDB.Driver (→3.7.1), StackExchange.Redis (→2.12.8), System.IdentityModel.Tokens.Jwt (→8.17.0), Microsoft.NET.Test.Sdk (→18.3.0), xunit.runner.visualstudio (→3.1.5).

**Done when**: All listed packages updated in all affected .csproj files, solution restores without errors.

---

### 02-microsoft-packages: Microsoft platform packages (9.x → 10.x)

Update all Microsoft.* packages from their current 9.x versions to 10.x, aligning with the net10.0 TFM already in use. Covers: Microsoft.AspNetCore.Authentication.JwtBearer (→10.0.5), Microsoft.AspNetCore.OpenApi (→10.0.5), Microsoft.EntityFrameworkCore + Design + Relational + SqlServer + InMemory (→10.0.5), Microsoft.Extensions.Caching.StackExchangeRedis (→10.0.5), Microsoft.Extensions.Caching.Hybrid (→10.4.0). Fix any compilation errors arising from the version bump.

**Done when**: All Microsoft.* packages updated, solution builds with 0 errors.

---

### 03-major-third-party: WolverineFx and Mapster major version upgrades

Update WolverineFx 4.12.3 → 5.24.0 (major version: message bus, EFCore integration, RabbitMQ, SqlServer) and Mapster 7.4.0 → 10.0.6 (with Mapster.Async, Mapster.DependencyInjection, Mapster.EFCore). These are large major version jumps that may require code changes in consuming projects (CatalogAPI, OrderingAPI, IdentityAPI). Fix all breaking changes before proceeding.

**Done when**: Both WolverineFx and Mapster families updated, solution builds with 0 errors.

---

### 04-validate: Final build and test validation

Run a full solution build and the complete test suite to confirm all package updates are compatible and no regressions were introduced.

**Done when**: `dotnet build` succeeds with 0 errors; test suite passes with no new failures compared to pre-upgrade baseline.

---
