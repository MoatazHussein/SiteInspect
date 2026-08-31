export interface ApiErrorResponse {
  readonly code: string;
  readonly message: string;
}

export interface ApiResponse<T> {
  readonly isSuccess: boolean;
  readonly data: T | null;
  readonly errors: readonly ApiErrorResponse[];
  readonly correlationId: string | null;
}
