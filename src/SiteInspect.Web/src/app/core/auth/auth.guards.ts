import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.isAuthenticated() ? true : router.createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const defaultRoute = authService.defaultRoute();
  return authService.isAuthenticated() && defaultRoute !== '/login'
    ? router.createUrlTree([defaultRoute])
    : true;
};

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const expectedRoles = (route.data['roles'] as readonly string[] | undefined) ?? [];

  return authService.hasAnyRole(expectedRoles)
    ? true
    : router.createUrlTree([authService.defaultRoute()]);
};
