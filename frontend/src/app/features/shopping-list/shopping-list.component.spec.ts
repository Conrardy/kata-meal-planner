import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ShoppingListComponent } from './shopping-list.component';
import { ShoppingListService } from '../../core/services/shopping-list.service';
import {
  ShoppingList,
  ShoppingItem,
  ShoppingCategory,
} from '../../core/models/shopping-list.model';

describe('ShoppingListComponent', () => {
  let shoppingListServiceMock: {
    generateShoppingList: ReturnType<typeof vi.fn>;
    toggleItem: ReturnType<typeof vi.fn>;
    addCustomItem: ReturnType<typeof vi.fn>;
    removeItem: ReturnType<typeof vi.fn>;
  };

  const mockItem: ShoppingItem = {
    id: 'item-1',
    name: 'Milk',
    quantity: '1',
    unit: 'liter',
    isChecked: false,
    isCustom: false,
  };

  const mockCategory: ShoppingCategory = {
    category: 'Dairy',
    items: [mockItem],
  };

  const mockShoppingList: ShoppingList = {
    startDate: '2026-01-19',
    endDate: '2026-01-25',
    categories: [mockCategory],
    wasUpdated: false,
  };

  beforeEach(async () => {
    shoppingListServiceMock = {
      generateShoppingList: vi.fn().mockReturnValue(of(mockShoppingList)),
      toggleItem: vi.fn().mockReturnValue(of(void 0)),
      addCustomItem: vi
        .fn()
        .mockReturnValue(of({ ...mockItem, id: 'new-item', isCustom: true })),
      removeItem: vi.fn().mockReturnValue(of(void 0)),
    };

    await TestBed.configureTestingModule({
      imports: [ShoppingListComponent],
      providers: [
        provideRouter([]),
        { provide: ShoppingListService, useValue: shoppingListServiceMock },
      ],
    }).compileComponents();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load shopping list on init', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(shoppingListServiceMock.generateShoppingList).toHaveBeenCalled();
    expect(fixture.componentInstance.shoppingList()).toEqual(mockShoppingList);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set error when loading fails', async () => {
    shoppingListServiceMock.generateShoppingList.mockReturnValue(
      throwError(() => new Error('Network error'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.error()).toBe(
      'Failed to generate shopping list. Please try again later.'
    );
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should show update notice when list was updated', async () => {
    shoppingListServiceMock.generateShoppingList.mockReturnValue(
      of({ ...mockShoppingList, wasUpdated: true })
    );

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.showUpdateNotice()).toBe(true);
  });

  it('should dismiss update notice', async () => {
    shoppingListServiceMock.generateShoppingList.mockReturnValue(
      of({ ...mockShoppingList, wasUpdated: true })
    );

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    fixture.componentInstance.dismissUpdateNotice();

    expect(fixture.componentInstance.showUpdateNotice()).toBe(false);
  });

  it('should navigate to previous week', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const initialDate = new Date(fixture.componentInstance.startDate());
    fixture.componentInstance.onPreviousWeek();
    await fixture.whenStable();

    const newDate = fixture.componentInstance.startDate();
    expect(newDate.getTime()).toBeLessThan(initialDate.getTime());
    expect(shoppingListServiceMock.generateShoppingList).toHaveBeenCalledTimes(2);
  });

  it('should navigate to next week', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const initialDate = new Date(fixture.componentInstance.startDate());
    fixture.componentInstance.onNextWeek();
    await fixture.whenStable();

    const newDate = fixture.componentInstance.startDate();
    expect(newDate.getTime()).toBeGreaterThan(initialDate.getTime());
    expect(shoppingListServiceMock.generateShoppingList).toHaveBeenCalledTimes(2);
  });

  it('should print', () => {
    const printSpy = vi.spyOn(window, 'print').mockImplementation(() => {});

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.componentInstance.onPrint();

    expect(printSpy).toHaveBeenCalled();
    printSpy.mockRestore();
  });

  it('should toggle item checked state', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onToggleItem(mockItem);
    await fixture.whenStable();

    expect(shoppingListServiceMock.toggleItem).toHaveBeenCalledWith(
      expect.any(Date),
      'item-1',
      true
    );
  });

  it('should revert toggle on error', async () => {
    shoppingListServiceMock.toggleItem.mockReturnValue(
      throwError(() => new Error('Toggle failed'))
    );

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    const initialState = component
      .shoppingList()!
      .categories[0].items[0].isChecked;

    component.onToggleItem(mockItem);
    await fixture.whenStable();

    const finalState = component.shoppingList()!.categories[0].items[0].isChecked;
    expect(finalState).toBe(initialState);
  });

  it('should show add form', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    const component = fixture.componentInstance;

    component.onShowAddForm();

    expect(component.showAddForm()).toBe(true);
  });

  it('should cancel add form and reset fields', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    const component = fixture.componentInstance;
    component.onShowAddForm();
    component.newItemName = 'Test';

    component.onCancelAddForm();

    expect(component.showAddForm()).toBe(false);
    expect(component.newItemName).toBe('');
  });

  it('should add custom item', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onShowAddForm();
    component.newItemName = 'Eggs';
    component.newItemQuantity = '12';
    component.newItemUnit = 'pcs';
    component.newItemCategory = 'Dairy';

    component.onAddCustomItem();
    await fixture.whenStable();

    expect(shoppingListServiceMock.addCustomItem).toHaveBeenCalledWith(
      expect.any(Date),
      {
        name: 'Eggs',
        quantity: '12',
        unit: 'pcs',
        category: 'Dairy',
      }
    );
    expect(component.showAddForm()).toBe(false);
  });

  it('should not add custom item with empty name', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.newItemName = '   ';

    component.onAddCustomItem();
    await fixture.whenStable();

    expect(shoppingListServiceMock.addCustomItem).not.toHaveBeenCalled();
  });

  it('should handle add custom item error', async () => {
    shoppingListServiceMock.addCustomItem.mockReturnValue(
      throwError(() => new Error('Add failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.newItemName = 'Test';
    component.onAddCustomItem();
    await fixture.whenStable();

    expect(component.showAddForm()).toBe(true);
  });

  it('should remove item', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    shoppingListServiceMock.generateShoppingList.mockClear();

    const component = fixture.componentInstance;
    component.onRemoveItem(mockItem);
    await fixture.whenStable();

    expect(shoppingListServiceMock.removeItem).toHaveBeenCalledWith(
      expect.any(Date),
      'item-1'
    );
    expect(shoppingListServiceMock.generateShoppingList).toHaveBeenCalled();
  });

  it('should handle remove item error', async () => {
    shoppingListServiceMock.removeItem.mockReturnValue(
      throwError(() => new Error('Remove failed'))
    );
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.onRemoveItem(mockItem);
    await fixture.whenStable();

    expect(shoppingListServiceMock.generateShoppingList).toHaveBeenCalledTimes(1);
  });

  it('should get category icon', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    const component = fixture.componentInstance;

    expect(component.getCategoryIcon('Produce')).toBe(component.Apple);
    expect(component.getCategoryIcon('Dairy')).toBe(component.Milk);
    expect(component.getCategoryIcon('Meat')).toBe(component.Beef);
    expect(component.getCategoryIcon('Pantry')).toBe(component.Package);
    expect(component.getCategoryIcon('Unknown')).toBe(component.Package);
  });

  it('should get category color', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    const component = fixture.componentInstance;

    expect(component.getCategoryColor('Produce')).toContain('green');
    expect(component.getCategoryColor('Dairy')).toContain('blue');
    expect(component.getCategoryColor('Meat')).toContain('red');
    expect(component.getCategoryColor('Pantry')).toContain('amber');
    expect(component.getCategoryColor('Unknown')).toContain('amber');
  });

  it('should format quantity with unit', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    const component = fixture.componentInstance;

    expect(component.formatQuantity(mockItem)).toBe('1 liter');
    expect(
      component.formatQuantity({ ...mockItem, unit: null })
    ).toBe('1');
  });

  it('should compute total items', async () => {
    const multiCategoryList: ShoppingList = {
      ...mockShoppingList,
      categories: [
        { category: 'Dairy', items: [mockItem, { ...mockItem, id: 'item-2' }] },
        { category: 'Produce', items: [{ ...mockItem, id: 'item-3' }] },
      ],
    };
    shoppingListServiceMock.generateShoppingList.mockReturnValue(
      of(multiCategoryList)
    );

    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.totalItems()).toBe(3);
  });

  it('should return 0 total items when no list', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    expect(fixture.componentInstance.totalItems()).toBe(0);
  });

  it('should compute formatted date range', async () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const range = fixture.componentInstance.formattedDateRange();
    expect(range).toContain('Jan');
  });

  it('should return empty string for date range when no list', () => {
    const fixture = TestBed.createComponent(ShoppingListComponent);
    expect(fixture.componentInstance.formattedDateRange()).toBe('');
  });
});
