import { Routes } from '@angular/router';
import { authGuard, roleGuard } from '../../core/auth/auth.guards';
import { roles } from '../../core/auth/auth.models';

export const CORRECTIVE_ACTION_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: [roles.contractor] },
    loadComponent: () =>
      import('./pages/contractor-actions/contractor-actions-page').then(
        (component) => component.ContractorActionsPage,
      ),
    title: 'My corrective actions · SiteInspect',
  },
];
