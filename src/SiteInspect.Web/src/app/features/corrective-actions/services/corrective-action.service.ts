import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiResponse } from '../../../core/api/api-response';
import {
  ContractorOption,
  CorrectiveAction,
  CorrectiveActionCreated,
  CorrectiveActionMutation,
  ContractorCorrectiveAction,
  CreateCorrectiveAction,
} from '../models/corrective-action.models';

@Injectable({ providedIn: 'root' })
export class CorrectiveActionService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/corrective-actions';

  list(inspectionId: string): Observable<readonly CorrectiveAction[]> {
    return this.http
      .get<ApiResponse<readonly CorrectiveAction[]>>(this.endpoint, {
        params: { inspectionId },
      })
      .pipe(map((response) => this.requireData(response)));
  }

  contractors(): Observable<readonly ContractorOption[]> {
    return this.http
      .get<ApiResponse<readonly ContractorOption[]>>(`${this.endpoint}/contractors`)
      .pipe(map((response) => this.requireData(response)));
  }

  create(request: CreateCorrectiveAction): Observable<CorrectiveActionCreated> {
    return this.http
      .post<ApiResponse<CorrectiveActionCreated>>(this.endpoint, request)
      .pipe(map((response) => this.requireData(response)));
  }

  mine(): Observable<readonly ContractorCorrectiveAction[]> {
    return this.http
      .get<ApiResponse<readonly ContractorCorrectiveAction[]>>(`${this.endpoint}/mine`)
      .pipe(map((response) => this.requireData(response)));
  }

  respond(
    correctiveActionId: string,
    resolutionNotes: string,
    rowVersion: string,
  ): Observable<CorrectiveActionMutation> {
    return this.http
      .post<ApiResponse<CorrectiveActionMutation>>(`${this.endpoint}/respond`, {
        correctiveActionId,
        resolutionNotes,
        rowVersion,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  approve(correctiveActionId: string, rowVersion: string): Observable<CorrectiveActionMutation> {
    return this.http
      .post<ApiResponse<CorrectiveActionMutation>>(`${this.endpoint}/approve`, {
        correctiveActionId,
        rowVersion,
      })
      .pipe(map((response) => this.requireData(response)));
  }

  reject(
    correctiveActionId: string,
    reason: string,
    rowVersion: string,
  ): Observable<CorrectiveActionMutation> {
    return this.http
      .post<ApiResponse<CorrectiveActionMutation>>(`${this.endpoint}/reject`, {
        correctiveActionId,
        reason,
        rowVersion,
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
