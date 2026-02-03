import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let authServiceMock: { isAuthenticated: ReturnType<typeof vi.fn>; isAdmin: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authServiceMock = {
      isAuthenticated: vi.fn(),
      isAdmin: vi.fn(),
    };

    await TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceMock },
      ],
    }).compileComponents();
  });

  it('should redirect to login when not authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);

    const router = TestBed.inject(Router);
    const result = TestBed.runInInjectionContext(() =>
      adminGuard({} as any, {} as any)
    );

    expect(router.serializeUrl(result as any)).toBe('/login');
  });

  it('should allow navigation for admins', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.isAdmin.mockReturnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      adminGuard({} as any, {} as any)
    );

    expect(result).toBe(true);
  });

  it('should redirect to home for non-admin users', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.isAdmin.mockReturnValue(false);

    const router = TestBed.inject(Router);
    const result = TestBed.runInInjectionContext(() =>
      adminGuard({} as any, {} as any)
    );

    expect(router.serializeUrl(result as any)).toBe('/');
  });
});
