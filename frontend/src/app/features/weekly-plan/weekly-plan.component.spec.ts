import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { WeeklyPlanComponent } from './weekly-plan.component';
import { WeeklyPlanService } from '../../core/services/weekly-plan.service';
import { DailyDigestService } from '../../core/services/daily-digest.service';
import { WeeklyPlan, DayPlan, WeeklyMeal } from '../../core/models/weekly-plan.model';

describe('WeeklyPlanComponent', () => {
  let weeklyPlanServiceMock: {
    getWeeklyPlan: ReturnType<typeof vi.fn>;
  };
  let dailyDigestServiceMock: {
    swapMeal: ReturnType<typeof vi.fn>;
  };
  let router: Router;

  const mockMeal: WeeklyMeal = {
    id: 'meal-1',
    recipeId: 'recipe-1',
    recipeName: 'Test Recipe',
    imageUrl: 'https://example.com/image.jpg',
  };

  const mockDayPlan: DayPlan = {
    date: '2026-01-23',
    dayName: 'Thursday',
    breakfast: mockMeal,
    lunch: { ...mockMeal, id: 'meal-2', recipeName: 'Lunch Recipe' },
    dinner: null,
  };

  const mockWeeklyPlan: WeeklyPlan = {
    startDate: '2026-01-19',
    endDate: '2026-01-25',
    days: [mockDayPlan],
  };

  beforeEach(async () => {
    weeklyPlanServiceMock = {
      getWeeklyPlan: vi.fn().mockReturnValue(of(mockWeeklyPlan)),
    };
    dailyDigestServiceMock = {
      swapMeal: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [WeeklyPlanComponent],
      providers: [
        provideRouter([]),
        { provide: WeeklyPlanService, useValue: weeklyPlanServiceMock },
        { provide: DailyDigestService, useValue: dailyDigestServiceMock },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load weekly plan on init', async () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(weeklyPlanServiceMock.getWeeklyPlan).toHaveBeenCalled();
    expect(fixture.componentInstance.weeklyPlan()).toEqual(mockWeeklyPlan);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    weeklyPlanServiceMock.getWeeklyPlan.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load weekly plan. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should navigate to previous week', async () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const initialDate = new Date(fixture.componentInstance.startDate());
    fixture.componentInstance.onPreviousWeek();
    await fixture.whenStable();

    const newDate = fixture.componentInstance.startDate();
    expect(newDate.getTime()).toBeLessThan(initialDate.getTime());
    expect(weeklyPlanServiceMock.getWeeklyPlan).toHaveBeenCalledTimes(2);
  });

  it('should navigate to next week', async () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const initialDate = new Date(fixture.componentInstance.startDate());
    fixture.componentInstance.onNextWeek();
    await fixture.whenStable();

    const newDate = fixture.componentInstance.startDate();
    expect(newDate.getTime()).toBeGreaterThan(initialDate.getTime());
    expect(weeklyPlanServiceMock.getWeeklyPlan).toHaveBeenCalledTimes(2);
  });

  it('should navigate to recipe when clicking on a meal', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;

    component.onMealClick(mockMeal);

    expect(router.navigate).toHaveBeenCalledWith(['/recipe', 'recipe-1']);
  });

  it('should not navigate when clicking on null meal', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;

    component.onMealClick(null);

    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('should set swapping meal when edit is clicked', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;
    const event = new MouseEvent('click');
    vi.spyOn(event, 'stopPropagation');

    component.onEditMeal(mockMeal, 'Breakfast', event);

    expect(event.stopPropagation).toHaveBeenCalled();
    expect(component.swappingMealId()).toBe('meal-1');
    expect(component.swappingMealType()).toBe('Breakfast');
  });

  it('should not set swapping meal for null meal', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;
    const event = new MouseEvent('click');

    component.onEditMeal(null, 'Breakfast', event);

    expect(component.swappingMealId()).toBeNull();
  });

  it('should close swap modal', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;
    component.onEditMeal(mockMeal, 'Breakfast', new MouseEvent('click'));

    component.onCloseSwapModal();

    expect(component.swappingMealId()).toBeNull();
    expect(component.swappingMealType()).toBeNull();
  });

  it('should get meal by type from day plan', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;

    expect(component.getMeal(mockDayPlan, 'Breakfast')).toEqual(mockMeal);
    expect(component.getMeal(mockDayPlan, 'Lunch')).toEqual(mockDayPlan.lunch);
    expect(component.getMeal(mockDayPlan, 'Dinner')).toBeNull();
    expect(component.getMeal(mockDayPlan, 'Unknown')).toBeNull();
  });

  it('should identify today correctly', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    const component = fixture.componentInstance;
    const today = new Date().toISOString().split('T')[0];

    expect(component.isToday(today)).toBe(true);
    expect(component.isToday('2020-01-01')).toBe(false);
  });

  it('should swap meal and update state', async () => {
    dailyDigestServiceMock.swapMeal.mockReturnValue(
      of({
        mealId: 'meal-1',
        mealType: 'Breakfast',
        recipeName: 'New Recipe',
        imageUrl: 'https://example.com/new.jpg',
      })
    );

    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onEditMeal(mockMeal, 'Breakfast', new MouseEvent('click'));
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(dailyDigestServiceMock.swapMeal).toHaveBeenCalledWith(
      'meal-1',
      'new-recipe-id'
    );
    expect(component.isSwapping()).toBe(false);
    expect(component.swappingMealId()).toBeNull();
  });

  it('should handle swap error gracefully', async () => {
    dailyDigestServiceMock.swapMeal.mockReturnValue(
      throwError(() => new Error('Swap failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onEditMeal(mockMeal, 'Breakfast', new MouseEvent('click'));
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(component.isSwapping()).toBe(false);
  });

  it('should not call swap when no meal is selected', async () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSelectRecipe('new-recipe-id');
    await fixture.whenStable();

    expect(dailyDigestServiceMock.swapMeal).not.toHaveBeenCalled();
  });

  it('should compute formatted date range', async () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const range = fixture.componentInstance.formattedDateRange();
    expect(range).toContain('Jan');
  });

  it('should return empty string for date range when no plan', () => {
    const fixture = TestBed.createComponent(WeeklyPlanComponent);
    expect(fixture.componentInstance.formattedDateRange()).toBe('');
  });
});
