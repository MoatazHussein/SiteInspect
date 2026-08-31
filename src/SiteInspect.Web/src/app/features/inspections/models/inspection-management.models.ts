import { InspectionStatus } from './inspection.models';

export interface ManagementProjectOption {
  readonly id: string;
  readonly name: string;
}

export interface ManagementLocationOption {
  readonly id: string;
  readonly projectId: string;
  readonly name: string;
}

export interface ManagementTemplateOption {
  readonly id: string;
  readonly name: string;
  readonly version: number;
}

export interface ManagementInspectorOption {
  readonly id: string;
  readonly name: string;
}

export interface InspectionManagementOptions {
  readonly projects: readonly ManagementProjectOption[];
  readonly locations: readonly ManagementLocationOption[];
  readonly templates: readonly ManagementTemplateOption[];
  readonly inspectors: readonly ManagementInspectorOption[];
}

export interface CreateInspectionRequest {
  readonly projectId: string;
  readonly locationId: string;
  readonly templateId: string;
  readonly inspectorId: string;
  readonly dueAtUtc: string;
}

export interface CreateInspectionResult {
  readonly id: string;
  readonly number: string;
  readonly status: InspectionStatus;
  readonly rowVersion: string;
}

export interface InspectionMutationResult {
  readonly id: string;
  readonly status: InspectionStatus;
  readonly assignedInspectorId: string;
  readonly rowVersion: string;
}
