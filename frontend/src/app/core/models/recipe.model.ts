export interface Ingredient {
  name: string;
  quantity: string;
  unit: string | null;
}

export interface CookingStep {
  stepNumber: number;
  instruction: string;
}

export interface RecipeDetails {
  id: string;
  name: string;
  imageUrl: string | null;
  description: string | null;
  tags: string[];
  mealType: string;
  ingredients: Ingredient[];
  steps: CookingStep[];
}

export interface RecipeItem {
  id: string;
  name: string;
  imageUrl: string | null;
  description: string | null;
  tags: string[];
}

export interface RecipeSearchResult {
  recipes: RecipeItem[];
  availableTags: string[];
}
