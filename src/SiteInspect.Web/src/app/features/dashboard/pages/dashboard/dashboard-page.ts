import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { InspectionService } from '../../../inspections/services/inspection.service';
import { InspectionFilterOptions } from '../../../inspections/models/inspection.models';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzSkeletonModule } from 'ng-zorro-antd/skeleton';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { ConnectivityService } from '../../../../core/connectivity/connectivity.service';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardSummary } from '../../models/dashboard.models';

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, FormsModule, RouterLink, NzAlertModule, NzButtonModule, NzCardModule, NzSkeletonModule],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage implements OnInit {
  private readonly service = inject(DashboardService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly inspections = inject(InspectionService);
  readonly options = signal<InspectionFilterOptions | null>(null);
  readonly optionsError = signal<string | null>(null);
  projectId = '';
  status = '';
  dueFrom = '';
  dueTo = '';
  readonly connectivity = inject(ConnectivityService);
  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly metrics = [
    { key: 'activeInspections', label: 'Active inspections', description: 'Assigned, in progress, submitted, or corrective actions open.' },
    { key: 'overdueInspections', label: 'Overdue inspections', description: 'Past due and still assigned or in progress.' },
    { key: 'completedInspections', label: 'Completed inspections', description: 'Inspections explicitly marked completed.' },
    { key: 'outstandingCorrectiveActions', label: 'Outstanding actions', description: 'Open actions plus actions awaiting review.' },
    { key: 'actionsAwaitingReview', label: 'Awaiting review', description: 'Contractor responses ready for manager review.' },
    { key: 'overdueCorrectiveActions', label: 'Overdue actions', description: 'Past due and not yet closed, including review.' },
  ] as const;

  ngOnInit(): void {
    this.loadOptions();
    this.refresh();
  }

  loadOptions(): void {
    this.optionsError.set(null);
    this.inspections.getFilterOptions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (options) => this.options.set(options),
      error: () => this.optionsError.set('Project and status options could not be loaded.'),
    });
  }

  resetFilters(): void {
    this.projectId = '';
    this.status = '';
    this.dueFrom = '';
    this.dueTo = '';
    this.refresh();
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      InProgress: 'In progress', ReadyForReview: 'Awaiting review',
      CorrectiveActionsOpen: 'Corrective actions open',
    };
    return labels[status] ?? status;
  }

  refresh(): void {
    if (this.loading()) return;
    if (!this.connectivity.isOnline()) {
      this.errorMessage.set('Reconnect to load the dashboard.');
      return;
    }
    if (this.dueFrom && this.dueTo && this.dueFrom > this.dueTo) {
      this.errorMessage.set('Due to must be on or after Due from.');
      return;
    }
    const start = this.dueFrom ? new Date(`${this.dueFrom}T00:00:00`) : null;
    const end = this.dueTo ? new Date(`${this.dueTo}T00:00:00`) : null;
    if ((start && isNaN(start.getTime())) || (end && isNaN(end.getTime()))) {
      this.errorMessage.set('Enter valid due dates.');
      return;
    }
    if (end) end.setDate(end.getDate() + 1);
    this.loading.set(true);
    this.errorMessage.set(null);
    this.summary.set(null);
    this.service.getSummary({
      projectId: this.projectId, status: this.status,
      dueFromUtc: start?.toISOString(), dueBeforeUtc: end?.toISOString(),
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: (summary) => this.summary.set(summary),
      error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
    });
  }
}
