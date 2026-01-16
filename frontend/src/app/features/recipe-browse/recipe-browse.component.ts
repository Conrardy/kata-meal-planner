import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, X, ChevronLeft } from 'lucide-angular';
import { RecipeService } from '../../core/services/recipe.service';
import { RecipeItem } from '../../core/models/recipe.model';
import { debounceTime, Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-recipe-browse',
  standalone: true,
  imports: [FormsModule, RouterLink, LucideAngularModule],
  templateUrl: './recipe-browse.component.html',
})
export class RecipeBrowseComponent implements OnInit {
  private readonly recipeService = inject(RecipeService);
  private readonly router = inject(Router);
  private readonly searchSubject = new Subject<string>();

  readonly recipes = signal<RecipeItem[]>([]);
  readonly availableTags = signal<string[]>([]);
  readonly selectedTags = signal<string[]>([]);
  readonly searchTerm = signal('');
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);

  readonly Search = Search;
  readonly X = X;
  readonly ChevronLeft = ChevronLeft;

  readonly filteredRecipesCount = computed(() => this.recipes().length);

  constructor() {
    this.searchSubject.pipe(
      debounceTime(300),
      takeUntilDestroyed()
    ).subscribe((term) => {
      this.searchTerm.set(term);
      this.loadRecipes();
    });
  }

  ngOnInit(): void {
    this.loadRecipes();
  }

  onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchSubject.next(input.value);
  }

  onToggleTag(tag: string): void {
    this.selectedTags.update((tags) =>
      tags.includes(tag) ? tags.filter((t) => t !== tag) : [...tags, tag]
    );
    this.loadRecipes();
  }

  onClearFilters(): void {
    this.searchTerm.set('');
    this.selectedTags.set([]);
    this.loadRecipes();
  }

  onRecipeClick(recipe: RecipeItem): void {
    this.router.navigate(['/recipe', recipe.id]);
  }

  isTagSelected(tag: string): boolean {
    return this.selectedTags().includes(tag);
  }

  getTagTestId(tag: string): string {
    return 'filter-chip-' + tag.toLowerCase().replace(/ /g, '-');
  }

  private loadRecipes(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const search = this.searchTerm() || undefined;
    const tags = this.selectedTags().length > 0 ? this.selectedTags() : undefined;

    this.recipeService.searchRecipes(search, tags).subscribe({
      next: (result) => {
        this.recipes.set(result.recipes);
        this.availableTags.set(result.availableTags);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load recipes:', err);
        this.error.set('Failed to load recipes. Please try again later.');
        this.isLoading.set(false);
      },
    });
  }
}
