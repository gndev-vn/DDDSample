export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface UserProfile {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  customerId?: string | null;
  roles: string[];
  permissions: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AuthPayload {
  token: string;
  expiresAt: string;
  user: UserProfile;
}

export interface RoleModel {
  id: string;
  name: string;
  description: string;
  permissions: string[];
  createdAt: string;
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  permissions: string[];
}

export interface UpdateRolePermissionsRequest {
  permissions: string[];
}

export interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
  customerId?: string | null;
}

export interface UpdateUserRequest {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  customerId?: string | null;
}

export interface AssignRolesRequest {
  userId: string;
  roles: string[];
}
