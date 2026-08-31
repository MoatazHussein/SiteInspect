# ADR 0001: Phase 0 architecture foundation

- Status: Accepted
- Date: 2026-08-29

## Context

SiteInspect needs a portfolio-grade architecture that supports a transactional inspection workflow, auditable state transitions, reliable asynchronous processing, and a narrowly scoped offline client. The implementation plan calls for a modular monolith and a separate worker, not microservices.

The project also needs a reusable persistence boundary without forcing every domain entity into an inheritance tree of entity-specific repositories.

## Decision

1. Target .NET 10 for all backend projects and Angular 22 for the standalone frontend.
2. Separate Domain, Application, Infrastructure, API, Worker, and Web responsibilities as described in the project plan.
3. Use `BaseEntity` for native UUIDv7 `Guid` identity and `AuditableEntity : BaseEntity` for created/modified UTC metadata. UUIDv7 values are generated when the entity is constructed, before EF begins tracking it.
4. Define one open generic `IRepository<TEntity>` contract in Application and one EF Core `Repository<TEntity>` implementation in Infrastructure. Consumers request `IRepository<Project>` or another closed generic through dependency injection; entities do not inherit repository behavior.
5. Keep the save/transaction boundary in `IUnitOfWork`. Repository mutation methods do not call `SaveChanges`.
6. Add feature-specific query or persistence interfaces only when a use case cannot be represented clearly by the generic contract. Do not grow the generic repository into a hidden query framework.
7. Keep Identity and EF implementations in Infrastructure. Identity's `ApplicationUser` is not a domain entity.
8. Register MassTransit/RabbitMQ transport conditionally in Phase 0. Consumers, outbox/inbox persistence, and retry policy belong to Phase 3.
9. Use application `Result`/`Result<T>` for expected failures and map every business API outcome to a unified `ApiResponse<T>` contract at the API edge. Preserve real HTTP status codes; do not serialize application results directly or convert failures into HTTP 200 responses.
10. Discover concrete `IEntityTypeConfiguration<TEntity>` implementations automatically. Entity configurations inherit shared key, audit, and optional concurrency rules while retaining control over entity-specific column lengths, indexes, constraints, and relationships.
11. Apply SQL Server `rowversion` only to mutable aggregate roots through `ConcurrentAuditableEntity`; append-only and immutable entities do not pay the concurrency cost.
12. Correlate HTTP logs and API responses through `X-Correlation-ID`. Defer client-visible distributed trace identifiers until end-to-end tracing is introduced.
13. Keep liveness and readiness endpoints minimal. They expose the standard health status rather than dependency-detail JSON.
14. Treat the API as the single web application entry point for the MVP. In Development, ASP.NET Core SPA Proxy starts the Angular development server and Angular proxies backend paths to the API. Publishing the API builds Angular and serves its static output from the same application.
15. Defer scheduled-job infrastructure. Start with a hosted periodic worker when the scheduled use case is implemented; introduce Quartz only if persistent schedules, misfire recovery, calendars, or clustering are required.

## Consequences

- Application code depends on persistence contracts rather than EF Core.
- A unit of work makes the transaction boundary explicit for later outbox writes.
- Generic CRUD remains small; specialized use cases can stay expressive rather than leaking `IQueryable` across layers.
- Audit metadata is applied centrally by the EF DbContext and uses `TimeProvider` plus the current-user abstraction.
- Expected application errors retain their stable handler-defined code while the API boundary owns HTTP status and the unified frontend response envelope.
- Optimistic concurrency conflicts can be represented consistently as `Concurrency.VersionConflict` and HTTP 409 when mutable aggregates are introduced.
- The API and Angular UI are released as one deployable web application for the MVP. The Worker remains independently deployable.
- Phase 0 contains no inspection entities, database migrations, seeded users, or message consumers.
