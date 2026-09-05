import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs';
import { ApiResponse } from '../../../core/api/api-response';
import { DashboardFilters, DashboardSummary } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getSummary(filters: DashboardFilters = {}) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value) params = params.set(key, value);
    }
    return this.http.get<ApiResponse<DashboardSummary>>('/api/dashboard/summary', { params }).pipe(
      map((response) => {
        if (!response.isSuccess || response.data === null) {
          throw new Error(response.errors[0]?.message ?? 'The dashboard could not be loaded.');
        }
        return response.data;
      }),
    );
  }
}
