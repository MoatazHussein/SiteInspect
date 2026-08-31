# Phase 0 local setup

## Selected versions

| Component | Version |
|---|---:|
| .NET SDK | 10.0.400 |
| ASP.NET Core / EF Core | 10.0.11 |
| MassTransit | 9.0.0 |
| Angular CLI / build | 22.1.6 |
| Angular | 22.1.4 |
| NG-ZORRO | 22.0.1 |
| Node.js | 24.18.0 |
| npm | 11.16.0 |
| SQL Server | Local SQL Server Express (`.\SQLEXPRESS`) |
| RabbitMQ | Local service, default endpoint `rabbitmq://localhost` |

`global.json`, central NuGet package management, and `package-lock.json` keep builds repeatable. Angular 22 supports the selected Node.js 24 line.

## Environment status observed on 2026-08-29

- .NET 10 SDK: installed
- Angular 22 CLI: installed
- Node.js and npm: installed
- SQL Server Express: installed and running
- RabbitMQ Windows service: not found

RabbitMQ is therefore configured but disabled by default. This allows API, worker, unit tests, and the Angular shell to run before a broker is installed, without pretending that broker connectivity has been verified.

## Build and test

From the repository root:

```powershell
dotnet restore SiteInspect.sln
dotnet build SiteInspect.sln --no-restore
dotnet test SiteInspect.sln --no-build

Set-Location src/SiteInspect.Web
npm ci
npm run lint
npm run format:check
npm run test:ci
npm run build
```

## SQL Server

The checked-in development connection string uses Windows authentication and the local `SQLEXPRESS` instance. It contains no password. TLS encryption is disabled only for this local named instance because the detected SQL Server/Windows combination cannot negotiate the client driver's required encryption; non-local environments must override this setting and require encryption. Beginning with Phase 1, the readiness health check connects to the actual `SiteInspect` database.

Override the connection string with user secrets when the local instance name differs:

```powershell
dotnet user-secrets set "ConnectionStrings:Database" "Server=YOUR_INSTANCE;Database=SiteInspect;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True" --project src/SiteInspect.Api
dotnet user-secrets set "ConnectionStrings:Database" "Server=YOUR_INSTANCE;Database=SiteInspect;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True" --project src/SiteInspect.Worker
```

## RabbitMQ

After installing and starting RabbitMQ, enable it independently for both hosts. Store the password in user secrets; never commit it.

```powershell
dotnet user-secrets set "Messaging:Enabled" "true" --project src/SiteInspect.Api
dotnet user-secrets set "Messaging:RabbitMq:Password" "YOUR_LOCAL_PASSWORD" --project src/SiteInspect.Api

dotnet user-secrets set "Messaging:Enabled" "true" --project src/SiteInspect.Worker
dotnet user-secrets set "Messaging:RabbitMq:Password" "YOUR_LOCAL_PASSWORD" --project src/SiteInspect.Worker
```

The configured local username is `guest`; override it through `Messaging:RabbitMq:Username` when needed. MassTransit contributes broker health when the transport is enabled.

## Runtime verification

### Visual Studio

Open `SiteInspect.sln`, set `SiteInspect.Api` as the startup project, and start it. The API launches `npm start` through ASP.NET Core SPA Proxy and opens the Angular application at `http://localhost:4200`.

The Angular development server proxies these relative paths to the API:

- `/api`
- `/health`
- `/hubs`, including WebSocket upgrades

Frontend code should therefore call relative API paths rather than embedding the development backend address.

### Separate terminals

Starting the API also starts Angular in Development. Run the Worker separately when its host is needed:

```powershell
dotnet run --project src/SiteInspect.Api
dotnet run --project src/SiteInspect.Worker
```

Then open:

- Angular shell: `http://localhost:4200`
- API liveness: use the HTTP URL printed by `dotnet run`, then append `/health/live`
- API readiness: append `/health/ready`
- OpenAPI in Development: append `/openapi/v1.json`

Health responses intentionally use the standard text status. Detailed dependency information remains in server logs instead of the public response body.

If RabbitMQ is disabled, readiness reports SQL Server only. If it is enabled, broker health is included and must pass before the host is ready.

## Business API response contract

All `/api` business endpoints return the same JSON envelope while preserving the meaningful HTTP status code:

```json
{
  "isSuccess": false,
  "data": null,
  "errors": [
    {
      "code": "Inspection.NotFound",
      "message": "The inspection was not found."
    }
  ],
  "correlationId": "client-or-server-correlation-id"
}
```

Handlers return `Result` or `Result<T>` with stable error codes. The API boundary translates that result into `ApiResponse<T>` and the appropriate `4xx` status. Successful commands use `ApiResponse<object?>` with `data: null`; successful queries populate `data`.

Unhandled exceptions are logged by the global exception handler and returned as the same envelope with HTTP `500` and the non-sensitive `Common.UnexpectedError` code. JWT challenges (`401`), forbidden responses (`403`), malformed requests, unsupported methods or media types, and unknown `/api` routes also use this contract.

Health checks, static Angular files, OpenAPI, and future SignalR transport messages are infrastructure endpoints and intentionally do not use the business API envelope.

## MVP publishing and hosting

Publish the API as the single web application:

```powershell
dotnet publish src/SiteInspect.Api --configuration Release --output artifacts/SiteInspect.Api
```

Publishing runs `npm ci` and the Angular production build, then copies the browser output into the published API's `wwwroot`. The deployed ASP.NET Core process serves the Angular application, API endpoints, health checks, and future SignalR hubs from one origin. Deploy the Worker separately when later phases give it active work.

## Scheduled work

Quartz is intentionally not installed in Phase 0. The later overdue-inspection scanner should begin as a hosted periodic worker. Adopt Quartz only if that phase establishes requirements for persistent schedules, cron calendars, misfire handling, or multi-node scheduler coordination.

## Git initialization

This workspace did not contain Git metadata when Phase 0 began. Initialize it locally when desired:

```powershell
git init
git add .
git status
```

Review the staged files before creating the initial commit.
