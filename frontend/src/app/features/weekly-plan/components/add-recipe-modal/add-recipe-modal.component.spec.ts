import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AddRecipeModalComponent } from './add-recipe-modal.component';
import { RecipeService } from '../../../../core/services/recipe.service';
import { RecipeSearchResult } from '../../../../core/models/recipe.model';

describe('AddRecipeModalComponent', () => {
  let mockRecipeService: {
    searchRecipes: ReturnType<typeof vi.fn>;
  };

  const mockRecipes: RecipeSearchResult = {
    recipes: [
      {
        id: '1',
        name: 'Pasta Carbonara',
        imageUrl: 'pasta.jpg',
        description: 'Classic Italian pasta',
        tags: ['Italian', 'Pasta'],
      },
      {
        id: '2',
        name: 'Caesar Salad',
        imageUrl: null,
        description: 'Fresh salad',
        tags: ['Salad', 'Healthy'],
      },
      {
        id: '3',
        name: 'Chicken Curry',
        imageUrl: 'curry.jpg',
        description: 'Spicy curry',
        tags: ['Indian', 'Spicy'],
      },
    ],
    availableTags: [],
  };

  beforeEach(() => {
    mockRecipeService = {
      searchRecipes: vi.fn().mockReturnValue(of(mockRecipes)),
    };

    TestBed.configureTestingModule({
      imports: [AddRecipeModalComponent],
      providers: [{ provide: RecipeService, useValue: mockRecipeService }],
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load recipes on init', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    expect(mockRecipeService.searchRecipes).toHaveBeenCalled();
    expect(fixture.componentInstance.recipes().length).toBe(3);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should sort recipes alphabetically by name', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    const recipes = fixture.componentInstance.recipes();
    expect(recipes[0].name).toBe('Caesar Salad');
    expect(recipes[1].name).toBe('Chicken Curry');
    expect(recipes[2].name).toBe('Pasta Carbonara');
  });

  it('should filter recipes based on search term (case-insensitive)', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    fixture.componentInstance.searchTerm.set('pasta');
    expect(fixture.componentInstance.filteredRecipes().length).toBe(1);
    expect(fixture.componentInstance.filteredRecipes()[0].name).toBe('Pasta Carbonara');

    fixture.componentInstance.searchTerm.set('SALAD');
    expect(fixture.componentInstance.filteredRecipes().length).toBe(1);
    expect(fixture.componentInstance.filteredRecipes()[0].name).toBe('Caesar Salad');
  });

  it('should return all recipes when search term is empty', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    fixture.componentInstance.searchTerm.set('');
    expect(fixture.componentInstance.filteredRecipes().length).toBe(3);
  });

  it('should return empty array when no recipes match search', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    fixture.componentInstance.searchTerm.set('nonexistent');
    expect(fixture.componentInstance.filteredRecipes().length).toBe(0);
  });

  it('should select recipe when clicked', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    fixture.componentInstance.onSelectRecipe('1');
    expect(fixture.componentInstance.selectedRecipeId()).toBe('1');
  });

  it('should emit close event when close button clicked', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');

    const closeSpy = vi.fn();
    fixture.componentInstance.close.subscribe(closeSpy);

    fixture.componentInstance.onClose();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should emit recipeSelected event when Add button clicked with selection', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');

    const selectSpy = vi.fn();
    fixture.componentInstance.recipeSelected.subscribe(selectSpy);

    fixture.componentInstance.selectedRecipeId.set('1');
    fixture.componentInstance.onAdd();

    expect(selectSpy).toHaveBeenCalledWith('1');
  });

  it('should not emit recipeSelected when Add button clicked without selection', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');

    const selectSpy = vi.fn();
    fixture.componentInstance.recipeSelected.subscribe(selectSpy);

    fixture.componentInstance.selectedRecipeId.set(null);
    fixture.componentInstance.onAdd();

    expect(selectSpy).not.toHaveBeenCalled();
  });

  it('should handle error when loading recipes fails', () => {
    mockRecipeService.searchRecipes.mockReturnValue(
      throwError(() => new Error('Failed to load'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
    fixture.detectChanges();

    expect(fixture.componentInstance.error()).toBe('Failed to load recipes. Please try again.');
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should close modal when backdrop is clicked', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');

    const closeSpy = vi.fn();
    fixture.componentInstance.close.subscribe(closeSpy);

    const mockEvent = {
      target: { classList: { contains: () => true } },
    } as any;

    fixture.componentInstance.onBackdropClick(mockEvent);

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should not close modal when clicking inside modal content', () => {
    const fixture = TestBed.createComponent(AddRecipeModalComponent);
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');

    const closeSpy = vi.fn();
    fixture.componentInstance.close.subscribe(closeSpy);

    const mockEvent = {
      target: { classList: { contains: () => false } },
    } as any;

    fixture.componentInstance.onBackdropClick(mockEvent);

    expect(closeSpy).not.toHaveBeenCalled();
  });
});
