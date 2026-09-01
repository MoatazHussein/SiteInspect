export type InspectionStatus =
  'Assigned' | 'InProgress' | 'Submitted' | 'CorrectiveActionsOpen' | 'Completed' | 'Cancelled';

export type ObservationOutcome = 'Pass' | 'Fail' | 'NotApplicable';
export type Severity = 'Low' | 'Medium' | 'High' | 'Critical';

export interface InspectionAttachment {
  readonly id: string;
  readonly fileName: string;
  readonly contentType: string;
  readonly length: number;
  readonly uploadedAtUtc: string;
}

export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface InspectionListItem {
  readonly id: string;
  readonly number: string;
  readonly projectName: string;
  readonly locationName: string;
  readonly templateName: string;
  readonly templateVersion: number;
  readonly assignedInspectorId: string;
  readonly assignedInspectorName: string;
  readonly status: InspectionStatus;
  readonly dueAtUtc: string;
  readonly rowVersion: string;
}

export interface InspectionObservation {
  readonly id: string;
  readonly sectionName: string;
  readonly question: string;
  readonly displayOrder: number;
  readonly isRequired: boolean;
  readonly outcome: ObservationOutcome | null;
  readonly severity: Severity;
  readonly notes: string | null;
  readonly observedAtUtc: string | null;
  readonly attachments: readonly InspectionAttachment[];
}

export interface InspectionDetail {
  readonly id: string;
  readonly number: string;
  readonly projectId: string;
  readonly projectName: string;
  readonly locationId: string;
  readonly locationName: string;
  readonly templateId: string;
  readonly templateName: string;
  readonly templateVersion: number;
  readonly assignedInspectorId: string;
  readonly assignedInspectorName: string;
  readonly status: InspectionStatus;
  readonly dueAtUtc: string;
  readonly startedAtUtc: string | null;
  readonly submittedAtUtc: string | null;
  readonly completedAtUtc: string | null;
  readonly cancelledAtUtc: string | null;
  readonly cancellationReason: string | null;
  readonly lastDraftSavedAtUtc: string | null;
  readonly rowVersion: string;
  readonly observations: readonly InspectionObservation[];
}

export interface FilterOption<T> {
  readonly value: T;
  readonly label: string;
}

export interface InspectionFilterOptions {
  readonly projects: readonly FilterOption<string>[];
  readonly locations: readonly FilterOption<string>[];
  readonly inspectors: readonly FilterOption<string>[];
  readonly statuses: readonly FilterOption<InspectionStatus>[];
}

export interface InspectionListQuery {
  readonly page: number;
  readonly pageSize: number;
  readonly search?: string;
  readonly projectId?: string;
  readonly locationId?: string;
  readonly inspectorId?: string;
  readonly status?: InspectionStatus;
  readonly dueFromUtc?: string;
  readonly dueToUtc?: string;
}
