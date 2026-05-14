import React, { useState, useEffect } from 'react';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';
import type { ApiResponse } from '../../services/authService';
import './ForgotPasswordModal.css';
import './CreateUserModal.css';

interface Props {
  onClose: () => void;
}

export const ChangePasswordModal: React.FC<Props> = ({ onClose }) => {
  const accessToken = useAuthStore(state => state.accessToken);
  const user = useAuthStore(state => state.user);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('La nueva clave y su confirmación no coinciden.');
      return;
    }

    setLoading(true);
    try {
      const token = accessToken ?? sessionStorage.getItem('accessToken');
      const response = await api.put<ApiResponse<boolean>>(
        `/users/ChangePassword/${encodeURIComponent(newPassword)}`,
        { ci: user?.ci ?? '', password: currentPassword },
        token ? { headers: { Authorization: `Bearer ${token}` } } : undefined
      );
      setSuccessMessage(response.data.message);
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Ocurrió un error al cambiar la clave.');
    } finally {
      setLoading(false);
    }
  };

  if (successMessage) {
    return (
      <div className="modal-overlay" onMouseDown={onClose}>
        <div className="modal-box" onMouseDown={(e) => e.stopPropagation()}>
          <button className="modal-close" onClick={onClose}>✕</button>
          <div className="modal-success">
            <div className="modal-success-icon">✓</div>
            <h3>Clave actualizada</h3>
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
    <div className="modal-overlay" onMouseDown={onClose}>
      <div className="modal-box" onMouseDown={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>

        <h3>Cambiar clave</h3>
        <p>Ingresa tu clave actual y luego la nueva clave.</p>

        <form onSubmit={handleSubmit}>
          <div className="modal-field">
            <label>Clave actual</label>
            <input
              type="password"
              placeholder="Clave actual"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              required
              autoFocus
            />
          </div>

          <div className="modal-field">
            <label>Nueva clave</label>
            <input
              type="password"
              placeholder="Mínimo 8 caracteres"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>

          <div className="modal-field">
            <label>Repetir nueva clave</label>
            <input
              type="password"
              placeholder="Repite la nueva clave"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>

          {error && <p className="modal-error">{error}</p>}

          <div className="modal-actions">
            <button type="button" className="modal-btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="modal-btn-primary" disabled={loading}>
              {loading ? 'Guardando...' : 'Cambiar clave'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
