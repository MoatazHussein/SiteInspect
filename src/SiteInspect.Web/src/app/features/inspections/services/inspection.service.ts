import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../../../core/api/api-response';
import {
  InspectionDetail,
  InspectionFilterOptions,
  InspectionListItem,
  InspectionListQuery,
  PagedResult,
} from '../models/inspection.models';
import {
  CreateInspectionRequest,
  CreateInspectionResult,
  InspectionManagementOptions,
  InspectionMutationResult,
} from '../models/inspection-management.models';
import { SaveInspectionObservationDraft } from '../models/inspection-execution.models';

@Injectable({ providedIn: 'root' })
export class InspectionService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/inspections';

  list(query: InspectionListQuery): Observable<PagedResult<InspectionListItem>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);

    for (const [key, value] of Object.entries(query)) {
      if (key !== 'page' && key !== 'pageSize' && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    }

    return this.http
      .get<ApiResponse<PagedResult<InspectionListItem>>>(this.endpoint, { params })
      .pipe(map((response) => this.requireData(response)));
  }

  get(inspectionId: string): Observable<InspectionDetail> {
    return this.http
      .get<ApiResponse<InspectionDetail>>(`${this.endpoint}/${inspectionId}`)
      .pipe(map((response) => this.requireData(response)));
  }

  getFilterOptions(): Observable<InspectionFilterOptions> {
    return this.http
      .get<ApiResponse<InspectionFilterOptions>>(`${this.endpoint}/filter-options`)
      .pipe(map((response) => this.requireData(response)));
  }

  getManagementOptions(): Observable<InspectionManagementOptions> {
    return this.http
      .get<ApiResponse<InspectionManagementOptions>>(`${this.endpoint}/management-options`)
      .pipe(map((response) => this.requireData(response)));
  }

  create(request: CreateInspectionRequest): Observable<CreateInspectionResult> {
    return this.http
      .post<ApiResponse<CreateInspectionResult>>(this.endpoint, request)
      .pipe(map((response) => this.requireData(response)));
  }

  reassign(
    inspectionId: string,
    inspectorId: string,
    rowVersion: string,
  ): Observable<InspectionMutationResult> {
    return this.http
      .put<ApiResponse<InspectionMutationResult>>(`${this.endpoint}/reassign`, {
        inspectionId,
        inspectorId,
        rowVersion,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  cancel(
    inspectionId: string,
    reason: string,
    rowVersion: string,
  ): Observable<InspectionMutationResult> {
    return this.http
      .post<ApiResponse<InspectionMutationResult>>(`${this.endpoint}/cancel`, {
        inspectionId,
        reason,
        rowVersion,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  start(inspectionId: string, rowVersion: string): Observable<InspectionMutationResult> {
    return this.http
      .post<ApiResponse<InspectionMutationResult>>(`${this.endpoint}/start`, {
        inspectionId,
        rowVersion,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  saveDraft(
    inspectionId: string,
    rowVersion: string,
    observations: readonly SaveInspectionObservationDraft[],
  ): Observable<InspectionMutationResult> {
    return this.http
      .put<ApiResponse<InspectionMutationResult>>(`${this.endpoint}/draft`, {
        inspectionId,
        rowVersion,
        observations,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  private requireData<T>(response: ApiResponse<T>): T {
    if (!response.isSuccess || response.data === null) {
      throw new Error(response.errors[0]?.message ?? 'The API returned an empty response.');
    }

    return response.data;
  }
}
