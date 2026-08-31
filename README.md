# SiteInspect

SiteInspect is a portfolio-grade field inspection and corrective-action platform built as a .NET 10 modular monolith with an Angular 22 PWA frontend and a separate background worker.

Phase 1 adds Identity sessions, demo seed data, and role-scoped inspection read models on top of the architecture foundation. Inspection execution begins in Phase 2; see the [full product and implementation plan](SiteInspect-README.md).

## Quick start

Prerequisites are documented in [Phase 0 local setup](docs/development/phase-0-setup.md). Configure the database, demo accounts, and JWT signing key using the [Phase 1 setup guide](docs/development/phase-1-setup.md).

```powershell
dotnet restore SiteInspect.sln
dotnet build SiteInspect.sln --no-restore
dotnet test SiteInspect.sln --no-build

Set-Location src/SiteInspect.Web
npm ci
npm run build
npm run test:ci
```

For Visual Studio, open `SiteInspect.sln`, set `SiteInspect.Api` as the startup project, and run it. In Development, the API automatically launches Angular through ASP.NET Core SPA Proxy. Angular uses its development proxy for `/api`, `/health`, and `/hubs`.

From the command line, starting the API also starts Angular. Start the Worker separately when needed:

```powershell
dotnet run --project src/SiteInspect.Api
dotnet run --project src/SiteInspect.Worker
```

The API exposes liveness at `/health/live`, dependency readiness at `/health/ready`, and its development OpenAPI document at `/openapi/v1.json`. Authentication is available at `/api/auth`; manager and inspector read models are available at `/api/inspections`. Business API successes and failures use one `ApiResponse<T>` envelope while retaining correct HTTP status codes and stable handler-defined error codes.

For the MVP, publishing `SiteInspect.Api` builds Angular and includes it in the API's `wwwroot`, producing one deployable web application. The Worker remains a separate deployment.

## Solution structure

```text
src/
  SiteInspect.Domain          # Business modules and aggregate folders
  SiteInspect.Application     # Features organized into Commands and Queries
  SiteInspect.Infrastructure  # Identity, persistence, messaging, and health implementations
  SiteInspect.Api             # Attributed controllers and HTTP concerns
  SiteInspect.Worker
  SiteInspect.Web             # Standalone Angular feature folders and lazy routes
tests/
  SiteInspect.Domain.Tests
  SiteInspect.Application.Tests
  SiteInspect.IntegrationTests
```

Material architecture choices are recorded under `docs/architecture`.
