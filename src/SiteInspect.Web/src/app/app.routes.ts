import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'dashboard',
    loadChildren: () =>
      import('./features/dashboard/dashboard.routes').then((routes) => routes.DASHBOARD_ROUTES),
  },
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
    path: 'corrective-actions',
    loadChildren: () =>
      import('./features/corrective-actions/corrective-actions.routes').then(
        (routes) => routes.CORRECTIVE_ACTION_ROUTES,
      ),
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
