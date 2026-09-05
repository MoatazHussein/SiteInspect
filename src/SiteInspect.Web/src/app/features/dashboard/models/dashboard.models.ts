import { InspectionStatus } from '../../inspections/models/inspection.models';

export interface DashboardFilters {
  readonly projectId?: string;
  readonly status?: string;
  readonly dueFromUtc?: string;
  readonly dueBeforeUtc?: string;
}

export interface DashboardInspectionItem {
  readonly id: string;
  readonly number: string;
  readonly projectName: string;
  readonly inspectorName: string;
  readonly status: InspectionStatus;
  readonly dueAtUtc: string;
}

export interface DashboardActionItem {
  readonly id: string;
  readonly inspectionId: string;
  readonly inspectionNumber: string;
  readonly projectName: string;
  readonly contractorName: string;
  readonly description: string;
  readonly status: 'Open' | 'ReadyForReview';
  readonly dueAtUtc: string;
}

export interface DashboardSummary {
  readonly activeInspections: number;
  readonly overdueInspections: number;
  readonly completedInspections: number;
  readonly outstandingCorrectiveActions: number;
  readonly actionsAwaitingReview: number;
  readonly overdueCorrectiveActions: number;
  readonly generatedAtUtc: string;
  readonly overdueInspectionItems: readonly DashboardInspectionItem[];
  readonly outstandingActionItems: readonly DashboardActionItem[];
}
