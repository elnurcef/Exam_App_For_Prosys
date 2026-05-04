import { HttpErrorResponse } from '@angular/common/http';

export function getApiError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const serverMessage = error.error?.message;
    if (typeof serverMessage === 'string' && serverMessage.trim().length > 0) {
      return serverMessage;
    }

    if (error.status === 0) {
      return 'API server is not reachable.';
    }
  }

  return 'Something went wrong. Please try again.';
}
