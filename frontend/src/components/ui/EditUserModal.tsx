import React, { useState } from 'react';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';
import type { ApiResponse } from '../../services/authService';
import './ForgotPasswordModal.css';
import './CreateUserModal.css';

interface Props {
  onClose: () => void;
}

const ROLES = [
  { id: 1, name: 'Administrador' },
  { id: 2, name: 'Supervisor' },
  { id: 3, name: 'Empleado' },
];

export const EditUserModal: React.FC<Props> = ({ onClose }) => {
  const accessToken = useAuthStore(state => state.accessToken);
  const [ci, setCi] = useState('');
  const [idUserRol, setIdUserRol] = useState<number | ''>('');
  const [enabled, setEnabled] = useState<boolean | ''>('');
  const [loading, setLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const token = accessToken ?? sessionStorage.getItem('accessToken');
      const payload: { ci: string; idUserRol?: number; enabled?: boolean } = { ci };
      if (idUserRol !== '') payload.idUserRol = idUserRol;
      if (enabled !== '') payload.enabled = enabled;

      const response = await api.put<ApiResponse<boolean>>(
        '/users',
        payload,
        token ? { headers: { Authorization: `Bearer ${token}` } } : undefined
      );
      setSuccessMessage(response.data.message);
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Ocurrió un error al actualizar el usuario.');
    } finally {
      setLoading(false);
    }
  };

  if (successMessage) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-box" onClick={(e) => e.stopPropagation()}>
          <button className="modal-close" onClick={onClose}>✕</button>
          <div className="modal-success">
            <div className="modal-success-icon">✓</div>
            <h3>Usuario actualizado</h3>
            <p>{successMessage}</p>
            <button className="modal-btn-primary" onClick={onClose}>
              Aceptar
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>

        <h3>Editar usuario</h3>
        <p>Ingresa la cédula del usuario y los campos que deseas modificar.</p>

        <form onSubmit={handleSubmit}>
          <div className="modal-field">
            <label>Cédula</label>
            <input
              type="text"
              placeholder="Ej. 12345678"
              value={ci}
              onChange={(e) => setCi(e.target.value)}
              required
              autoFocus
            />
          </div>

          <div className="modal-field">
            <label>Rol</label>
            <select
              value={idUserRol}
              onChange={(e) => setIdUserRol(e.target.value === '' ? '' : Number(e.target.value))}
            >
              <option value="">-- Sin cambio --</option>
              {ROLES.map((r) => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
          </div>

          <div className="modal-field">
            <label>Estado</label>
            <select
              value={enabled === '' ? '' : String(enabled)}
              onChange={(e) => setEnabled(e.target.value === '' ? '' : e.target.value === 'true')}
            >
              <option value="">-- Sin cambio --</option>
              <option value="true">Activo</option>
              <option value="false">Inactivo</option>
            </select>
          </div>

          {error && <p className="modal-error">{error}</p>}

          <div className="modal-actions">
            <button type="button" className="modal-btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="modal-btn-primary" disabled={loading}>
              {loading ? 'Guardando...' : 'Guardar cambios'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
