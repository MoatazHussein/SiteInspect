import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { roles } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import {
  InspectionFilterOptions,
  InspectionListItem,
  InspectionStatus,
  PagedResult,
} from '../../models/inspection.models';
import { InspectionService } from '../../services/inspection.service';

@Component({
  selector: 'app-inspection-list-page',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzDatePickerModule,
    NzFormModule,
    NzInputModule,
    NzPaginationModule,
    NzSelectModule,
    NzTableModule,
    NzTagModule,
  ],
  templateUrl: './inspection-list-page.html',
  styleUrl: './inspection-list-page.scss',
})
export class InspectionListPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly inspectionService = inject(InspectionService);

  readonly auth = inject(AuthService);
  readonly roles = roles;

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly result = signal<PagedResult<InspectionListItem> | null>(null);
  readonly options = signal<InspectionFilterOptions | null>(null);
  readonly pageSize = 20;
  readonly filters = this.formBuilder.group({
    search: this.formBuilder.control(''),
    projectId: this.formBuilder.control<string | null>(null),
    locationId: this.formBuilder.control<string | null>(null),
    inspectorId: this.formBuilder.control<string | null>(null),
    status: this.formBuilder.control<InspectionStatus | null>(null),
    dueRange: this.formBuilder.control<Date[] | null>(null),
  });

  ngOnInit(): void {
    this.load(1, true);
  }

  applyFilters(): void {
    this.load(1);
  }

  resetFilters(): void {
    this.filters.reset();
    this.load(1);
  }

  changePage(page: number): void {
    this.load(page);
  }

  statusColor(status: InspectionStatus): string {
    const colors: Record<InspectionStatus, string> = {
      Assigned: 'blue',
      InProgress: 'gold',
      Submitted: 'purple',
      CorrectiveActionsOpen: 'orange',
      Completed: 'green',
      Cancelled: 'default',
    };

    return colors[status];
  }

  private load(page: number, includeOptions = false): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    const value = this.filters.getRawValue();
    const dueRange = value.dueRange;
    const listRequest = this.inspectionService.list({
      page,
      pageSize: this.pageSize,
      search: value.search?.trim() || undefined,
      projectId: value.projectId ?? undefined,
      locationId: value.locationId ?? undefined,
      inspectorId: value.inspectorId ?? undefined,
      status: value.status ?? undefined,
      dueFromUtc: dueRange?.[0]?.toISOString(),
      dueToUtc: dueRange?.[1]?.toISOString(),
    });

    if (includeOptions) {
      this.inspectionService.getFilterOptions().subscribe({
        next: (options) => this.options.set(options),
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
    }

    listRequest.pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (response) => this.result.set(response),
      error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
    });
  }
}
