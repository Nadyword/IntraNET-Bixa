import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import { useUserProfileStore } from './userProfileStore';

export interface User {
  id: string;
  taxId: string;
  role: string;
  rolId: string;
}

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (token: string, refreshToken: string) => void;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

function isTokenExpired(token: string): boolean {
  try {
    const decoded: any = jwtDecode(token);
    if (!decoded.exp) return false;
    return Date.now() / 1000 > decoded.exp;
  } catch {
    return true;
  }
}

function decodeUser(token: string): User | null {
  try {
    const decoded: any = jwtDecode(token);
    return {
      id: decoded.id || '',
      taxId: decoded.ci || '',
      role: decoded.Rol || '',
      rolId: decoded.RolId || '',
    };
  } catch {
    return null;
  }
}

export const useAuthStore = create<AuthState>((set, get) => {
  const savedToken = sessionStorage.getItem('accessToken');
  const savedRefresh = sessionStorage.getItem('refreshToken');

  let initialToken: string | null = null;
  let initialUser: User | null = null;

  if (savedToken) {
    if (!isTokenExpired(savedToken)) {
      initialToken = savedToken;
      initialUser = decodeUser(savedToken);
    } else {
      sessionStorage.removeItem('accessToken');
      sessionStorage.removeItem('refreshToken');
    }
  }

  return {
    user: initialUser,
    accessToken: initialToken,
    refreshToken: initialToken ? savedRefresh : null,
    isAuthenticated: !!initialToken,

    login: (token: string, refreshToken: string) => {
      sessionStorage.setItem('accessToken', token);
      sessionStorage.setItem('refreshToken', refreshToken);
      const user = decodeUser(token);
      set({ user, accessToken: token, refreshToken, isAuthenticated: true });
    },

    logout: () => {
      sessionStorage.removeItem('accessToken');
      sessionStorage.removeItem('refreshToken');
      useUserProfileStore.getState().clearProfile();
      set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
    },

    hasPermission: (permission: string) => {
      const { accessToken } = get();
      if (!accessToken) return false;
      try {
        const decoded: any = jwtDecode(accessToken);
        const perms: string[] = decoded['permission']
          ? Array.isArray(decoded['permission'])
            ? decoded['permission']
            : [decoded['permission']]
          : [];
        return perms.includes(permission);
      } catch {
        return false;
      }
    },

    hasRole: (role: string) => get().user?.role === role,
  };
});
