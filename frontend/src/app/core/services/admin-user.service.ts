import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdminUsersResponse,
  CreateAdminUserRequest,
  CreateAdminUserResponse,
} from '../models/admin-user.model';
import { ApiConfigService } from './api-config.service';

@Injectable({
  providedIn: 'root',
})
export class AdminUserService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfigService);
  private readonly baseUrl = this.apiConfig.apiBaseUrl;

  getUsers(): Observable<AdminUsersResponse> {
    return this.http.get<AdminUsersResponse>(`${this.baseUrl}/admin/users`);
  }

  createUser(request: CreateAdminUserRequest): Observable<CreateAdminUserResponse> {
    return this.http.post<CreateAdminUserResponse>(
      `${this.baseUrl}/admin/users`,
      request
    );
  }
}
