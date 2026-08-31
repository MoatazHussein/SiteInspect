# ADR 0002: Phase 1 identity sessions and inspection read models

- Status: Accepted
- Date: 2026-08-29

## Context

Phase 1 must establish authentication and the first durable business schema without pulling inspection execution, corrective actions, messaging, or offline synchronization forward. The Angular PWA needs a session design that can survive page reloads and later support reconnect synchronization without placing a long-lived bearer credential in browser storage.

## Decision

1. Keep ASP.NET Core Identity users in Infrastructure and use the fixed MVP roles Manager, Inspector, and Contractor.
2. Assign future corrective actions directly to a contractor user in v0.1. Contractor-company membership remains post-MVP.
3. Issue 30-minute JWT access tokens and seven-day opaque refresh tokens. Keep access tokens in Angular memory and refresh tokens in `HttpOnly`, same-site cookies.
4. Store only SHA-256 refresh-token hashes in SQL Server. Rotate on refresh, revoke on logout, reject expired or security-stamp-invalid sessions, and revoke active sessions when a previously revoked token is reused.
5. Keep a separate configurable password for each demo role. The repository defaults are public MVP demo credentials and must be overridden before exposing a deployment. Seed through Identity managers so password hashing, normalization, roles, and security stamps follow Identity behavior.
6. Run migration and idempotent demo seeding as separate, ordered startup operations in every MVP environment.
7. Add Project, ProjectLocation, InspectionTemplate, InspectionTemplateItem, Inspection, and InspectionObservation in Phase 1. Defer attachments, corrective actions, activity entries, and reliability entities.
8. Treat template versions as immutable. Each inspection observation owns the selected template item's section, question, order, required flag, and severity snapshot.
9. Apply SQL Server `rowversion` to Inspection, the mutable Phase 1 aggregate root. Encode the token as Base64 in read DTOs.
10. Implement inspection reads as direct EF projections behind `IInspectionReadService` and Application query handlers. Managers see all inspections; inspectors are constrained by assigned user ID; contractors have no inspection read policy.
11. Use paged list responses with search, project, location, inspector, status, and due-date filters. Sort by due date and inspection number.
12. Use attributed API controllers, MediatR command/query requests and handlers, an Application validation pipeline backed by FluentValidation, manual DTO projections, Angular signals, functional HTTP interceptors, and route guards. Do not introduce AutoMapper or NgRx without a demonstrated need.
13. Organize the Domain by business module and aggregate, the Application by feature then Commands/Queries, Infrastructure by capability, and Angular by lazy feature routes with feature-owned models, services, pages, and components.

## Consequences

- Normal API requests validate the short-lived JWT without a refresh-session database lookup.
- Refresh and logout operations use server-side state, allowing per-session revocation and rotation.
- Angular route guards improve navigation but do not replace API authorization policies.
- Historical inspection questions remain stable even when a later template version is introduced.
- The read layer can evolve independently of later aggregate command behavior.
- Migration and demo seeding remain independently replaceable in tests despite both running at startup.
- MediatR and validator assembly scanning remove per-handler registrations. The validation behavior throws a cataloged request-validation exception that the global exception handler maps to the same unified API envelope; feature handlers continue to use `Result` for expected business outcomes.
- Phase 2 can add inspection transitions without redesigning Identity, snapshots, pagination, or concurrency tokens.
