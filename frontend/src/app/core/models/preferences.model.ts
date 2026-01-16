export interface UserPreferences {
  dietaryPreference: string;
  allergies: string[];
  availableDietaryPreferences: string[];
  availableAllergies: string[];
  mealsPerDay: number;
  planLength: number;
  includeLeftovers: boolean;
  autoGenerateShoppingList: boolean;
  excludedIngredients: string[];
  availableMealsPerDay: number[];
  availablePlanLengths: number[];
}

export interface UpdatePreferencesRequest {
  dietaryPreference: string;
  allergies: string[];
  mealsPerDay?: number;
  planLength?: number;
  includeLeftovers?: boolean;
  autoGenerateShoppingList?: boolean;
  excludedIngredients?: string[];
}
