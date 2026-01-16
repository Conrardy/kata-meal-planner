export interface ShoppingItem {
  id: string;
  name: string;
  quantity: string;
  unit: string | null;
  isChecked: boolean;
  isCustom: boolean;
}

export interface ShoppingCategory {
  category: string;
  items: ShoppingItem[];
}

export interface ShoppingList {
  startDate: string;
  endDate: string;
  categories: ShoppingCategory[];
}

export interface AddCustomItemRequest {
  name: string;
  quantity: string;
  unit: string | null;
  category: string;
}

export interface ToggleItemRequest {
  isChecked: boolean;
}
