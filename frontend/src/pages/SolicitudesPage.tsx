import React, { useState } from 'react';
import './SolicitudesPage.css';

type TabType = 'porAprobar' | 'enEspera';

interface SolicitudPendiente {
  id: number;
  empleadoNombre: string;
  empleadoCi: string;
  tipoTramite: string;
  tipoIcon: string;
  fechaSolicitud: string;
  detalle: string;
}

interface AccionModal {
  solicitud: SolicitudPendiente;
  accion: 'aprobar' | 'rechazar';
}

const SOLICITUDES_POR_APROBAR: SolicitudPendiente[] = [
  {
    id: 1,
    empleadoNombre: 'Juan Pérez',
    empleadoCi: '12.345.678',
    tipoTramite: 'Vacaciones',
    tipoIcon: '✈️',
    fechaSolicitud: '01/05/2026',
    detalle: '15 al 25 de Mayo · 10 días',
  },
  {
    id: 2,
    empleadoNombre: 'María García',
    empleadoCi: '9.876.543',
    tipoTramite: 'Día Especial',
    tipoIcon: '📝',
    fechaSolicitud: '18/04/2026',
    detalle: '30 de Abril · Cita médica',
  },
  {
    id: 3,
    empleadoNombre: 'Carlos Rodríguez',
    empleadoCi: '15.234.567',
    tipoTramite: 'Préstamo Utilidades',
    tipoIcon: '💳',
    fechaSolicitud: '10/04/2026',
    detalle: 'Monto solicitado: $800',
  },
];

const SOLICITUDES_EN_ESPERA: SolicitudPendiente[] = [
  {
    id: 4,
    empleadoNombre: 'Ana Martínez',
    empleadoCi: '8.765.432',
    tipoTramite: 'Vacaciones',
    tipoIcon: '✈️',
    fechaSolicitud: '05/05/2026',
    detalle: '01 al 15 de Junio · 15 días',
  },
  {
    id: 5,
    empleadoNombre: 'Luis Torres',
    empleadoCi: '11.223.344',
    tipoTramite: 'Prestaciones Sociales',
    tipoIcon: '📋',
    fechaSolicitud: '28/04/2026',
    detalle: 'Liquidación parcial de prestaciones',
  },
];

interface SolicitudCardProps {
  solicitud: SolicitudPendiente;
  tipo: TabType;
  onAprobar?: () => void;
  onRechazar?: () => void;
}

const SolicitudCard: React.FC<SolicitudCardProps> = ({ solicitud, tipo, onAprobar, onRechazar }) => (
  <div className={`sol-card ${tipo === 'enEspera' ? 'sol-card--espera' : ''}`}>
    <div className="sol-card-header">
      <div className="sol-card-tipo">
        <span className="sol-tipo-icon">{solicitud.tipoIcon}</span>
        <span className="sol-tipo-label">{solicitud.tipoTramite}</span>
      </div>
      <span className={`status-badge ${tipo === 'porAprobar' ? 'badge-revision' : 'badge-espera'}`}>
        {tipo === 'porAprobar' ? 'En Revisión' : 'En Espera'}
      </span>
    </div>

    <div className="sol-card-body">
      <p className="sol-empleado">{solicitud.empleadoNombre}</p>
      <p className="sol-ci">CI: {solicitud.empleadoCi}</p>
      <p className="sol-detalle">{solicitud.detalle}</p>
      <p className="sol-fecha">Solicitado: {solicitud.fechaSolicitud}</p>
    </div>

    {tipo === 'porAprobar' ? (
      <div className="sol-card-actions">
        <button className="btn-reject" onClick={onRechazar}>✕ Rechazar</button>
        <button className="btn-approve" onClick={onAprobar}>✓ Aprobar</button>
      </div>
    ) : (
      <div className="sol-card-waiting">
        <span className="waiting-icon">⏳</span>
        <span className="waiting-text">Pendiente de aprobación previa</span>
      </div>
    )}
  </div>
);

export const SolicitudesPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabType>('porAprobar');
  const [accionModal, setAccionModal] = useState<AccionModal | null>(null);
  const [comentario, setComentario] = useState('');
  const [comentarioError, setComentarioError] = useState(false);

  const handleAccion = (solicitud: SolicitudPendiente, accion: 'aprobar' | 'rechazar') => {
    setAccionModal({ solicitud, accion });
    setComentario('');
    setComentarioError(false);
  };

  const handleConfirmar = () => {
    if (!comentario.trim()) {
      setComentarioError(true);
      return;
    }
    // TODO: conectar con backend
    console.log({ accion: accionModal?.accion, id: accionModal?.solicitud.id, comentario });
    setAccionModal(null);
    setComentario('');
  };

  const handleCloseModal = () => {
    setAccionModal(null);
    setComentario('');
    setComentarioError(false);
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
          <span className="tab-count">{SOLICITUDES_POR_APROBAR.length}</span>
        </button>
        <button
          className={`sol-tab ${activeTab === 'enEspera' ? 'active' : ''}`}
          onClick={() => setActiveTab('enEspera')}
        >
          En Espera
          <span className="tab-count muted">{SOLICITUDES_EN_ESPERA.length}</span>
        </button>
      </div>

      {activeTab === 'porAprobar' && (
        <div className="sol-section">
          <p className="sol-section-desc">
            Solicitudes asignadas a ti que están listas para tu revisión.
          </p>
          {SOLICITUDES_POR_APROBAR.length === 0 ? (
            <div className="sol-empty">
              <span className="sol-empty-icon">✅</span>
              <p>No tienes solicitudes pendientes de aprobación.</p>
            </div>
          ) : (
            <div className="sol-grid">
              {SOLICITUDES_POR_APROBAR.map((sol) => (
                <SolicitudCard
                  key={sol.id}
                  solicitud={sol}
                  tipo="porAprobar"
                  onAprobar={() => handleAccion(sol, 'aprobar')}
                  onRechazar={() => handleAccion(sol, 'rechazar')}
                />
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'enEspera' && (
        <div className="sol-section">
          <p className="sol-section-desc">
            Solicitudes en las que deberás actuar, pero primero requieren aprobación de otro paso.
          </p>
          {SOLICITUDES_EN_ESPERA.length === 0 ? (
            <div className="sol-empty">
              <span className="sol-empty-icon">📭</span>
              <p>No hay solicitudes en espera.</p>
            </div>
          ) : (
            <div className="sol-grid">
              {SOLICITUDES_EN_ESPERA.map((sol) => (
                <SolicitudCard key={sol.id} solicitud={sol} tipo="enEspera" />
              ))}
            </div>
          )}
        </div>
      )}

      {accionModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className={`modal-header ${accionModal.accion === 'rechazar' ? 'modal-header--danger' : 'modal-header--success'}`}>
              <h2>
                {accionModal.accion === 'aprobar' ? '✓ Aprobar Solicitud' : '✕ Rechazar Solicitud'}
              </h2>
              <button className="close-btn" onClick={handleCloseModal}>✕</button>
            </div>

            <div className="modal-body">
              <div className="modal-sol-info">
                <span className="modal-sol-icon">{accionModal.solicitud.tipoIcon}</span>
                <div>
                  <p className="modal-sol-nombre">{accionModal.solicitud.empleadoNombre}</p>
                  <p className="modal-sol-detalle">
                    {accionModal.solicitud.tipoTramite} · {accionModal.solicitud.detalle}
                  </p>
                  <p className="modal-sol-fecha">
                    Solicitado el {accionModal.solicitud.fechaSolicitud}
                  </p>
                </div>
              </div>

              <div className="form-group">
                <label>
                  Comentario <span className="required">*</span>
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
                  onChange={(e) => {
                    setComentario(e.target.value);
                    if (e.target.value.trim()) setComentarioError(false);
                  }}
                />
                {comentarioError && (
                  <span className="error-msg">El comentario es obligatorio.</span>
                )}
              </div>

              <div className="form-actions">
                <button className="btn-secondary" onClick={handleCloseModal}>
                  Cancelar
                </button>
                <button
                  className={accionModal.accion === 'aprobar' ? 'btn-success' : 'btn-danger'}
                  onClick={handleConfirmar}
                >
                  {accionModal.accion === 'aprobar' ? 'Confirmar Aprobación' : 'Confirmar Rechazo'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
