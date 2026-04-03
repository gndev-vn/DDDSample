import { apiRequest } from '../lib/http';
import type {
  ApiResponse,
  AssignRolesRequest,
  AuthPayload,
  CreateRoleRequest,
  CreateUserRequest,
  GoogleLoginRequest,
  LoginRequest,
  RegisterRequest,
  RoleModel,
  UpdateRolePermissionsRequest,
  UpdateUserRequest,
  UserProfile,
} from '../types/contracts';

export const identityApi = {
  login(request: LoginRequest): Promise<ApiResponse<AuthPayload>> {
    return apiRequest<AuthPayload>('identity', '/api/Auth/login', {
      method: 'POST',
      body: request,
    });
  },

  register(request: RegisterRequest): Promise<ApiResponse<{ userId: string }>> {
    return apiRequest<{ userId: string }>('identity', '/api/Auth/register', {
      method: 'POST',
      body: request,
    });
  },

  googleLogin(request: GoogleLoginRequest): Promise<ApiResponse<AuthPayload>> {
    return apiRequest<AuthPayload>('identity', '/api/Auth/google-login', {
      method: 'POST',
      body: request,
    });
  },

  getCurrentUser(token: string): Promise<ApiResponse<UserProfile>> {
    return apiRequest<UserProfile>('identity', '/api/Users/me', {
      token,
    });
  },

  logout(token: string): Promise<ApiResponse<null>> {
    return apiRequest<null>('identity', '/api/Auth/logout', {
      method: 'POST',
      token,
    });
  },

  async getUsers(token: string): Promise<UserProfile[]> {
    const response = await apiRequest<UserProfile[]>('identity', '/api/Users', { token });
    return response.data ?? [];
  },

  async getUser(token: string, userId: string): Promise<UserProfile | null> {
    const response = await apiRequest<UserProfile>('identity', '/api/Users/' + userId, { token });
    return response.data ?? null;
  },

  async createUser(token: string, request: CreateUserRequest): Promise<UserProfile | null> {
    const response = await apiRequest<UserProfile>('identity', '/api/Users', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async updateUser(token: string, request: UpdateUserRequest): Promise<UserProfile | null> {
    const response = await apiRequest<UserProfile>('identity', '/api/Users/' + request.id, {
      method: 'PUT',
      token,
      body: request,
    });

    return response.data ?? null;
  },

  async deleteUser(token: string, userId: string): Promise<void> {
    await apiRequest<null>('identity', '/api/Users/' + userId, {
      method: 'DELETE',
      token,
    });
  },

  async getRoles(token: string): Promise<RoleModel[]> {
    const response = await apiRequest<RoleModel[]>('identity', '/api/Roles', { token });
    return response.data ?? [];
  },

  async createRole(token: string, request: CreateRoleRequest): Promise<void> {
    await apiRequest<{ roleId: string }>('identity', '/api/Roles', {
      method: 'POST',
      token,
      body: request,
    });
  },

  async updateRolePermissions(token: string, roleId: string, request: UpdateRolePermissionsRequest): Promise<void> {
    await apiRequest<{ roleId: string }>('identity', '/api/Roles/' + roleId + '/permissions', {
      method: 'PUT',
      token,
      body: request,
    });
  },

  async assignRoles(token: string, request: AssignRolesRequest): Promise<string[]> {
    const response = await apiRequest<{ roles: string[] }>('identity', '/api/Roles/assign', {
      method: 'POST',
      token,
      body: request,
    });

    return response.data?.roles ?? [];
  },
};
