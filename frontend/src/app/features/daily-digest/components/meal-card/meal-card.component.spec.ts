import { describe, it, expect, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { MealCardComponent } from './meal-card.component';
import { PlannedMeal } from '../../../../core/models/daily-digest.model';

describe('MealCardComponent', () => {
  const mockMeal: PlannedMeal = {
    id: 'meal-1',
    mealType: 'Breakfast',
    recipeId: 'recipe-1',
    recipeName: 'Pancakes',
    imageUrl: 'https://example.com/pancakes.jpg',
  };

  it('should create the component', () => {
    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mockMeal);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display meal information', () => {
    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mockMeal);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Breakfast');
    expect(compiled.textContent).toContain('Pancakes');
  });

  it('should emit swapMeal event when swap button is clicked', () => {
    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mockMeal);
    const component = fixture.componentInstance;

    const swapSpy = vi.fn();
    component.swapMeal.subscribe(swapSpy);

    component.onSwapMeal();

    expect(swapSpy).toHaveBeenCalledWith('meal-1');
  });

  it('should emit cookNow event when cook now button is clicked', () => {
    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mockMeal);
    const component = fixture.componentInstance;

    const cookSpy = vi.fn();
    component.cookNow.subscribe(cookSpy);

    component.onCookNow();

    expect(cookSpy).toHaveBeenCalledWith('recipe-1');
  });

  it('should handle meal without image', () => {
    const mealWithoutImage: PlannedMeal = {
      ...mockMeal,
      imageUrl: null,
    };

    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mealWithoutImage);
    fixture.detectChanges();

    expect(fixture.componentInstance.meal().imageUrl).toBeNull();
  });

  it('should display correct meal type', () => {
    const lunchMeal: PlannedMeal = {
      id: 'meal-2',
      mealType: 'Lunch',
      recipeId: 'recipe-2',
      recipeName: 'Salad',
      imageUrl: null,
    };

    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', lunchMeal);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Lunch');
  });

  it('should have data-testid attributes for testing', () => {
    const fixture = TestBed.createComponent(MealCardComponent);
    fixture.componentRef.setInput('meal', mockMeal);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="meal-card"]')).toBeTruthy();
    expect(compiled.querySelector('[data-testid="swap-meal-button"]')).toBeTruthy();
    expect(compiled.querySelector('[data-testid="cook-now-button"]')).toBeTruthy();
  });
});
