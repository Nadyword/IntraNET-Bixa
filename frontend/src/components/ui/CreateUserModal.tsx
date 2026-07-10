import React, { useState } from 'react';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';
import { validateFirmaFile } from '../../lib/firmaValidation';
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

interface CreatedUserInfo {
  message: string;
  userId: number;
}

export const CreateUserModal: React.FC<Props> = ({ onClose }) => {
  const accessToken = useAuthStore(state => state.accessToken);
  const [ci, setCi] = useState('');
  const [idUserRol, setIdUserRol] = useState<number>(3);
  const [firma, setFirma] = useState<File | null>(null);
  const [firmaError, setFirmaError] = useState('');
  const [loading, setLoading] = useState(false);
  const [created, setCreated] = useState<CreatedUserInfo | null>(null);
  const [error, setError] = useState('');

  const handleFirmaChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setFirmaError('');
    setFirma(null);
    if (!file) return;

    const validationError = await validateFirmaFile(file);
    if (validationError) {
      setFirmaError(validationError);
      e.target.value = '';
      return;
    }
    setFirma(file);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const token = accessToken ?? sessionStorage.getItem('accessToken');
      const formData = new FormData();
      formData.append('ci', ci);
      formData.append('idUserRol', String(idUserRol));
      formData.append('firstName', '');
      formData.append('lastName', '');
      formData.append('passwordHash', '');
      if (firma) formData.append('firma', firma);

      const response = await api.post<ApiResponse<number>>(
        '/users',
        formData,
        token ? { headers: { Authorization: `Bearer ${token}` } } : undefined
      );
      setCreated({ message: response.data.message, userId: response.data.data });
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Ocurrió un error al crear el usuario.');
    } finally {
      setLoading(false);
    }
  };

  if (created) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-box" onClick={(e) => e.stopPropagation()}>
          <button className="modal-close" onClick={onClose}>✕</button>
          <div className="modal-success">
            <div className="modal-success-icon">✓</div>
            <h3>Usuario creado</h3>
            <p>{created.message}</p>
            {created.userId > 0 && (
              <p className="create-user-id">ID asignado: <strong>#{created.userId}</strong></p>
            )}
            <button className="modal-btn-primary" onClick={() => window.location.reload()}>
              Aceptar y recargar
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

        <h3>Crear usuario</h3>
        <p>Ingresa la cédula y el rol del nuevo usuario. Los datos del empleado se obtendrán automáticamente.</p>

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
              onChange={(e) => setIdUserRol(Number(e.target.value))}
              required
            >
              {ROLES.map((r) => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
          </div>

          <div className="modal-field">
            <label>Foto de firma (opcional)</label>
            <input
              type="file"
              accept="image/png"
              onChange={handleFirmaChange}
            />
            <p className="modal-hint">PNG con proporción 2:1 (ej. 450x225). Si no se sube ninguna, se usa la firma por defecto.</p>
            {firmaError && <p className="modal-error">{firmaError}</p>}
          </div>

          {error && <p className="modal-error">{error}</p>}

          <div className="modal-actions">
            <button type="button" className="modal-btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="modal-btn-primary" disabled={loading}>
              {loading ? 'Creando...' : 'Crear usuario'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
