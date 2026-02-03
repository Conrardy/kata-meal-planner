import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AdminUser,
  AdminUsersResponse,
  CreateAdminUserResponse,
} from '../../core/models/admin-user.model';
import { AdminUserService } from '../../core/services/admin-user.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-users.component.html',
})
export class AdminUsersComponent implements OnInit {
  private readonly adminUserService = inject(AdminUserService);
  private readonly fb = inject(FormBuilder);

  readonly users = signal<AdminUser[]>([]);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly listErrorMessage = signal<string | null>(null);
  readonly formErrorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly userForm: FormGroup = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.formErrorMessage.set(null);
    this.successMessage.set(null);

    const { username, password } = this.userForm.value as {
      username: string;
      password: string;
    };

    this.adminUserService.createUser({ username, password }).subscribe({
      next: (response: CreateAdminUserResponse) => {
        const newUser: AdminUser = {
          id: response.userId,
          username: response.username,
          isAdmin: false,
        };
        this.users.set([newUser, ...this.users()]);
        this.userForm.reset({ username: '', password: '' });
        this.isSaving.set(false);
        this.successMessage.set('User created successfully.');
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.formErrorMessage.set(
          this.buildErrorMessage(error, 'Failed to create user. Please try again.')
        );
      },
    });
  }

  hasError(field: string, error: string): boolean {
    const control = this.userForm.get(field);
    return control ? control.hasError(error) && control.touched : false;
  }

  private loadUsers(): void {
    this.isLoading.set(true);
    this.listErrorMessage.set(null);

    this.adminUserService.getUsers().subscribe({
      next: (response: AdminUsersResponse) => {
        this.users.set(response.users ?? []);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.listErrorMessage.set(
          this.buildErrorMessage(error, 'Failed to load users. Please try again later.')
        );
      },
    });
  }

  private buildErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const responseBody = error.error as
        | { detail?: string; title?: string; errors?: Record<string, string[]> }
        | undefined;

      if (responseBody?.detail) {
        return responseBody.detail;
      }

      if (responseBody?.errors) {
        const messages = Object.values(responseBody.errors).flat();
        if (messages.length > 0) {
          return messages.join(' ');
        }
      }

      if (responseBody?.title) {
        return responseBody.title;
      }

      if (error.message) {
        return error.message;
      }
    }

    return fallback;
  }
}
