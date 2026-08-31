import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

const authPaths = ['/api/auth/login', '/api/auth/refresh', '/api/auth/logout'];

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const isApiRequest = request.url.startsWith('/api/');
  const isAuthRequest = authPaths.some((path) => request.url.startsWith(path));
  const accessToken = authService.accessToken();

  const authenticatedRequest =
    isApiRequest && accessToken
      ? request.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
      : request;

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthRequest) {
        return throwError(() => error);
      }

      return authService.restoreSession().pipe(
        switchMap((restored) => {
          const replacementToken = authService.accessToken();
          if (!restored || !replacementToken) {
            void router.navigate(['/login']);
            return throwError(() => error);
          }

          return next(
            request.clone({ setHeaders: { Authorization: `Bearer ${replacementToken}` } }),
          );
        }),
      );
    }),
  );
};
