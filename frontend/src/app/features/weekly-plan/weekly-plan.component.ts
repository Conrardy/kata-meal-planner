import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { LucideAngularModule, ChefHat, ChevronLeft, ChevronRight } from 'lucide-angular';
import { WeeklyPlanService } from '../../core/services/weekly-plan.service';
import { WeeklyPlan, DayPlan, WeeklyMeal } from '../../core/models/weekly-plan.model';
import { SwapModalComponent } from '../daily-digest/components/swap-modal/swap-modal.component';
import { AddRecipeModalComponent } from './components/add-recipe-modal/add-recipe-modal.component';
import { DailyDigestService } from '../../core/services/daily-digest.service';
import { MealPlanService } from '../../core/services/meal-plan.service';

@Component({
  selector: 'app-weekly-plan',
  standalone: true,
  imports: [DatePipe, RouterLink, LucideAngularModule, SwapModalComponent, AddRecipeModalComponent],
  templateUrl: './weekly-plan.component.html',
})
export class WeeklyPlanComponent implements OnInit {
  private readonly weeklyPlanService = inject(WeeklyPlanService);
  private readonly dailyDigestService = inject(DailyDigestService);
  private readonly mealPlanService = inject(MealPlanService);
  private readonly router = inject(Router);

  readonly startDate = signal<Date>(this.getStartOfWeek(new Date()));
  readonly weeklyPlan = signal<WeeklyPlan | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly swappingMealId = signal<string | null>(null);
  readonly swappingMealType = signal<string | null>(null);
  readonly isSwapping = signal(false);
  readonly addingRecipeDate = signal<string | null>(null);
  readonly addingRecipeMealType = signal<string | null>(null);
  readonly isAdding = signal(false);
  readonly toastMessage = signal<string | null>(null);
  readonly toastType = signal<'success' | 'error'>('success');

  readonly ChefHat = ChefHat;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;

  readonly mealTypes = ['Breakfast', 'Lunch', 'Dinner'] as const;

  readonly formattedDateRange = computed(() => {
    const plan = this.weeklyPlan();
    if (!plan) return '';
    return `${this.formatDisplayDate(plan.startDate)} - ${this.formatDisplayDate(plan.endDate)}`;
  });

  ngOnInit(): void {
    this.loadWeeklyPlan();
  }

  private loadWeeklyPlan(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.weeklyPlanService.getWeeklyPlan(this.startDate()).subscribe({
      next: (plan: WeeklyPlan) => {
        this.weeklyPlan.set(plan);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load weekly plan:', err);
        this.error.set('Failed to load weekly plan. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }

  onPreviousWeek(): void {
    const newStart = new Date(this.startDate());
    newStart.setDate(newStart.getDate() - 7);
    this.startDate.set(newStart);
    this.loadWeeklyPlan();
  }

  onNextWeek(): void {
    const newStart = new Date(this.startDate());
    newStart.setDate(newStart.getDate() + 7);
    this.startDate.set(newStart);
    this.loadWeeklyPlan();
  }

  onMealClick(meal: WeeklyMeal | null): void {
    if (meal) {
      this.router.navigate(['/recipe', meal.recipeId]);
    }
  }

  onEmptyCellClick(date: string, mealType: string): void {
    this.addingRecipeDate.set(date);
    this.addingRecipeMealType.set(mealType);
  }

  onCloseAddModal(): void {
    this.addingRecipeDate.set(null);
    this.addingRecipeMealType.set(null);
  }

  onRecipeSelectedForAdd(recipeId: string): void {
    const date = this.addingRecipeDate();
    const mealType = this.addingRecipeMealType();
    if (!date || !mealType) return;

    this.isAdding.set(true);
    this.mealPlanService.addRecipeToMealPlan(recipeId, date, mealType).subscribe({
      next: (result) => {
        this.updatePlanWithNewMeal(date, mealType, result);
        this.onCloseAddModal();
        this.showToast(`${result.recipeName} added to ${mealType}`, 'success');
        this.isAdding.set(false);
      },
      error: (err) => {
        console.error('Failed to add recipe to meal plan:', err);
        this.showToast('Failed to add recipe. Please try again.', 'error');
        this.isAdding.set(false);
      },
    });
  }

  private showToast(message: string, type: 'success' | 'error'): void {
    this.toastMessage.set(message);
    this.toastType.set(type);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 3000);
  }

  private updatePlanWithNewMeal(
    date: string,
    mealType: string,
    result: { mealId: string; recipeName: string; date: string; mealType: string }
  ): void {
    this.weeklyPlan.update((plan) => {
      if (!plan) return plan;

      return {
        ...plan,
        days: plan.days.map((day) => {
          if (day.date !== date) return day;

          const newMeal: WeeklyMeal = {
            id: result.mealId,
            recipeId: '', // Not provided in response, but not needed for display
            recipeName: result.recipeName,
            imageUrl: null,
          };

          switch (mealType) {
            case 'Breakfast':
              return { ...day, breakfast: newMeal };
            case 'Lunch':
              return { ...day, lunch: newMeal };
            case 'Dinner':
              return { ...day, dinner: newMeal };
            default:
              return day;
          }
        }),
      };
    });
  }

  onEditMeal(meal: WeeklyMeal | null, mealType: string, event: MouseEvent): void {
    event.stopPropagation();
    if (meal) {
      this.swappingMealId.set(meal.id);
      this.swappingMealType.set(mealType);
    }
  }

  onCloseSwapModal(): void {
    this.swappingMealId.set(null);
    this.swappingMealType.set(null);
  }

  onSelectRecipe(recipeId: string): void {
    const mealId = this.swappingMealId();
    if (!mealId) return;

    this.isSwapping.set(true);
    this.dailyDigestService.swapMeal(mealId, recipeId).subscribe({
      next: (result) => {
        this.weeklyPlan.update((plan) => {
          if (!plan) return plan;
          return {
            ...plan,
            days: plan.days.map((day) => ({
              ...day,
              breakfast: this.updateMealIfMatches(day.breakfast, mealId, result),
              lunch: this.updateMealIfMatches(day.lunch, mealId, result),
              dinner: this.updateMealIfMatches(day.dinner, mealId, result),
            })),
          };
        });
        this.swappingMealId.set(null);
        this.swappingMealType.set(null);
        this.isSwapping.set(false);
      },
      error: (err) => {
        console.error('Failed to swap meal:', err);
        this.isSwapping.set(false);
      },
    });
  }

  getMeal(day: DayPlan, mealType: string): WeeklyMeal | null {
    switch (mealType) {
      case 'Breakfast': return day.breakfast;
      case 'Lunch': return day.lunch;
      case 'Dinner': return day.dinner;
      default: return null;
    }
  }

  isToday(dateStr: string): boolean {
    const today = new Date();
    const date = new Date(dateStr);
    return today.toDateString() === date.toDateString();
  }

  private updateMealIfMatches(
    meal: WeeklyMeal | null,
    mealId: string,
    result: { recipeName: string; imageUrl: string | null }
  ): WeeklyMeal | null {
    if (meal?.id === mealId) {
      return { ...meal, recipeName: result.recipeName, imageUrl: result.imageUrl };
    }
    return meal;
  }

  private getStartOfWeek(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    d.setDate(d.getDate() - day);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private formatDisplayDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
