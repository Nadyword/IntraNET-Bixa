import React, { useState } from 'react';
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

export const TramitesPage: React.FC = () => {
  const [filterStatus, setFilterStatus] = useState<string | null>(null);

  const tramites: Tramite[] = [
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

  const statusConfig = {
    pendiente: { label: 'Pendiente', color: '#f57c00', step: 1 },
    revision: { label: 'En Revisión', color: '#9c27b0', step: 2 },
    procesando: { label: 'Procesando', color: '#2196f3', step: 3 },
    finalizado: { label: 'Finalizado', color: '#27ae60', step: 4 },
    rechazado: { label: 'Rechazado', color: '#c62828', step: 4 },
  };

  const filteredTramites = filterStatus
    ? tramites.filter((t) => t.status === filterStatus)
    : tramites;

  const getStatusColor = (status: Tramite['status']) => {
    return statusConfig[status].color;
  };

  const getStatusLabel = (status: Tramite['status']) => {
    return statusConfig[status].label;
  };

  return (
    <div className="tramites-page">
      {/* Header */}
      <div className="tramites-header">
        <h1>Mis Trámites</h1>
        <p>Sigue el estado de tus solicitudes y procesos</p>
      </div>

      {/* Filter Buttons */}
      <div className="filter-section">
        <button
          className={`filter-btn ${!filterStatus ? 'active' : ''}`}
          onClick={() => setFilterStatus(null)}
        >
          Todos ({tramites.length})
        </button>
        {Object.entries(statusConfig).map(([key, config]) => {
          const count = tramites.filter((t) => t.status === key).length;
          return count > 0 ? (
            <button
              key={key}
              className={`filter-btn ${filterStatus === key ? 'active' : ''}`}
              onClick={() => setFilterStatus(key)}
              style={{ borderColor: filterStatus === key ? config.color : 'transparent' }}
            >
              {config.label} ({count})
            </button>
          ) : null;
        })}
      </div>

      {/* Tramites List */}
      <div className="tramites-list">
        {filteredTramites.length > 0 ? (
          filteredTramites.map((tramite) => (
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
                {/* Status Flow */}
                <div className="status-flow">
                  <div className={`flow-step ${tramite.status !== 'pendiente' && tramite.status !== 'rechazado' ? 'completed' : ''}`}>
                    <div className="flow-circle">1</div>
                    <span className="flow-label">Creado</span>
                  </div>
                  <div className="flow-line"></div>
                  <div className={`flow-step ${tramite.status === 'revision' || tramite.status === 'procesando' || tramite.status === 'finalizado' ? 'completed' : ''}`}>
                    <div className="flow-circle">2</div>
                    <span className="flow-label">Revisión</span>
                  </div>
                  <div className="flow-line"></div>
                  <div className={`flow-step ${tramite.status === 'procesando' || tramite.status === 'finalizado' ? 'completed' : ''}`}>
                    <div className="flow-circle">3</div>
                    <span className="flow-label">Procesando</span>
                  </div>
                  <div className="flow-line"></div>
                  <div className={`flow-step ${tramite.status === 'finalizado' ? 'completed' : tramite.status === 'rechazado' ? 'rejected' : ''}`}>
                    <div className="flow-circle">4</div>
                    <span className="flow-label">Finalizado</span>
                  </div>
                </div>

                {/* Status Badge */}
                <div className="tramite-status">
                  <span
                    className="status-badge"
                    style={{ backgroundColor: `${getStatusColor(tramite.status)}20`, color: getStatusColor(tramite.status) }}
                  >
                    {getStatusLabel(tramite.status)}
                  </span>
                </div>
              </div>

              <div className="tramite-right">
                <div className="progress-info">
                  <div className="progress-bar">
                    <div className="progress-fill" style={{ width: `${tramite.progress}%`, backgroundColor: getStatusColor(tramite.status) }}></div>
                  </div>
                  <span className="progress-text">{tramite.progress}%</span>
                </div>
                <div className="tramite-actions">
                  {tramite.documents && tramite.documents > 0 && (
                    <a href="#" className="action-link">
                      📎 {tramite.documents}
                    </a>
                  )}
                  <a href="#" className="action-link">
                    Ver →
                  </a>
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="no-results">
            <p>No hay trámites con este estado</p>
          </div>
        )}
      </div>
    </div>
  );
};
