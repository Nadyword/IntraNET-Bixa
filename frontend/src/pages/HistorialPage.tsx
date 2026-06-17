import React, { useState } from 'react';
import './HistorialPage.css';

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

const HISTORIAL: TramiteHistorial[] = [

];

const ESTADO_CONFIG: Record<EstadoHistorial, { label: string; className: string }> = {
  finalizado: { label: 'Finalizado', className: 'badge-finalizado' },
  archivado: { label: 'Archivado', className: 'badge-archivado' },
  rechazado: { label: 'Rechazado', className: 'badge-rechazado' },
};

type FiltroEstado = 'todos' | EstadoHistorial;

export const HistorialPage: React.FC = () => {
  const [filtro, setFiltro] = useState<FiltroEstado>('todos');

  const tramitesFiltrados =
    filtro === 'todos' ? HISTORIAL : HISTORIAL.filter((t) => t.estado === filtro);

  return (
    <div className="historial-page">
      <div className="historial-header">
        <h1>Historial de Trámites</h1>
      </div>

      <div className="historial-filtros">
        {(['todos', 'finalizado', 'archivado', 'rechazado'] as const).map((f) => (
          <button
            key={f}
            className={`filtro-btn ${filtro === f ? 'active' : ''}`}
            onClick={() => setFiltro(f)}
          >
            {f === 'todos'
              ? `Todos (${HISTORIAL.length})`
              : `${ESTADO_CONFIG[f].label} (${HISTORIAL.filter((t) => t.estado === f).length})`}
          </button>
        ))}
      </div>

      {tramitesFiltrados.length === 0 ? (
        <div className="historial-empty">
          <span className="historial-empty-icon">📂</span>
          <p>No hay trámites en esta categoría.</p>
        </div>
      ) : (
        <div className="historial-list">
          {tramitesFiltrados.map((tramite) => {
            const config = ESTADO_CONFIG[tramite.estado];
            return (
              <div key={tramite.id} className="historial-card">
                <div className="historial-card-left">
                  <span className="historial-tipo-icon">{tramite.tipoIcon}</span>
                </div>

                <div className="historial-card-body">
                  <div className="historial-card-top">
                    <span className="historial-tipo">{tramite.tipoTramite}</span>
                    <span className={`status-badge ${config.className}`}>{config.label}</span>
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
          })}
        </div>
      )}
    </div>
  );
};
