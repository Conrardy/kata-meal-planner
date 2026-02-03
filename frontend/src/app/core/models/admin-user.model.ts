export interface AdminUser {
  id: string;
  username: string;
  isAdmin: boolean;
}

export interface AdminUsersResponse {
  users: AdminUser[];
}

export interface CreateAdminUserRequest {
  username: string;
  password: string;
}

export interface CreateAdminUserResponse {
  userId: string;
  username: string;
}
