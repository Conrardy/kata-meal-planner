import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AdminUsersComponent } from './admin-users.component';
import { AdminUserService } from '../../core/services/admin-user.service';
import { AdminUser } from '../../core/models/admin-user.model';

describe('AdminUsersComponent', () => {
  let adminUserServiceMock: {
    getUsers: ReturnType<typeof vi.fn>;
    createUser: ReturnType<typeof vi.fn>;
  };

  const mockUsers: AdminUser[] = [
    { id: 'user-1', username: 'Emmanuel', isAdmin: true },
  ];

  beforeEach(async () => {
    adminUserServiceMock = {
      getUsers: vi.fn().mockReturnValue(of({ users: mockUsers })),
      createUser: vi.fn().mockReturnValue(
        of({ userId: 'user-2', username: 'Gabrielle' })
      ),
    };

    await TestBed.configureTestingModule({
      imports: [AdminUsersComponent],
      providers: [{ provide: AdminUserService, useValue: adminUserServiceMock }],
    }).compileComponents();
  });

  it('should create the component', () => {
    const fixture = TestBed.createComponent(AdminUsersComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load users on init', async () => {
    const fixture = TestBed.createComponent(AdminUsersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(adminUserServiceMock.getUsers).toHaveBeenCalled();
    expect(fixture.componentInstance.users()).toEqual(mockUsers);
    expect(fixture.componentInstance.isLoading()).toBe(false);
  });

  it('should set list error when load fails', async () => {
    adminUserServiceMock.getUsers.mockReturnValue(
      throwError(() => new Error('Network error'))
    );

    const fixture = TestBed.createComponent(AdminUsersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.listErrorMessage()).toBe(
      'Failed to load users. Please try again later.'
    );
  });

  it('should create a user and update the list', async () => {
    const fixture = TestBed.createComponent(AdminUsersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.userForm.setValue({ username: 'Gabrielle', password: 'StrongPass1' });

    component.onSubmit();
    await fixture.whenStable();

    expect(adminUserServiceMock.createUser).toHaveBeenCalledWith({
      username: 'Gabrielle',
      password: 'StrongPass1',
    });
    expect(component.users().length).toBe(2);
    expect(component.successMessage()).toBe('User created successfully.');
  });

  it('should set form error when creation fails', async () => {
    adminUserServiceMock.createUser.mockReturnValue(
      throwError(() => new Error('Validation error'))
    );

    const fixture = TestBed.createComponent(AdminUsersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.userForm.setValue({ username: 'Test', password: 'WeakPass1' });

    component.onSubmit();
    await fixture.whenStable();

    expect(component.formErrorMessage()).toBe(
      'Failed to create user. Please try again.'
    );
  });
});
