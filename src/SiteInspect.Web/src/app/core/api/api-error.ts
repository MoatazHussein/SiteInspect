import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from './api-response';

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const response = error.error as Partial<ApiResponse<unknown>> | null;
    const message = response?.errors?.[0]?.message;

    if (message) {
      return message;
    }
    if (error.status === 0) {
      return 'Cannot reach SiteInspect. Check your connection and try again.';
    }
    if (error.status === 429) {
      return 'Too many requests. Wait a minute and try again.';
    }
  }

  return 'The request could not be completed. Please try again.';
}
