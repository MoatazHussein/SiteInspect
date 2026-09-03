# ADR 0003: MVP corrective actions

Status: Accepted (Stages 3.0–3.4 MVP)

## Product rules

- Managers create actions only for failed observations on Submitted or CorrectiveActionsOpen inspections.
- One action at most per observation; every failed observation must have a closed action before completion.
- Each action is assigned directly to one existing Contractor user. Companies, teams and multiple assignees are deferred.
- Lifecycle: Open → ReadyForReview → Closed. Rejection requires a reason and returns the action to Open.
- Only the assigned contractor may submit resolution notes; notes are mandatory. Corrective-action evidence is deferred.
- Only managers review actions. Closed actions and completed inspections are immutable.
- Completion is an explicit manager operation, never automatic. Inspections without failures can be completed directly.
- Creation requires a description (maximum 2000 characters), contractor, future UTC due date and inspection row version.

## Architecture

CorrectiveAction is a separate concurrent, auditable aggregate linked to an inspection and observation.
Its unique ObservationId index prevents duplicate actions. A composite observation foreign key ensures
the observation belongs to the same inspection. Contractor identity remains in Infrastructure.

Creation and the transition to CorrectiveActionsOpen are saved in one database transaction.
Every creation forces an inspection-root update, including when its status is already CorrectiveActionsOpen,
so the supplied row version protects concurrent additions. Duplicate races return HTTP 409.
No messages are published in these stages; transactional outbox/inbox delivery belongs to Stage 3.5.

## Delivery boundaries

- 3.0: this ADR and agreed rules.
- 3.1: aggregate, lifecycle guards, mapping, migration and tests.
- 3.2: manager-only create, contractor options and inspection-scoped list API; manager panel on inspection details.
- 3.3: contractor workspace and response endpoint. Evidence is deferred for the MVP.
- 3.4: manager approve/reject endpoints, latest rejection details and explicit inspection completion.
- 3.5: reliable messaging.
- 3.6: broader hardening.

Stages 3.0–3.4 now expose the complete synchronous MVP lifecycle. Full history, evidence,
notifications, reliable messaging and broader hardening remain deferred.
