import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../core/api/api-error';
import { InspectionDetail } from '../../inspections/models/inspection.models';
import { InspectionService } from '../../inspections/services/inspection.service';
import {
  ContractorOption,
  CorrectiveAction,
  CorrectiveActionCreated,
} from '../models/corrective-action.models';
import { CorrectiveActionService } from '../services/corrective-action.service';

@Component({
  selector: 'app-corrective-action-panel',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzDatePickerModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzTagModule,
  ],
  templateUrl: './corrective-action-panel.html',
  styleUrl: './corrective-action-panel.scss',
})
export class CorrectiveActionPanel implements OnInit {
  private readonly service = inject(CorrectiveActionService);
  private readonly inspectionService = inject(InspectionService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly inspection = input.required<InspectionDetail>();
  readonly created = output<CorrectiveActionCreated>();
  readonly completed = output<void>();
  readonly reloadRequested = output<void>();
  readonly actions = signal<readonly CorrectiveAction[]>([]);
  readonly contractors = signal<readonly ContractorOption[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly reviewingActionId = signal<string | null>(null);
  readonly completing = signal(false);
  readonly loadFailed = signal(false);
  readonly concurrencyBlocked = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly availableObservations = computed(() => {
    const assigned = new Set(this.actions().map((action) => action.observationId));
    return this.inspection().observations.filter(
      (observation) => observation.outcome === 'Fail' && !assigned.has(observation.id),
    );
  });
  readonly canCreate = computed(
    () =>
      this.inspection().status === 'Submitted' ||
      this.inspection().status === 'CorrectiveActionsOpen',
  );
  readonly canComplete = computed(() => {
    const inspection = this.inspection();
    if (inspection.status !== 'Submitted' && inspection.status !== 'CorrectiveActionsOpen') {
      return false;
    }

    const failedObservationIds = inspection.observations
      .filter((observation) => observation.outcome === 'Fail')
      .map((observation) => observation.id);
    return failedObservationIds.every((observationId) =>
      this.actions().some(
        (action) => action.observationId === observationId && action.status === 'Closed',
      ),
    );
  });
  readonly form = this.formBuilder.group({
    observationId: this.formBuilder.control<string | null>(null, Validators.required),
    contractorId: this.formBuilder.control<string | null>(null, Validators.required),
    description: this.formBuilder.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(2000),
    ]),
    dueAt: this.formBuilder.control<Date | null>(null, Validators.required),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.errorMessage.set(null);
    forkJoin({
      actions: this.service.list(this.inspection().id),
      contractors: this.service.contractors(),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: ({ actions, contractors }) => {
          this.actions.set(actions);
          this.contractors.set(contractors);
          if (contractors.length === 1) {
            this.form.controls.contractorId.setValue(contractors[0].id);
          }
        },
        error: (error: unknown) => {
          this.loadFailed.set(true);
          this.errorMessage.set(getApiErrorMessage(error));
        },
      });
  }

  submit(): void {
    if (
      this.saving() ||
      this.loading() ||
      this.loadFailed() ||
      this.concurrencyBlocked() ||
      !this.canCreate()
    ) {
      return;
    }
    const value = this.form.getRawValue();
    if (this.form.invalid || !value.description.trim()) {
      this.form.markAllAsTouched();
      this.errorMessage.set(
        'Select a failed observation and contractor, then enter a description and due date.',
      );
      return;
    }
    if (!this.availableObservations().some((item) => item.id === value.observationId)) {
      this.errorMessage.set('Select a failed observation that does not already have an action.');
      return;
    }
    if (
      !value.dueAt ||
      !Number.isFinite(value.dueAt.getTime()) ||
      value.dueAt.getTime() <= Date.now()
    ) {
      this.errorMessage.set('Choose a due date and time in the future.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.form.disable({ emitEvent: false });
    const inspection = this.inspection();
    this.service
      .create({
        inspectionId: inspection.id,
        observationId: value.observationId!,
        contractorId: value.contractorId!,
        description: value.description.trim(),
        dueAtUtc: value.dueAt.toISOString(),
        rowVersion: inspection.rowVersion,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.saving.set(false);
          if (!this.concurrencyBlocked()) {
            this.form.enable({ emitEvent: false });
          }
        }),
      )
      .subscribe({
        next: (result) => {
          this.created.emit(result);
          this.successMessage.set('Corrective action created and assigned.');
          this.form.reset();
          this.load();
        },
        error: (error: unknown) => {
          this.errorMessage.set(getApiErrorMessage(error));
          if (error instanceof HttpErrorResponse && error.status === 409) {
            this.concurrencyBlocked.set(true);
            this.errorMessage.set(
              'This inspection changed elsewhere. Reload it before creating another action.',
            );
          }
        },
      });
  }

  approve(action: CorrectiveAction): void {
    if (action.status !== 'ReadyForReview' || this.reviewingActionId() !== null) {
      return;
    }

    this.reviewingActionId.set(action.id);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.service
      .approve(action.id, action.rowVersion)
      .pipe(finalize(() => this.reviewingActionId.set(null)))
      .subscribe({
        next: () => {
          this.successMessage.set('Corrective action approved and closed.');
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  reject(action: CorrectiveAction, reason: string): void {
    const trimmedReason = reason.trim();
    if (
      action.status !== 'ReadyForReview' ||
      !trimmedReason ||
      this.reviewingActionId() !== null
    ) {
      return;
    }

    this.reviewingActionId.set(action.id);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.service
      .reject(action.id, trimmedReason, action.rowVersion)
      .pipe(finalize(() => this.reviewingActionId.set(null)))
      .subscribe({
        next: () => {
          this.successMessage.set('The action was returned to the contractor.');
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  completeInspection(): void {
    const inspection = this.inspection();
    if (!this.canComplete() || this.completing()) {
      return;
    }

    this.completing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.inspectionService
      .complete(inspection.id, inspection.rowVersion)
      .pipe(finalize(() => this.completing.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('The inspection was completed.');
          this.completed.emit();
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  observationLabel(observationId: string): string {
    const observation = this.inspection().observations.find((item) => item.id === observationId);
    return observation ? `#${observation.displayOrder} — ${observation.question}` : 'Observation';
  }
}
