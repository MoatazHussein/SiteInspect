import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, finalize, map, Observable, of, shareReplay, tap } from 'rxjs';
import { ApiResponse } from '../api/api-response';
import { CurrentUser, LoginRequest, PublicSession, roles } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/auth';
  private readonly accessTokenState = signal<string | null>(null);
  private readonly currentUserState = signal<CurrentUser | null>(null);
  private refreshRequest: Observable<boolean> | null = null;

  readonly currentUser = this.currentUserState.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserState() !== null);

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.http
      .post<ApiResponse<PublicSession>>(`${this.endpoint}/login`, request, {
        withCredentials: true,
      })
      .pipe(
        map((response) => this.requireData(response)),
        tap((session) => this.applySession(session)),
        map((session) => session.user),
      );
  }

  restoreSession(): Observable<boolean> {
    if (this.refreshRequest) {
      return this.refreshRequest;
    }

    this.refreshRequest = this.http
      .post<ApiResponse<PublicSession>>(`${this.endpoint}/refresh`, null, {
        withCredentials: true,
      })
      .pipe(
        map((response) => this.requireData(response)),
        tap((session) => this.applySession(session)),
        map(() => true),
        catchError(() => {
          this.clearSession();
          return of(false);
        }),
        finalize(() => (this.refreshRequest = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.refreshRequest;
  }

  logout(): Observable<void> {
    return this.http
      .post<ApiResponse<null>>(`${this.endpoint}/logout`, null, { withCredentials: true })
      .pipe(
        map(() => undefined),
        finalize(() => this.clearSession()),
      );
  }

  accessToken(): string | null {
    return this.accessTokenState();
  }

  hasAnyRole(expectedRoles: readonly string[]): boolean {
    const user = this.currentUserState();
    return user !== null && expectedRoles.some((role) => user.roles.includes(role));
  }

  defaultRoute(): string {
    if (this.hasAnyRole([roles.manager, roles.inspector])) {
      return '/inspections';
    }

    return this.hasAnyRole([roles.contractor]) ? '/corrective-actions' : '/foundation';
  }

  clearSession(): void {
    this.accessTokenState.set(null);
    this.currentUserState.set(null);
  }

  private applySession(session: PublicSession): void {
    this.accessTokenState.set(session.accessToken);
    this.currentUserState.set(session.user);
  }

  private requireData<T>(response: ApiResponse<T>): T {
    if (!response.isSuccess || response.data === null) {
      throw new Error(response.errors[0]?.message ?? 'The API returned an empty response.');
    }

    return response.data;
  }
}
