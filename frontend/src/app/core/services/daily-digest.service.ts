import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DailyDigest,
  SuggestionsResponse,
  SwapMealResponse,
} from '../models/daily-digest.model';

@Injectable({
  providedIn: 'root',
})
export class DailyDigestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/v1';

  getDailyDigest(date: Date): Observable<DailyDigest> {
    const formattedDate = this.formatDate(date);
    return this.http.get<DailyDigest>(
      `${this.baseUrl}/daily-digest/${formattedDate}`
    );
  }

  getSuggestions(mealId: string): Observable<SuggestionsResponse> {
    return this.http.get<SuggestionsResponse>(
      `${this.baseUrl}/meals/${mealId}/suggestions`
    );
  }

  swapMeal(mealId: string, newRecipeId: string): Observable<SwapMealResponse> {
    return this.http.post<SwapMealResponse>(
      `${this.baseUrl}/meals/${mealId}/swap`,
      { newRecipeId }
    );
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
