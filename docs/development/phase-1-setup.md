# Phase 1 local setup

Phase 1 introduces the initial SQL schema, Identity users and roles, rotating refresh sessions, demo data, and role-scoped inspection read controllers.

## Configure local secrets

Choose a different password for each fictional demo account and a stable JWT signing key. All demo passwords must satisfy the configured Identity password policy.

```powershell
dotnet user-secrets set "Jwt:SigningKey" "YOUR_LONG_RANDOM_SIGNING_KEY" --project src/SiteInspect.Api
dotnet user-secrets set "DemoData:Users:Manager:Password" "YOUR_MANAGER_DEMO_PASSWORD" --project src/SiteInspect.Api
dotnet user-secrets set "DemoData:Users:Inspector:Password" "YOUR_INSPECTOR_DEMO_PASSWORD" --project src/SiteInspect.Api
dotnet user-secrets set "DemoData:Users:Contractor:Password" "YOUR_CONTRACTOR_DEMO_PASSWORD" --project src/SiteInspect.Api
```

Demo accounts:

| Role | Email |
|---|---|
| Manager | `manager@siteinspect.demo` |
| Inspector | `inspector@siteinspect.demo` |
| Contractor | `contractor@siteinspect.demo` |

The repository currently provides MVP demo-password defaults in `appsettings.json`. Treat them as public demo credentials and override all three through user-secrets or environment configuration before exposing a deployment. The seeder never logs passwords and synchronizes the configured values only for the three fixed fictional demo accounts.

MediatR 14 discovers its license key from the `MEDIATR_LICENSE_KEY` environment variable. Do not commit the key to application settings or source control.

## Database migration

Restore the repository-local EF tool and apply the checked-in migration:

```powershell
dotnet tool restore
dotnet restore SiteInspect.sln
dotnet ef database update --project src/SiteInspect.Infrastructure --startup-project src/SiteInspect.Api
```

API startup always performs two explicit, ordered operations: apply pending migrations, then run the demo seeder. They use separate services so schema migration and data creation remain independently testable even though both always run for the MVP.

Demo seeding is idempotent and always creates or synchronizes:

- Manager, Inspector, and Contractor roles and users
- Harbor View Apartments project
- Building A → Floor 5 → Eastern Corridor hierarchy
- Immutable Electrical and Safety Inspection template version 1 with ten requirements
- Thirty deterministic inspections spanning every status, overdue and future due dates, partial and completed checklists, failed findings, and enough rows to exercise pagination

## Authentication session

The browser receives a 30-minute access JWT in the API response and a seven-day opaque refresh token in an `HttpOnly`, same-site cookie. SQL Server stores only the SHA-256 refresh-token hash. Refreshing rotates the token; logout revokes it and clears the cookie.

```text
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

Angular keeps the access JWT in memory, restores the session through the refresh cookie after a page reload, and adds the JWT to API requests through a functional interceptor. Production hosting must use HTTPS so the refresh cookie is transported securely.

## Inspection read endpoints

```text
GET /api/inspections
GET /api/inspections/filter-options
GET /api/inspections/{inspectionId}
```

Managers see all inspections. Inspectors see only inspections assigned to their user ID. Contractors receive `403` for inspection reads until their corrective-action workspace is introduced in Phase 3.

The list supports pagination, search, project, location, inspector, status, and due-date filters. Queries project dedicated response DTOs and never serialize EF entities.
