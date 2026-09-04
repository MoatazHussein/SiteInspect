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
  finalize,
  from,
  map,
  merge,
  Observable,
  of,
  Subject,
  tap,
  throwError,
} from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzProgressModule } from 'ng-zorro-antd/progress';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import { AuthService } from '../../../../core/auth/auth.service';
import { ConnectivityService } from '../../../../core/connectivity/connectivity.service';
import {
  InspectionAttachmentUploaded,
  InspectionDraftSaved,
  SaveInspectionObservationDraft,
} from '../../models/inspection-execution.models';
import {
  InspectionAttachment,
  InspectionDetail,
  InspectionObservation,
  ObservationOutcome,
  Severity,
} from '../../models/inspection.models';
import { OfflineInspectionDraft } from '../../models/offline-inspection-draft.models';
import { InspectionService } from '../../services/inspection.service';
import { OfflineInspectionDraftStore } from '../../services/offline-inspection-draft.store';

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

type SaveState = 'idle' | 'saving' | 'saved' | 'saved-locally' | 'error';

type DraftPersistenceResult =
  | {
      readonly location: 'server';
      readonly rowVersion: string;
      readonly observations: readonly SaveInspectionObservationDraft[];
    }
  | {
      readonly location: 'device';
      readonly observations: readonly SaveInspectionObservationDraft[];
    };

@Component({
  selector: 'app-inspection-checklist',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    NzAlertModule,
    NzButtonModule,
    NzCardModule,
    NzInputModule,
    NzPopconfirmModule,
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
  private readonly auth = inject(AuthService);
  private readonly offlineDraftStore = inject(OfflineInspectionDraftStore);
  private readonly manualSaveRequests = new Subject<void>();
  private readonly rowVersion = signal('');

  readonly inspection = input.required<InspectionDetail>();
  readonly editable = input(false);
  readonly connectivity = inject(ConnectivityService);
  readonly draftSaved = output<InspectionDraftSaved>();
  readonly attachmentUploaded = output<InspectionAttachmentUploaded>();
  readonly readinessChange = output<boolean>();
  readonly reloadRequested = output<void>();
  readonly observationForms = this.formBuilder.array<ObservationDraftForm>([]);
  readonly form = this.formBuilder.group({ observations: this.observationForms });
  readonly saveState = signal<SaveState>('idle');
  readonly errorMessage = signal<string | null>(null);
  readonly savedAt = signal<Date | null>(null);
  readonly answeredCount = signal(0);
  readonly uploadingObservationId = signal<string | null>(null);
  readonly hasLocalDraft = signal(false);
  readonly conflictDetected = signal(false);
  readonly syncingLocalDraft = signal(false);
  readonly discardingLocalDraft = signal(false);
  private readonly concurrencyBlocked = signal(false);
  private readonly attachments = signal(new Map<string, readonly InspectionAttachment[]>());
  sections: readonly ChecklistSection[] = [];

  ngOnInit(): void {
    const inspection = this.inspection();
    this.rowVersion.set(inspection.rowVersion);
    this.sections = this.buildSections(inspection.observations);
    this.attachments.set(new Map(
      inspection.observations.map((observation) => [observation.id, observation.attachments]),
    ));

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

    this.form.disable({ emitEvent: false });
    void this.initializeEditableChecklist();
  }

  private configureDraftPersistence(): void {
    const automaticSaveRequests = this.form.valueChanges.pipe(
      tap(() => {
        this.updateAnsweredCount();
        this.readinessChange.emit(false);
      }),
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

  private async initializeEditableChecklist(): Promise<void> {
    const userId = this.auth.currentUser()?.id;

    try {
      if (userId) {
        const localDraft = await this.offlineDraftStore.get(userId, this.inspection().id);

        if (localDraft) {
          this.applyLocalDraft(localDraft);
          this.hasLocalDraft.set(true);
          this.savedAt.set(new Date(localDraft.savedAtUtc));
          this.saveState.set('saved-locally');
          this.readinessChange.emit(false);

          if (localDraft.rowVersion !== this.inspection().rowVersion) {
            this.concurrencyBlocked.set(true);
            this.conflictDetected.set(true);
            this.errorMessage.set(
              'The server changed after this offline draft was created. The local copy is preserved; use the server version to resolve the conflict.',
            );
            return;
          }

          this.rowVersion.set(localDraft.rowVersion);
        }
      }
    } catch {
      if (!this.connectivity.isOnline()) {
        this.concurrencyBlocked.set(true);
        this.readinessChange.emit(false);
        this.errorMessage.set(
          'Offline storage is unavailable. Reconnect before editing this inspection.',
        );
        return;
      }

      this.errorMessage.set('Offline storage is unavailable. Online saving is still available.');
    }

    this.form.enable({ emitEvent: false });
    this.configureDraftPersistence();
  }

  private applyLocalDraft(draft: OfflineInspectionDraft): void {
    const observationsById = new Map(
      draft.observations.map((observation) => [observation.observationId, observation]),
    );

    for (const observationForm of this.observationForms.controls) {
      const localObservation = observationsById.get(
        observationForm.controls.observationId.value,
      );

      if (!localObservation) {
        continue;
      }

      observationForm.patchValue(
        {
          outcome: localObservation.outcome,
          notes: localObservation.notes ?? '',
        },
        { emitEvent: false },
      );
    }

    this.form.markAsPristine();
    this.updateAnsweredCount();
  }

  saveNow(): void {
    this.manualSaveRequests.next();
  }

  syncLocalDraft(): void {
    const userId = this.auth.currentUser()?.id;
    if (
      !userId ||
      !this.connectivity.isOnline() ||
      !this.hasLocalDraft() ||
      this.conflictDetected() ||
      this.form.dirty ||
      this.form.invalid
    ) {
      return;
    }

    const observations = this.getDrafts();
    this.syncingLocalDraft.set(true);
    this.readinessChange.emit(false);
    this.errorMessage.set(null);
    this.form.disable({ emitEvent: false });

    this.inspectionService
      .saveDraft(this.inspection().id, this.rowVersion(), observations)
      .pipe(
        concatMap((result) =>
          from(this.offlineDraftStore.delete(userId, this.inspection().id)).pipe(
            map(() => ({ result, localDraftDeleted: true })),
            catchError(() => of({ result, localDraftDeleted: false })),
          ),
        ),
        finalize(() => {
          this.syncingLocalDraft.set(false);
          if (!this.concurrencyBlocked()) {
            this.form.enable({ emitEvent: false });
          }
        }),
      )
      .subscribe({
        next: ({ result, localDraftDeleted }) => {
          this.rowVersion.set(result.rowVersion);
          this.draftSaved.emit({ rowVersion: result.rowVersion, observations });
          this.form.markAsPristine();
          this.savedAt.set(new Date());

          if (!localDraftDeleted) {
            this.saveState.set('saved-locally');
            this.errorMessage.set(
              'The server was updated, but the local copy could not be removed. Press Sync now again to retry cleanup.',
            );
            return;
          }

          this.hasLocalDraft.set(false);
          this.saveState.set('saved');
          this.readinessChange.emit(true);
        },
        error: (error: unknown) => this.handleMutationError(error),
      });
  }

  discardLocalDraft(): void {
    const userId = this.auth.currentUser()?.id;
    if (!userId || !this.connectivity.isOnline() || !this.hasLocalDraft()) {
      return;
    }

    this.discardingLocalDraft.set(true);
    this.errorMessage.set(null);

    from(this.offlineDraftStore.delete(userId, this.inspection().id))
      .pipe(finalize(() => this.discardingLocalDraft.set(false)))
      .subscribe({
        next: () => {
          this.hasLocalDraft.set(false);
          this.conflictDetected.set(false);
          this.concurrencyBlocked.set(false);
          this.reloadRequested.emit();
        },
        error: () => this.errorMessage.set('The local draft could not be discarded.'),
      });
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

  formatFileSize(length: number): string {
    return length < 1024 * 1024
      ? `${Math.ceil(length / 1024)} KB`
      : `${(length / (1024 * 1024)).toFixed(1)} MB`;
  }

  attachmentsFor(observationId: string): readonly InspectionAttachment[] {
    return this.attachments().get(observationId) ?? [];
  }

  selectAttachment(observationId: string, event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const file = inputElement.files?.[0];
    inputElement.value = '';

    if (!file || !this.editable()) {
      return;
    }

    if (!this.connectivity.isOnline()) {
      this.errorMessage.set('Reconnect before adding photo evidence.');
      return;
    }

    if (this.hasLocalDraft()) {
      this.errorMessage.set('The local checklist draft must be synchronized before adding photos.');
      return;
    }

    if (this.form.dirty || this.saveState() === 'saving') {
      this.errorMessage.set('Wait for the current checklist changes to save before adding a photo.');
      return;
    }

    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 10 * 1024 * 1024) {
      this.errorMessage.set('Choose a JPEG, PNG, or WebP image no larger than 10 MB.');
      return;
    }

    this.uploadingObservationId.set(observationId);
    this.readinessChange.emit(false);
    this.errorMessage.set(null);
    this.form.disable({ emitEvent: false });

    this.inspectionService
      .uploadAttachment(this.inspection().id, observationId, this.rowVersion(), file)
      .pipe(finalize(() => {
        this.uploadingObservationId.set(null);
        if (!this.concurrencyBlocked()) {
          this.form.enable({ emitEvent: false });
        }
      }))
      .subscribe({
        next: (uploaded) => {
          this.rowVersion.set(uploaded.rowVersion);
          this.attachments.update((current) => {
            const updated = new Map(current);
            updated.set(observationId, [
              ...(updated.get(observationId) ?? []),
              {
                id: uploaded.id,
                fileName: uploaded.fileName,
                contentType: uploaded.contentType,
                length: uploaded.length,
                uploadedAtUtc: uploaded.uploadedAtUtc,
              },
            ]);
            return updated;
          });
          this.attachmentUploaded.emit(uploaded);
          this.readinessChange.emit(true);
        },
        error: (error: unknown) => this.handleMutationError(error),
      });
  }

  downloadAttachment(attachment: InspectionAttachment): void {
    if (!this.connectivity.isOnline()) {
      this.errorMessage.set('Reconnect before downloading photo evidence.');
      return;
    }

    this.inspectionService
      .downloadAttachment(this.inspection().id, attachment.id)
      .subscribe({
        next: (content) => {
          const url = URL.createObjectURL(content);
          const link = document.createElement('a');
          link.href = url;
          link.download = attachment.fileName;
          link.click();
          URL.revokeObjectURL(url);
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  private persistDraft(): Observable<void> {
    return defer(() => {
      const inspection = this.inspection();
      this.saveState.set('saving');
      this.errorMessage.set(null);

      const observations = this.getDrafts();

      if (!this.connectivity.isOnline() || this.hasLocalDraft()) {
        return this.saveLocalDraft(observations);
      }

      return this.inspectionService.saveDraft(
        inspection.id,
        this.rowVersion(),
        observations,
      ).pipe(
        map((result): DraftPersistenceResult => ({
          location: 'server',
          rowVersion: result.rowVersion,
          observations,
        })),
        catchError((error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 0) {
            return this.saveLocalDraft(observations);
          }

          return throwError(() => error);
        }),
      );
    }).pipe(
      tap((result) => {
        this.form.markAsPristine();
        this.savedAt.set(new Date());

        if (result.location === 'device') {
          this.hasLocalDraft.set(true);
          this.saveState.set('saved-locally');
          this.readinessChange.emit(false);
          return;
        }

        this.rowVersion.set(result.rowVersion);
        this.draftSaved.emit({
          rowVersion: result.rowVersion,
          observations: result.observations,
        });
        this.saveState.set('saved');
        this.readinessChange.emit(true);
      }),
      map(() => undefined),
      catchError((error: unknown) => {
        this.handleMutationError(error);
        return EMPTY;
      }),
    );
  }

  private saveLocalDraft(
    observations: readonly SaveInspectionObservationDraft[],
  ): Observable<DraftPersistenceResult> {
    const userId = this.auth.currentUser()?.id;

    if (!userId) {
      return throwError(() => new Error('A signed-in user is required for offline storage.'));
    }

    const draft: OfflineInspectionDraft = {
      userId,
      inspectionId: this.inspection().id,
      rowVersion: this.rowVersion(),
      observations,
      savedAtUtc: new Date().toISOString(),
    };

    return from(this.offlineDraftStore.save(draft)).pipe(
      map((): DraftPersistenceResult => ({ location: 'device', observations })),
    );
  }

  private getDrafts(): readonly SaveInspectionObservationDraft[] {
    return this.form.getRawValue().observations.map((draft) => ({
      observationId: draft.observationId,
      outcome: draft.outcome,
      notes: draft.notes.trim() || null,
    }));
  }

  private handleMutationError(error: unknown): void {
    this.readinessChange.emit(false);
    this.saveState.set('error');
    this.errorMessage.set(getApiErrorMessage(error));

    if (!(error instanceof HttpErrorResponse)) {
      this.errorMessage.set(
        'Changes could not be saved on this device. Reconnect before continuing.',
      );
      return;
    }

    if (error.status === 409) {
      this.concurrencyBlocked.set(true);
      this.form.disable({ emitEvent: false });

      if (this.hasLocalDraft()) {
        this.conflictDetected.set(true);
        this.errorMessage.set(
          'The inspection changed on the server. Your local draft is preserved; use the server version to resolve the conflict.',
        );
        return;
      }

      this.errorMessage.set('This inspection changed elsewhere. Refresh the page before continuing.');
    }
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
