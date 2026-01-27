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
import { WeeklyPlanService } from './weekly-plan.service';
import { ApiConfigService } from './api-config.service';
import { WeeklyPlan } from '../models/weekly-plan.model';

describe('WeeklyPlanService', () => {
  let service: WeeklyPlanService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  const mockWeeklyPlan: WeeklyPlan = {
    startDate: '2026-01-19',
    endDate: '2026-01-25',
    days: [
      {
        date: '2026-01-19',
        dayName: 'Sunday',
        breakfast: {
          id: 'meal-1',
          recipeId: 'recipe-1',
          recipeName: 'Pancakes',
          imageUrl: 'https://example.com/pancakes.jpg',
        },
        lunch: null,
        dinner: null,
      },
    ],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        WeeklyPlanService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(WeeklyPlanService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get weekly plan', () => {
    const testDate = new Date(2026, 0, 19);

    service.getWeeklyPlan(testDate).subscribe((result) => {
      expect(result).toEqual(mockWeeklyPlan);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/weekly-plan/2026-01-19'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockWeeklyPlan);
  });

  it('should format date correctly with padding', () => {
    const testDate = new Date(2026, 0, 5);

    service.getWeeklyPlan(testDate).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/weekly-plan/2026-01-05'
    );
    req.flush(mockWeeklyPlan);
  });

  it('should format date correctly for December', () => {
    const testDate = new Date(2026, 11, 25);

    service.getWeeklyPlan(testDate).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/weekly-plan/2026-12-25'
    );
    req.flush(mockWeeklyPlan);
  });
});
