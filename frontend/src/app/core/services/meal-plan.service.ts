import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from './api-config.service';

export interface AddRecipeToMealPlanResult {
  mealId: string;
  recipeName: string;
  date: string;
  mealType: string;
}

@Injectable({
  providedIn: 'root',
})
export class MealPlanService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  addRecipeToMealPlan(
    recipeId: string,
    date: string,
    mealType: string
  ): Observable<AddRecipeToMealPlanResult> {
    return this.http.post<AddRecipeToMealPlanResult>(`${this.baseUrl}/meal-plan`, {
      recipeId,
      date,
      mealType,
    });
  }
}
