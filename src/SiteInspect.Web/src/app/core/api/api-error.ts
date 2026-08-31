import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from './api-response';

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const response = error.error as Partial<ApiResponse<unknown>> | null;
    const message = response?.errors?.[0]?.message;

    if (message) {
      return message;
    }
  }

  return 'The request could not be completed. Please try again.';
}
