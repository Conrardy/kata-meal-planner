import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserPreferences, UpdatePreferencesRequest } from '../models/preferences.model';

@Injectable({
  providedIn: 'root',
})
export class PreferencesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/v1';

  getPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.baseUrl}/preferences`);
  }

  updatePreferences(request: UpdatePreferencesRequest): Observable<UserPreferences> {
    return this.http.put<UserPreferences>(`${this.baseUrl}/preferences`, request);
  }
}
