import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, timer, retry } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ErrorNotificationService } from '../services/error-notification.service';
import { LoggingService } from '../services/logging.service';

const MAX_RETRIES = 3;
const INITIAL_BACKOFF_MS = 1000;

export const httpErrorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const errorNotification = inject(ErrorNotificationService);
  const logger = inject(LoggingService);

  return next(req).pipe(
    retry({
      count: MAX_RETRIES,
      delay: (error, retryCount) => {
        if (!isServerError(error)) {
          throw error;
        }
        const delayMs = INITIAL_BACKOFF_MS * Math.pow(2, retryCount - 1);
        logger.warn(`Retry attempt ${retryCount}/${MAX_RETRIES} after ${delayMs}ms`, {
          url: req.url,
          status: error.status,
        });
        return timer(delayMs);
      },
    }),
    catchError((error: HttpErrorResponse) => {
      logger.error('HTTP request failed', {
        url: req.url,
        method: req.method,
        status: error.status,
        message: error.message,
        correlationId: req.headers.get('X-Correlation-ID'),
      });

      if (error.status === 0) {
        errorNotification.showError(
          'Network error. Please check your internet connection and try again.'
        );
      } else if (error.status === 401 && !isAuthRequest(req.url)) {
        authService.logout();
        router.navigate(['/login']);
      } else if (error.status === 403) {
        errorNotification.showError(
          'You do not have permission to perform this action.'
        );
      } else if (error.status >= 500) {
        errorNotification.showError(
          'Server error. Please try again later. If the problem persists, contact support.'
        );
      }

      return throwError(() => error);
    })
  );
};

function isServerError(error: unknown): error is HttpErrorResponse {
  return error instanceof HttpErrorResponse && error.status >= 500;
}

function isAuthRequest(url: string): boolean {
  return (
    url.includes('/auth/login') ||
    url.includes('/auth/register') ||
    url.includes('/auth/refresh')
  );
}
