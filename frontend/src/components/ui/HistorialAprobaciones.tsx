import React, { Fragment, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api, { API_ORIGIN } from '../../lib/api';
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

/** Datos propios de la solicitud según su tipo, devueltos por `/solicitudes/Detalle/{id}`. */
interface TramiteDetalleAPI {
  id: number;
  vacaciones?: {
    desde: string;
    hasta: string;
    diasTotales: number;
    observaciones?: string;
  };
  diaEspecial?: {
    fecha: string;
    motivo: string;
  };
  utilidades?: {
    monto: number;
    motivo: string;
  };
  prestaciones?: {
    esPrestamo: boolean;
    monto: number;
    destino: string;
    observaciones?: string;
    cuotas?: number;
    montoCuota?: number;
    archivoAdjuntoUrl?: string;
  };
  constanciaTrabajo?: {
    conSueldo: boolean;
    dirigidoAEspecifico: boolean;
    dirigidoA?: string;
  };
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

const fetchTramiteDetalle = async (tramiteId: number): Promise<TramiteDetalleAPI> => {
  const res = await api.get<ApiResponse<TramiteDetalleAPI>>(`/solicitudes/Detalle/${tramiteId}`);
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

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('es-VE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

const formatMonto = (monto: number) =>
  monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

// ─── Detalle de la solicitud ──────────────────────────────────────────────────

interface CampoProps {
  label: string;
  full?: boolean;
  highlight?: boolean;
  children: React.ReactNode;
}

const Campo: React.FC<CampoProps> = ({ label, full, highlight, children }) => (
  <div className={`hist-detalle-field ${full ? 'hist-detalle-field--full' : ''}`}>
    <span className="hist-detalle-label">{label}</span>
    <span className={`hist-detalle-value ${highlight ? 'hist-detalle-value--highlight' : ''}`}>{children}</span>
  </div>
);

/** Datos específicos de la solicitud (fechas, días, montos...), cargados al expandir la fila. */
const DetalleSolicitud: React.FC<{ tramiteId: number }> = ({ tramiteId }) => {
  const { data: detalle, isLoading, isError } = useQuery({
    queryKey: ['tramiteDetalle', tramiteId],
    queryFn: () => fetchTramiteDetalle(tramiteId),
    retry: false,
  });

  if (isLoading) {
    return <p className="hist-detalle-solicitud-estado">Cargando datos de la solicitud...</p>;
  }

  if (isError || !detalle) {
    return <p className="hist-detalle-solicitud-estado">No se pudieron cargar los datos de la solicitud.</p>;
  }

  const { vacaciones, diaEspecial, utilidades, prestaciones, constanciaTrabajo } = detalle;

  let titulo: string;
  let campos: React.ReactNode;

  if (vacaciones) {
    titulo = 'Vacaciones solicitadas';
    campos = (
      <>
        <Campo label="Desde">{formatDate(vacaciones.desde)}</Campo>
        <Campo label="Hasta">{formatDate(vacaciones.hasta)}</Campo>
        <Campo label="Días solicitados" highlight>
          {vacaciones.diasTotales} día{vacaciones.diasTotales !== 1 ? 's' : ''}
        </Campo>
        {vacaciones.observaciones && (
          <Campo label="Observaciones" full>{vacaciones.observaciones}</Campo>
        )}
      </>
    );
  } else if (diaEspecial) {
    titulo = 'Día especial solicitado';
    campos = (
      <>
        <Campo label="Día solicitado" highlight>{formatDate(diaEspecial.fecha)}</Campo>
        <Campo label="Motivo" full>{diaEspecial.motivo}</Campo>
      </>
    );
  } else if (utilidades) {
    titulo = 'Anticipo de utilidades solicitado';
    campos = (
      <>
        <Campo label="Monto solicitado" highlight>${formatMonto(utilidades.monto)}</Campo>
        <Campo label="Motivo" full>{utilidades.motivo}</Campo>
      </>
    );
  } else if (prestaciones) {
    titulo = prestaciones.esPrestamo
      ? 'Préstamo sobre prestaciones sociales solicitado'
      : 'Anticipo de prestaciones sociales solicitado';
    campos = (
      <>
        <Campo label="Monto solicitado" highlight>Bs {formatMonto(prestaciones.monto)}</Campo>
        <Campo label="Destino">{prestaciones.destino}</Campo>
        {prestaciones.esPrestamo && prestaciones.cuotas && (
          <Campo label="Cuotas">
            {prestaciones.cuotas}
            {prestaciones.montoCuota != null && ` de Bs ${formatMonto(prestaciones.montoCuota)} c/u`}
          </Campo>
        )}
        {prestaciones.observaciones && (
          <Campo label="Observaciones" full>{prestaciones.observaciones}</Campo>
        )}
        {prestaciones.archivoAdjuntoUrl && (
          <Campo label="Archivo adjunto" full>
            <a
              className="hist-detalle-adjunto"
              href={`${API_ORIGIN}${prestaciones.archivoAdjuntoUrl}`}
              target="_blank"
              rel="noopener noreferrer"
            >
              📎 Ver adjunto
            </a>
          </Campo>
        )}
      </>
    );
  } else if (constanciaTrabajo) {
    titulo = 'Constancia de trabajo solicitada';
    campos = (
      <>
        <Campo label="Con sueldo">{constanciaTrabajo.conSueldo ? 'Sí' : 'No'}</Campo>
        <Campo label="Dirigida a">
          {constanciaTrabajo.dirigidoAEspecifico && constanciaTrabajo.dirigidoA
            ? constanciaTrabajo.dirigidoA
            : 'A quien pueda interesar'}
        </Campo>
      </>
    );
  } else {
    return <p className="hist-detalle-solicitud-estado">Esta solicitud no tiene datos adicionales.</p>;
  }

  return (
    <div className="hist-detalle-solicitud">
      <h4 className="hist-detalle-titulo">{titulo}</h4>
      <div className="hist-detalle-grid">{campos}</div>
    </div>
  );
};

// ─── Component ────────────────────────────────────────────────────────────────

/** Identifica una firma: un mismo trámite tiene un registro por cada paso de la cadena. */
const registroKey = (r: HistorialAprobacionAPI) => `${r.tramiteId}-${r.aprobadorCi}-${r.orden}`;

export const HistorialAprobaciones: React.FC<HistorialAprobacionesProps> = ({ scope }) => {
  const [filtro, setFiltro] = useState<FiltroAccion>('todos');
  const [busqueda, setBusqueda] = useState('');
  const [detalleAbierto, setDetalleAbierto] = useState<string | null>(null);

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
                  <th className="hist-th-detalle"><span className="hist-sr-only">Detalle</span></th>
                </tr>
              </thead>
              <tbody>
                {registrosFiltrados.map(r => {
                  const accion = ACCION_INFO[r.estado];
                  const key = registroKey(r);
                  const abierto = detalleAbierto === key;
                  return (
                    <Fragment key={key}>
                    <tr className={`hist-row ${abierto ? 'hist-row--abierta' : ''}`}>
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
                      <td className="hist-td-detalle">
                        <button
                          type="button"
                          className="hist-detalle-btn"
                          aria-expanded={abierto}
                          aria-label={abierto ? 'Ocultar detalle' : 'Ver detalle'}
                          onClick={() => setDetalleAbierto(abierto ? null : key)}
                        >
                          {abierto ? '▲' : '▼'}
                        </button>
                      </td>
                    </tr>

                    {abierto && (
                      <tr className="hist-detalle-row">
                        <td colSpan={scope === 'global' ? 9 : 8}>
                          <div className="hist-detalle">
                            <DetalleSolicitud tramiteId={r.tramiteId} />
                            <h4 className="hist-detalle-titulo">Firma y trámite</h4>
                            <div className="hist-detalle-grid">
                              <div className="hist-detalle-field hist-detalle-field--principal">
                                <span className="hist-detalle-label">Solicitante</span>
                                <span className="hist-detalle-value">{r.solicitanteNombre ?? '—'}</span>
                                {r.solicitanteCi && (
                                  <span className="hist-detalle-sub">CI {r.solicitanteCi}</span>
                                )}
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">N° Trámite</span>
                                <span className="hist-detalle-value">#{r.tramiteId}</span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Tipo de trámite</span>
                                <span className="hist-detalle-value">
                                  {TIPO_TRAMITE_ICON[r.tipoTramiteId] ?? '📄'} {r.tipoTramiteNombre ?? 'Trámite'}
                                </span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Fecha de solicitud</span>
                                <span className="hist-detalle-value">{formatDateTime(r.fechaSolicitud)}</span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Firmante</span>
                                <span className="hist-detalle-value">{r.aprobadorNombre ?? r.aprobadorCi}</span>
                                <span className="hist-detalle-sub">Paso {r.orden}</span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Fecha de la firma</span>
                                <span className="hist-detalle-value">{formatDateTime(r.fechaRespuesta)}</span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Acción</span>
                                <span className={`hist-badge ${accion.className}`}>{accion.label}</span>
                              </div>
                              <div className="hist-detalle-field">
                                <span className="hist-detalle-label">Estado del trámite</span>
                                <span className="hist-detalle-value">
                                  {ESTADO_TRAMITE_LABEL[r.estadoTramite] ?? '—'}
                                </span>
                              </div>
                              <div className="hist-detalle-field hist-detalle-field--full">
                                <span className="hist-detalle-label">Comentario de la firma</span>
                                <span className="hist-detalle-value">
                                  {r.comentario ? `"${r.comentario}"` : 'Sin comentario'}
                                </span>
                              </div>
                              {r.motivoRechazo && (
                                <div className="hist-detalle-field hist-detalle-field--full">
                                  <span className="hist-detalle-label">Motivo de rechazo del trámite</span>
                                  <span className="hist-detalle-value">{r.motivoRechazo}</span>
                                </div>
                              )}
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                    </Fragment>
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
