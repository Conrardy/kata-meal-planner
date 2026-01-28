import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { AddRecipeModalComponent } from './add-recipe-modal.component';
import { RecipeService } from '../../../../core/services/recipe.service';
import { RecipeSearchResult } from '../../../../core/models/recipe.model';

describe('AddRecipeModalComponent', () => {
  let component: AddRecipeModalComponent;
  let fixture: ComponentFixture<AddRecipeModalComponent>;
  let mockRecipeService: jasmine.SpyObj<RecipeService>;

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

  beforeEach(async () => {
    mockRecipeService = jasmine.createSpyObj('RecipeService', ['searchRecipes']);
    mockRecipeService.searchRecipes.and.returnValue(of(mockRecipes));

    await TestBed.configureTestingModule({
      imports: [AddRecipeModalComponent],
      providers: [{ provide: RecipeService, useValue: mockRecipeService }],
    }).compileComponents();

    fixture = TestBed.createComponent(AddRecipeModalComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('date', '2024-01-15');
    fixture.componentRef.setInput('mealType', 'Lunch');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load recipes on init', () => {
    fixture.detectChanges();

    expect(mockRecipeService.searchRecipes).toHaveBeenCalled();
    expect(component.recipes().length).toBe(3);
    expect(component.isLoading()).toBeFalse();
  });

  it('should sort recipes alphabetically by name', () => {
    fixture.detectChanges();

    const recipes = component.recipes();
    expect(recipes[0].name).toBe('Caesar Salad');
    expect(recipes[1].name).toBe('Chicken Curry');
    expect(recipes[2].name).toBe('Pasta Carbonara');
  });

  it('should filter recipes based on search term (case-insensitive)', () => {
    fixture.detectChanges();

    component.searchTerm.set('pasta');
    expect(component.filteredRecipes().length).toBe(1);
    expect(component.filteredRecipes()[0].name).toBe('Pasta Carbonara');

    component.searchTerm.set('SALAD');
    expect(component.filteredRecipes().length).toBe(1);
    expect(component.filteredRecipes()[0].name).toBe('Caesar Salad');
  });

  it('should return all recipes when search term is empty', () => {
    fixture.detectChanges();

    component.searchTerm.set('');
    expect(component.filteredRecipes().length).toBe(3);
  });

  it('should return empty array when no recipes match search', () => {
    fixture.detectChanges();

    component.searchTerm.set('nonexistent');
    expect(component.filteredRecipes().length).toBe(0);
  });

  it('should select recipe when clicked', () => {
    fixture.detectChanges();

    component.onSelectRecipe('1');
    expect(component.selectedRecipeId()).toBe('1');
  });

  it('should emit close event when close button clicked', () => {
    const closeSpy = jasmine.createSpy('close');
    component.close.subscribe(closeSpy);

    component.onClose();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should emit recipeSelected event when Add button clicked with selection', () => {
    const selectSpy = jasmine.createSpy('recipeSelected');
    component.recipeSelected.subscribe(selectSpy);

    component.selectedRecipeId.set('1');
    component.onAdd();

    expect(selectSpy).toHaveBeenCalledWith('1');
  });

  it('should not emit recipeSelected when Add button clicked without selection', () => {
    const selectSpy = jasmine.createSpy('recipeSelected');
    component.recipeSelected.subscribe(selectSpy);

    component.selectedRecipeId.set(null);
    component.onAdd();

    expect(selectSpy).not.toHaveBeenCalled();
  });

  it('should handle error when loading recipes fails', () => {
    mockRecipeService.searchRecipes.and.returnValue(
      throwError(() => new Error('Failed to load'))
    );

    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load recipes. Please try again.');
    expect(component.isLoading()).toBeFalse();
  });

  it('should close modal when backdrop is clicked', () => {
    const closeSpy = jasmine.createSpy('close');
    component.close.subscribe(closeSpy);

    const mockEvent = {
      target: { classList: { contains: () => true } },
    } as any;

    component.onBackdropClick(mockEvent);

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should not close modal when clicking inside modal content', () => {
    const closeSpy = jasmine.createSpy('close');
    component.close.subscribe(closeSpy);

    const mockEvent = {
      target: { classList: { contains: () => false } },
    } as any;

    component.onBackdropClick(mockEvent);

    expect(closeSpy).not.toHaveBeenCalled();
  });
});
