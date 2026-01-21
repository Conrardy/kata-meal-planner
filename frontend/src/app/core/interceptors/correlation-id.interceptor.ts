import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';

export const correlationIdInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const correlationId = generateCorrelationId();

  const clonedRequest = req.clone({
    setHeaders: {
      'X-Correlation-ID': correlationId,
    },
  });

  return next(clonedRequest);
};

function generateCorrelationId(): string {
  const timestamp = Date.now().toString(36);
  const randomPart = Math.random().toString(36).substring(2, 10);
  return `${timestamp}-${randomPart}`;
}
