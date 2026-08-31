import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guards';

export const FOUNDATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/foundation/foundation-page').then((component) => component.FoundationPage),
    canActivate: [authGuard],
    title: 'Foundation · SiteInspect',
  },
];
