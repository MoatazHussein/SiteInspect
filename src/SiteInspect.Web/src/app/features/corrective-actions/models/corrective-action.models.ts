import { InspectionStatus } from '../../inspections/models/inspection.models';

export type CorrectiveActionStatus = 'Open' | 'ReadyForReview' | 'Closed';

export interface CorrectiveAction {
  readonly id: string;
  readonly inspectionId: string;
  readonly observationId: string;
  readonly assignedContractorId: string;
  readonly assignedContractorName: string;
  readonly description: string;
  readonly dueAtUtc: string;
  readonly status: CorrectiveActionStatus;
  readonly resolutionNotes: string | null;
  readonly rejectionReason: string | null;
  readonly submittedAtUtc: string | null;
  readonly rejectedAtUtc: string | null;
  readonly closedAtUtc: string | null;
  readonly createdAtUtc: string;
  readonly rowVersion: string;
}

export interface ContractorCorrectiveAction {
  readonly id: string;
  readonly inspectionId: string;
  readonly inspectionNumber: string;
  readonly observationId: string;
  readonly observationDisplayOrder: number;
  readonly observationQuestion: string;
  readonly description: string;
  readonly dueAtUtc: string;
  readonly status: CorrectiveActionStatus;
  readonly resolutionNotes: string | null;
  readonly rejectionReason: string | null;
  readonly submittedAtUtc: string | null;
  readonly rejectedAtUtc: string | null;
  readonly closedAtUtc: string | null;
  readonly createdAtUtc: string;
  readonly rowVersion: string;
}

export interface CorrectiveActionMutation {
  readonly id: string;
  readonly status: CorrectiveActionStatus;
  readonly rowVersion: string;
}

export interface ContractorOption {
  readonly id: string;
  readonly name: string;
}

export interface CreateCorrectiveAction {
  readonly inspectionId: string;
  readonly observationId: string;
  readonly contractorId: string;
  readonly description: string;
  readonly dueAtUtc: string;
  readonly rowVersion: string;
}

export interface CorrectiveActionCreated {
  readonly id: string;
  readonly inspectionStatus: InspectionStatus;
  readonly rowVersion: string;
}
