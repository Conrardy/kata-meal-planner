import { Component, input, output, signal, computed, inject, OnInit } from '@angular/core';
import { LucideAngularModule, X, ChefHat } from 'lucide-angular';
import { RecipeService } from '../../../../core/services/recipe.service';
import { RecipeItem } from '../../../../core/models/recipe.model';

@Component({
  selector: 'app-add-recipe-modal',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './add-recipe-modal.component.html',
})
export class AddRecipeModalComponent implements OnInit {
  private readonly recipeService = inject(RecipeService);

  date = input.required<string>();
  mealType = input.required<string>();

  close = output<void>();
  recipeSelected = output<string>();

  readonly recipes = signal<RecipeItem[]>([]);
  readonly searchTerm = signal<string>('');
  readonly selectedRecipeId = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly filteredRecipes = computed(() => {
    const search = this.searchTerm().toLowerCase();
    if (!search) {
      return this.recipes();
    }
    return this.recipes().filter(recipe =>
      recipe.name.toLowerCase().includes(search)
    );
  });

  readonly XIcon = X;
  readonly ChefHat = ChefHat;

  ngOnInit(): void {
    this.loadRecipes();
  }

  private loadRecipes(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.recipeService.searchRecipes().subscribe({
      next: (result) => {
        const sortedRecipes = [...result.recipes].sort((a, b) =>
          a.name.localeCompare(b.name)
        );
        this.recipes.set(sortedRecipes);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load recipes:', err);
        this.error.set('Failed to load recipes. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
  }

  onSelectRecipe(recipeId: string): void {
    this.selectedRecipeId.set(recipeId);
  }

  onClose(): void {
    this.close.emit();
  }

  onAdd(): void {
    const recipeId = this.selectedRecipeId();
    if (recipeId) {
      this.recipeSelected.emit(recipeId);
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.close.emit();
    }
  }
}
