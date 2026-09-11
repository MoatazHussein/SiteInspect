# Portfolio MVP deployment

The deployable is one API application with Angular in wwwroot. Use a dedicated IIS site at `/`,
one SQL Server database, and a private persistent upload directory. Keep Messaging:Enabled=false;
the separate Worker is not needed. Do not use real client or construction-site data for the demo.

## 1. Create a local release

From the repository root on the development computer (.NET 10 SDK and Node/npm installed):

```powershell
./scripts/publish-mvp.ps1
```

This runs restore, npm ci, production Angular compilation and dotnet publish. It creates a unique
folder under artifacts without deleting any previous release. It checks for the API DLL, web.config,
index.html, service worker, generated worker manifest and PWA manifest. It does not start a server,
modify a database, install IIS, or upload anything. No tests run.

## 2. Prepare the host

### Updating an existing IIS folder

Stop the SiteInspect app pool, then publish and copy in one command:

```powershell
.\scripts\publish-mvp.ps1 -DeployPath 'D:\IIS Sites\SiteInspect'
```

Or copy an already published release:

```powershell
.\scripts\deploy-mvp.ps1 -ReleasePath '.\artifacts\YOUR-RELEASE-FOLDER' -DestinationPath 'D:\IIS Sites\SiteInspect'
```

The copy preserves existing root `web.config`, `appsettings.json`, and
`appsettings.Production.json` byte-for-byte. Missing files are copied for fresh installs.
Publish output always contains the complete configuration. Merge any new required settings
into the server files manually. The scripts show live command output and do not manage IIS.
Apply required migrations, then start the app pool. Copying requires write access to the IIS folder.
Destination-only files are retained; this is an in-place update, not a clean release or rollback.

### Host setup

Install IIS and the .NET 10 Hosting Bundle on a Windows host. Use a dedicated application pool with
No Managed Code, and point the site at the published folder. Bind your hostname with a valid HTTPS
certificate on port 443. Keep the generated web.config. Node is needed to build, not to run this package.
Follow [Microsoft's IIS hosting guide](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0).

Create a separate SQL database for the portfolio. Create a private directory such as
`D:\SiteInspectData\attachments`; grant Modify only to the application-pool identity and administrators.
Do not make it an IIS virtual directory. Grant the application pool Read/Execute on the release folder.
Do not host from a temporary or ephemeral filesystem, and keep a single app instance while using local uploads.

## 3. Set server configuration

Set these environment variables for the IIS application process, using server configuration or a
protected server-only web.config environmentVariables section. Do not commit or distribute secrets.
Double underscores map to configuration colons. Restart the pool after changing environment variables.

| Variable | Value |
| --- | --- |
| ASPNETCORE_ENVIRONMENT | Production |
| AllowedHosts | Your exact hostname, e.g. portfolio.example.com (no scheme or port) |
| ConnectionStrings__Database | Host's SQL connection string; use encrypted SQL with a trusted server certificate |
| Jwt__SigningKey | Random persistent secret of at least 32 bytes; a base64-encoded 64-byte random key works |
| Storage__AttachmentsPath | Absolute private persistent folder outside the release folder |
| Messaging__Enabled | false |

Generate the signing key in a private server session or password manager, and keep it stable across
deployments. Never include it in frontend environment files. Production startup reports missing setting
names without printing their values. Backend Development mode is not the public-host configuration.

MediatR is pinned centrally to 12.5.0 under Apache-2.0, which permits local and remote production
deployment without a MediatR license key, subject to the license terms. Preserve applicable license
and attribution notices when distributing the application. Review licensing before upgrading to 13
or later; see the [maintainer's release notes](https://github.com/LuckyPennySoftware/MediatR/releases/tag/v13.0.0).
Republish and redeploy the API to replace any previously deployed MediatR 14 binaries.

npm reported one existing moderate audit finding during the earlier publish; no automatic dependency
upgrades were applied.

## 4. Initialize once, before starting IIS

Stop the application pool during migration. In a server console with the same configuration variables,
change to the published folder. Use a database identity allowed to alter the schema for this step:

```powershell
dotnet ./SiteInspect.Api.dll --Database:InitializeOnly=true
```

The command applies migrations and exits without serving HTTP. For first-time fictional demo accounts
and data, also supply these server-only values:

- DemoData__Users__Manager__Password
- DemoData__Users__Inspector__Password
- DemoData__Users__Contractor__Password

Use distinct strong passwords meeting the configured Identity requirements. Then run:

```powershell
dotnet ./SiteInspect.Api.dll --DemoData:SeedOnly=true
```

This creates manager@siteinspect.demo, inspector@siteinspect.demo, inspector2@siteinspect.demo and
contractor@siteinspect.demo plus sample projects, templates and inspections. Both inspectors share
the configured inspector seed password. Explicit reseeding synchronizes these passwords again; it
does not erase visitor records. Do not leave either command flag enabled in IIS configuration.

For normal hosting, use a runtime SQL identity with data read/write permissions but no schema-change
permission. Seed passwords are not needed during normal runtime. Start the application pool.

## 5. Logs, monitoring and backups

- GET /health/live reports whether the process responds. GET /health/ready checks SQL connectivity.
  Neither reports detailed secrets; readiness does not verify attachment permissions or migration version.
- Application request/error logs go to stdout and include X-Correlation-ID. Configure the host to
  capture and rotate stdout. With IIS, temporarily enable the generated aspNetCore stdoutLogEnabled
  option to diagnose startup; IIS stdout files are not automatically rotated, so disable it after
  diagnosis or configure host retention. Keep IIS access logs with a small retention window.
- For a failed API call, copy its response X-Correlation-ID and locate the corresponding server log.
  Do not enable request-body, token, password or EF sensitive-data logging on the public demo.
- Before each upgrade, stop the app pool and back up SQL plus the entire attachment directory. Use
  your host's SQL backup facility or SSMS Tasks > Back Up; SQL Express needs an external schedule.
  Keep a daily backup and a pre-release backup outside the host. Confirm restore works in a separate
  database and directory before relying on it. Restrict backup access because it includes account data.
- Deploy to a new release folder, apply required migrations, switch the IIS physical path and restart.
  Preserve the persistent attachment directory. Never overwrite it with a deployment package.
- Roll back to the previous release folder if schema compatibility allows. If not, restore the paired
  SQL/upload backup with the site stopped; this loses changes after the backup. Do not automatically
  reverse migrations or reset the live database.

## 6. Manual go-live check

1. HTTP redirects to HTTPS. Open /login and a direct /dashboard URL. Static files and ngsw.json load;
   development OpenAPI is unavailable. API responses use no-store and include a correlation ID.
2. Confirm /health/live and /health/ready return 200. Sign in/out, refresh the page and verify session
   restoration. Inspector/Contractor cannot call the Manager dashboard API.
3. Manager creates an inspection; Inspector starts it, edits checklist, uploads/downloads a photo,
   and submits it when requirements are satisfied. Manager creates an action; Contractor responds;
   Manager approves and completes the inspection.
4. On an open InProgress checklist, go offline, edit, reconnect and Sync now. Confirm the local record
   is removed after success. Use a separate session to check stale-version handling.
5. Check dashboard counts/filters and the narrow/mobile layout. Disconnect during a request and
   confirm a useful error. Inspect Service Workers and Cache Storage after the first production visit.
6. Restart the pool: existing users, photos, records and sessions remain usable. Confirm seeding did
   not reset passwords. Repeat photo download after switching release directories.
7. Take portfolio screenshots and record a short workflow demo. Include the public URL, repository
   link, stack and MVP limitations in the portfolio. Share demo credentials deliberately and only
   for the isolated fictional dataset; never reuse personal passwords.

## Portfolio description

SiteInspect is a .NET 10 and Angular inspection workflow MVP. It supports role-based inspection
assignment, checklists, protected photo evidence, corrective-action review, offline outcome/note drafts
with manual synchronization and optimistic concurrency, plus a manager dashboard.

Current limits: one host instance, online-only attachments and submission, manually refreshed dashboard,
no offline first-time sign-in or inspection fetch, no field-level conflict merge, and no notifications.
The release is ready for owner-configured hosting; this guide is not evidence that a public site is live.
