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
import { RecipeService } from './recipe.service';
import { ApiConfigService } from './api-config.service';
import { RecipeDetails, RecipeSearchResult } from '../models/recipe.model';

describe('RecipeService', () => {
  let service: RecipeService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        RecipeService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(RecipeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get recipe details', () => {
    const mockResponse: RecipeDetails = {
      id: 'recipe-1',
      name: 'Pasta Carbonara',
      imageUrl: 'https://example.com/pasta.jpg',
      description: 'Creamy Italian pasta',
      tags: ['Dinner', 'Italian'],
      mealType: 'Dinner',
      ingredients: [
        { name: 'Pasta', quantity: '500', unit: 'g' },
        { name: 'Bacon', quantity: '200', unit: 'g' },
      ],
      steps: [
        { stepNumber: 1, instruction: 'Boil pasta' },
        { stepNumber: 2, instruction: 'Fry bacon' },
      ],
    };

    service.getRecipeDetails('recipe-1').subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/recipes/recipe-1'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should search recipes without parameters', () => {
    const mockResponse: RecipeSearchResult = {
      recipes: [
        {
          id: 'recipe-1',
          name: 'Pasta',
          imageUrl: null,
          description: 'Italian pasta',
          tags: ['Dinner'],
        },
      ],
      availableTags: ['Dinner', 'Lunch', 'Vegetarian'],
    };

    service.searchRecipes().subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/recipes');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys().length).toBe(0);
    req.flush(mockResponse);
  });

  it('should search recipes with search term', () => {
    const mockResponse: RecipeSearchResult = {
      recipes: [],
      availableTags: [],
    };

    service.searchRecipes('pasta').subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      (request) =>
        request.url === 'http://localhost:5000/api/v1/recipes' &&
        request.params.get('search') === 'pasta'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should search recipes with tags', () => {
    const mockResponse: RecipeSearchResult = {
      recipes: [],
      availableTags: [],
    };

    service.searchRecipes(undefined, ['Dinner', 'Italian']).subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      (request) =>
        request.url === 'http://localhost:5000/api/v1/recipes' &&
        request.params.get('tags') === 'Dinner,Italian'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should search recipes with both search term and tags', () => {
    const mockResponse: RecipeSearchResult = {
      recipes: [],
      availableTags: [],
    };

    service.searchRecipes('pasta', ['Dinner']).subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      (request) =>
        request.url === 'http://localhost:5000/api/v1/recipes' &&
        request.params.get('search') === 'pasta' &&
        request.params.get('tags') === 'Dinner'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should create a recipe', () => {
    const payload = {
      name: 'New Recipe',
      description: 'A new recipe',
      ingredients: [{ name: 'Flour', quantity: '500', unit: 'g' }],
      steps: [{ stepNumber: 1, instruction: 'Mix ingredients' }],
    };
    const mockResponse = { id: 'new-recipe-id', ...payload };

    service.createRecipe(payload).subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/recipes');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(mockResponse);
  });
});
