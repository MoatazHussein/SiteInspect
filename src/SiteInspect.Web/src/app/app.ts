import { Component, inject, viewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { AuthService } from './core/auth/auth.service';
import { roles } from './core/auth/auth.models';
import { ConnectivityService } from './core/connectivity/connectivity.service';

@Component({
  selector: 'app-root',
  imports: [
    NzButtonModule,
    NzAlertModule,
    NzLayoutModule,
    NzMenuModule,
    NzTagModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly router = inject(Router);
  private readonly outlet = viewChild(RouterOutlet);

  readonly auth = inject(AuthService);
  readonly connectivity = inject(ConnectivityService);
  readonly roles = roles;

  logout(): void {
    // Ask before clearing the session, which also removes the current page.
    const outlet = this.outlet();
    const page = outlet?.isActivated
      ? outlet.component as { canLeavePage?: () => boolean }
      : undefined;
    if (page?.canLeavePage && !page.canLeavePage()) {
      return;
    }

    this.auth.logout().subscribe({
      next: () => void this.router.navigate(['/login']),
      error: () => void this.router.navigate(['/login']),
    });
  }
}
