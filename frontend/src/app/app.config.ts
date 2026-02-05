import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { PreloadAllModules, provideRouter, withPreloading } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { correlationIdInterceptor } from './core/interceptors/correlation-id.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { httpErrorInterceptor } from './core/interceptors/http-error.interceptor';
import { acceptLanguageInterceptor } from './core/interceptors/accept-language.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withPreloading(PreloadAllModules)),
    provideHttpClient(
      withInterceptors([
        acceptLanguageInterceptor,
        correlationIdInterceptor,
        authInterceptor,
        httpErrorInterceptor,
      ])
    ),
  ],
};
