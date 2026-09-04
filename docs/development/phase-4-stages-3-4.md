# Phase 4 Stages 4.3–4.4

This increment completes the MVP offline-draft flow with explicit synchronization, conflict
recovery, and local cleanup.

## Included

- Stage 4.3: **Sync now** sends the complete local checklist draft through the existing online
  draft endpoint.
- Stage 4.4: row-version conflicts preserve the local copy and block editing. **Use server version**
  explicitly discards the local draft and reloads current server data.
- A successful synchronization or explicit discard removes the IndexedDB record.

Automatic/background synchronization and field-level conflict merging are intentionally excluded.

## Manual UI check

1. Open an assigned `InProgress` inspection while online, switch the browser network to Offline,
   edit outcomes or notes, and wait for **Saved on this device**.
2. Reconnect. Press **Sync now** and confirm the local notice disappears, the IndexedDB record is
   removed, and normal photo and submission actions become available.
3. To exercise a conflict, create another offline draft, then update the same inspection from a
   separate browser session before pressing **Sync now** in the first session.
4. Confirm the first session preserves its local draft, disables editing, and displays **Use server
   version**. Confirming that action removes the local record and reloads the newer server state.
