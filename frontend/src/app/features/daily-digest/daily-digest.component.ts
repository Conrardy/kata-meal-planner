import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MealCardComponent } from './components/meal-card/meal-card.component';
import { DailyDigestService } from '../../core/services/daily-digest.service';
import { DailyDigest, PlannedMeal } from '../../core/models/daily-digest.model';

@Component({
  selector: 'app-daily-digest',
  standalone: true,
  imports: [DatePipe, MealCardComponent],
  templateUrl: './daily-digest.component.html',
})
export class DailyDigestComponent implements OnInit {
  private readonly dailyDigestService = inject(DailyDigestService);

  readonly today = signal(new Date());
  readonly meals = signal<PlannedMeal[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

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
    console.log('Swap meal:', mealId);
  }

  onCookNow(mealId: string): void {
    console.log('Cook now:', mealId);
  }
}
