# Phase 3: Stages 3.3–3.4 MVP

## Included

- Contractor-only workspace listing actions assigned to the signed-in contractor.
- Mandatory resolution notes and submission to ReadyForReview.
- Manager approval to Closed or rejection back to Open with a required reason.
- Explicit manager completion after every failed observation has a closed action.
- Direct completion of submitted inspections with no failed observations.
- Row-version conflict protection on every action and inspection mutation.

Corrective-action evidence, reassignment, comments, full activity history, notifications and
reliable messaging remain deferred. The current UI retains the latest rejection reason only.

## API

| Method | Route | Role | Purpose |
|---|---|---|---|
| GET | /api/corrective-actions/mine | Contractor | List actions assigned to the current contractor |
| POST | /api/corrective-actions/respond | Contractor | Submit resolution notes for manager review |
| POST | /api/corrective-actions/approve | Manager | Approve and close an action |
| POST | /api/corrective-actions/reject | Manager | Return an action to Open with a reason |
| POST | /api/inspections/complete | Manager | Explicitly complete an eligible inspection |

Every mutation requires the current rowVersion. Stale writes return HTTP 409 and the user can
refresh before retrying.

## Manual UI flow

1. As a manager, create an action for every failed observation and assign the demo contractor.
2. Sign in as the assigned contractor and open Corrective actions.
3. Enter resolution notes for an Open action and select Submit for review.
4. Sign in as manager, open the inspection and find the ReadyForReview action.
5. Use Request changes with a reason; sign back in as contractor and verify it returned to Open.
6. Update the resolution notes and submit again.
7. As manager, select Approve & close.
8. Repeat until every failed observation has a Closed action.
9. Select Complete inspection and verify the inspection status becomes Completed.

For an inspection without failed observations, the manager can complete it directly after submission.

## Verification

From the repository root:

```powershell
dotnet build
Set-Location src/SiteInspect.Web
npm run build
```
