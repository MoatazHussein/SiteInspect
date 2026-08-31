import { ObservationOutcome } from './inspection.models';

export interface SaveInspectionObservationDraft {
  readonly observationId: string;
  readonly outcome: ObservationOutcome | null;
  readonly notes: string | null;
}
