# Manual checklist saving (MVP)

Checklist outcomes and notes now require **Save changes**. There is no autosave or checklist
Cancel action. The top action bar stays visible while scrolling through observations.

## Manual UI check

1. Sign in as the assigned inspector and open an `InProgress` inspection. Edit a note or outcome,
   including using **Clear**. Confirm **Unsaved changes** appears and no draft-save request occurs
   until you press **Save changes**.
2. Scroll through the checklist and press the sticky **Save changes** button. With a slow network,
   confirm outcomes, notes, Clear, and photo upload cannot be edited during saving. After success,
   confirm **Saved** appears and the save button is disabled until another edit.
3. Edit again, then try the register link, browser Back, refresh, or Sign out. Choose to stay and
   confirm the edits remain (and Sign out has not cleared the session). Choosing to leave discards
   unsaved edits. Refresh/close uses the browser's own warning text.
4. While the checklist is open, switch to Offline. Edit and explicitly save. Confirm **Saved on
   this device** and the draft in Application > IndexedDB > `siteinspect-offline` > `inspection-drafts`.
   Before Save, IndexedDB retains only the previous saved draft, if any. Once saved, leaving needs
   no unsaved-edit warning. Reconnect and choose **Sync now** to update the API and remove the draft.
5. If a save fails, confirm edits remain unsaved and can be retried. A row-version conflict still
   blocks editing instead of overwriting newer server data. Existing device-draft conflict recovery
   and the confirmed **Discard local changes / Use server version** actions are unchanged.

Saving a checklist does not submit the inspection. Photos and final submission remain separate
online actions, and require saved/synchronized checklist changes first. Browser warnings cannot
protect edits from a crash or forced shutdown; press Save changes before leaving the device.
