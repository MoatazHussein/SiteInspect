# SiteInspect

SiteInspect is a portfolio-grade field inspection and corrective-action platform built as a .NET 10 modular monolith with an Angular 22 PWA frontend and a separate background worker.

Phase 2 supports the synchronous inspection-execution workflow: Manager assignment, Inspector checklist drafts, protected photo evidence, submission validation, and immutable submitted observations.

Phase 3 Stages 3.0–3.4 provide the synchronous corrective-action MVP: manager assignment,
contractor responses, manager review, and explicit inspection completion. See the
[MVP rules and stage boundaries](docs/architecture/0003-phase-3-corrective-actions.md),
[Stages 3.0–3.2 guide](docs/development/phase-3-stages-0-2.md), and
[Stages 3.3–3.4 guide](docs/development/phase-3-stages-3-4.md).
Evidence, full history, notifications, and reliable messaging remain deferred.

Phase 4 Stages 4.0–4.4 provide the offline-first inspection MVP: a cached application shell,
connection status, device-local checklist drafts, explicit synchronization, and simple conflict
recovery. See the
[offline draft decision](docs/architecture/0004-phase-4-offline-inspection-drafts.md) and
[Stages 4.0–4.2 guide](docs/development/phase-4-stages-0-2.md) and
[Stages 4.3–4.4 guide](docs/development/phase-4-stages-3-4.md).

## Quick start

Phase 6 prepares the portfolio release: explicit production configuration and initialization,
private persistent uploads, login throttling, HTTPS, and a publish script. See the
[release decision](docs/architecture/0006-mvp-release.md) and
[hosting, backup and go-live guide](docs/development/phase-6-hosting.md).

Phase 5 stages 5.0–5.5 add six manager dashboard counts, overdue/outstanding lists, project/status/due-date
filters, and manual refresh. Managers land on the dashboard after signing in. See the
[dashboard rules](docs/architecture/0005-phase-5-dashboard.md) and
[initial verification guide](docs/development/phase-5-stages-0-2.md) and
[remaining stages guide](docs/development/phase-5-stages-3-5.md).

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
