import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../../lib/api';
import './HistorialAprobaciones.css';

// ─── Types ────────────────────────────────────────────────────────────────────

/** Estados de EstadoAprobacionEnum que llegan al historial. */
type AccionFirma = 2 | 3;

export interface HistorialAprobacionAPI {
  tramiteId: number;
  tipoTramiteId: number;
  tipoTramiteNombre?: string;
  solicitanteCi?: string;
  solicitanteNombre?: string;
  aprobadorCi: string;
  aprobadorNombre?: string;
  orden: number;
  estado: AccionFirma;
  comentario?: string;
  fechaRespuesta?: string;
  fechaSolicitud: string;
  estadoTramite: number;
  motivoRechazo?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
}

interface HistorialAprobacionesProps {
  /** `propio` usa el CI del token; `global` trae las firmas de todos los aprobadores (solo admin). */
  scope: 'propio' | 'global';
}

// ─── Constants ────────────────────────────────────────────────────────────────

const TIPO_TRAMITE_ICON: Record<number, string> = {
  1: '💰',
  2: '📋',
  3: '🏦',
  4: '✈️',
  5: '📅',
  6: '📃',
};

const ACCION_INFO: Record<AccionFirma, { label: string; className: string }> = {
  2: { label: 'Aprobado',  className: 'hist-badge--aprobado'  },
  3: { label: 'Rechazado', className: 'hist-badge--rechazado' },
};

const ESTADO_TRAMITE_LABEL: Record<number, string> = {
  1: 'Creado',
  2: 'En Revisión',
  3: 'Firmado',
  4: 'Aprobado',
  5: 'Tramitando',
  6: 'Rechazado',
};

type FiltroAccion = 'todos' | 'aprobados' | 'rechazados';

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchHistorial = async (scope: 'propio' | 'global'): Promise<HistorialAprobacionAPI[]> => {
  const endpoint = scope === 'global'
    ? '/solicitudes/HistorialAprobaciones'
    : '/solicitudes/MiHistorialAprobaciones';
  const res = await api.get<ApiResponse<HistorialAprobacionAPI[]>>(endpoint);
  return res.data.data;
};

const formatDateTime = (iso?: string) => {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('es-VE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

// ─── Component ────────────────────────────────────────────────────────────────

export const HistorialAprobaciones: React.FC<HistorialAprobacionesProps> = ({ scope }) => {
  const [filtro, setFiltro] = useState<FiltroAccion>('todos');
  const [busqueda, setBusqueda] = useState('');

  const { data: registros = [], isLoading, isError } = useQuery({
    queryKey: ['historialAprobaciones', scope],
    queryFn: () => fetchHistorial(scope),
    retry: false,
  });

  const aprobados  = registros.filter(r => r.estado === 2).length;
  const rechazados = registros.filter(r => r.estado === 3).length;

  const registrosFiltrados = useMemo(() => {
    const termino = busqueda.trim().toLowerCase();

    return registros.filter(r => {
      if (filtro === 'aprobados'  && r.estado !== 2) return false;
      if (filtro === 'rechazados' && r.estado !== 3) return false;
      if (!termino) return true;

      const campos = [
        r.solicitanteNombre,
        r.solicitanteCi,
        r.tipoTramiteNombre,
        r.aprobadorNombre,
        `#${r.tramiteId}`,
      ];
      return campos.some(campo => campo?.toLowerCase().includes(termino));
    });
  }, [registros, filtro, busqueda]);

  if (isLoading) {
    return (
      <div className="hist-empty">
        <span className="hist-empty-icon">⏳</span>
        <p>Cargando historial...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="hist-empty">
        <span className="hist-empty-icon">⚠️</span>
        <p>Error al cargar el historial de aprobaciones.</p>
      </div>
    );
  }

  return (
    <div className="hist-wrapper">
      <div className="hist-toolbar">
        <div className="hist-filtros">
          {([
            ['todos',      `Todos (${registros.length})`],
            ['aprobados',  `Aprobados (${aprobados})`],
            ['rechazados', `Rechazados (${rechazados})`],
          ] as const).map(([valor, label]) => (
            <button
              key={valor}
              type="button"
              className={`hist-filtro-btn ${filtro === valor ? 'active' : ''}`}
              onClick={() => setFiltro(valor)}
            >
              {label}
            </button>
          ))}
        </div>

        <input
          type="text"
          className="hist-search"
          placeholder={scope === 'global'
            ? 'Buscar por empleado, aprobador, CI o #trámite...'
            : 'Buscar por empleado, CI o #trámite...'}
          value={busqueda}
          onChange={e => setBusqueda(e.target.value)}
        />
      </div>

      {registrosFiltrados.length === 0 ? (
        <div className="hist-empty">
          <span className="hist-empty-icon">📂</span>
          <p>
            {registros.length === 0
              ? 'Todavía no hay solicitudes aprobadas o rechazadas.'
              : 'Ningún registro coincide con la búsqueda.'}
          </p>
        </div>
      ) : (
        <>
          <div className="hist-table-wrapper">
            <table className="hist-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Tipo de Trámite</th>
                  <th>Empleado</th>
                  {scope === 'global' && <th>Aprobador</th>}
                  <th>Acción</th>
                  <th>Fecha</th>
                  <th>Comentario</th>
                  <th>Estado del trámite</th>
                </tr>
              </thead>
              <tbody>
                {registrosFiltrados.map(r => {
                  const accion = ACCION_INFO[r.estado];
                  return (
                    <tr key={`${r.tramiteId}-${r.aprobadorCi}-${r.orden}`}>
                      <td className="hist-td-num">#{r.tramiteId}</td>
                      <td>
                        <span className="hist-tipo-icon">{TIPO_TRAMITE_ICON[r.tipoTramiteId] ?? '📄'}</span>
                        {r.tipoTramiteNombre ?? 'Trámite'}
                      </td>
                      <td>
                        <span className="hist-nombre">{r.solicitanteNombre ?? '—'}</span>
                        {r.solicitanteCi && <span className="hist-ci">{r.solicitanteCi}</span>}
                      </td>
                      {scope === 'global' && (
                        <td>
                          <span className="hist-nombre">{r.aprobadorNombre ?? '—'}</span>
                          <span className="hist-ci">Paso {r.orden}</span>
                        </td>
                      )}
                      <td>
                        <span className={`hist-badge ${accion.className}`}>{accion.label}</span>
                      </td>
                      <td className="hist-td-fecha">{formatDateTime(r.fechaRespuesta)}</td>
                      <td className="hist-td-comentario">
                        {r.comentario
                          ? <span title={r.comentario}>"{r.comentario}"</span>
                          : <span className="hist-vacio">—</span>}
                      </td>
                      <td>
                        <span className="hist-estado-tramite">
                          {ESTADO_TRAMITE_LABEL[r.estadoTramite] ?? '—'}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <p className="hist-count">
            {registrosFiltrados.length} registro{registrosFiltrados.length !== 1 ? 's' : ''}
          </p>
        </>
      )}
    </div>
  );
};
