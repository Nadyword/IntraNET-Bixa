import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';

export interface User {
  id: string;
  email: string;
  nombre: string;
  cargo: string;
  role?: string;
  permissions?: string[];
}

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  login: (user: User, token: string) => void;
  logout: () => void;
  setUser: (user: User) => void;
  setToken: (token: string) => void;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => {
  // Intentar recuperar usuario del localStorage y decodificar JWT
  const savedToken = localStorage.getItem('accessToken');
  let initialUser: User | null = null;

  if (savedToken) {
    try {
      const decoded: any = jwtDecode(savedToken);
      const permissions = decoded['permission']
        ? Array.isArray(decoded['permission'])
          ? decoded['permission']
          : [decoded['permission']]
        : [];

      initialUser = {
        id: decoded.sub || decoded.NameIdentifier || '',
        email: decoded.email || '',
        nombre: decoded.name || '',
        cargo: decoded.cargo || '',
        role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '',
        permissions: permissions,
      };
    } catch (error) {
      localStorage.removeItem('accessToken');
    }
  }

  return {
    user: initialUser,
    accessToken: savedToken,
    isAuthenticated: !!savedToken,

    login: (user, token) => {
      localStorage.setItem('accessToken', token);

      // Decodificar token para extraer permisos
      try {
        const decoded: any = jwtDecode(token);
        const permissions = decoded['permission']
          ? Array.isArray(decoded['permission'])
            ? decoded['permission']
            : [decoded['permission']]
          : [];

        const userWithPermissions: User = {
          ...user,
          role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '',
          permissions: permissions,
        };

        set({
          user: userWithPermissions,
          accessToken: token,
          isAuthenticated: true,
        });
      } catch (error) {
        // Si hay error decodificando, usar el usuario tal cual
        set({ user, accessToken: token, isAuthenticated: true });
      }
    },

    logout: () => {
      localStorage.removeItem('accessToken');
      set({ user: null, accessToken: null, isAuthenticated: false });
    },

    setUser: (user) => set({ user }),
    setToken: (token) => {
      localStorage.setItem('accessToken', token);
      set({ accessToken: token, isAuthenticated: true });
    },

    hasPermission: (permission: string) => {
      const state = get();
      return state.user?.permissions?.includes(permission) ?? false;
    },

    hasRole: (role: string) => {
      const state = get();
      return state.user?.role === role;
    },
  };
});
