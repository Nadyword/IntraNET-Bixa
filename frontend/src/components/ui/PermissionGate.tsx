import React from 'react';
import { usePermission, useRole } from '../../hooks/usePermission';

interface PermissionGateProps {
  permission?: string;
  role?: string;
  requireAll?: boolean;
  fallback?: React.ReactNode;
  children: React.ReactNode;
}

/**
 * Wrapper declarativo que oculta/muestra elementos según permisos o roles.
 * Solo renderiza los hijos si el usuario tiene el permiso/rol requerido.
 *
 * @param permission - Permiso requerido (ej: "usuarios.crear")
 * @param role - Rol requerido (ej: "SuperAdmin")
 * @param requireAll - Si es true y hay ambos, requiere ambos (default: false = OR)
 * @param fallback - Elemento a mostrar si no tiene permiso (default: null)
 * @param children - Contenido a mostrar si tiene permiso
 */
export const PermissionGate: React.FC<PermissionGateProps> = ({
  permission,
  role,
  requireAll = false,
  fallback = null,
  children,
}) => {
  const hasPermission = permission ? usePermission(permission) : true;
  const hasRole = role ? useRole(role) : true;

  let isAllowed = true;

  if (permission && role) {
    // Si ambos están definidos
    isAllowed = requireAll ? hasPermission && hasRole : hasPermission || hasRole;
  } else if (permission) {
    isAllowed = hasPermission;
  } else if (role) {
    isAllowed = hasRole;
  }

  return isAllowed ? <>{children}</> : <>{fallback}</>;
};

export default PermissionGate;
