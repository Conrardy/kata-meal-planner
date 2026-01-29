import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { SwapModalComponent } from './swap-modal.component';
import { DailyDigestService } from '../../../../core/services/daily-digest.service';
import { of, throwError } from 'rxjs';
import { RecipeSuggestion } from '../../../../core/models/daily-digest.model';

describe('SwapModalComponent', () => {
  let dailyDigestServiceMock: {
    getSuggestions: ReturnType<typeof vi.fn>;
  };

  const mockSuggestions: RecipeSuggestion[] = [
    {
      recipeId: 'recipe-1',
      recipeName: 'Pancakes',
      imageUrl: 'https://example.com/pancakes.jpg',
    },
    {
      recipeId: 'recipe-2',
      recipeName: 'Waffles',
      imageUrl: null,
    },
  ];

  beforeEach(() => {
    dailyDigestServiceMock = {
      getSuggestions: vi
        .fn()
        .mockReturnValue(of({ suggestions: mockSuggestions })),
    };

    TestBed.configureTestingModule({
      imports: [SwapModalComponent],
      providers: [
        { provide: DailyDigestService, useValue: dailyDigestServiceMock },
      ],
    });
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load suggestions on init', async () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(dailyDigestServiceMock.getSuggestions).toHaveBeenCalledWith(
      'meal-1'
    );
    expect(fixture.componentInstance.suggestions()).toEqual(mockSuggestions);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    dailyDigestServiceMock.getSuggestions.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load suggestions. Please try again.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should emit close event when onClose is called', () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    const component = fixture.componentInstance;

    const closeSpy = vi.fn();
    component.close.subscribe(closeSpy);

    component.onClose();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should emit selectRecipe event when onSelectRecipe is called', () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    const component = fixture.componentInstance;

    const selectSpy = vi.fn();
    component.selectRecipe.subscribe(selectSpy);

    component.onSelectRecipe('recipe-1');

    expect(selectSpy).toHaveBeenCalledWith('recipe-1');
  });

  it('should close modal on backdrop click', () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    const component = fixture.componentInstance;

    const closeSpy = vi.fn();
    component.close.subscribe(closeSpy);

    const mockEvent = {
      target: {
        classList: {
          contains: (className: string) => className === 'modal-backdrop',
        },
      },
    } as unknown as MouseEvent;

    component.onBackdropClick(mockEvent);

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should not close modal when clicking inside modal content', () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    const component = fixture.componentInstance;

    const closeSpy = vi.fn();
    component.close.subscribe(closeSpy);

    const mockEvent = {
      target: {
        classList: {
          contains: (className: string) => className === 'modal-content',
        },
      },
    } as unknown as MouseEvent;

    component.onBackdropClick(mockEvent);

    expect(closeSpy).not.toHaveBeenCalled();
  });

  it('should display loading state initially', async () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');

    expect(fixture.componentInstance.isLoading()).toBe(true);
  });

  it('should have data-testid attributes for testing', async () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[data-testid="swap-modal"]')).toBeTruthy();
    expect(
      compiled.querySelector('[data-testid="close-modal-button"]')
    ).toBeTruthy();
  });

  it('should display suggestions list when loaded', async () => {
    const fixture = TestBed.createComponent(SwapModalComponent);
    fixture.componentRef.setInput('mealId', 'meal-1');
    fixture.componentRef.setInput('mealType', 'Breakfast');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const suggestionsList = compiled.querySelector(
      '[data-testid="suggestions-list"]'
    );
    expect(suggestionsList).toBeTruthy();
  });
});
