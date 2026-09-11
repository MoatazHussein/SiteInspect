# Phase 4 Stages 4.0–4.2

This increment establishes the MVP rules, adds the PWA application shell and connection status,
and saves inspector checklist outcomes and notes to IndexedDB while offline.

## Included

- Stage 4.0: the scope and safety rules in ADR 0004.
- Stage 4.1: static application-shell caching and an online/offline indicator.
- Stage 4.2: local checklist draft save and restore for the same user and inspection.

Manual synchronization, conflict resolution, and local-draft cleanup are covered in the
[Stages 4.3–4.4 guide](phase-4-stages-3-4.md).

## Manual UI check

1. While online, sign in as the assigned inspector and open an `InProgress` inspection.
2. In browser developer tools, switch the network to Offline. Confirm the header and warning banner
   show the offline state.
3. Change an outcome or note, press **Save changes**, and wait for **Saved on this device**. In developer tools, confirm the
   draft exists under Application > IndexedDB > `siteinspect-offline` > `inspection-drafts`.
4. Return online. Confirm the local-draft notice remains and offers an explicit **Sync now** action.

Use a production build when checking service-worker installation and application-shell caching;
Angular does not enable the service worker in the development build.
