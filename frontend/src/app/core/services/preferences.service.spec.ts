import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import {
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { PreferencesService } from './preferences.service';
import { ApiConfigService } from './api-config.service';
import { UserPreferences, UpdatePreferencesRequest } from '../models/preferences.model';

describe('PreferencesService', () => {
  let service: PreferencesService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  const mockPreferences: UserPreferences = {
    dietaryPreference: 'Omnivore',
    allergies: ['Gluten'],
    availableDietaryPreferences: ['None', 'Omnivore', 'Vegetarian', 'Vegan'],
    availableAllergies: ['Gluten', 'Dairy', 'Nuts'],
    mealsPerDay: 3,
    planLength: 1,
    includeLeftovers: false,
    autoGenerateShoppingList: true,
    excludedIngredients: ['Cilantro'],
    availableMealsPerDay: [2, 3, 4],
    availablePlanLengths: [1, 2],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        PreferencesService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(PreferencesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get preferences', () => {
    service.getPreferences().subscribe((result) => {
      expect(result).toEqual(mockPreferences);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/preferences');
    expect(req.request.method).toBe('GET');
    req.flush(mockPreferences);
  });

  it('should update preferences', () => {
    const updateRequest: UpdatePreferencesRequest = {
      dietaryPreference: 'Vegan',
      allergies: ['Nuts'],
      mealsPerDay: 2,
      planLength: 2,
      includeLeftovers: true,
      autoGenerateShoppingList: false,
      excludedIngredients: ['Onions'],
    };

    const expectedResponse: UserPreferences = {
      ...mockPreferences,
      dietaryPreference: 'Vegan',
      allergies: ['Nuts'],
      mealsPerDay: 2,
      planLength: 2,
      includeLeftovers: true,
      autoGenerateShoppingList: false,
      excludedIngredients: ['Onions'],
    };

    service.updatePreferences(updateRequest).subscribe((result) => {
      expect(result).toEqual(expectedResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/preferences');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateRequest);
    req.flush(expectedResponse);
  });
});
