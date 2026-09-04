import { SaveInspectionObservationDraft } from './inspection-execution.models';

export interface OfflineInspectionDraft {
  readonly userId: string;
  readonly inspectionId: string;
  readonly rowVersion: string;
  readonly observations: readonly SaveInspectionObservationDraft[];
  readonly savedAtUtc: string;
}
