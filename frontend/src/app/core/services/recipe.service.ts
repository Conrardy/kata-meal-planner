import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RecipeDetails, RecipeSearchResult } from '../models/recipe.model';
import { map } from 'rxjs/operators';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class RecipeService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  getRecipeDetails(recipeId: string): Observable<RecipeDetails> {
    return this.http.get<RecipeDetails>(`${this.baseUrl}/recipes/${recipeId}`);
  }

  searchRecipes(searchTerm?: string, tags?: string[]): Observable<RecipeSearchResult> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('search', searchTerm);
    }
    if (tags && tags.length > 0) {
      params = params.set('tags', tags.join(','));
    }
    return this.http.get<RecipeSearchResult>(`${this.baseUrl}/recipes`, { params });
  }

  createRecipe(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/recipes`, payload).pipe(map((r: any) => r));
  }
}
