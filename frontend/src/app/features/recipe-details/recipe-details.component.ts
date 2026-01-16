import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LucideAngularModule, ArrowLeft, ChefHat } from 'lucide-angular';
import { RecipeService } from '../../core/services/recipe.service';
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

  readonly recipe = signal<RecipeDetails | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly ArrowLeft = ArrowLeft;
  readonly ChefHat = ChefHat;

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
    this.router.navigate(['/']);
  }
}
