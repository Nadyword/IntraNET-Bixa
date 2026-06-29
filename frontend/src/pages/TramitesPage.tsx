import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { NuevaSolicitudModal } from '../components/ui/NuevaSolicitudModal';
import { useAuthStore } from '../store/authStore';
import api from '../lib/api';
import './TramitesPage.css';

// ─── Types ────────────────────────────────────────────────────────────────────

interface VacacionesDetalle {
  desde: string;
  hasta: string;
  diasTotales: number;
  observaciones?: string;
}

interface TramiteAPI {
  id: number;
  tipoTramiteId: number;
  tipoTramiteNombre: string;
  userCi: string;
  fechaSolicitud: string;
  updatedAt: string;
  estado: number; // 1=Creado 2=Revision 3=Aprobado 4=Archivado 5=Finalizado 6=Rechazado
  motivoRechazo?: string;
  vacaciones?: VacacionesDetalle;
}

interface AprobacionAPI {
  orden: number;
  tramiteId: number;
  nombre?: string;
  comentario?: string;
  aprobadorCi: string;
  fechaRespuesta?: string;
  estado: number; // 1=Pendiente 2=Aprobado 3=Rechazado
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
}

// ─── Constants ────────────────────────────────────────────────────────────────

const ESTADOS_HISTORIAL = new Set([3, 5, 6]);

const TIPO_TRAMITE_ICONS: Record<number, string> = {
  1: '💰',
  2: '📋',
  3: '🏦',
  4: '✈️',
  5: '📅',
};

const ESTADO_CONFIG: Record<number, { label: string; color: string }> = {
  1: { label: 'Creado',      color: '#f57c00' },
  2: { label: 'En Revisión', color: '#9c27b0' },
  3: { label: 'Aprobado',    color: '#27ae60' },
  4: { label: 'Archivado',   color: '#1976d2' },
  5: { label: 'Finalizado',  color: '#27ae60' },
  6: { label: 'Rechazado',   color: '#c62828' },
};

const APROBACION_CONFIG: Record<number, { label: string; color: string }> = {
  1: { label: 'Pendiente',  color: '#f57c00' },
  2: { label: 'Aprobado',   color: '#27ae60' },
  3: { label: 'Rechazado',  color: '#c62828' },
};

const FLOW_STEPS = ['Creado', 'En Revisión'] as const;

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-VE', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function getTramiteDetalle(tramite: TramiteAPI): string {
  if (tramite.vacaciones) {
    const { desde, hasta, diasTotales } = tramite.vacaciones;
    return `Del ${formatDate(desde)} al ${formatDate(hasta)} · ${diasTotales} días`;
  }
  return '';
}

function getFlowStepClass(estado: number, stepIndex: number): string {
  if (stepIndex === 0) return estado >= 1 ? 'completed' : '';
  if (stepIndex === 1) return estado >= 2 ? 'completed' : '';
  return '';
}

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchTramites = async (ci: string): Promise<TramiteAPI[]> => {
  const res = await api.get<ApiResponse<TramiteAPI[]>>(`/solicitudes/${ci}`);
  return res.data.data;
};

const fetchAprobaciones = async (tramiteId: number): Promise<AprobacionAPI[]> => {
  const res = await api.get<ApiResponse<AprobacionAPI[]>>(`/solicitudes/Aprobaciones/${tramiteId}`);
  return res.data.data;
};

// ─── Aprobaciones Modal ───────────────────────────────────────────────────────

interface AprobacionesModalProps {
  tramiteId: number;
  tipoNombre: string;
  onClose: () => void;
}

const AprobacionesModal: React.FC<AprobacionesModalProps> = ({ tramiteId, tipoNombre, onClose }) => {
  const { data: aprobaciones = [], isLoading, isError } = useQuery({
    queryKey: ['aprobaciones', tramiteId],
    queryFn: () => fetchAprobaciones(tramiteId),
  });

  const sorted = [...aprobaciones].sort((a, b) => a.orden - b.orden);
  const aprobadas = sorted.filter(a => a.estado === 2).length;
  const pendientes = sorted.filter(a => a.estado === 1).length;
  const rechazadas = sorted.filter(a => a.estado === 3).length;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content aprobaciones-modal" onClick={e => e.stopPropagation()}>
        <div className="modal-header modal-header--info">
          <h2>Aprobaciones · {tipoNombre}</h2>
          <button className="close-btn" onClick={onClose}>✕</button>
        </div>

        <div className="modal-body">
          {isLoading && <p className="aprobaciones-status">Cargando aprobaciones...</p>}
          {isError && <p className="aprobaciones-status aprobaciones-error">Error al cargar las aprobaciones.</p>}

          {!isLoading && !isError && (
            <>
              <div className="aprobaciones-summary">
                <span className="aprobaciones-chip aprobaciones-chip--aprobada">✓ {aprobadas} aprobadas</span>
                <span className="aprobaciones-chip aprobaciones-chip--pendiente">⏳ {pendientes} pendientes</span>
                {rechazadas > 0 && (
                  <span className="aprobaciones-chip aprobaciones-chip--rechazada">✕ {rechazadas} rechazadas</span>
                )}
              </div>

              <div className="aprobaciones-list">
                {sorted.map((aprobacion) => {
                  const cfg = APROBACION_CONFIG[aprobacion.estado] ?? APROBACION_CONFIG[1];
                  return (
                    <div key={aprobacion.orden} className="aprobador-item">
                      <div className="aprobador-orden">{aprobacion.orden}</div>
                      <div className="aprobador-info">
                        <span className="aprobador-nombre">
                          {aprobacion.nombre ?? aprobacion.aprobadorCi}
                        </span>
                        <span className="aprobador-ci">CI: {aprobacion.aprobadorCi}</span>
                        {aprobacion.comentario && (
                          <span className="aprobador-comentario">"{aprobacion.comentario}"</span>
                        )}
                        {aprobacion.fechaRespuesta && (
                          <span className="aprobador-fecha">{formatDate(aprobacion.fechaRespuesta)}</span>
                        )}
                      </div>
                      <span
                        className="aprobador-estado"
                        style={{ backgroundColor: `${cfg.color}20`, color: cfg.color }}
                      >
                        {cfg.label}
                      </span>
                    </div>
                  );
                })}
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

// ─── Main Page ────────────────────────────────────────────────────────────────

export const TramitesPage: React.FC = () => {
  const [filterEstado, setFilterEstado] = useState<number | 'todos'>('todos');
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedTramiteId, setSelectedTramiteId] = useState<number | null>(null);

  const user = useAuthStore(state => state.user);

  const { data: allTramites = [], isLoading, isError } = useQuery({
    queryKey: ['tramites', user?.ci],
    queryFn: () => fetchTramites(user!.ci),
    enabled: !!user?.ci,
  });

  const tramitesEnTransito = allTramites.filter(t => !ESTADOS_HISTORIAL.has(t.estado));
  const tramitesHistorial  = allTramites.filter(t =>  ESTADOS_HISTORIAL.has(t.estado));

  const filteredTransito = filterEstado === 'todos'
    ? tramitesEnTransito
    : tramitesEnTransito.filter(t => t.estado === filterEstado);

  const filteredHistorial = filterEstado === 'todos'
    ? tramitesHistorial
    : tramitesHistorial.filter(t => t.estado === filterEstado);

  const estadosConItems = ([1, 2, 3, 4, 5, 6] as number[]).filter(e =>
    allTramites.some(t => t.estado === e)
  );

  const selectedTramite = allTramites.find(t => t.id === selectedTramiteId);

  return (
    <div className="tramites-page">
      <div className="tramites-header">
        <div className="tramites-header-top">
          <div>
            <h1>Mis Trámites</h1>
            <p>Sigue el estado de tus solicitudes y procesos</p>
          </div>
          <button className="tramites-nueva-btn" onClick={() => setModalOpen(true)}>
            + Nueva Solicitud
          </button>
        </div>
      </div>

      {isLoading && <div className="tramites-feedback">Cargando trámites...</div>}
      {isError  && <div className="tramites-feedback tramites-feedback--error">Error al cargar los trámites.</div>}

      {!isLoading && !isError && (
        <>
          {/* Filtros */}
          <div className="filter-section">
            <button
              className={`filter-btn ${filterEstado === 'todos' ? 'active' : ''}`}
              onClick={() => setFilterEstado('todos')}
            >
              Todos ({allTramites.length})
            </button>
            {estadosConItems.map(e => {
              const count = allTramites.filter(t => t.estado === e).length;
              const cfg = ESTADO_CONFIG[e];
              return (
                <button
                  key={e}
                  className={`filter-btn ${filterEstado === e ? 'active' : ''}`}
                  onClick={() => setFilterEstado(e)}
                  style={{ borderColor: filterEstado === e ? cfg.color : 'transparent' }}
                >
                  {cfg.label} ({count})
                </button>
              );
            })}
          </div>

          {/* ── En Tránsito ── */}
          <div className="tramites-section-label">En Tránsito</div>

          <div className="tramites-list">
            {filteredTransito.length > 0 ? (
              filteredTransito.map(tramite => {
                const cfg    = ESTADO_CONFIG[tramite.estado] ?? ESTADO_CONFIG[1];
                const icon   = TIPO_TRAMITE_ICONS[tramite.tipoTramiteId] ?? '📄';
                const detalle = getTramiteDetalle(tramite);

                return (
                  <div key={tramite.id} className="tramite-card">
                    {/* Izquierda: info del trámite */}
                    <div className="tramite-left">
                      <div className="tramite-type">
                        <span className="tramite-icon">{icon}</span>
                        <div>
                          <h3>{tramite.tipoTramiteNombre}</h3>
                          {detalle && <p>{detalle}</p>}
                        </div>
                      </div>
                      <div className="tramite-date">
                        <span>Iniciado:</span> {formatDate(tramite.fechaSolicitud)}
                      </div>
                    </div>

                    {/* Centro: tracker de estados */}
                    <div className="tramite-tracker">
                      <div className="status-flow">
                        {FLOW_STEPS.map((label, i) => (
                          <React.Fragment key={label}>
                            <div className={`flow-step ${getFlowStepClass(tramite.estado, i)}`}>
                              <div className="flow-circle">{i + 1}</div>
                              <span className="flow-label">{label}</span>
                            </div>
                            {i < FLOW_STEPS.length - 1 && <div className="flow-line" />}
                          </React.Fragment>
                        ))}
                      </div>
                      <div className="tramite-status">
                        <span
                          className="status-badge"
                          style={{ backgroundColor: `${cfg.color}20`, color: cfg.color }}
                        >
                          {cfg.label}
                        </span>
                      </div>
                    </div>

                    {/* Derecha: botón ver aprobaciones */}
                    {/* <div className="tramite-right">
                      <button
                        className="ver-aprobaciones-btn"
                        onClick={() => setSelectedTramiteId(tramite.id)}
                      >
                        Ver Aprobaciones
                      </button>
                    </div> */}
               
                  </div>
                );
              })
            ) : (
              <div className="no-results">
                <p>No hay trámites en tránsito{filterEstado !== 'todos' ? ' para este filtro' : ''}</p>
              </div>
            )}
          </div>

          {/* ── Historial ── */}
          <div className="tramites-section-divider" />
          <div className="tramites-section-label">Historial</div>

          <div className="historial-list">
            {filteredHistorial.length > 0 ? (
              filteredHistorial.map(tramite => {
                const cfg    = ESTADO_CONFIG[tramite.estado] ?? ESTADO_CONFIG[5];
                const icon   = TIPO_TRAMITE_ICONS[tramite.tipoTramiteId] ?? '📄';
                const detalle = getTramiteDetalle(tramite);

                return (
                  <div key={tramite.id} className="historial-card">
                    <div className="historial-card-left">
                      <span className="historial-tipo-icon">{icon}</span>
                    </div>

                    <div className="historial-card-body">
                      <div className="historial-card-top">
                        <span className="historial-tipo">{tramite.tipoTramiteNombre}</span>
                        <span
                          className="status-badge"
                          style={{ backgroundColor: `${cfg.color}18`, color: cfg.color }}
                        >
                          {cfg.label}
                        </span>
                      </div>
                      {detalle && <p className="historial-detalle">{detalle}</p>}
                      {tramite.motivoRechazo && (
                        <p className="historial-comentario">"{tramite.motivoRechazo}"</p>
                      )}
                    </div>

                    <div className="historial-card-right">
                      <div className="historial-fecha-group">
                        <span className="historial-fecha-label">Solicitado</span>
                        <span className="historial-fecha">{formatDate(tramite.fechaSolicitud)}</span>
                      </div>
                      <div className="historial-fecha-group">
                        <span className="historial-fecha-label">Actualizado</span>
                        <span className="historial-fecha">{formatDate(tramite.updatedAt)}</span>
                      </div>
                    </div>
                  </div>
                );
              })
            ) : (
              <div className="no-results">
                <p>No hay historial{filterEstado !== 'todos' ? ' para este filtro' : ''}</p>
              </div>
            )}
          </div>
        </>
      )}

      {/* Modal de aprobaciones */}
      {selectedTramiteId !== null && selectedTramite && (
        <AprobacionesModal
          tramiteId={selectedTramiteId}
          tipoNombre={selectedTramite.tipoTramiteNombre}
          onClose={() => setSelectedTramiteId(null)}
        />
      )}

      {modalOpen && <NuevaSolicitudModal onClose={() => setModalOpen(false)} />}
    </div>
  );
};
