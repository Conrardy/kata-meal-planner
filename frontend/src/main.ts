import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { autoLoginForDevelopment } from './app/core/helpers/dev-auth.helper';

// Auto-login in development mode
autoLoginForDevelopment();

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
