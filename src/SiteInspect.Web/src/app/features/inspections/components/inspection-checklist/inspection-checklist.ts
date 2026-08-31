import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  catchError,
  concatMap,
  debounceTime,
  defer,
  EMPTY,
  filter,
  map,
  merge,
  Observable,
  Subject,
  tap,
} from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzProgressModule } from 'ng-zorro-antd/progress';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { InspectionMutationResult } from '../../models/inspection-management.models';
import {
  InspectionDetail,
  InspectionObservation,
  ObservationOutcome,
  Severity,
} from '../../models/inspection.models';
import { InspectionService } from '../../services/inspection.service';

type ObservationDraftForm = FormGroup<{
  observationId: FormControl<string>;
  outcome: FormControl<ObservationOutcome | null>;
  notes: FormControl<string>;
}>;

interface ChecklistItem {
  readonly observation: InspectionObservation;
  readonly formIndex: number;
}

interface ChecklistSection {
  readonly name: string;
  readonly items: readonly ChecklistItem[];
}

type SaveState = 'idle' | 'saving' | 'saved' | 'error';

@Component({
  selector: 'app-inspection-checklist',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzInputModule,
    NzProgressModule,
    NzRadioModule,
    NzTagModule,
  ],
  templateUrl: './inspection-checklist.html',
  styleUrl: './inspection-checklist.scss',
})
export class InspectionChecklist implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly inspectionService = inject(InspectionService);
  private readonly manualSaveRequests = new Subject<void>();
  private readonly rowVersion = signal('');

  readonly inspection = input.required<InspectionDetail>();
  readonly editable = input(false);
  readonly rowVersionChange = output<string>();
  readonly observationForms = this.formBuilder.array<ObservationDraftForm>([]);
  readonly form = this.formBuilder.group({ observations: this.observationForms });
  readonly saveState = signal<SaveState>('idle');
  readonly errorMessage = signal<string | null>(null);
  readonly savedAt = signal<Date | null>(null);
  readonly answeredCount = signal(0);
  sections: readonly ChecklistSection[] = [];

  ngOnInit(): void {
    const inspection = this.inspection();
    this.rowVersion.set(inspection.rowVersion);
    this.sections = this.buildSections(inspection.observations);

    for (const observation of inspection.observations) {
      this.observationForms.push(this.formBuilder.group({
        observationId: this.formBuilder.nonNullable.control(observation.id),
        outcome: this.formBuilder.control<ObservationOutcome | null>(observation.outcome),
        notes: this.formBuilder.nonNullable.control(observation.notes ?? '', Validators.maxLength(2000)),
      }));
    }

    this.updateAnsweredCount();

    if (!this.editable()) {
      this.form.disable({ emitEvent: false });
      return;
    }

    const automaticSaveRequests = this.form.valueChanges.pipe(
      tap(() => this.updateAnsweredCount()),
      debounceTime(800),
      map(() => undefined),
    );

    merge(automaticSaveRequests, this.manualSaveRequests)
      .pipe(
        filter(() => this.form.valid && this.form.dirty && this.form.enabled),
        concatMap(() => this.persistDraft()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  saveNow(): void {
    this.manualSaveRequests.next();
  }

  clearOutcome(formIndex: number): void {
    this.observationForms.at(formIndex).controls.outcome.setValue(null);
  }

  severityColor(severity: Severity): string {
    const colors: Record<Severity, string> = {
      Low: 'default',
      Medium: 'blue',
      High: 'orange',
      Critical: 'red',
    };

    return colors[severity];
  }

  progressPercent(): number {
    const count = this.observationForms.length;
    return count === 0 ? 0 : Math.round((this.answeredCount() / count) * 100);
  }

  private persistDraft(): Observable<void> {
    return defer(() => {
      const inspection = this.inspection();
      this.saveState.set('saving');
      this.errorMessage.set(null);

      const observations = this.form.getRawValue().observations.map((draft) => ({
        observationId: draft.observationId,
        outcome: draft.outcome,
        notes: draft.notes.trim() || null,
      }));

      return this.inspectionService.saveDraft(
        inspection.id,
        this.rowVersion(),
        observations,
      );
    }).pipe(
      tap((result: InspectionMutationResult) => {
        this.rowVersion.set(result.rowVersion);
        this.rowVersionChange.emit(result.rowVersion);
        this.form.markAsPristine();
        this.savedAt.set(new Date());
        this.saveState.set('saved');
      }),
      map(() => undefined),
      catchError((error: unknown) => {
        this.saveState.set('error');
        this.errorMessage.set(getApiErrorMessage(error));

        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.form.disable({ emitEvent: false });
          this.errorMessage.set('This inspection changed elsewhere. Refresh the page before continuing.');
        }

        return EMPTY;
      }),
    );
  }

  private updateAnsweredCount(): void {
    this.answeredCount.set(
      this.form.getRawValue().observations.filter((draft) => draft.outcome !== null).length,
    );
  }

  private buildSections(observations: readonly InspectionObservation[]): readonly ChecklistSection[] {
    const sections = new Map<string, ChecklistItem[]>();

    observations.forEach((observation, formIndex) => {
      const items = sections.get(observation.sectionName) ?? [];
      items.push({ observation, formIndex });
      sections.set(observation.sectionName, items);
    });

    return Array.from(sections, ([name, items]) => ({ name, items }));
  }
}
