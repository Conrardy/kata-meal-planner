import { Routes } from '@angular/router';
import { DailyDigestComponent } from './features/daily-digest/daily-digest.component';

export const routes: Routes = [
  { path: '', component: DailyDigestComponent },
  { path: '**', redirectTo: '' },
];
