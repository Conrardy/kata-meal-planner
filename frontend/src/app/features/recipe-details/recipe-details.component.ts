import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  LucideAngularModule,
  ArrowLeft,
  ChefHat,
  UtensilsCrossed,
  ListChecks,
  CalendarPlus,
  X,
} from 'lucide-angular';
import { RecipeService } from '../../core/services/recipe.service';
import { MealPlanService } from '../../core/services/meal-plan.service';
import { RecipeDetails } from '../../core/models/recipe.model';

@Component({
  selector: 'app-recipe-details',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './recipe-details.component.html',
})
export class RecipeDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly recipeService = inject(RecipeService);
  private readonly mealPlanService = inject(MealPlanService);

  readonly recipe = signal<RecipeDetails | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly showAddToMealPlanModal = signal(false);
  readonly selectedDate = signal(this.getTodayDateString());
  readonly selectedMealSlot = signal<string | null>(null);

  readonly mealSlots = ['Breakfast', 'Lunch', 'Dinner'];

  readonly ArrowLeft = ArrowLeft;
  readonly ChefHat = ChefHat;
  readonly UtensilsCrossed = UtensilsCrossed;
  readonly ListChecks = ListChecks;
  readonly CalendarPlus = CalendarPlus;
  readonly X = X;

  ngOnInit(): void {
    const recipeId = this.route.snapshot.paramMap.get('recipeId');
    if (recipeId) {
      this.loadRecipe(recipeId);
    } else {
      this.error.set('Recipe ID not found');
      this.isLoading.set(false);
    }
  }

  private loadRecipe(recipeId: string): void {
    this.recipeService.getRecipeDetails(recipeId).subscribe({
      next: (recipe) => {
        this.recipe.set(recipe);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load recipe:', err);
        this.error.set('Failed to load recipe details. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }

  goBack(): void {
    window.history.back();
  }

  openAddToMealPlan(): void {
    this.selectedDate.set(this.getTodayDateString());
    this.selectedMealSlot.set(this.recipe()?.mealType ?? null);
    this.showAddToMealPlanModal.set(true);
  }

  closeAddToMealPlanModal(): void {
    this.showAddToMealPlanModal.set(false);
  }

  onDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedDate.set(input.value);
  }

  selectMealSlot(slot: string): void {
    this.selectedMealSlot.set(slot);
  }

  confirmAddToMealPlan(): void {
    const recipe = this.recipe();
    const date = this.selectedDate();
    const mealSlot = this.selectedMealSlot();

    if (!recipe || !date || !mealSlot) {
      return;
    }

    this.mealPlanService.addRecipeToMealPlan(recipe.id, date, mealSlot).subscribe({
      next: () => {
        this.closeAddToMealPlanModal();
        this.router.navigate(['/weekly-plan']);
      },
      error: (err) => {
        console.error('Failed to add recipe to meal plan:', err);
      },
    });
  }

  private getTodayDateString(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }
}
