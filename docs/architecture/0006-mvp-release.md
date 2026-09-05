# ADR 0006: Portfolio MVP release

## Decision

Deploy one ASP.NET Core application containing the Angular production build, using one SQL Server
database and one persistent private attachment directory. The initial documented host is Windows IIS
at a dedicated site root over HTTPS. RabbitMQ and the separate Worker are unnecessary for the current
synchronous MVP and stay disabled. This release uses fictional portfolio data only.

## Stages

- 6.0: one-instance deployment, synthetic data, manually shared demo access and release checklist.
- 6.1: explicit production settings, HTTPS/HSTS, response headers, no-store API responses,
  sign-in throttling (10 attempts/minute/IP), external signing key and attachment location.
- 6.2: friendly network/rate-limit messages, portfolio label, refresh-session concurrency fix.
- 6.3: explicit database initialization/seed commands, durable attachment path, documented backup
  and rollback. Normal production startup never migrates, seeds, or resets demo passwords.
- 6.4: console request/error logging with correlation IDs and existing live/SQL readiness endpoints.
  Host captures and rotates logs; no monitoring service is required for the first release.
- 6.5: timestamped local publish folder, production Angular bundle checks, IIS deployment guide.
- 6.6: host-specific smoke checklist and portfolio handoff. Public deployment and go-live verification
  require the owner's hosting target, secrets, database and HTTPS binding.

## Configuration and scope

Development retains its local database, demo accounts and automatic initialization. Development
configuration is excluded from publishing. Other environments require a database connection,
an explicit AllowedHosts value, an external absolute attachment path and a signing key of at least
32 UTF-8 bytes. Use a random persistent secret, not a memorable password.

Attachments must be stored outside the deployment directory and outside any IIS static-file mapping.
Back up SQL and attachments together while writes are stopped. Browser offline drafts are local to
each visitor and are not part of server backups. Signing out preserves unsynchronized drafts.

The rate limiter is in-process, resets on restart and is not distributed. Identity lockout already
handles repeated incorrect passwords. No automatic database resets or public administrative reset
endpoint is introduced. A shared demo login allows visitors to modify shared fictional data.

IIS integration handles forwarded scheme/client information for the standard IIS deployment. An
additional reverse proxy requires an explicit trusted-proxy configuration; do not trust arbitrary
forwarding headers or disable HTTPS to work around a redirect loop.

No new unit or integration tests are added. Build, lint, publish artifact inspection and the manual
host checklist provide the release checks for this increment.
