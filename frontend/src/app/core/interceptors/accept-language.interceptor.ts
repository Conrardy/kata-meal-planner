import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LocaleService } from '../services/locale.service';

export const acceptLanguageInterceptor: HttpInterceptorFn = (req, next) => {
  const localeService = inject(LocaleService);

  const localizedReq = req.clone({
    setHeaders: {
      'Accept-Language': localeService.getAcceptLanguageHeader(),
    },
  });

  return next(localizedReq);
};
