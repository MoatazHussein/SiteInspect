# Phase 5 stages 5.0–5.2

## Manual verification

1. Sign in as Manager and select Dashboard. Confirm six cards and the last-loaded timestamp.
2. Create an inspection and refresh: active increases. Start it: active stays unchanged.
   Submit it: active stays unchanged and it stops being overdue. Complete it when allowed:
   active decreases and completed increases. Cancel an eligible inspection: it leaves active counts.
3. Create a corrective action: outstanding increases. Submit its response: awaiting review increases
   while outstanding stays unchanged. Approve: both decrease. Reject: review decreases while
   outstanding stays unchanged. A past-due non-closed action also contributes to overdue actions.
4. With an empty database, expect zeros. Switch offline after loading: expect a stale-summary notice
   and disabled Refresh. Reconnect and refresh to retrieve current counts.
5. Sign in as Inspector or Contractor: Dashboard is hidden and direct /dashboard access is blocked.
   GET /api/dashboard/summary returns 403 for those users and 401 without authentication.

See [metric definitions](../architecture/0005-phase-5-dashboard.md). This increment adds no unit or
integration tests. Frontend build/lint and the solution build accompany these manual checks.
