export interface CurrentUser {
  readonly id: string;
  readonly email: string;
  readonly displayName: string;
  readonly roles: readonly string[];
}

export interface PublicSession {
  readonly accessToken: string;
  readonly accessTokenExpiresAtUtc: string;
  readonly user: CurrentUser;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

export const roles = {
  manager: 'Manager',
  inspector: 'Inspector',
  contractor: 'Contractor',
} as const;
