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
import { DailyDigestService } from './daily-digest.service';
import { ApiConfigService } from './api-config.service';
import {
  DailyDigest,
  SuggestionsResponse,
  SwapMealResponse,
} from '../models/daily-digest.model';

describe('DailyDigestService', () => {
  let service: DailyDigestService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        DailyDigestService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(DailyDigestService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get daily digest', () => {
    const mockResponse: DailyDigest = {
      date: '2026-01-23',
      meals: [
        {
          id: 'meal-1',
          mealType: 'Breakfast',
          recipeId: 'recipe-1',
          recipeName: 'Pancakes',
          imageUrl: 'https://example.com/pancakes.jpg',
        },
      ],
    };

    const testDate = new Date(2026, 0, 23);
    service.getDailyDigest(testDate).subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/daily-digest/2026-01-23'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get suggestions for a meal', () => {
    const mockResponse: SuggestionsResponse = {
      mealId: 'meal-1',
      mealType: 'Breakfast',
      suggestions: [
        {
          id: 'recipe-2',
          name: 'Waffles',
          imageUrl: 'https://example.com/waffles.jpg',
          description: 'Fluffy waffles',
        },
      ],
    };

    service.getSuggestions('meal-1').subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/meals/meal-1/suggestions'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should swap a meal', () => {
    const mockResponse: SwapMealResponse = {
      mealId: 'meal-1',
      mealType: 'Breakfast',
      recipeName: 'Waffles',
      imageUrl: 'https://example.com/waffles.jpg',
    };

    service.swapMeal('meal-1', 'recipe-2').subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/meals/meal-1/swap'
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ newRecipeId: 'recipe-2' });
    req.flush(mockResponse);
  });

  it('should format date correctly with padding', () => {
    const mockResponse: DailyDigest = { date: '2026-01-05', meals: [] };

    const testDate = new Date(2026, 0, 5);
    service.getDailyDigest(testDate).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/daily-digest/2026-01-05'
    );
    req.flush(mockResponse);
  });
});
