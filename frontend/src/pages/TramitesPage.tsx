import React, { useState } from 'react';
import { NuevaSolicitudModal } from '../components/ui/NuevaSolicitudModal';
import './TramitesPage.css';

interface Tramite {
  id: number;
  type: string;
  description: string;
  startDate: string;
  status: 'pendiente' | 'revision' | 'procesando' | 'finalizado' | 'rechazado';
  progress: number;
  documents?: number;
}

type EstadoHistorial = 'finalizado' | 'archivado' | 'rechazado';

interface TramiteHistorial {
  id: number;
  tipoTramite: string;
  tipoIcon: string;
  detalle: string;
  fechaSolicitud: string;
  fechaResolucion: string;
  estado: EstadoHistorial;
  comentario?: string;
}

type FilterStatus = 'todos' | Tramite['status'] | 'archivado';

const TRAMITES: Tramite[] = [
  {
    id: 1,
    type: 'Vacaciones',
    description: 'Solicitud de 10 días de vacaciones',
    startDate: '15 Abril 2026',
    status: 'finalizado',
    progress: 100,
    documents: 2,
  },
  {
    id: 2,
    type: 'Permiso Especial',
    description: 'Permiso de 4 horas - Cita médica',
    startDate: '22 Marzo 2026',
    status: 'procesando',
    progress: 75,
    documents: 1,
  },
  {
    id: 3,
    type: 'Préstamo Utilidades',
    description: 'Solicitud de adelanto de utilidades',
    startDate: '10 Marzo 2026',
    status: 'revision',
    progress: 50,
    documents: 3,
  },
  {
    id: 4,
    type: 'Constancia Laboral',
    description: 'Solicitud de constancia para trámite bancario',
    startDate: '01 Marzo 2026',
    status: 'finalizado',
    progress: 100,
    documents: 1,
  },
  {
    id: 5,
    type: 'Finiquito',
    description: 'Liquidación de beneficios',
    startDate: '25 Febrero 2026',
    status: 'pendiente',
    progress: 25,
    documents: 0,
  },
];

const HISTORIAL: TramiteHistorial[] = [
  {
    id: 1,
    tipoTramite: 'Vacaciones',
    tipoIcon: '✈️',
    detalle: '15 al 25 de Abril · 10 días',
    fechaSolicitud: '01/04/2026',
    fechaResolucion: '10/04/2026',
    estado: 'finalizado',
    comentario: 'Aprobado sin observaciones.',
  },
  {
    id: 2,
    tipoTramite: 'Permiso Especial',
    tipoIcon: '📝',
    detalle: '22 de Marzo · 2 horas',
    fechaSolicitud: '18/03/2026',
    fechaResolucion: '20/03/2026',
    estado: 'archivado',
    comentario: 'Permiso concedido y archivado por administración.',
  },
  {
    id: 3,
    tipoTramite: 'Préstamo Utilidades',
    tipoIcon: '💳',
    detalle: 'Monto solicitado: $500',
    fechaSolicitud: '05/02/2026',
    fechaResolucion: '12/02/2026',
    estado: 'rechazado',
    comentario: 'No cumple con los requisitos mínimos del período de antigüedad.',
  },
  {
    id: 4,
    tipoTramite: 'Constancia Laboral',
    tipoIcon: '📄',
    detalle: 'Para trámite bancario',
    fechaSolicitud: '10/01/2026',
    fechaResolucion: '13/01/2026',
    estado: 'finalizado',
    comentario: 'Documento generado y entregado.',
  },
];

const STATUS_CONFIG: Record<string, { label: string; color: string }> = {
  pendiente:  { label: 'Pendiente',   color: '#f57c00' },
  revision:   { label: 'En Revisión', color: '#9c27b0' },
  procesando: { label: 'Procesando',  color: '#2196f3' },
  finalizado: { label: 'Finalizado',  color: '#27ae60' },
  rechazado:  { label: 'Rechazado',   color: '#c62828' },
  archivado:  { label: 'Archivado',   color: '#1976d2' },
};

const FLOW_STEPS = ['Creado', 'Revisión', 'Procesando', 'Finalizado'] as const;

function getFlowClass(tramite: Tramite, stepIndex: number): string {
  const stepMap: Record<Tramite['status'], number> = {
    pendiente: 0, revision: 1, procesando: 2, finalizado: 3, rechazado: 3,
  };
  const reached = stepMap[tramite.status];
  if (stepIndex < reached) return 'completed';
  if (stepIndex === reached && tramite.status === 'rechazado') return 'rejected';
  if (stepIndex <= reached && tramite.status !== 'rechazado') return 'completed';
  return '';
}

export const TramitesPage: React.FC = () => {
  const [filterStatus, setFilterStatus] = useState<FilterStatus>('todos');
  const [modalOpen, setModalOpen] = useState(false);

  const filteredTramites =
    filterStatus === 'todos' ? TRAMITES : TRAMITES.filter((t) => t.status === filterStatus);

  const filteredHistorial =
    filterStatus === 'todos' ? HISTORIAL : HISTORIAL.filter((t) => t.estado === filterStatus);

  const allFilters: FilterStatus[] = ['pendiente', 'revision', 'procesando', 'finalizado', 'rechazado', 'archivado'];
  const filtersWithItems = allFilters.filter(
    (s) => TRAMITES.some((t) => t.status === s) || HISTORIAL.some((t) => t.estado === s),
  );

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

      <div className="filter-section">
        <button
          className={`filter-btn ${filterStatus === 'todos' ? 'active' : ''}`}
          onClick={() => setFilterStatus('todos')}
        >
          Todos ({TRAMITES.length + HISTORIAL.length})
        </button>
        {filtersWithItems.map((key) => {
          const count =
            TRAMITES.filter((t) => t.status === key).length +
            HISTORIAL.filter((t) => t.estado === key).length;
          const config = STATUS_CONFIG[key];
          return (
            <button
              key={key}
              className={`filter-btn ${filterStatus === key ? 'active' : ''}`}
              onClick={() => setFilterStatus(key)}
              style={{ borderColor: filterStatus === key ? config.color : 'transparent' }}
            >
              {config.label} ({count})
            </button>
          );
        })}
      </div>

      {/* Sección en tránsito */}
      <div className="tramites-section-label">En Tránsito</div>

      <div className="tramites-list">
        {filteredTramites.length > 0 ? (
          filteredTramites.map((tramite) => {
            const cfg = STATUS_CONFIG[tramite.status];
            return (
              <div key={tramite.id} className="tramite-card">
                <div className="tramite-left">
                  <div className="tramite-type">
                    <h3>{tramite.type}</h3>
                    <p>{tramite.description}</p>
                  </div>
                  <div className="tramite-date">
                    <span>Iniciado:</span> {tramite.startDate}
                  </div>
                </div>

                <div className="tramite-tracker">
                  <div className="status-flow">
                    {FLOW_STEPS.map((label, i) => (
                      <React.Fragment key={label}>
                        <div className={`flow-step ${getFlowClass(tramite, i)}`}>
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

                <div className="tramite-right">
                  <div className="progress-info">
                    <div className="progress-bar">
                      <div
                        className="progress-fill"
                        style={{ width: `${tramite.progress}%`, backgroundColor: cfg.color }}
                      />
                    </div>
                    <span className="progress-text">{tramite.progress}%</span>
                  </div>
                  <div className="tramite-actions">
                    {tramite.documents && tramite.documents > 0 ? (
                      <a href="#" className="action-link">📎 {tramite.documents}</a>
                    ) : null}
                    <a href="#" className="action-link">Ver →</a>
                  </div>
                </div>
              </div>
            );
          })
        ) : (
          <div className="no-results">
            <p>No hay trámites en tránsito para este filtro</p>
          </div>
        )}
      </div>

      {/* Sección historial */}
      <div className="tramites-section-divider" />
      <div className="tramites-section-label">Historial</div>

      <div className="historial-list">
        {filteredHistorial.length > 0 ? (
          filteredHistorial.map((tramite) => {
            const cfg = STATUS_CONFIG[tramite.estado];
            return (
              <div key={tramite.id} className="historial-card">
                <div className="historial-card-left">
                  <span className="historial-tipo-icon">{tramite.tipoIcon}</span>
                </div>

                <div className="historial-card-body">
                  <div className="historial-card-top">
                    <span className="historial-tipo">{tramite.tipoTramite}</span>
                    <span
                      className="status-badge"
                      style={{ backgroundColor: `${cfg.color}18`, color: cfg.color }}
                    >
                      {cfg.label}
                    </span>
                  </div>
                  <p className="historial-detalle">{tramite.detalle}</p>
                  {tramite.comentario && (
                    <p className="historial-comentario">"{tramite.comentario}"</p>
                  )}
                </div>

                <div className="historial-card-right">
                  <div className="historial-fecha-group">
                    <span className="historial-fecha-label">Solicitado</span>
                    <span className="historial-fecha">{tramite.fechaSolicitud}</span>
                  </div>
                  <div className="historial-fecha-group">
                    <span className="historial-fecha-label">Resuelto</span>
                    <span className="historial-fecha">{tramite.fechaResolucion}</span>
                  </div>
                </div>
              </div>
            );
          })
        ) : (
          <div className="no-results">
            <p>No hay historial para este filtro</p>
          </div>
        )}
      </div>
      {modalOpen && <NuevaSolicitudModal onClose={() => setModalOpen(false)} />}
    </div>
  );
};
