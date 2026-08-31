import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () =>
      import('./features/authentication/authentication.routes').then(
        (routes) => routes.AUTHENTICATION_ROUTES,
      ),
  },
  {
    path: 'foundation',
    loadChildren: () =>
      import('./features/foundation/foundation.routes').then((routes) => routes.FOUNDATION_ROUTES),
  },
  {
    path: 'inspections',
    loadChildren: () =>
      import('./features/inspections/inspections.routes').then(
        (routes) => routes.INSPECTIONS_ROUTES,
      ),
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'inspections',
  },
  {
    path: '**',
    redirectTo: 'inspections',
  },
];
