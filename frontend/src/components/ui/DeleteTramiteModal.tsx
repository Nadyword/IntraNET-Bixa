import React, { useState } from 'react';
import api from '../../lib/api';
import type { ApiResponse } from '../../services/authService';
import './ForgotPasswordModal.css';

interface Props {
  onClose: () => void;
}

interface TramiteResumen {
  id: number;
  tipoTramiteNombre: string;
  userCi: string;
  userNombre: string | null;
  fechaSolicitud: string;
  estado: number;
}

// Valores de EstadoTramiteEnum en el backend
const ESTADO_LABEL: Record<number, string> = {
  1: 'Creado',
  2: 'En Revisión',
  3: 'Firmado',
  4: 'Aprobado',
  5: 'Tramitando',
  6: 'Rechazado',
};

const getErrorMessage = (err: unknown, fallback: string): string => {
  const axiosError = err as { response?: { data?: { message?: string } } };
  return axiosError?.response?.data?.message ?? fallback;
};

export const DeleteTramiteModal: React.FC<Props> = ({ onClose }) => {
  const [tramiteId, setTramiteId] = useState('');
  const [tramite, setTramite] = useState<TramiteResumen | null>(null);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleBuscar = async (e: React.FormEvent) => {
    e.preventDefault();
    const id = parseInt(tramiteId, 10);
    if (!Number.isFinite(id) || id <= 0) {
      setError('Ingresa un ID de trámite válido.');
      return;
    }
    setError('');
    setLoading(true);
    try {
      const res = await api.get<ApiResponse<TramiteResumen>>(`/solicitudes/Detalle/${id}`);
      setTramite(res.data.data);
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'No se encontró el trámite.'));
    } finally {
      setLoading(false);
    }
  };

  const handleEliminar = async () => {
    if (!tramite) return;
    setError('');
    setLoading(true);
    try {
      await api.delete<ApiResponse<boolean>>(`/solicitudes/Eliminar/${tramite.id}`);
      setSuccess(true);
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'Ocurrió un error al eliminar el trámite.'));
    } finally {
      setLoading(false);
    }
  };

  if (success && tramite) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-box" onClick={(e) => e.stopPropagation()}>
          <button className="modal-close" onClick={onClose}>✕</button>
          <div className="modal-success">
            <div className="modal-success-icon">✓</div>
            <h3>Trámite eliminado</h3>
            <p>El trámite #{tramite.id} y toda su información asociada fueron eliminados.</p>
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

        <h3>Eliminar trámite</h3>

        {!tramite ? (
          <>
            <p>Ingresa el ID del trámite que deseas eliminar. Podrás revisar sus datos antes de confirmar.</p>

            <form onSubmit={handleBuscar}>
              <div className="modal-field">
                <label>ID del trámite</label>
                <input
                  type="number"
                  min={1}
                  step={1}
                  placeholder="Ej. 123"
                  value={tramiteId}
                  onChange={(e) => setTramiteId(e.target.value)}
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
                  {loading ? 'Buscando...' : 'Buscar trámite'}
                </button>
              </div>
            </form>
          </>
        ) : (
          <>
            <p>
              Se eliminará <strong>definitivamente</strong> este trámite junto con sus aprobaciones,
              notificaciones y archivo adjunto. Esta acción no se puede deshacer.
            </p>

            <div className="modal-field">
              <label>Trámite</label>
              <p>#{tramite.id} · {tramite.tipoTramiteNombre}</p>
            </div>
            <div className="modal-field">
              <label>Solicitante</label>
              <p>{tramite.userNombre ? `${tramite.userNombre} (${tramite.userCi})` : tramite.userCi}</p>
            </div>
            <div className="modal-field">
              <label>Fecha de solicitud · Estado</label>
              <p>
                {new Date(tramite.fechaSolicitud).toLocaleDateString('es-VE')} · {ESTADO_LABEL[tramite.estado] ?? tramite.estado}
              </p>
            </div>

            {error && <p className="modal-error">{error}</p>}

            <div className="modal-actions">
              <button
                type="button"
                className="modal-btn-secondary"
                disabled={loading}
                onClick={() => { setTramite(null); setError(''); }}
              >
                Volver
              </button>
              <button type="button" className="modal-btn-primary" onClick={handleEliminar} disabled={loading}>
                {loading ? 'Eliminando...' : 'Sí, eliminar trámite'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};
