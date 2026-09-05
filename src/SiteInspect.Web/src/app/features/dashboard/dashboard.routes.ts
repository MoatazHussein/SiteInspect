import { Routes } from '@angular/router';
import { authGuard, roleGuard } from '../../core/auth/auth.guards';
import { roles } from '../../core/auth/auth.models';

export const DASHBOARD_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: [roles.manager] },
    loadComponent: () =>
      import('./pages/dashboard/dashboard-page').then((component) => component.DashboardPage),
    title: 'Dashboard · SiteInspect',
  },
];
