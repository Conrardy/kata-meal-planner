export interface WeeklyPlan {
  startDate: string;
  endDate: string;
  days: DayPlan[];
}

export interface DayPlan {
  date: string;
  dayName: string;
  breakfast: WeeklyMeal | null;
  lunch: WeeklyMeal | null;
  dinner: WeeklyMeal | null;
}

export interface WeeklyMeal {
  id: string;
  recipeId: string;
  recipeName: string;
  imageUrl: string | null;
}
