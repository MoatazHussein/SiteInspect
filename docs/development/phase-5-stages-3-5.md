# Phase 5 stages 5.3–5.5

## User verification

1. Sign in as Manager: Dashboard opens. Confirm counts, last-loaded time, overdue inspections,
   and outstanding corrective actions. Lists show up to 10 rows with totals, earliest due first.
2. Choose a project and press Apply filters. Cards and both lists should only contain that project.
   Choose an inspection status: action rows must belong to inspections with that status.
3. Set Due from and Due to to the same date. Items due anywhere on that local calendar day match.
   Inspection metrics use inspection deadlines; action metrics use action deadlines. Reverse the
   dates: an error appears without sending the request. Reset restores the unfiltered dashboard.
4. Open an overdue inspection link and confirm the inspection number. Open an action link and
   review its corrective-action panel on the inspection page. Return using Dashboard in the sidebar.
5. Submit/complete an inspection or approve an action as allowed by the workflow, then refresh
   Dashboard and verify changed counts and list membership.
6. Choose filters with no matches: expect zero cards and explicit empty-list messages. Disconnect
   after loading: old results are labeled offline and refresh/filter controls are disabled. Reconnect
   and refresh. On request failure, expect an error with Refresh available to retry.
7. Sign in as Inspector and Contractor: keep their original landing pages; Dashboard stays hidden
   and direct dashboard API access returns 403. Unauthenticated API access returns 401.

## API

GET /api/dashboard/summary accepts optional ProjectId, Status, DueFromUtc, DueBeforeUtc.
The upper date bound is exclusive. Invalid enum values, empty project identifiers, and reversed/equal
timestamp bounds fail validation. Unknown nonempty project IDs simply return no matches.
The response includes OverdueInspectionItems and OutstandingActionItems with a maximum of 10 each.
Cards count all matches, not only the returned rows. Manager authorization remains required.

Automated unit/integration tests are excluded by request. Build and lint checks do not substitute
for the manual browser and database checks above.
