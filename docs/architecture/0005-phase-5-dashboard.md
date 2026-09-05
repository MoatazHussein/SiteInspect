# ADR 0005: MVP operational dashboard

## Status

Accepted for Phase 5 stages 5.0–5.5.

## Rules

Managers can view all projects, matching existing manager access. The API policy, application
handler, and frontend route enforce Manager access. Counts use current persisted records, optionally
filtered by project, parent inspection status, and due-date range.

| Metric | Definition |
| --- | --- |
| Active inspections | Assigned, InProgress, Submitted, or CorrectiveActionsOpen |
| Overdue inspections | Assigned or InProgress with DueAtUtc strictly before current UTC time |
| Completed inspections | Completed within the applied filters |
| Outstanding actions | Open or ReadyForReview |
| Awaiting review | ReadyForReview |
| Overdue actions | Not Closed and DueAtUtc strictly before current UTC time |

Cancelled inspections are excluded from inspection counts. Submitted inspections have met the
submission deadline; corrective-action deadlines are counted separately. Overdue and review counts
overlap with active or outstanding totals; the six cards must not be added together.

Empty tables produce zeros. A single TimeProvider UTC timestamp is used for both overdue calculations.
Two sequential database aggregate queries and two bounded list queries avoid loading whole entities. This is an operational view,
not a transactional report snapshot. No schema migration is needed.

The page loads on entry, Apply filters, Reset, and Refresh, with a timestamp and loading/error states.
Each request replaces the previous results so failed filter requests cannot show unrelated totals.
Offline drafts do not affect counts until synchronized.
There is no background refresh or offline API caching.

## Stage boundaries

- 5.0: metrics and manager access rules.
- 5.1: GET /api/dashboard/summary with the existing API envelope and query pattern.
- 5.2: six responsive manager cards and a sidebar entry.
- 5.3: earliest-due 10 overdue inspections and 10 outstanding actions, with total counts and
  inspection links. Ordering uses due date and identifier for stable ties.
- 5.4: project, inspection-status, and due-date filters apply to cards and lists. Actions inherit
  the project and status of their parent inspection but use their own due dates. API dates use
  an inclusive DueFromUtc and exclusive DueBeforeUtc. UI end dates include the full local day.
- 5.5: Manager sign-in lands on Dashboard; Inspector/Contractor destinations remain role-specific.
  Includes loading, retry, offline, and empty-list states and manual verification instructions.

No charts, exports, background updates, schema changes, or additional tests are introduced.
