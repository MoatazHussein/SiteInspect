import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { getApiErrorMessage } from '../../../../core/api/api-error';
import {
  ContractorCorrectiveAction,
  CorrectiveActionStatus,
} from '../../models/corrective-action.models';
import { CorrectiveActionService } from '../../services/corrective-action.service';

@Component({
  selector: 'app-contractor-actions-page',
  imports: [DatePipe, NzAlertModule, NzButtonModule, NzCardModule, NzInputModule, NzTagModule],
  templateUrl: './contractor-actions-page.html',
  styleUrl: './contractor-actions-page.scss',
})
export class ContractorActionsPage implements OnInit {
  private readonly service = inject(CorrectiveActionService);

  readonly actions = signal<readonly ContractorCorrectiveAction[]>([]);
  readonly loading = signal(true);
  readonly submittingId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.service
      .mine()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (actions) => this.actions.set(actions),
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  submit(action: ContractorCorrectiveAction, notes: string): void {
    const trimmedNotes = notes.trim();
    if (!trimmedNotes || this.submittingId() !== null) {
      return;
    }

    this.submittingId.set(action.id);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.service
      .respond(action.id, trimmedNotes, action.rowVersion)
      .pipe(finalize(() => this.submittingId.set(null)))
      .subscribe({
        next: () => {
          this.successMessage.set('The response was submitted for manager review.');
          this.load();
        },
        error: (error: unknown) => this.errorMessage.set(getApiErrorMessage(error)),
      });
  }

  statusColor(status: CorrectiveActionStatus): string {
    return status === 'Closed' ? 'green' : status === 'ReadyForReview' ? 'blue' : 'orange';
  }
}
