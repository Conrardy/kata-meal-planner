import { Component, input, output, signal, inject, OnInit } from '@angular/core';
import { LucideAngularModule, X, ChefHat } from 'lucide-angular';
import { RecipeSuggestion } from '../../../../core/models/daily-digest.model';
import { DailyDigestService } from '../../../../core/services/daily-digest.service';

@Component({
  selector: 'app-swap-modal',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './swap-modal.component.html',
})
export class SwapModalComponent implements OnInit {
  private readonly dailyDigestService = inject(DailyDigestService);

  mealId = input.required<string>();
  mealType = input.required<string>();

  close = output<void>();
  selectRecipe = output<string>();

  readonly suggestions = signal<RecipeSuggestion[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly XIcon = X;
  readonly ChefHat = ChefHat;

  ngOnInit(): void {
    this.loadSuggestions();
  }

  private loadSuggestions(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.dailyDigestService.getSuggestions(this.mealId()).subscribe({
      next: (response) => {
        this.suggestions.set(response.suggestions);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load suggestions:', err);
        this.error.set('Failed to load suggestions. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  onClose(): void {
    this.close.emit();
  }

  onSelectRecipe(recipeId: string): void {
    this.selectRecipe.emit(recipeId);
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.close.emit();
    }
  }
}
