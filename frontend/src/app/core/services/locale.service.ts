import { LOCALE_ID, Injectable, inject } from '@angular/core';

export interface LocaleOption {
  code: string;
  label: string;
}

@Injectable({ providedIn: 'root' })
export class LocaleService {
  private readonly localeId = inject(LOCALE_ID);
  private readonly storageKey = 'mealplanner-locale';

  readonly availableLocales: LocaleOption[] = [
    { code: 'fr', label: 'Français' },
    { code: 'en', label: 'English' },
  ];

  get currentLocale(): string {
    return this.localeId;
  }

  switchLocale(localeCode: string): void {
    if (!this.availableLocales.some((l) => l.code === localeCode)) {
      throw new Error(`Unsupported locale: ${localeCode}`);
    }

    this.saveLocaleToStorage(localeCode);
    this.navigateToLocale(localeCode);
  }

  getStoredLocale(): string | null {
    return localStorage.getItem(this.storageKey);
  }

  getAcceptLanguageHeader(): string {
    const localeMap: Record<string, string> = {
      fr: 'fr-FR',
      en: 'en-US',
    };
    return localeMap[this.currentLocale] || 'en-US';
  }

  private saveLocaleToStorage(localeCode: string): void {
    localStorage.setItem(this.storageKey, localeCode);
  }

  private navigateToLocale(localeCode: string): void {
    const currentPath = window.location.pathname;
    const localeSegmentPattern = /^\/(fr|en)(\/|$)/;

    if (localeSegmentPattern.test(currentPath)) {
      const newPath = currentPath.replace(localeSegmentPattern, `/${localeCode}$2`);
      window.location.href = newPath;
    } else {
      window.location.reload();
    }
  }
}
