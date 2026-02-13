import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StockItem, StockList, CreateStockItemRequest, UpdateStockItemRequest } from '../models/stock.model';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class StockService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  getStockItems(): Observable<StockList> {
    return this.http.get<StockList>(`${this.baseUrl}/stock`);
  }

  createStockItem(request: CreateStockItemRequest): Observable<StockItem> {
    return this.http.post<StockItem>(`${this.baseUrl}/stock`, request);
  }

  updateStockItem(id: string, request: UpdateStockItemRequest): Observable<StockItem> {
    return this.http.put<StockItem>(`${this.baseUrl}/stock/${id}`, request);
  }

  deleteStockItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/stock/${id}`);
  }

  adjustQuantity(id: string, adjustment: number): Observable<StockItem> {
    return this.http.patch<StockItem>(
      `${this.baseUrl}/stock/${id}/quantity`,
      { adjustment }
    );
  }
}
