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
import { ShoppingListService } from './shopping-list.service';
import { ApiConfigService } from './api-config.service';
import {
  ShoppingList,
  ShoppingItem,
  AddCustomItemRequest,
} from '../models/shopping-list.model';

describe('ShoppingListService', () => {
  let service: ShoppingListService;
  let httpMock: HttpTestingController;

  const mockApiConfig = {
    apiBaseUrl: 'http://localhost:5000/api/v1',
  };

  const mockShoppingList: ShoppingList = {
    startDate: '2026-01-19',
    endDate: '2026-01-25',
    categories: [
      {
        category: 'Dairy',
        items: [
          {
            id: 'item-1',
            name: 'Milk',
            quantity: '1',
            unit: 'liter',
            isChecked: false,
            isCustom: false,
          },
        ],
      },
    ],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        ShoppingListService,
        { provide: ApiConfigService, useValue: mockApiConfig },
      ],
    });

    service = TestBed.inject(ShoppingListService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should generate shopping list', () => {
    const testDate = new Date(2026, 0, 19);

    service.generateShoppingList(testDate).subscribe((result) => {
      expect(result).toEqual(mockShoppingList);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-19'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockShoppingList);
  });

  it('should toggle item', () => {
    const testDate = new Date(2026, 0, 19);

    service.toggleItem(testDate, 'item-1', true).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-19/items/item-1'
    );
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ isChecked: true });
    req.flush(null);
  });

  it('should toggle item with encoded id', () => {
    const testDate = new Date(2026, 0, 19);

    service.toggleItem(testDate, 'item with spaces', false).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-19/items/item%20with%20spaces'
    );
    expect(req.request.method).toBe('PATCH');
    req.flush(null);
  });

  it('should add custom item', () => {
    const testDate = new Date(2026, 0, 19);
    const request: AddCustomItemRequest = {
      name: 'Eggs',
      quantity: '12',
      unit: 'pcs',
      category: 'Dairy',
    };

    const mockResponse: ShoppingItem = {
      id: 'new-item-1',
      name: 'Eggs',
      quantity: '12',
      unit: 'pcs',
      isChecked: false,
      isCustom: true,
    };

    service.addCustomItem(testDate, request).subscribe((result) => {
      expect(result).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-19/items'
    );
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(mockResponse);
  });

  it('should remove item', () => {
    const testDate = new Date(2026, 0, 19);

    service.removeItem(testDate, 'item-1').subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-19/items/item-1'
    );
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should format date with single digit month and day', () => {
    const testDate = new Date(2026, 0, 5);

    service.generateShoppingList(testDate).subscribe();

    const req = httpMock.expectOne(
      'http://localhost:5000/api/v1/shopping-list/2026-01-05'
    );
    req.flush(mockShoppingList);
  });
});
