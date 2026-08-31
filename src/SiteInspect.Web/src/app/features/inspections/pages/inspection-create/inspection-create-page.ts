import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { InspectionManagementOptions } from '../../models/inspection-management.models';
import { InspectionService } from '../../services/inspection.service';

@Component({
  selector: 'app-inspection-create-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzDatePickerModule,
    NzFormModule,
    NzSelectModule,
  ],
  templateUrl: './inspection-create-page.html',
  styleUrl: './inspection-create-page.scss',
})
export class InspectionCreatePage implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly inspectionService = inject(InspectionService);
  private readonly router = inject(Router);

  readonly loadingOptions = signal(true);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly options = signal<InspectionManagementOptions | null>(null);
  readonly selectedProjectId = signal<string | null>(null);
  readonly form = this.formBuilder.group({
    projectId: this.formBuilder.control<string | null>(null, Validators.required),
    locationId: this.formBuilder.control<string | null>(null, Validators.required),
    templateId: this.formBuilder.control<string | null>(null, Validators.required),
    inspectorId: this.formBuilder.control<string | null>(null, Validators.required),
    dueAt: this.formBuilder.control<Date | null>(null, Validators.required),
  });

  constructor() {
    this.form.controls.projectId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((projectId) => {
        this.selectedProjectId.set(projectId);
        this.form.controls.locationId.reset();
      });
  }

  ngOnInit(): void {
    this.inspectionService
      .getManagementOptions()
      .pipe(finalize(() => this.loadingOptions.set(false)))
      .subscribe({
        next: (options) => this.applyOptions(options),
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  filteredLocations() {
    const projectId = this.selectedProjectId();
    return (this.options()?.locations ?? []).filter((location) => location.projectId === projectId);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.errorMessage.set(null);

    this.inspectionService
      .create({
        projectId: value.projectId!,
        locationId: value.locationId!,
        templateId: value.templateId!,
        inspectorId: value.inspectorId!,
        dueAtUtc: value.dueAt!.toISOString(),
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (created) => void this.router.navigate(['/inspections', created.id]),
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  private applyOptions(options: InspectionManagementOptions): void {
    this.options.set(options);

    if (options.projects.length === 1) {
      this.form.controls.projectId.setValue(options.projects[0].id);
    }

    if (options.templates.length === 1) {
      this.form.controls.templateId.setValue(options.templates[0].id);
    }

    if (options.inspectors.length === 1) {
      this.form.controls.inspectorId.setValue(options.inspectors[0].id);
    }
  }
}
