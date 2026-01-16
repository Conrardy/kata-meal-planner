import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  private readonly baseUrl = 'http://localhost:5000/api/v1';

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
