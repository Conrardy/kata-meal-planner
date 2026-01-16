export interface RecipeDetails {
  id: string;
  name: string;
  imageUrl: string | null;
  description: string | null;
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
