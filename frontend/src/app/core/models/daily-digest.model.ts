export interface DailyDigest {
  date: string;
  meals: PlannedMeal[];
}

export interface PlannedMeal {
  id: string;
  mealType: string;
  recipeName: string;
  imageUrl: string | null;
}
