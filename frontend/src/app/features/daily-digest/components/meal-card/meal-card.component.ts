import { Component, input, output } from '@angular/core';
import { LucideAngularModule, RefreshCw, ChefHat } from 'lucide-angular';
import { PlannedMeal } from '../../../../core/models/daily-digest.model';

@Component({
  selector: 'app-meal-card',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './meal-card.component.html',
})
export class MealCardComponent {
  meal = input.required<PlannedMeal>();

  swapMeal = output<string>();
  cookNow = output<string>();

  readonly RefreshCw = RefreshCw;
  readonly ChefHat = ChefHat;

  onSwapMeal(): void {
    this.swapMeal.emit(this.meal().id);
  }

  onCookNow(): void {
    this.cookNow.emit(this.meal().recipeId);
  }
}
