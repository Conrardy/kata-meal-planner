import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PreferencesComponent } from './preferences.component';
import { PreferencesService } from '../../core/services/preferences.service';
import { UserPreferences } from '../../core/models/preferences.model';

describe('PreferencesComponent', () => {
  let preferencesServiceMock: {
    getPreferences: ReturnType<typeof vi.fn>;
    updatePreferences: ReturnType<typeof vi.fn>;
  };

  const mockPreferences: UserPreferences = {
    dietaryPreference: 'Omnivore',
    allergies: ['Gluten'],
    availableDietaryPreferences: [
      'None',
      'Omnivore',
      'Vegetarian',
      'Vegan',
      'Keto',
    ],
    availableAllergies: ['Gluten', 'Dairy', 'Nuts', 'Shellfish'],
    mealsPerDay: 3,
    planLength: 1,
    includeLeftovers: false,
    autoGenerateShoppingList: true,
    excludedIngredients: ['Cilantro'],
    availableMealsPerDay: [2, 3, 4],
    availablePlanLengths: [1, 2],
  };

  beforeEach(async () => {
    preferencesServiceMock = {
      getPreferences: vi.fn().mockReturnValue(of(mockPreferences)),
      updatePreferences: vi.fn().mockReturnValue(of(mockPreferences)),
    };

    await TestBed.configureTestingModule({
      imports: [PreferencesComponent],
      providers: [
        provideRouter([]),
        { provide: PreferencesService, useValue: preferencesServiceMock },
      ],
    }).compileComponents();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load preferences on init', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    expect(preferencesServiceMock.getPreferences).toHaveBeenCalled();
    expect(component.preferences()).toEqual(mockPreferences);
    expect(component.selectedDietaryPreference).toBe('Omnivore');
    expect(component.selectedAllergies).toEqual(['Gluten']);
    expect(component.selectedMealsPerDay).toBe(3);
    expect(component.excludedIngredients).toEqual(['Cilantro']);
    expect(component.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    preferencesServiceMock.getPreferences.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to load preferences. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should change dietary preference', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onDietaryPreferenceChange('Vegan');

    expect(component.selectedDietaryPreference).toBe('Vegan');
    expect(component.successMessage()).toBeNull();
  });

  it('should toggle allergy', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;

    component.onAllergyToggle('Dairy');
    expect(component.selectedAllergies).toContain('Dairy');

    component.onAllergyToggle('Dairy');
    expect(component.selectedAllergies).not.toContain('Dairy');
  });

  it('should check if allergy is selected', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    expect(component.isAllergySelected('Gluten')).toBe(true);
    expect(component.isAllergySelected('Dairy')).toBe(false);
  });

  it('should change meals per day', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onMealsPerDayChange(4);

    expect(component.selectedMealsPerDay).toBe(4);
    expect(component.successMessage()).toBeNull();
  });

  it('should change plan length', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onPlanLengthChange(2);

    expect(component.selectedPlanLength).toBe(2);
    expect(component.successMessage()).toBeNull();
  });

  it('should toggle leftovers', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    expect(component.selectedIncludeLeftovers).toBe(false);

    component.onLeftoversToggle();
    expect(component.selectedIncludeLeftovers).toBe(true);
    expect(component.successMessage()).toBeNull();

    component.onLeftoversToggle();
    expect(component.selectedIncludeLeftovers).toBe(false);
  });

  it('should toggle auto generate shopping list', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    expect(component.selectedAutoGenerateShoppingList).toBe(true);

    component.onAutoGenerateShoppingListToggle();
    expect(component.selectedAutoGenerateShoppingList).toBe(false);
    expect(component.successMessage()).toBeNull();
  });

  it('should add excluded ingredient', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.newExcludedIngredient = 'Onions';

    component.addExcludedIngredient();

    expect(component.excludedIngredients).toContain('Onions');
    expect(component.newExcludedIngredient).toBe('');
    expect(component.successMessage()).toBeNull();
  });

  it('should not add duplicate excluded ingredient', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.newExcludedIngredient = 'Cilantro';

    component.addExcludedIngredient();

    expect(
      component.excludedIngredients.filter((i) => i === 'Cilantro').length
    ).toBe(1);
  });

  it('should not add empty excluded ingredient', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    const initialLength = component.excludedIngredients.length;
    component.newExcludedIngredient = '   ';

    component.addExcludedIngredient();

    expect(component.excludedIngredients.length).toBe(initialLength);
  });

  it('should remove excluded ingredient', async () => {
    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;

    component.removeExcludedIngredient('Cilantro');

    expect(component.excludedIngredients).not.toContain('Cilantro');
    expect(component.successMessage()).toBeNull();
  });

  it('should save preferences', async () => {
    preferencesServiceMock.updatePreferences.mockReturnValue(
      of({ ...mockPreferences, dietaryPreference: 'Vegan' })
    );

    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.selectedDietaryPreference = 'Vegan';

    component.onSavePreferences();
    expect(component.isSaving()).toBe(true);

    await fixture.whenStable();

    expect(preferencesServiceMock.updatePreferences).toHaveBeenCalledWith({
      dietaryPreference: 'Vegan',
      allergies: ['Gluten'],
      mealsPerDay: 3,
      planLength: 1,
      includeLeftovers: false,
      autoGenerateShoppingList: true,
      excludedIngredients: ['Cilantro'],
    });
    expect(component.isSaving()).toBe(false);
    expect(component.successMessage()).toBe('Preferences saved successfully!');
  });

  it('should handle save error', async () => {
    preferencesServiceMock.updatePreferences.mockReturnValue(
      throwError(() => new Error('Save failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSavePreferences();
    await fixture.whenStable();

    expect(component.isSaving()).toBe(false);
    expect(component.error()).toBe('Failed to save preferences. Please try again.');
  });

  it('should clear success message when settings change', async () => {
    preferencesServiceMock.updatePreferences.mockReturnValue(of(mockPreferences));

    const fixture = TestBed.createComponent(PreferencesComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onSavePreferences();
    await fixture.whenStable();

    expect(component.successMessage()).toBe('Preferences saved successfully!');

    component.onDietaryPreferenceChange('Keto');
    expect(component.successMessage()).toBeNull();
  });
});
