import { useAuthStore } from '../store/authStore';

/**
 * Hook para verificar si el usuario tiene un permiso específico.
 * @param permission - El nombre del permiso (ej: "tramites.leer")
 * @returns true si el usuario tiene el permiso, false si no
 */
export const usePermission = (permission: string): boolean => {
  return useAuthStore((state) => state.hasPermission(permission));
};

/**
 * Hook para verificar si el usuario tiene un rol específico.
 * @param role - El nombre del rol (ej: "SuperAdmin")
 * @returns true si el usuario tiene el rol, false si no
 */
export const useRole = (role: string): boolean => {
  return useAuthStore((state) => state.hasRole(role));
};
