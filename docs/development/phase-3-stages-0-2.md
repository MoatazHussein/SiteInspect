# Phase 3: Stages 3.0–3.2

## Included

- Agreed MVP rules in ADR 0003.
- CorrectiveAction aggregate with Open, ReadyForReview and Closed states.
- SQL Server mapping, row versions, audit fields, contractor FK, unique observation index,
  and composite FK ensuring an action's observation belongs to its inspection.
- Manager creation and assignment for failed observations on submitted inspections.
- Atomic action creation and inspection transition to CorrectiveActionsOpen.
- Manager panel displaying actions, contractors, work descriptions, due dates and statuses.
- Validation, authorization, concurrency and failure/retry UI states.

Response and review transitions exist in the domain foundation only. Contractor UI/API,
evidence uploads, manager review/completion endpoints, activity history and messaging are not delivered here.

## Running locally

Build the solution and restart the API using the existing Phase 1 configuration.
The API's existing startup migration mechanism applies Phase3CorrectiveActions automatically.
RabbitMQ can remain disabled.

The migration adds a table and observation alternate key; it does not overwrite inspection data.
Review and back up any non-development database before applying migrations there.

## API

All three endpoints require the Manager role:

| Method | Route | Purpose |
|---|---|---|
| POST | /api/corrective-actions | Create and assign an action |
| GET | /api/corrective-actions?inspectionId={id} | List actions for one inspection |
| GET | /api/corrective-actions/contractors | List contractor users |

Create request fields: inspectionId, observationId, contractorId, description, dueAtUtc and rowVersion.
The rowVersion is the inspection version, not an action version.
Successful creation returns HTTP 201 with id, inspectionStatus and the new inspection rowVersion.
All responses use the existing ApiResponse envelope.

Invalid input returns 400; missing inspection returns 404; duplicate action, invalid lifecycle state
or stale version returns 409. Every creation updates the inspection version, even if its status was
already CorrectiveActionsOpen. The UI requires reloading after a conflict rather than silently retrying a stale write.

## Manual smoke test

1. Sign in as a manager and open a submitted inspection containing failed observations.
2. In Corrective actions, choose a failed observation without an existing action.
3. Enter the work required, select the contractor and choose a future local date/time.
4. Select Create & assign action.
5. Verify the saved action, contractor and due date appear, and the inspection status is CorrectiveActionsOpen.
6. Refresh: the action remains and its observation is no longer available for another action.
7. Create an action for another failed observation to verify subsequent additions.
8. Open the same inspection in two tabs; create in one, then submit the stale form in the other.
   The stale tab must show a reload-required conflict.
9. Inspectors and contractors must receive 403 when calling these manager-only endpoints.

There is intentionally no contractor response, close or complete button yet.

## Automated verification

Run the normal backend and frontend tests:

```powershell
dotnet test SiteInspect.sln
Set-Location src/SiteInspect.Web
npm run build
npm run test:ci
npm run lint
```

SQL-backed API tests are opt-in. From the repository root:

```powershell
$env:SITEINSPECT_TEST_SQLSERVER = 'Server=.\SQLEXPRESS;Integrated Security=True;Encrypt=False;Connect Timeout=15'
dotnet test tests/SiteInspect.IntegrationTests
Remove-Item Env:SITEINSPECT_TEST_SQLSERVER
```

Use an account allowed to create/drop databases. The fixture replaces the configured database name
with a unique SiteInspect_Phase3Tests_ identifier, applies the real migrations, seeds isolated data,
and removes only that database afterward. It never uses the application database for test data.
The test-only authentication scheme is confined to the integration test assembly.
Without the environment variable, SQL-backed tests are reported as skipped.
