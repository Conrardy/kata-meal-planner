import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WeeklyPlan } from '../models/weekly-plan.model';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class WeeklyPlanService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  getWeeklyPlan(startDate: Date): Observable<WeeklyPlan> {
    const formattedDate = this.formatDate(startDate);
    return this.http.get<WeeklyPlan>(
      `${this.baseUrl}/weekly-plan/${formattedDate}`
    );
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
