import { InspectionAttachment, ObservationOutcome } from './inspection.models';

export interface SaveInspectionObservationDraft {
  readonly observationId: string;
  readonly outcome: ObservationOutcome | null;
  readonly notes: string | null;
}

export interface InspectionDraftSaved {
  readonly rowVersion: string;
  readonly observations: readonly SaveInspectionObservationDraft[];
}

export interface InspectionAttachmentUploaded extends InspectionAttachment {
  readonly observationId: string;
  readonly rowVersion: string;
}
