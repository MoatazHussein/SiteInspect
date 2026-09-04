import { DOCUMENT } from '@angular/common';
import { inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { fromEvent, merge } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ConnectivityService {
  private readonly browserWindow = inject(DOCUMENT).defaultView;

  readonly isOnline = signal(this.browserWindow?.navigator.onLine ?? true);

  constructor() {
    if (!this.browserWindow) {
      return;
    }

    merge(
      fromEvent(this.browserWindow, 'online'),
      fromEvent(this.browserWindow, 'offline'),
    )
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.isOnline.set(this.browserWindow?.navigator.onLine ?? true));
  }
}
