export interface ShoppingItem {
  name: string;
  quantity: string;
  unit: string | null;
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
