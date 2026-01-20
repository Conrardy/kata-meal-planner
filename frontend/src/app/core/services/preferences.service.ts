import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserPreferences, UpdatePreferencesRequest } from '../models/preferences.model';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class PreferencesService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  getPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.baseUrl}/preferences`);
  }

  updatePreferences(request: UpdatePreferencesRequest): Observable<UserPreferences> {
    return this.http.put<UserPreferences>(`${this.baseUrl}/preferences`, request);
  }
}
