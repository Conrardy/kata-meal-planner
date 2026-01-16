import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RecipeDetails } from '../models/recipe.model';

@Injectable({
  providedIn: 'root',
})
export class RecipeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/v1';

  getRecipeDetails(recipeId: string): Observable<RecipeDetails> {
    return this.http.get<RecipeDetails>(`${this.baseUrl}/recipes/${recipeId}`);
  }
}
