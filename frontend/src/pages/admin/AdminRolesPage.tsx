import React, { useEffect, useState } from 'react';
import { usePermission } from '../../hooks/usePermission';
import api from '../../lib/api';
import '../styles/AdminRolesPage.css';

interface Permission {
  name: string;
  description: string;
}

interface RoleWithPermissions {
  id: string;
  name: string;
  permissions: string[]; // ahora es un array de nombres de permiso
}

interface PermissionsByCategory {
  [category: string]: Permission[];
}

export const AdminRolesPage: React.FC = () => {
  const hasPermission = usePermission('roles.gestionar');
  const [roles, setRoles] = useState<RoleWithPermissions[]>([]);
  const [permissions, setPermissions] = useState<PermissionsByCategory>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [checkedPermissions, setCheckedPermissions] = useState<
    Record<string, Record<string, boolean>>
  >({});

  // Cargar datos iniciales
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        const [rolesRes, permsRes] = await Promise.all([
          api.get('/permissions/roles'),
          api.get('/permissions'),
        ]);

        setRoles(rolesRes.data);

        // Agrupar permisos por categoría
        const grouped: PermissionsByCategory = {};
        (permsRes.data as Array<{ category: string; permissions: Permission[] }>).forEach(
          (group) => {
            grouped[group.category] = group.permissions;
          }
        );
        setPermissions(grouped);

        // Inicializar checkboxes
        const checked: Record<string, Record<string, boolean>> = {};
        rolesRes.data.forEach((role: RoleWithPermissions) => {
          checked[role.id] = {};
          Object.values(grouped).forEach((perms) => {
            perms.forEach((perm) => {
              const hasIt = role.permissions.includes(perm.name);
              checked[role.id][perm.name] = hasIt;
            });
          });
        });
        setCheckedPermissions(checked);
      } catch (error) {
        setMessage('Error cargando datos');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const handleToggle = (roleId: string, permissionId: string) => {
    setCheckedPermissions((prev) => ({
      ...prev,
      [roleId]: {
        ...prev[roleId],
        [permissionId]: !prev[roleId][permissionId],
      },
    }));
  };

  const handleSaveRole = async (roleId: string) => {
    setSaving(true);
    setMessage('');

    try {
      const permissionNames = Object.entries(checkedPermissions[roleId])
        .filter(([_, checked]) => checked)
        .map(([permName, _]) => permName);

      await api.put(`/permissions/roles/${roleId}`, { permissionNames });
      setMessage(`Permisos de ${roles.find((r) => r.id === roleId)?.name} actualizados`);
    } catch (error: any) {
      setMessage(
        error.response?.data?.message || 'Error al guardar permisos'
      );
    } finally {
      setSaving(false);
    }
  };

  if (!hasPermission) {
    return (
      <div className="admin-roles-page">
        <div className="access-denied">
          <h2>❌ Acceso Denegado</h2>
          <p>No tienes permiso para acceder a esta sección.</p>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="admin-roles-page">
        <div className="loading">Cargando...</div>
      </div>
    );
  }

  return (
    <div className="admin-roles-page">
      <div className="admin-container">
        <h1>Gestión de Permisos por Rol</h1>
        <p className="subtitle">
          Configura qué permisos tiene cada rol en el sistema
        </p>

        {message && (
          <div className={`alert ${message.includes('Error') ? 'alert-error' : 'alert-success'}`}>
            {message}
          </div>
        )}

        <div className="roles-grid">
          {roles.map((role) => (
            <div key={role.id} className="role-card">
              <div className="role-header">
                <h3>{role.name}</h3>
                <button
                  className="btn-save"
                  onClick={() => handleSaveRole(role.id)}
                  disabled={saving}
                >
                  {saving ? 'Guardando...' : 'Guardar'}
                </button>
              </div>

              <div className="permissions-list">
                {Object.entries(permissions).map(([category, perms]) => (
                  <div key={category} className="permission-category">
                    <h4 className="category-title">{category}</h4>
                    <div className="permission-items">
                      {perms.map((perm) => (
                        <label key={perm.name} className="permission-checkbox">
                          <input
                            type="checkbox"
                            checked={checkedPermissions[role.id]?.[perm.name] || false}
                            onChange={() => handleToggle(role.id, perm.name)}
                            disabled={saving}
                          />
                          <span className="checkbox-label">
                            <span className="perm-name">{perm.name}</span>
                            <span className="perm-description">{perm.description}</span>
                          </span>
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
