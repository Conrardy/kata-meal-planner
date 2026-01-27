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
import { MealPlanService, AddRecipeToMealPlanResult } from './meal-plan.service';
import { ApiConfigService } from './api-config.service';

describe('MealPlanService', () => {
  let service: MealPlanService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        MealPlanService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(MealPlanService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add recipe to meal plan', () => {
    const mockResponse: AddRecipeToMealPlanResult = {
      mealId: 'new-meal-1',
      recipeName: 'Pasta Carbonara',
      date: '2026-01-23',
      mealType: 'Dinner',
    };

    service
      .addRecipeToMealPlan('recipe-1', '2026-01-23', 'Dinner')
      .subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/meal-plan');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      recipeId: 'recipe-1',
      date: '2026-01-23',
      mealType: 'Dinner',
    });
    req.flush(mockResponse);
  });

  it('should add recipe to meal plan for breakfast', () => {
    const mockResponse: AddRecipeToMealPlanResult = {
      mealId: 'new-meal-2',
      recipeName: 'Pancakes',
      date: '2026-01-24',
      mealType: 'Breakfast',
    };

    service
      .addRecipeToMealPlan('recipe-2', '2026-01-24', 'Breakfast')
      .subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/meal-plan');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      recipeId: 'recipe-2',
      date: '2026-01-24',
      mealType: 'Breakfast',
    });
    req.flush(mockResponse);
  });
});
