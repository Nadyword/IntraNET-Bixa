import React, { useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '../store/authStore';
import api, { API_ORIGIN } from '../lib/api';
import { ButtonSpinner } from '../components/ui/ButtonSpinner';
import { HistorialAprobaciones } from '../components/ui/HistorialAprobaciones';
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

type TabType = 'porAprobar' | 'enEspera' | 'historial';

interface AccionModal {
  item: PorAprobarAPI;
  accion: 'aprobar' | 'rechazar';
}

interface VacacionesDetalle {
  desde: string;
  hasta: string;
  diasTotales: number;
  observaciones?: string;
}

interface DiaEspecialDetalle {
  fecha: string;
  motivo: string;
}

interface UtilidadesDetalle {
  monto: number;
  motivo: string;
}

interface PrestacionesDetalle {
  esPrestamo: boolean;
  monto: number;
  destino: string;
  observaciones?: string;
  cuotas?: number;
  montoCuota?: number;
  archivoAdjuntoUrl?: string;
}

interface TramiteDetalleDTO {
  id: number;
  tipoTramiteId: number;
  tipoTramiteNombre: string;
  userCi: string;
  userNombre?: string;
  fechaSolicitud: string;
  updatedAt: string;
  estado: number;
  motivoRechazo?: string;
  vacaciones?: VacacionesDetalle;
  diaEspecial?: DiaEspecialDetalle;
  utilidades?: UtilidadesDetalle;
  prestaciones?: PrestacionesDetalle;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const TIPO_TRAMITE_INFO: Record<number, { nombre: string; icon: string }> = {
  1: { nombre: 'Anticipo de Utilidades',  icon: '💰' },
  2: { nombre: 'Prestaciones Sociales',   icon: '📋' },
  3: { nombre: 'Préstamo Prestaciones',   icon: '🏦' },
  4: { nombre: 'Vacaciones',              icon: '✈️' },
  5: { nombre: 'Día Especial',            icon: '📅' },
};

const ESTADO_LABEL: Record<number, { label: string; className: string }> = {
  1: { label: 'Creado',      className: 'sol-detalle-badge--creado'     },
  2: { label: 'En Revisión', className: 'sol-detalle-badge--revision'   },
  3: { label: 'Firmado',     className: 'sol-detalle-badge--firmado'    },
  4: { label: 'Aprobado',    className: 'sol-detalle-badge--aprobado'   },
  5: { label: 'Tramitando',  className: 'sol-detalle-badge--tramitando' },
  6: { label: 'Finalizado',  className: 'sol-detalle-badge--finalizado' },
  7: { label: 'Rechazado',   className: 'sol-detalle-badge--rechazado'  },
};

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchPorAprobar = async (ci: string): Promise<PorAprobarAPI[]> => {
  const res = await api.get<ApiResponse<PorAprobarAPI[]>>(`/solicitudes/PorAprobarByCi/${ci}`);
  return res.data.data;
};

const fetchTramiteDetalle = async (tramiteId: number): Promise<TramiteDetalleDTO> => {
  const res = await api.get<ApiResponse<TramiteDetalleDTO>>(`/solicitudes/Detalle/${tramiteId}`);
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

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('es-VE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

// ─── Card ─────────────────────────────────────────────────────────────────────

interface SolicitudCardProps {
  item: PorAprobarAPI;
  tipo: TabType;
  onAprobar?: () => void;
  onRechazar?: () => void;
  onVer: () => void;
}

const SolicitudCard: React.FC<SolicitudCardProps> = ({ item, tipo, onAprobar, onRechazar, onVer }) => {
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
        <button className="sol-btn-ver" onClick={onVer}>👁 Ver información</button>
      </div>

      {tipo === 'porAprobar' ? (
        <div className="sol-card-actions" style={{ margin: '3%'}}>
          {item.tipoTramiteId !== 6 && (
            <button className="btn-reject" onClick={onRechazar}>✕ Rechazar</button>
          )}
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
  const [verTramiteId, setVerTramiteId] = useState<number | null>(null);

  const user = useAuthStore(state => state.user);
  const queryClient = useQueryClient();

  const ci = user?.ci ?? '';

  const { data: items = [], isLoading, isError } = useQuery({
    queryKey: ['porAprobar', ci],
    queryFn: () => fetchPorAprobar(ci),
    enabled: ci.length > 0,
    retry: false,
  });

  const { data: detalle, isLoading: detalleLoading, isError: detalleError } = useQuery({
    queryKey: ['tramiteDetalle', verTramiteId],
    queryFn: () => fetchTramiteDetalle(verTramiteId as number),
    enabled: verTramiteId !== null,
    retry: false,
  });

  const mutation = useMutation({
    mutationFn: aprobarTramite,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['porAprobar', ci] });
      queryClient.invalidateQueries({ queryKey: ['historialAprobaciones'] });
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
        <button
          className={`sol-tab ${activeTab === 'historial' ? 'active' : ''}`}
          onClick={() => setActiveTab('historial')}
        >
          Historial
        </button>
      </div>

      {activeTab === 'historial' && (
        <div className="sol-section">
          <p className="sol-section-desc">
            Solicitudes que ya aprobaste o rechazaste, de la más reciente a la más antigua.
          </p>
          <HistorialAprobaciones scope="propio" />
        </div>
      )}

      {activeTab !== 'historial' && isLoading && (
        <div className="sol-empty">
          <span className="sol-empty-icon">⏳</span>
          <p>Cargando solicitudes...</p>
        </div>
      )}

      {activeTab !== 'historial' && isError && (
        <div className="sol-empty">
          <span className="sol-empty-icon">⚠️</span>
          <p>Error al cargar las solicitudes.</p>
        </div>
      )}

      {activeTab !== 'historial' && !isLoading && !isError && (
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
                <div className="sol-grid sol-grid--large-btns"  >
                  {porAprobar.map(item => (
                    <div key={item.tramiteId} className="solicitud-card-wrapper solicitud-card-wrapper--big-actions">
                      <SolicitudCard
                        item={item}
                        tipo="porAprobar"
                        onAprobar={() => handleAccion(item, 'aprobar')}
                        onRechazar={() => handleAccion(item, 'rechazar')}
                        onVer={() => setVerTramiteId(item.tramiteId)}
                        />
                    </div>
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
                    <SolicitudCard
                      key={item.tramiteId}
                      item={item}
                      tipo="enEspera"
                      onVer={() => setVerTramiteId(item.tramiteId)}
                    />
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

              {accionModal.accion === 'rechazar' && (
                <div className="form-group">
                  <label>
                    Motivo de rechazo <span className="required">*</span>
                  </label>
                  <textarea
                    className={`form-input ${comentarioError ? 'input-error' : ''}`}
                    rows={4}
                    placeholder="Indica el motivo del rechazo..."
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
              )}
        

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
                    ? <><ButtonSpinner /> Procesando...</>
                    : accionModal.accion === 'aprobar'
                      ? 'Confirmar Aprobación'
                      : 'Confirmar Rechazo'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal ver información de la solicitud */}
      {verTramiteId !== null && createPortal(
        <div className="sol-detalle-overlay" onClick={() => setVerTramiteId(null)}>
          <div className="sol-detalle-modal" onClick={e => e.stopPropagation()}>
            {detalleLoading && (
              <div className="sol-detalle-status">
                <span className="sol-empty-icon">⏳</span>
                <p>Cargando información...</p>
              </div>
            )}

            {detalleError && (
              <div className="sol-detalle-status">
                <span className="sol-empty-icon">⚠️</span>
                <p>No se pudo cargar la información del trámite.</p>
              </div>
            )}

            {detalle && (
              <>
                <div className="sol-detalle-header">
                  <div className="sol-detalle-header-info">
                    <div className="sol-detalle-header-icon-wrap">
                      {TIPO_TRAMITE_INFO[detalle.tipoTramiteId]?.icon ?? '📄'}
                    </div>
                    <div>
                      <h2>{detalle.tipoTramiteNombre}</h2>
                      <span className={`sol-detalle-badge ${ESTADO_LABEL[detalle.estado]?.className ?? ''}`}>
                        {ESTADO_LABEL[detalle.estado]?.label ?? detalle.estado}
                      </span>
                    </div>
                  </div>
                  <button className="sol-detalle-close-btn" onClick={() => setVerTramiteId(null)}>✕</button>
                </div>

                <div className="sol-detalle-body">
                  <div className="sol-detalle-section">
                    <h3 className="sol-detalle-section-title">Información general</h3>
                    <div className="sol-detalle-grid">
                      <div className="sol-detalle-field">
                        <span className="sol-detalle-label">N° Trámite</span>
                        <span className="sol-detalle-value">#{detalle.id}</span>
                      </div>
                      <div className="sol-detalle-field">
                        <span className="sol-detalle-label">Empleado</span>
                        <span className="sol-detalle-value">{detalle.userNombre ?? detalle.userCi}</span>
                      </div>
                      <div className="sol-detalle-field">
                        <span className="sol-detalle-label">Fecha de solicitud</span>
                        <span className="sol-detalle-value">{formatDate(detalle.fechaSolicitud)}</span>
                      </div>
                      <div className="sol-detalle-field">
                        <span className="sol-detalle-label">Última actualización</span>
                        <span className="sol-detalle-value">{formatDate(detalle.updatedAt)}</span>
                      </div>
                    </div>
                  </div>

                  {detalle.vacaciones && (
                    <div className="sol-detalle-section">
                      <h3 className="sol-detalle-section-title">Detalle de vacaciones</h3>
                      <div className="sol-detalle-grid">
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Desde</span>
                          <span className="sol-detalle-value">{formatDate(detalle.vacaciones.desde)}</span>
                        </div>
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Hasta</span>
                          <span className="sol-detalle-value">{formatDate(detalle.vacaciones.hasta)}</span>
                        </div>
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Días totales</span>
                          <span className="sol-detalle-value sol-detalle-value--highlight">{detalle.vacaciones.diasTotales} días</span>
                        </div>
                        {detalle.vacaciones.observaciones && (
                          <div className="sol-detalle-field sol-detalle-field--full">
                            <span className="sol-detalle-label">Observaciones</span>
                            <span className="sol-detalle-value">{detalle.vacaciones.observaciones}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  )}

                  {detalle.diaEspecial && (
                    <div className="sol-detalle-section">
                      <h3 className="sol-detalle-section-title">Detalle de día especial</h3>
                      <div className="sol-detalle-grid">
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Fecha</span>
                          <span className="sol-detalle-value">{formatDate(detalle.diaEspecial.fecha)}</span>
                        </div>
                        <div className="sol-detalle-field sol-detalle-field--full">
                          <span className="sol-detalle-label">Motivo</span>
                          <span className="sol-detalle-value">{detalle.diaEspecial.motivo}</span>
                        </div>
                      </div>
                    </div>
                  )}

                  {detalle.utilidades && (
                    <div className="sol-detalle-section">
                      <h3 className="sol-detalle-section-title">Detalle de anticipo de utilidades</h3>
                      <div className="sol-detalle-grid">
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Monto solicitado</span>
                          <span className="sol-detalle-value sol-detalle-value--highlight">
                            ${detalle.utilidades.monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                          </span>
                        </div>
                        <div className="sol-detalle-field sol-detalle-field--full">
                          <span className="sol-detalle-label">Motivo</span>
                          <span className="sol-detalle-value">{detalle.utilidades.motivo}</span>
                        </div>
                      </div>
                    </div>
                  )}

                  {detalle.prestaciones && (
                    <div className="sol-detalle-section">
                      <h3 className="sol-detalle-section-title">
                        Detalle de {detalle.prestaciones.esPrestamo ? 'préstamo sobre prestaciones sociales' : 'anticipo de prestaciones sociales'}
                      </h3>
                      <div className="sol-detalle-grid">
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Monto solicitado</span>
                          <span className="sol-detalle-value sol-detalle-value--highlight">
                            Bs {detalle.prestaciones.monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                          </span>
                        </div>
                        <div className="sol-detalle-field">
                          <span className="sol-detalle-label">Destino</span>
                          <span className="sol-detalle-value">{detalle.prestaciones.destino}</span>
                        </div>
                        {detalle.prestaciones.esPrestamo && detalle.prestaciones.cuotas && (
                          <div className="sol-detalle-field">
                            <span className="sol-detalle-label">Cuotas</span>
                            <span className="sol-detalle-value sol-detalle-value--highlight">
                              {detalle.prestaciones.cuotas} de Bs {detalle.prestaciones.montoCuota?.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} c/u
                            </span>
                          </div>
                        )}
                        {detalle.prestaciones.observaciones && (
                          <div className="sol-detalle-field sol-detalle-field--full">
                            <span className="sol-detalle-label">Observaciones</span>
                            <span className="sol-detalle-value">{detalle.prestaciones.observaciones}</span>
                          </div>
                        )}
                        {detalle.prestaciones.archivoAdjuntoUrl && (
                          <div className="sol-detalle-field sol-detalle-field--full">
                            <span className="sol-detalle-label">Archivo adjunto</span>
                            <a
                              className="sol-detalle-value sol-detalle-adjunto-link"
                              href={`${API_ORIGIN}${detalle.prestaciones.archivoAdjuntoUrl}`}
                              target="_blank"
                              rel="noopener noreferrer"
                            >
                              📎 Ver adjunto
                            </a>
                          </div>
                        )}
                      </div>
                    </div>
                  )}

                  {detalle.motivoRechazo && (
                    <div className="sol-detalle-section sol-detalle-section--rechazo">
                      <h3 className="sol-detalle-section-title sol-detalle-section-title--rechazo">Motivo de rechazo</h3>
                      <p className="sol-detalle-rechazo-text">"{detalle.motivoRechazo}"</p>
                    </div>
                  )}
                </div>
              </>
            )}
          </div>
        </div>,
        document.body
      )}
    </div>
  );
};
