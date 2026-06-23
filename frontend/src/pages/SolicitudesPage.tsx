import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import api from '../lib/api';
import './SolicitudesPage.css';

// ─── Types ────────────────────────────────────────────────────────────────────

interface PorAprobarAPI {
  tramiteId: number;
  aprobadorCi: string;
  orden: number;
  comentario?: string;
  tipoTramiteId: number;
  firstName?: string;
  lastName?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
}

type TabType = 'porAprobar' | 'enEspera';

interface AccionModal {
  item: PorAprobarAPI;
  accion: 'aprobar' | 'rechazar';
}

// ─── Constants ────────────────────────────────────────────────────────────────

const TIPO_TRAMITE_INFO: Record<number, { nombre: string; icon: string }> = {
  1: { nombre: 'Anticipo de Utilidades',  icon: '💰' },
  2: { nombre: 'Prestaciones Sociales',   icon: '📋' },
  3: { nombre: 'Préstamo Prestaciones',   icon: '🏦' },
  4: { nombre: 'Vacaciones',              icon: '✈️' },
  5: { nombre: 'Día Especial',            icon: '📅' },
};

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchPorAprobar = async (ci: string): Promise<PorAprobarAPI[]> => {
  const res = await api.get<ApiResponse<PorAprobarAPI[]>>(`/solicitudes/PorAprobarByCi/${ci}`);
  return res.data.data;
};

interface AprobarTramiteDTO {
  tramiteId: number;
  ci: string;
  estado: number;
  motivo?: string;
}

const aprobarTramite = async (dto: AprobarTramiteDTO): Promise<void> => {
  await api.put('/solicitudes/Aprobar', dto);
};

// ─── Card ─────────────────────────────────────────────────────────────────────

interface SolicitudCardProps {
  item: PorAprobarAPI;
  tipo: TabType;
  onAprobar?: () => void;
  onRechazar?: () => void;
}

const SolicitudCard: React.FC<SolicitudCardProps> = ({ item, tipo, onAprobar, onRechazar }) => {
  const info = TIPO_TRAMITE_INFO[item.tipoTramiteId] ?? { nombre: 'Trámite', icon: '📄' };
  const nombreEmpleado = [item.firstName, item.lastName].filter(Boolean).join(' ') || '—';

  return (
    <div className={`sol-card ${tipo === 'enEspera' ? 'sol-card--espera' : ''}`}>
      <div className="sol-card-header">
        <div className="sol-card-tipo">
          <span className="sol-tipo-icon">{info.icon}</span>
          <span className="sol-tipo-label">{info.nombre}</span>
        </div>
        <span className={`status-badge ${tipo === 'porAprobar' ? 'badge-revision' : 'badge-espera'}`}>
          {tipo === 'porAprobar' ? 'En Revisión' : 'En Espera'}
        </span>
      </div>

      <div className="sol-card-body">
        <p className="sol-empleado">{nombreEmpleado}</p>
        <p className="sol-ci">Trámite #{item.tramiteId}</p>
        {item.comentario && (
          <p className="sol-detalle">"{item.comentario}"</p>
        )}
      </div>

      {tipo === 'porAprobar' ? (
        <div className="sol-card-actions">
          <button className="btn-reject" onClick={onRechazar}>✕ Rechazar</button>
          <button className="btn-approve" onClick={onAprobar}>✓ Aprobar</button>
        </div>
      ) : (
        <div className="sol-card-waiting">
          <span className="waiting-icon">⏳</span>
          <span className="waiting-text">Pendiente de aprobación previa (paso {item.orden})</span>
        </div>
      )}
    </div>
  );
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export const SolicitudesPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabType>('porAprobar');
  const [accionModal, setAccionModal] = useState<AccionModal | null>(null);
  const [comentario, setComentario] = useState('');
  const [comentarioError, setComentarioError] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const user = useAuthStore(state => state.user);
  const queryClient = useQueryClient();

  const ci = user?.ci ?? '';

  const { data: items = [], isLoading, isError } = useQuery({
    queryKey: ['porAprobar', ci],
    queryFn: () => fetchPorAprobar(ci),
    enabled: ci.length > 0,
    retry: false,
  });

  const mutation = useMutation({
    mutationFn: aprobarTramite,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['porAprobar', ci] });
      setAccionModal(null);
      setComentario('');
      setApiError(null);
    },
    onError: () => {
      setApiError('Ocurrió un error al procesar la solicitud. Inténtalo de nuevo.');
    },
  });

  const porAprobar = items.filter(i => i.orden === 1);
  const enEspera   = items.filter(i => i.orden > 1);

  const handleAccion = (item: PorAprobarAPI, accion: 'aprobar' | 'rechazar') => {
    setAccionModal({ item, accion });
    setComentario('');
    setComentarioError(false);
    setApiError(null);
  };

  const handleConfirmar = () => {
    if (!accionModal) return;

    if (accionModal.accion === 'rechazar' && !comentario.trim()) {
      setComentarioError(true);
      return;
    }

    mutation.mutate({
      tramiteId: accionModal.item.tramiteId,
      ci,
      estado: accionModal.accion === 'aprobar' ? 2 : 3,
      motivo: comentario.trim() || undefined,
    });
  };

  const handleCloseModal = () => {
    if (mutation.isPending) return;
    setAccionModal(null);
    setComentario('');
    setComentarioError(false);
    setApiError(null);
  };

  return (
    <div className="solicitudes-page">
      <div className="solicitudes-header">
        <h1>Solicitudes</h1>
      </div>

      <div className="sol-tabs">
        <button
          className={`sol-tab ${activeTab === 'porAprobar' ? 'active' : ''}`}
          onClick={() => setActiveTab('porAprobar')}
        >
          Por Aprobar
          {!isLoading && (
            <span className="tab-count">{porAprobar.length}</span>
          )}
        </button>
        <button
          className={`sol-tab ${activeTab === 'enEspera' ? 'active' : ''}`}
          onClick={() => setActiveTab('enEspera')}
        >
          En Espera
          {!isLoading && (
            <span className="tab-count muted">{enEspera.length}</span>
          )}
        </button>
      </div>

      {isLoading && (
        <div className="sol-empty">
          <span className="sol-empty-icon">⏳</span>
          <p>Cargando solicitudes...</p>
        </div>
      )}

      {isError && (
        <div className="sol-empty">
          <span className="sol-empty-icon">⚠️</span>
          <p>Error al cargar las solicitudes.</p>
        </div>
      )}

      {!isLoading && !isError && (
        <>
          {activeTab === 'porAprobar' && (
            <div className="sol-section">
              <p className="sol-section-desc">
                Solicitudes asignadas a ti que están listas para tu revisión.
              </p>
              {porAprobar.length === 0 ? (
                <div className="sol-empty">
                  <span className="sol-empty-icon">✅</span>
                  <p>No tienes solicitudes pendientes de aprobación.</p>
                </div>
              ) : (
                <div className="sol-grid">
                  {porAprobar.map(item => (
                    <SolicitudCard
                      key={item.tramiteId}
                      item={item}
                      tipo="porAprobar"
                      onAprobar={() => handleAccion(item, 'aprobar')}
                      onRechazar={() => handleAccion(item, 'rechazar')}
                    />
                  ))}
                </div>
              )}
            </div>
          )}

          {activeTab === 'enEspera' && (
            <div className="sol-section">
              <p className="sol-section-desc">
                Solicitudes en las que deberás actuar, pero primero requieren la aprobación de un paso anterior.
              </p>
              {enEspera.length === 0 ? (
                <div className="sol-empty">
                  <span className="sol-empty-icon">📭</span>
                  <p>No hay solicitudes en espera.</p>
                </div>
              ) : (
                <div className="sol-grid">
                  {enEspera.map(item => (
                    <SolicitudCard key={item.tramiteId} item={item} tipo="enEspera" />
                  ))}
                </div>
              )}
            </div>
          )}
        </>
      )}

      {/* Modal aprobar / rechazar */}
      {accionModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <div className={`modal-header ${accionModal.accion === 'rechazar' ? 'modal-header--danger' : 'modal-header--success'}`}>
              <h2>
                {accionModal.accion === 'aprobar' ? '✓ Aprobar Solicitud' : '✕ Rechazar Solicitud'}
              </h2>
              <button className="close-btn" onClick={handleCloseModal}>✕</button>
            </div>

            <div className="modal-body">
              <div className="modal-sol-info">
                <span className="modal-sol-icon">
                  {TIPO_TRAMITE_INFO[accionModal.item.tipoTramiteId]?.icon ?? '📄'}
                </span>
                <div>
                  <p className="modal-sol-nombre">
                    {[accionModal.item.firstName, accionModal.item.lastName].filter(Boolean).join(' ') || '—'}
                  </p>
                  <p className="modal-sol-detalle">
                    {TIPO_TRAMITE_INFO[accionModal.item.tipoTramiteId]?.nombre ?? 'Trámite'} · #{accionModal.item.tramiteId}
                  </p>
                </div>
              </div>

              <div className="form-group">
                <label>
                  {accionModal.accion === 'rechazar' ? (
                    <>Motivo de rechazo <span className="required">*</span></>
                  ) : (
                    <>Comentario <span className="optional">(opcional)</span></>
                  )}
                </label>
                <textarea
                  className={`form-input ${comentarioError ? 'input-error' : ''}`}
                  rows={4}
                  placeholder={
                    accionModal.accion === 'aprobar'
                      ? 'Escribe un comentario sobre la aprobación...'
                      : 'Indica el motivo del rechazo...'
                  }
                  value={comentario}
                  onChange={e => {
                    setComentario(e.target.value);
                    if (e.target.value.trim()) setComentarioError(false);
                  }}
                  disabled={mutation.isPending}
                />
                {comentarioError && (
                  <span className="error-msg">El motivo de rechazo es obligatorio.</span>
                )}
              </div>

              {apiError && (
                <p className="error-msg" style={{ marginBottom: '0.75rem' }}>{apiError}</p>
              )}

              <div className="form-actions">
                <button className="btn-secondary" onClick={handleCloseModal} disabled={mutation.isPending}>
                  Cancelar
                </button>
                <button
                  className={accionModal.accion === 'aprobar' ? 'btn-success' : 'btn-danger'}
                  onClick={handleConfirmar}
                  disabled={mutation.isPending}
                >
                  {mutation.isPending
                    ? 'Procesando...'
                    : accionModal.accion === 'aprobar'
                      ? 'Confirmar Aprobación'
                      : 'Confirmar Rechazo'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
