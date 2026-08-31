import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSkeletonModule } from 'ng-zorro-antd/skeleton';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { roles } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { InspectionChecklist } from '../../components/inspection-checklist/inspection-checklist';
import { InspectionManagementOptions } from '../../models/inspection-management.models';
import { InspectionDetail, InspectionStatus } from '../../models/inspection.models';
import { InspectionService } from '../../services/inspection.service';

@Component({
  selector: 'app-inspection-detail-page',
  imports: [
    DatePipe,
    InspectionChecklist,
    ReactiveFormsModule,
    RouterLink,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzDescriptionsModule,
    NzFormModule,
    NzInputModule,
    NzPopconfirmModule,
    NzSelectModule,
    NzSkeletonModule,
    NzTagModule,
  ],
  templateUrl: './inspection-detail-page.html',
  styleUrl: './inspection-detail-page.scss',
})
export class InspectionDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly inspectionService = inject(InspectionService);

  readonly auth = inject(AuthService);
  readonly roles = roles;

  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly inspection = signal<InspectionDetail | null>(null);
  readonly managementOptions = signal<InspectionManagementOptions | null>(null);
  readonly managing = signal(false);
  readonly starting = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly assignmentForm = this.formBuilder.group({
    inspectorId: this.formBuilder.control<string | null>(null, Validators.required),
  });
  readonly cancellationForm = this.formBuilder.group({
    reason: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(1000),
    ]),
  });

  ngOnInit(): void {
    const inspectionId = this.route.snapshot.paramMap.get('inspectionId');
    if (!inspectionId) {
      this.errorMessage.set('The inspection identifier is missing.');
      this.loading.set(false);
      return;
    }

    this.loadInspection(inspectionId);

    if (this.auth.hasAnyRole([roles.manager])) {
      this.inspectionService.getManagementOptions().subscribe({
        next: (options) => this.managementOptions.set(options),
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
    }
  }

  reassign(): void {
    const item = this.inspection();
    const inspectorId = this.assignmentForm.controls.inspectorId.value;
    if (!item || !inspectorId || inspectorId === item.assignedInspectorId) {
      return;
    }

    this.managing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.inspectionService
      .reassign(item.id, inspectorId, item.rowVersion)
      .pipe(finalize(() => this.managing.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('The inspection was reassigned.');
          this.loadInspection(item.id);
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  cancelInspection(): void {
    const item = this.inspection();
    const reason = this.cancellationForm.controls.reason.value.trim();
    if (!item || !reason) {
      this.cancellationForm.markAllAsTouched();
      return;
    }

    this.managing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.inspectionService
      .cancel(item.id, reason, item.rowVersion)
      .pipe(finalize(() => this.managing.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('The inspection was cancelled.');
          this.cancellationForm.reset();
          this.loadInspection(item.id);
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  startInspection(): void {
    const item = this.inspection();
    if (!item) {
      return;
    }

    this.starting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.inspectionService
      .start(item.id, item.rowVersion)
      .pipe(finalize(() => this.starting.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('The inspection is now in progress.');
          this.loadInspection(item.id);
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  canStart(item: InspectionDetail): boolean {
    const user = this.auth.currentUser();
    return item.status === 'Assigned' &&
      user?.id === item.assignedInspectorId &&
      this.auth.hasAnyRole([roles.inspector]);
  }

  canEditChecklist(item: InspectionDetail): boolean {
    const user = this.auth.currentUser();
    return item.status === 'InProgress' &&
      user?.id === item.assignedInspectorId &&
      this.auth.hasAnyRole([roles.inspector]);
  }

  updateDraftRowVersion(rowVersion: string): void {
    this.inspection.update((item) => item
      ? { ...item, rowVersion, lastDraftSavedAtUtc: new Date().toISOString() }
      : null);
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

  private loadInspection(inspectionId: string): void {
    this.loading.set(true);
    this.inspectionService
      .get(inspectionId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (inspection) => {
          this.inspection.set(inspection);
          this.assignmentForm.controls.inspectorId.setValue(inspection.assignedInspectorId);
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }
}
