import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MealCardComponent } from './components/meal-card/meal-card.component';
import { SwapModalComponent } from './components/swap-modal/swap-modal.component';
import { DailyDigestService } from '../../core/services/daily-digest.service';
import { DailyDigest, PlannedMeal } from '../../core/models/daily-digest.model';

@Component({
  selector: 'app-daily-digest',
  standalone: true,
  imports: [DatePipe, MealCardComponent, SwapModalComponent],
  templateUrl: './daily-digest.component.html',
})
export class DailyDigestComponent implements OnInit {
  private readonly dailyDigestService = inject(DailyDigestService);

  readonly today = signal(new Date());
  readonly meals = signal<PlannedMeal[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly swappingMealId = signal<string | null>(null);
  readonly isSwapping = signal(false);

  readonly swappingMeal = computed(() => {
    const mealId = this.swappingMealId();
    if (!mealId) return null;
    return this.meals().find((m) => m.id === mealId) ?? null;
  });

  ngOnInit(): void {
    this.loadDailyDigest();
  }

  private loadDailyDigest(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.dailyDigestService.getDailyDigest(this.today()).subscribe({
      next: (digest: DailyDigest) => {
        this.meals.set(digest.meals);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load daily digest:', err);
        this.error.set('Failed to load meals. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }

  onSwapMeal(mealId: string): void {
    this.swappingMealId.set(mealId);
  }

  onCloseSwapModal(): void {
    this.swappingMealId.set(null);
  }

  onSelectRecipe(recipeId: string): void {
    const mealId = this.swappingMealId();
    if (!mealId) return;

    this.isSwapping.set(true);
    this.dailyDigestService.swapMeal(mealId, recipeId).subscribe({
      next: (result) => {
        this.meals.update((meals) =>
          meals.map((meal) =>
            meal.id === mealId
              ? { ...meal, recipeName: result.recipeName, imageUrl: result.imageUrl }
              : meal
          )
        );
        this.swappingMealId.set(null);
        this.isSwapping.set(false);
      },
      error: (err) => {
        console.error('Failed to swap meal:', err);
        this.isSwapping.set(false);
      },
    });
  }

  onCookNow(mealId: string): void {
    console.log('Cook now:', mealId);
  }
}
