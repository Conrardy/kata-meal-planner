export interface StockItem {
  id: string;
  ingredientName: string;
  quantity: number;
  unit: string;
  category: string;
  expirationDate: string | null;
  lowStockThreshold: number | null;
}

export interface StockList {
  items: StockItem[];
}

export interface CreateStockItemRequest {
  ingredientName: string;
  quantity: number;
  unit: string;
  category: string;
  expirationDate: string | null;
  lowStockThreshold: number | null;
}

export interface UpdateStockItemRequest {
  ingredientName: string;
  quantity: number;
  unit: string;
  category: string;
  expirationDate: string | null;
  lowStockThreshold: number | null;
}
