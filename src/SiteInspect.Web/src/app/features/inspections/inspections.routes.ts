import { Routes } from '@angular/router';
import { authGuard, roleGuard } from '../../core/auth/auth.guards';
import { roles } from '../../core/auth/auth.models';

export const INSPECTIONS_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: [roles.manager, roles.inspector] },
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/inspection-list/inspection-list-page').then(
            (component) => component.InspectionListPage,
          ),
        title: 'Inspections · SiteInspect',
      },
      {
        path: 'new',
        canActivate: [roleGuard],
        data: { roles: [roles.manager] },
        loadComponent: () =>
          import('./pages/inspection-create/inspection-create-page').then(
            (component) => component.InspectionCreatePage,
          ),
        title: 'Create inspection · SiteInspect',
      },
      {
        path: ':inspectionId',
        loadComponent: () =>
          import('./pages/inspection-detail/inspection-detail-page').then(
            (component) => component.InspectionDetailPage,
          ),
        title: 'Inspection details · SiteInspect',
      },
    ],
  },
];
