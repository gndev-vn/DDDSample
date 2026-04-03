import { defineStore } from 'pinia';
import { identityApi } from '../api/identity';
import { ApiError } from '../lib/http';
import type { LoginRequest, RegisterRequest, UserProfile } from '../types/contracts';

const sessionStorageKey = 'dddsample-webapp-session';

interface StoredSession {
  token: string;
  expiresAt: string;
}

function readStoredSession(): StoredSession | null {
  const raw = window.localStorage.getItem(sessionStorageKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as StoredSession;
  } catch {
    window.localStorage.removeItem(sessionStorageKey);
    return null;
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: (readStoredSession()?.token ?? null) as string | null,
    expiresAt: (readStoredSession()?.expiresAt ?? null) as string | null,
    user: null as UserProfile | null,
    loading: false,
    initialized: false,
    error: null as string | null,
    reauthRequired: false,
    eventsBound: false,
  }),

  getters: {
    isAuthenticated: (state) => Boolean(state.token && state.user),
    permissions: (state) => state.user?.permissions ?? [],
    isAdmin: (state) => Boolean(state.user?.roles.some((role) => role.toLowerCase() === 'admin')),
    hasPermission: (state) => (permission: string) => Boolean(
      state.user?.roles.some((role) => role.toLowerCase() === 'admin') ||
      state.user?.permissions.includes(permission),
    ),
  },

  actions: {
    bindAuthEvents() {
      if (this.eventsBound) {
        return;
      }

      window.addEventListener('dddsample:auth-expired', () => {
        if (!this.user) {
          this.clearSession();
          return;
        }

        this.reauthRequired = true;
        this.error = 'Your session expired. Sign in again to continue working.';
      });

      this.eventsBound = true;
    },

    persistSession() {
      if (!this.token || !this.expiresAt) {
        window.localStorage.removeItem(sessionStorageKey);
        return;
      }

      window.localStorage.setItem(
        sessionStorageKey,
        JSON.stringify({
          token: this.token,
          expiresAt: this.expiresAt,
        }),
      );
    },

    clearSession() {
      this.token = null;
      this.expiresAt = null;
      this.user = null;
      this.error = null;
      this.reauthRequired = false;
      window.localStorage.removeItem(sessionStorageKey);
    },

    async initialize() {
      if (this.initialized) {
        return;
      }

      this.bindAuthEvents();

      if (!this.token) {
        this.initialized = true;
        return;
      }

      try {
        await this.refreshProfile();
      } catch {
        this.clearSession();
      } finally {
        this.initialized = true;
      }
    },

    async refreshProfile() {
      if (!this.token) {
        this.user = null;
        return;
      }

      this.loading = true;
      this.error = null;

      try {
        const response = await identityApi.getCurrentUser(this.token);
        this.user = response.data ?? null;
        this.reauthRequired = false;
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Unable to load the current user.';
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async login(request: LoginRequest) {
      this.loading = true;
      this.error = null;

      try {
        const response = await identityApi.login(request);
        const payload = response.data;
        if (!payload) {
          throw new ApiError('Login completed without an auth payload.', 500);
        }

        this.token = payload.token;
        this.expiresAt = payload.expiresAt;
        this.user = payload.user;
        this.reauthRequired = false;
        this.persistSession();
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Login failed.';
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async register(request: RegisterRequest) {
      this.loading = true;
      this.error = null;

      try {
        await identityApi.register(request);
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Registration failed.';
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async loginWithGoogle(idToken: string) {
      this.loading = true;
      this.error = null;

      try {
        const response = await identityApi.googleLogin({ idToken });
        const payload = response.data;
        if (!payload) {
          throw new ApiError('Google login completed without an auth payload.', 500);
        }

        this.token = payload.token;
        this.expiresAt = payload.expiresAt;
        this.user = payload.user;
        this.reauthRequired = false;
        this.persistSession();
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Google login failed.';
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async logout() {
      this.loading = true;
      this.error = null;

      try {
        if (this.token) {
          await identityApi.logout(this.token);
        }
      } finally {
        this.clearSession();
        this.loading = false;
      }
    },
  },
});
