export interface DailyDigest {
  date: string;
  meals: PlannedMeal[];
}

export interface PlannedMeal {
  id: string;
  mealType: string;
  recipeId: string;
  recipeName: string;
  imageUrl: string | null;
}

export interface RecipeSuggestion {
  id: string;
  name: string;
  imageUrl: string | null;
  description: string | null;
}

export interface SuggestionsResponse {
  mealId: string;
  mealType: string;
  suggestions: RecipeSuggestion[];
}

export interface SwapMealResponse {
  mealId: string;
  mealType: string;
  recipeName: string;
  imageUrl: string | null;
}
