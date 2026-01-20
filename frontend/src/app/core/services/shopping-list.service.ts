import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ShoppingList,
  ShoppingItem,
  AddCustomItemRequest,
  ToggleItemRequest,
} from '../models/shopping-list.model';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class ShoppingListService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  generateShoppingList(startDate: Date): Observable<ShoppingList> {
    const formattedDate = this.formatDate(startDate);
    return this.http.get<ShoppingList>(
      `${this.baseUrl}/shopping-list/${formattedDate}`
    );
  }

  toggleItem(
    startDate: Date,
    itemId: string,
    isChecked: boolean
  ): Observable<void> {
    const formattedDate = this.formatDate(startDate);
    const request: ToggleItemRequest = { isChecked };
    return this.http.patch<void>(
      `${this.baseUrl}/shopping-list/${formattedDate}/items/${encodeURIComponent(itemId)}`,
      request
    );
  }

  addCustomItem(
    startDate: Date,
    request: AddCustomItemRequest
  ): Observable<ShoppingItem> {
    const formattedDate = this.formatDate(startDate);
    return this.http.post<ShoppingItem>(
      `${this.baseUrl}/shopping-list/${formattedDate}/items`,
      request
    );
  }

  removeItem(startDate: Date, itemId: string): Observable<void> {
    const formattedDate = this.formatDate(startDate);
    return this.http.delete<void>(
      `${this.baseUrl}/shopping-list/${formattedDate}/items/${encodeURIComponent(itemId)}`
    );
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
