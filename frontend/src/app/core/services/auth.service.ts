import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import {
  AuthResponse,
  AuthState,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
  RegisterResponse,
} from '../models/auth.model';

const TOKEN_KEY = 'mealplanner_access_token';
const REFRESH_TOKEN_KEY = 'mealplanner_refresh_token';
const USER_KEY = 'mealplanner_user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/v1/auth';

  private readonly authState = signal<AuthState>(this.loadStoredState());

  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly currentUser = computed(() => ({
    userId: this.authState().userId,
    email: this.authState().email,
  }));
  readonly accessToken = computed(() => this.authState().accessToken);

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(
      tap((response) => this.handleAuthSuccess(response)),
      catchError((error) => {
        this.clearAuth();
        return throwError(() => error);
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.authState().refreshToken;
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<AuthResponse>(`${this.baseUrl}/refresh`, request).pipe(
      tap((response) => this.handleAuthSuccess(response)),
      catchError((error) => {
        this.clearAuth();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.clearAuth();
  }

  getAccessToken(): string | null {
    return this.authState().accessToken;
  }

  private handleAuthSuccess(response: AuthResponse): void {
    const newState: AuthState = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      userId: response.userId,
      email: response.email,
      isAuthenticated: true,
    };

    this.authState.set(newState);
    this.persistState(newState);
  }

  private clearAuth(): void {
    const emptyState: AuthState = {
      accessToken: null,
      refreshToken: null,
      userId: null,
      email: null,
      isAuthenticated: false,
    };

    this.authState.set(emptyState);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  private persistState(state: AuthState): void {
    if (state.accessToken) {
      localStorage.setItem(TOKEN_KEY, state.accessToken);
    }
    if (state.refreshToken) {
      localStorage.setItem(REFRESH_TOKEN_KEY, state.refreshToken);
    }
    if (state.userId && state.email) {
      localStorage.setItem(
        USER_KEY,
        JSON.stringify({ userId: state.userId, email: state.email })
      );
    }
  }

  private loadStoredState(): AuthState {
    const accessToken = localStorage.getItem(TOKEN_KEY);
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    const userJson = localStorage.getItem(USER_KEY);

    if (!accessToken || !refreshToken) {
      return {
        accessToken: null,
        refreshToken: null,
        userId: null,
        email: null,
        isAuthenticated: false,
      };
    }

    let user: { userId: string; email: string } | null = null;
    if (userJson) {
      try {
        user = JSON.parse(userJson);
      } catch {
        user = null;
      }
    }

    return {
      accessToken,
      refreshToken,
      userId: user?.userId ?? null,
      email: user?.email ?? null,
      isAuthenticated: true,
    };
  }
}
