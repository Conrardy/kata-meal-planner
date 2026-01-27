import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RecipeBrowseComponent } from './recipe-browse.component';
import { RecipeService } from '../../core/services/recipe.service';
import { RecipeItem, RecipeSearchResult } from '../../core/models/recipe.model';

describe('RecipeBrowseComponent', () => {
  let recipeServiceMock: {
    searchRecipes: ReturnType<typeof vi.fn>;
  };
  let router: Router;

  const mockRecipes: RecipeItem[] = [
    {
      id: 'recipe-1',
      name: 'Pasta Carbonara',
      imageUrl: 'https://example.com/pasta.jpg',
      description: 'Creamy Italian pasta',
      tags: ['Dinner', 'Italian'],
    },
    {
      id: 'recipe-2',
      name: 'Caesar Salad',
      imageUrl: null,
      description: 'Classic salad',
      tags: ['Lunch', 'Healthy'],
    },
  ];

  const mockSearchResult: RecipeSearchResult = {
    recipes: mockRecipes,
    availableTags: ['Dinner', 'Lunch', 'Italian', 'Healthy', 'Vegetarian'],
  };

  beforeEach(async () => {
    recipeServiceMock = {
      searchRecipes: vi.fn().mockReturnValue(of(mockSearchResult)),
    };

    await TestBed.configureTestingModule({
      imports: [RecipeBrowseComponent],
      providers: [
        provideRouter([]),
        { provide: RecipeService, useValue: recipeServiceMock },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load recipes on init', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(recipeServiceMock.searchRecipes).toHaveBeenCalled();
    expect(fixture.componentInstance.recipes()).toEqual(mockRecipes);
    expect(fixture.componentInstance.availableTags()).toEqual(
      mockSearchResult.availableTags
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    recipeServiceMock.searchRecipes.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load recipes. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should toggle tag and reload recipes', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    recipeServiceMock.searchRecipes.mockClear();

    const component = fixture.componentInstance;
    component.onToggleTag('Vegetarian');
    await fixture.whenStable();

    expect(component.selectedTags()).toContain('Vegetarian');
    expect(recipeServiceMock.searchRecipes).toHaveBeenCalled();
  });

  it('should remove tag when toggled again', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onToggleTag('Vegetarian');
    await fixture.whenStable();
    component.onToggleTag('Vegetarian');
    await fixture.whenStable();

    expect(component.selectedTags()).not.toContain('Vegetarian');
  });

  it('should clear filters', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onToggleTag('Vegetarian');
    await fixture.whenStable();
    recipeServiceMock.searchRecipes.mockClear();

    component.onClearFilters();
    await fixture.whenStable();

    expect(component.searchTerm()).toBe('');
    expect(component.selectedTags()).toEqual([]);
    expect(recipeServiceMock.searchRecipes).toHaveBeenCalled();
  });

  it('should navigate to recipe on click', () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    const component = fixture.componentInstance;

    component.onRecipeClick(mockRecipes[0]);

    expect(router.navigate).toHaveBeenCalledWith(['/recipe', 'recipe-1']);
  });

  it('should check if tag is selected', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    expect(component.isTagSelected('Vegetarian')).toBe(false);

    component.onToggleTag('Vegetarian');
    await fixture.whenStable();

    expect(component.isTagSelected('Vegetarian')).toBe(true);
  });

  it('should generate tag test id', () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    const component = fixture.componentInstance;

    expect(component.getTagTestId('Quick & Easy')).toBe('filter-chip-quick-&-easy');
    expect(component.getTagTestId('Vegetarian')).toBe('filter-chip-vegetarian');
  });

  it('should compute filtered recipes count', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.filteredRecipesCount()).toBe(2);
  });

  it('should search with tags when tags are selected', async () => {
    const fixture = TestBed.createComponent(RecipeBrowseComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    recipeServiceMock.searchRecipes.mockClear();

    const component = fixture.componentInstance;
    component.onToggleTag('Dinner');
    component.onToggleTag('Italian');
    await fixture.whenStable();

    expect(recipeServiceMock.searchRecipes).toHaveBeenLastCalledWith(
      undefined,
      ['Dinner', 'Italian']
    );
  });
});
