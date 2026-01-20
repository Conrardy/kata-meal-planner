import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ApiConfigService {
  readonly apiUrl = environment.apiUrl;
  readonly apiBaseUrl = `${environment.apiUrl}/api/v1`;

  getEndpoint(path: string): string {
    return `${this.apiBaseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }
}
