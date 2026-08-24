import React, { useState } from 'react';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';
import type { ApiResponse } from '../../services/authService';
import './ForgotPasswordModal.css';

interface Props {
  onClose: () => void;
}

export const DeleteUserModal: React.FC<Props> = ({ onClose }) => {
  const accessToken = useAuthStore(state => state.accessToken);
  const [ci, setCi] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const token = accessToken ?? sessionStorage.getItem('accessToken');
      const response = await api.delete<ApiResponse<boolean>>(
        `/users/${ci}`,
        token ? { headers: { Authorization: `Bearer ${token}` } } : undefined
      );
      setSuccess(response.data.message);
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Ocurrió un error al desactivar el usuario.');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-box" onClick={(e) => e.stopPropagation()}>
          <button className="modal-close" onClick={onClose}>✕</button>
          <div className="modal-success">
            <div className="modal-success-icon">✓</div>
            <h3>Usuario desactivado</h3>
            <p>{success}</p>
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

        <h3>Desactivar usuario</h3>
        <p>Ingresa la cédula del usuario que deseas desactivar. El usuario dejará de aparecer en los listados y no podrá iniciar sesión, pero podrás reactivarlo creándolo nuevamente.</p>

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

          {error && <p className="modal-error">{error}</p>}

          <div className="modal-actions">
            <button type="button" className="modal-btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="modal-btn-primary" disabled={loading}>
              {loading ? 'Desactivando...' : 'Desactivar usuario'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
