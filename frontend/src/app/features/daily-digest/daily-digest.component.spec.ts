import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DailyDigestComponent } from './daily-digest.component';
import { DailyDigestService } from '../../core/services/daily-digest.service';
import { DailyDigest, PlannedMeal } from '../../core/models/daily-digest.model';

describe('DailyDigestComponent', () => {
  let dailyDigestServiceMock: {
    getDailyDigest: ReturnType<typeof vi.fn>;
    swapMeal: ReturnType<typeof vi.fn>;
  };
  let router: Router;

  const mockMeals: PlannedMeal[] = [
    {
      id: 'meal-1',
      mealType: 'Breakfast',
      recipeId: 'recipe-1',
      recipeName: 'Pancakes',
      imageUrl: 'https://example.com/pancakes.jpg',
    },
    {
      id: 'meal-2',
      mealType: 'Lunch',
      recipeId: 'recipe-2',
      recipeName: 'Salad',
      imageUrl: null,
    },
    {
      id: 'meal-3',
      mealType: 'Dinner',
      recipeId: 'recipe-3',
      recipeName: 'Pasta',
      imageUrl: 'https://example.com/pasta.jpg',
    },
  ];

  const mockDailyDigest: DailyDigest = {
    date: '2026-01-23',
    meals: mockMeals,
  };

  beforeEach(async () => {
    dailyDigestServiceMock = {
      getDailyDigest: vi.fn().mockReturnValue(of(mockDailyDigest)),
      swapMeal: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [DailyDigestComponent],
      providers: [
        provideRouter([]),
        { provide: DailyDigestService, useValue: dailyDigestServiceMock },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load daily digest on init', async () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(dailyDigestServiceMock.getDailyDigest).toHaveBeenCalled();
    expect(fixture.componentInstance.meals()).toEqual(mockMeals);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    dailyDigestServiceMock.getDailyDigest.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load meals. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set swappingMealId when onSwapMeal is called', () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    const component = fixture.componentInstance;

    component.onSwapMeal('meal-1');

    expect(component.swappingMealId()).toBe('meal-1');
  });

  it('should clear swappingMealId when onCloseSwapModal is called', () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    const component = fixture.componentInstance;
    component.onSwapMeal('meal-1');

    component.onCloseSwapModal();

    expect(component.swappingMealId()).toBeNull();
  });

  it('should return swapping meal from computed signal', async () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSwapMeal('meal-1');

    expect(component.swappingMeal()).toEqual(mockMeals[0]);
  });

  it('should return null for swappingMeal when no meal is being swapped', async () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.swappingMeal()).toBeNull();
  });

  it('should navigate to recipe on cookNow', () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    const component = fixture.componentInstance;

    component.onCookNow('recipe-1');

    expect(router.navigate).toHaveBeenCalledWith(['/recipe', 'recipe-1']);
  });

  it('should swap meal and update state on successful swap', async () => {
    dailyDigestServiceMock.swapMeal.mockReturnValue(
      of({
        mealId: 'meal-1',
        mealType: 'Breakfast',
        recipeName: 'Waffles',
        imageUrl: 'https://example.com/waffles.jpg',
      })
    );

    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSwapMeal('meal-1');
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(dailyDigestServiceMock.swapMeal).toHaveBeenCalledWith(
      'meal-1',
      'new-recipe-id'
    );
    expect(component.meals()[0].recipeName).toBe('Waffles');
    expect(component.meals()[0].imageUrl).toBe('https://example.com/waffles.jpg');
    expect(component.swappingMealId()).toBeNull();
    expect(component.isSwapping()).toBe(false);
  });

  it('should not call swapMeal when no meal is selected for swapping', async () => {
    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(dailyDigestServiceMock.swapMeal).not.toHaveBeenCalled();
  });

  it('should handle swap error gracefully', async () => {
    dailyDigestServiceMock.swapMeal.mockReturnValue(
      throwError(() => new Error('Swap failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(DailyDigestComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSwapMeal('meal-1');
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(component.isSwapping()).toBe(false);
    expect(component.meals()[0].recipeName).toBe('Pancakes');
  });
});
