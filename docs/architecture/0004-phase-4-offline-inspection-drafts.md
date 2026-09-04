# ADR 0004: Phase 4 offline inspection drafts

## Status

Accepted for Stages 4.0–4.4.

## Context

Inspectors can lose connectivity while completing a checklist. The MVP needs to protect their
work without introducing background synchronization, broad API caching, or a second server-side
workflow.

## Decision

- Offline editing is limited to an inspection that the assigned inspector already opened while it
  was `InProgress`.
- Only checklist outcomes and notes are stored offline. Starting or submitting an inspection,
  photo upload and download, manager actions, and corrective actions remain online-only.
- One local draft is stored in IndexedDB for each signed-in user and inspection. It includes the
  observations, the server row-version token, and the local save time.
- The service worker caches the application shell and static assets only. Authenticated API
  responses are not cached.
- A local draft is restored only for the same user and inspection. If its row version differs from
  the currently loaded server version, the draft is preserved and editing is blocked.
- Reconnecting does not automatically upload a local draft. The inspector explicitly chooses
  **Sync now**.
- Synchronization sends the stored row-version token through the existing draft endpoint. A
  successful save removes the local draft. A concurrency conflict preserves it and blocks further
  editing.
- Conflict recovery is intentionally simple for the MVP: the inspector may keep the device copy or
  explicitly discard it and reload the server version. Field-level merging is deferred.

## Consequences

Inspectors get durable local saves with a small, understandable scope. The application never
silently overwrites newer server data. A local draft cannot be submitted or used with photo
mutations until synchronization succeeds.

IndexedDB contains inspection notes on the local browser profile. Drafts are removed after a
successful synchronization or an explicit discard. Signing out does not delete pending work;
access is separated by user identifier and the normal application authentication boundary still
applies.
