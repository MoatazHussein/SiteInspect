import { Routes } from '@angular/router';
import { guestGuard } from '../../core/auth/auth.guards';

export const AUTHENTICATION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/login/login-page').then((component) => component.LoginPage),
    canActivate: [guestGuard],
    title: 'Sign in · SiteInspect',
  },
];
