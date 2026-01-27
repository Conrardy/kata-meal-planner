import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RecipeDetailsComponent } from './recipe-details.component';
import { RecipeService } from '../../core/services/recipe.service';
import { MealPlanService } from '../../core/services/meal-plan.service';
import { RecipeDetails } from '../../core/models/recipe.model';

describe('RecipeDetailsComponent', () => {
  let recipeServiceMock: {
    getRecipeDetails: ReturnType<typeof vi.fn>;
  };
  let mealPlanServiceMock: {
    addRecipeToMealPlan: ReturnType<typeof vi.fn>;
  };
  let router: Router;

  const mockRecipe: RecipeDetails = {
    id: 'recipe-1',
    name: 'Test Recipe',
    imageUrl: 'https://example.com/image.jpg',
    description: 'A delicious test recipe',
    tags: ['Dinner', 'Italian'],
    mealType: 'Dinner',
    ingredients: [
      { name: 'Pasta', quantity: '500', unit: 'g' },
      { name: 'Cheese', quantity: '100', unit: 'g' },
    ],
    steps: [
      { stepNumber: 1, instruction: 'Boil water' },
      { stepNumber: 2, instruction: 'Cook pasta' },
    ],
  };

  beforeEach(async () => {
    recipeServiceMock = {
      getRecipeDetails: vi.fn().mockReturnValue(of(mockRecipe)),
    };
    mealPlanServiceMock = {
      addRecipeToMealPlan: vi.fn().mockReturnValue(of({ mealId: 'new-meal-1' })),
    };

    await TestBed.configureTestingModule({
      imports: [RecipeDetailsComponent],
      providers: [
        provideRouter([]),
        { provide: RecipeService, useValue: recipeServiceMock },
        { provide: MealPlanService, useValue: mealPlanServiceMock },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => (key === 'recipeId' ? 'recipe-1' : null),
              },
            },
          },
        },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load recipe on init', async () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(recipeServiceMock.getRecipeDetails).toHaveBeenCalledWith('recipe-1');
    expect(fixture.componentInstance.recipe()).toEqual(mockRecipe);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when recipe ID is not found', async () => {
    TestBed.overrideProvider(ActivatedRoute, {
      useValue: {
        snapshot: {
          paramMap: {
            get: () => null,
          },
        },
      },
    });

    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe('Recipe ID not found');
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    recipeServiceMock.getRecipeDetails.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load recipe details. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should go back when goBack is called', () => {
    const historySpy = vi.spyOn(window.history, 'back').mockImplementation(() => {});

    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;

    component.goBack();

    expect(historySpy).toHaveBeenCalled();
    historySpy.mockRestore();
  });

  it('should open add to meal plan modal', async () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.openAddToMealPlan();

    expect(component.showAddToMealPlanModal()).toBe(true);
    expect(component.selectedMealSlot()).toBe('Dinner');
  });

  it('should close add to meal plan modal', () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;
    component.openAddToMealPlan();

    component.closeAddToMealPlanModal();

    expect(component.showAddToMealPlanModal()).toBe(false);
  });

  it('should handle date change', () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;
    const event = { target: { value: '2026-02-15' } } as unknown as Event;

    component.onDateChange(event);

    expect(component.selectedDate()).toBe('2026-02-15');
  });

  it('should select meal slot', () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;

    component.selectMealSlot('Breakfast');

    expect(component.selectedMealSlot()).toBe('Breakfast');
  });

  it('should add recipe to meal plan and navigate', async () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.openAddToMealPlan();
    component.selectMealSlot('Lunch');

    component.confirmAddToMealPlan();
    await fixture.whenStable();

    expect(mealPlanServiceMock.addRecipeToMealPlan).toHaveBeenCalledWith(
      'recipe-1',
      expect.any(String),
      'Lunch'
    );
    expect(component.showAddToMealPlanModal()).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/weekly-plan']);
  });

  it('should not add to meal plan when recipe is null', async () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;

    component.confirmAddToMealPlan();
    await fixture.whenStable();

    expect(mealPlanServiceMock.addRecipeToMealPlan).not.toHaveBeenCalled();
  });

  it('should not add to meal plan when meal slot is null', async () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;

    component.confirmAddToMealPlan();
    await fixture.whenStable();

    expect(mealPlanServiceMock.addRecipeToMealPlan).not.toHaveBeenCalled();
  });

  it('should handle add to meal plan error', async () => {
    mealPlanServiceMock.addRecipeToMealPlan.mockReturnValue(
      throwError(() => new Error('Failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.openAddToMealPlan();
    component.confirmAddToMealPlan();
    await fixture.whenStable();

    expect(router.navigate).not.toHaveBeenCalledWith(['/weekly-plan']);
  });

  it('should have correct meal slots', () => {
    const fixture = TestBed.createComponent(RecipeDetailsComponent);
    const component = fixture.componentInstance;

    expect(component.mealSlots).toEqual(['Breakfast', 'Lunch', 'Dinner']);
  });
});
