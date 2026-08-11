import React, { useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api, { API_ORIGIN } from '../lib/api';
import { ButtonSpinner } from '../components/ui/ButtonSpinner';
import './AdministracionPage.css';

// ─── Types ────────────────────────────────────────────────────────────────────

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

interface ConstanciaTrabajoDetalle {
  conSueldo: boolean;
  dirigidoAEspecifico: boolean;
  dirigidoA?: string;
}

interface TramiteDTO {
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
  constanciaTrabajo?: ConstanciaTrabajoDetalle;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
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

const ESTADO_LABEL: Record<number, { label: string; className: string }> = {
  1: { label: 'Creado',      className: 'badge-creado'     },
  2: { label: 'En Revisión', className: 'badge-revision'   },
  3: { label: 'Firmado',     className: 'badge-firmado'    },
  4: { label: 'Aprobado',    className: 'badge-aprobado'   },
  5: { label: 'Tramitando',  className: 'badge-tramitando' },
  6: { label: 'Finalizado',  className: 'badge-finalizado' },
  7: { label: 'Rechazado',   className: 'badge-rechazado'  },
};

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchAprobados = async (): Promise<TramiteDTO[]> => {
  const res = await api.get<ApiResponse<TramiteDTO[]>>('/solicitudes/Aprobados');
  return res.data.data;
};

const apiArchivarTramite = (tramiteId: number) =>
  api.put(`/solicitudes/Reporte/Archivar/${tramiteId}`);

const apiAprobarTramite = (tramiteId: number) =>
  api.put(`/solicitudes/Reporte/Aprobar/${tramiteId}`);

const apiRechazarTramite = (tramiteId: number, razon: string) =>
  api.put(`/solicitudes/Reporte/Rechazar/${tramiteId}`, null, { params: { razon } });

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('es-VE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

// ─── Row ──────────────────────────────────────────────────────────────────────

const TramiteRow: React.FC<{ tramite: TramiteDTO }> = ({ tramite }) => {
  const [loading, setLoading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [showDetalle, setShowDetalle] = useState(false);
  const [showRechazarModal, setShowRechazarModal] = useState(false);
  const [razonRechazo, setRazonRechazo] = useState('');

  const queryClient = useQueryClient();
  const { mutate: archivar, isPending: archivando } = useMutation({
    mutationFn: () => apiArchivarTramite(tramite.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['aprobados'] }),
  });

  const { mutate: aprobar, isPending: aprobando } = useMutation({
    mutationFn: () => apiAprobarTramite(tramite.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['aprobados'] }),
  });

  const { mutate: rechazar, isPending: rechazando } = useMutation({
    mutationFn: (razon: string) => apiRechazarTramite(tramite.id, razon),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['aprobados'] });
      setShowRechazarModal(false);
      setRazonRechazo('');
    },
  });

  const esActivo = tramite.estado <= 3;
  const esTramitando = tramite.estado === 4;

  const icon   = TIPO_TRAMITE_ICON[tramite.tipoTramiteId] ?? '📄';
  const estado = ESTADO_LABEL[tramite.estado] ?? { label: String(tramite.estado), className: '' };

  const reporteEndpoint = tramite.diaEspecial ? 'DiaEspecial' : tramite.utilidades ? 'Utilidades' : tramite.prestaciones ? 'Prestaciones' : 'Vacaciones';
  const tieneReporte = Boolean(tramite.vacaciones || tramite.diaEspecial || tramite.utilidades || tramite.prestaciones);
  const reporteLabel = tramite.diaEspecial
    ? 'día especial'
    : tramite.utilidades
    ? 'anticipo de utilidades'
    : tramite.prestaciones
    ? (tramite.prestaciones.esPrestamo ? 'préstamo sobre prestaciones sociales' : 'anticipo de prestaciones sociales')
    : 'vacaciones';

  const handlePlanilla = async () => {
    setLoading(true);
    try {
      const response = await api.get(`/solicitudes/Reporte/${reporteEndpoint}/${tramite.id}`, {
        responseType: 'blob',
      });
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
      setPreviewUrl(url);
    } finally {
      setLoading(false);
    }
  };

  const handleClosePreview = () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setPreviewUrl(null);
  };

  return (
    <>
      <tr className="adm-row">
        <td className="adm-td adm-td-id">#{tramite.id}</td>
        <td className="adm-td adm-td-tipo">
          <span className="adm-tipo-icon">{icon}</span>
          <span className="adm-tipo-nombre">{tramite.tipoTramiteNombre}</span>
        </td>
        <td className="adm-td adm-td-nombre">{tramite.userNombre ?? tramite.userCi}</td>
        <td className="adm-td adm-td-fecha">{formatDate(tramite.fechaSolicitud)}</td>
        <td className="adm-td adm-td-acciones">
          <button
            className="adm-btn adm-btn-ver"
            onClick={() => setShowDetalle(true)}
          >
            👁 Ver
          </button>
          {esActivo ? (
            <>
              {tramite.tipoTramiteId !== 6 && (
                <button
                  className="adm-btn adm-btn-rechazar"
                  disabled={aprobando || rechazando}
                  onClick={() => setShowRechazarModal(true)}
                >
                  {rechazando ? <><ButtonSpinner /> Rechazando...</> : '✕ Rechazar'}
                </button>
              )}
              <button
                className="adm-btn adm-btn-aprobar"
                disabled={aprobando || rechazando}
                onClick={() => aprobar()}
              >
                {aprobando ? <><ButtonSpinner /> Aprobando...</> : '✓ Aprobar'}
              </button>
            </>
          ) : (
            <>
              <button
                className="adm-btn adm-btn-reporte"
                disabled={loading || !tieneReporte}
                onClick={handlePlanilla}
              >
                {loading ? '⏳ Generando...' : '📄 Planilla'}
              </button>
              {esTramitando && (
                <button
                  className="adm-btn adm-btn-archivar"
                  disabled={archivando}
                  onClick={() => archivar()}
                >
                  {archivando ? '⏳ Archivando...' : '📦 Archivar'}
                </button>
              )}
            </>
          )}
        </td>
      </tr>

      {showDetalle && createPortal(
        <div className="detalle-overlay" onClick={() => setShowDetalle(false)}>
          <div className="detalle-modal" onClick={e => e.stopPropagation()}>
            <div className="detalle-header">
              <div className="detalle-header-info">
                <div className="detalle-header-icon-wrap">{icon}</div>
                <div>
                  <h2>{tramite.tipoTramiteNombre}</h2>
                  <span className={`adm-badge ${estado.className}`}>{estado.label}</span>
                </div>
              </div>
              <button className="detalle-close-btn" onClick={() => setShowDetalle(false)}>✕</button>
            </div>

            <div className="detalle-body">
              <div className="detalle-section">
                <h3 className="detalle-section-title">Información general</h3>
                <div className="detalle-grid">
                  <div className="detalle-field">
                    <span className="detalle-label">N° Trámite</span>
                    <span className="detalle-value">#{tramite.id}</span>
                  </div>
                  <div className="detalle-field">
                    <span className="detalle-label">CI Empleado</span>
                    <span className="detalle-value">{tramite.userCi}</span>
                  </div>
                  <div className="detalle-field">
                    <span className="detalle-label">Fecha de solicitud</span>
                    <span className="detalle-value">{formatDate(tramite.fechaSolicitud)}</span>
                  </div>
                  <div className="detalle-field">
                    <span className="detalle-label">Última actualización</span>
                    <span className="detalle-value">{formatDate(tramite.updatedAt)}</span>
                  </div>
                </div>
              </div>

              {tramite.vacaciones && (
                <div className="detalle-section">
                  <h3 className="detalle-section-title">Detalle de vacaciones</h3>
                  <div className="detalle-grid">
                    <div className="detalle-field">
                      <span className="detalle-label">Desde</span>
                      <span className="detalle-value">{formatDate(tramite.vacaciones.desde)}</span>
                    </div>
                    <div className="detalle-field">
                      <span className="detalle-label">Hasta</span>
                      <span className="detalle-value">{formatDate(tramite.vacaciones.hasta)}</span>
                    </div>
                    <div className="detalle-field">
                      <span className="detalle-label">Días totales</span>
                      <span className="detalle-value detalle-value--highlight">{tramite.vacaciones.diasTotales} días</span>
                    </div>
                    {tramite.vacaciones.observaciones && (
                      <div className="detalle-field detalle-field--full">
                        <span className="detalle-label">Observaciones</span>
                        <span className="detalle-value">{tramite.vacaciones.observaciones}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {tramite.diaEspecial && (
                <div className="detalle-section">
                  <h3 className="detalle-section-title">Detalle de día especial</h3>
                  <div className="detalle-grid">
                    <div className="detalle-field">
                      <span className="detalle-label">Fecha</span>
                      <span className="detalle-value">{formatDate(tramite.diaEspecial.fecha)}</span>
                    </div>
                    <div className="detalle-field detalle-field--full">
                      <span className="detalle-label">Motivo</span>
                      <span className="detalle-value">{tramite.diaEspecial.motivo}</span>
                    </div>
                  </div>
                </div>
              )}

              {tramite.utilidades && (
                <div className="detalle-section">
                  <h3 className="detalle-section-title">Detalle de anticipo de utilidades</h3>
                  <div className="detalle-grid">
                    <div className="detalle-field">
                      <span className="detalle-label">Monto solicitado</span>
                      <span className="detalle-value detalle-value--highlight">
                        ${tramite.utilidades.monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </span>
                    </div>
                    <div className="detalle-field detalle-field--full">
                      <span className="detalle-label">Motivo</span>
                      <span className="detalle-value">{tramite.utilidades.motivo}</span>
                    </div>
                  </div>
                </div>
              )}

              {tramite.prestaciones && (
                <div className="detalle-section">
                  <h3 className="detalle-section-title">
                    Detalle de {tramite.prestaciones.esPrestamo ? 'préstamo sobre prestaciones sociales' : 'anticipo de prestaciones sociales'}
                  </h3>
                  <div className="detalle-grid">
                    <div className="detalle-field">
                      <span className="detalle-label">Monto solicitado</span>
                      <span className="detalle-value detalle-value--highlight">
                        Bs {tramite.prestaciones.monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </span>
                    </div>
                    <div className="detalle-field">
                      <span className="detalle-label">Destino</span>
                      <span className="detalle-value">{tramite.prestaciones.destino}</span>
                    </div>
                    {tramite.prestaciones.esPrestamo && tramite.prestaciones.cuotas && (
                      <div className="detalle-field">
                        <span className="detalle-label">Cuotas</span>
                        <span className="detalle-value detalle-value--highlight">
                          {tramite.prestaciones.cuotas} de Bs {tramite.prestaciones.montoCuota?.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} c/u
                        </span>
                      </div>
                    )}
                    {tramite.prestaciones.observaciones && (
                      <div className="detalle-field detalle-field--full">
                        <span className="detalle-label">Observaciones</span>
                        <span className="detalle-value">{tramite.prestaciones.observaciones}</span>
                      </div>
                    )}
                    {tramite.prestaciones.archivoAdjuntoUrl && (
                      <div className="detalle-field detalle-field--full">
                        <span className="detalle-label">Archivo adjunto</span>
                        <a
                          className="detalle-value detalle-adjunto-link"
                          href={`${API_ORIGIN}${tramite.prestaciones.archivoAdjuntoUrl}`}
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

              {tramite.constanciaTrabajo && (
                <div className="detalle-section">
                  <h3 className="detalle-section-title">Detalle de constancia de trabajo</h3>
                  <div className="detalle-grid">
                    <div className="detalle-field">
                      <span className="detalle-label">Tipo</span>
                      <span className="detalle-value detalle-value--highlight">
                        {tramite.constanciaTrabajo.conSueldo ? 'Con sueldo' : 'Sin sueldo'}
                      </span>
                    </div>
                    <div className="detalle-field">
                      <span className="detalle-label">Dirigida a</span>
                      <span className="detalle-value">
                        {tramite.constanciaTrabajo.dirigidoAEspecifico && tramite.constanciaTrabajo.dirigidoA
                          ? tramite.constanciaTrabajo.dirigidoA
                          : 'A quien pueda interesar'}
                      </span>
                    </div>
                  </div>
                </div>
              )}

              {tramite.motivoRechazo && (
                <div className="detalle-section detalle-section--rechazo">
                  <h3 className="detalle-section-title detalle-section-title--rechazo">Motivo de rechazo</h3>
                  <p className="detalle-rechazo-text">"{tramite.motivoRechazo}"</p>
                </div>
              )}
            </div>
          </div>
        </div>,
        document.body
      )}

      {showRechazarModal && createPortal(
        <div
          className="rechazar-overlay"
          onClick={() => !rechazando && setShowRechazarModal(false)}
        >
          <div className="rechazar-modal" onClick={e => e.stopPropagation()}>
            <div className="rechazar-header">
              <h3>Rechazar trámite #{tramite.id}</h3>
              <button
                className="detalle-close-btn"
                disabled={rechazando}
                onClick={() => setShowRechazarModal(false)}
              >
                ✕
              </button>
            </div>
            <div className="rechazar-body">
              <label className="rechazar-label" htmlFor={`razon-rechazo-${tramite.id}`}>
                Motivo del rechazo
              </label>
              <textarea
                id={`razon-rechazo-${tramite.id}`}
                className="rechazar-textarea"
                rows={4}
                value={razonRechazo}
                onChange={e => setRazonRechazo(e.target.value)}
                placeholder="Explica el motivo del rechazo..."
                disabled={rechazando}
              />
            </div>
            <div className="rechazar-footer">
              <button
                className="adm-btn adm-btn-cancelar"
                disabled={rechazando}
                onClick={() => setShowRechazarModal(false)}
              >
                Cancelar
              </button>
              <button
                className="adm-btn adm-btn-rechazar"
                disabled={rechazando || !razonRechazo.trim()}
                onClick={() => rechazar(razonRechazo.trim())}
              >
                {rechazando ? <><ButtonSpinner /> Rechazando...</> : 'Confirmar rechazo'}
              </button>
            </div>
          </div>
        </div>,
        document.body
      )}

      {previewUrl && createPortal(
        <div className="pdf-preview-overlay" onClick={handleClosePreview}>
          <div className="pdf-preview-modal" onClick={e => e.stopPropagation()}>
            <div className="pdf-preview-header">
              <span>Planilla de {reporteLabel} · Trámite #{tramite.id}</span>
              <div className="pdf-preview-actions">
                <a
                  href={previewUrl}
                  download={`planilla_${reporteEndpoint.toLowerCase()}_${tramite.id}.pdf`}
                  className="pdf-download-btn"
                >
                  ⬇ Descargar
                </a>
                <button className="pdf-close-btn" onClick={handleClosePreview}>✕</button>
              </div>
            </div>
            <iframe
              src={previewUrl}
              className="pdf-preview-iframe"
              title={`Planilla ${reporteEndpoint} ${tramite.id}`}
            />
          </div>
        </div>,
        document.body
      )}
    </>
  );
};

// ─── Page ─────────────────────────────────────────────────────────────────────

const TramiteTable: React.FC<{ tramites: TramiteDTO[]; emptyText: string }> = ({ tramites, emptyText }) => {
  if (tramites.length === 0) {
    return (
      <div className="adm-empty adm-empty--inline">
        <span className="adm-empty-icon">📭</span>
        <p>{emptyText}</p>
      </div>
    );
  }

  return (
    <div className="adm-table-wrapper">
      <table className="adm-table">
        <thead>
          <tr>
            <th className="adm-th">#</th>
            <th className="adm-th">Tipo de Trámite</th>
            <th className="adm-th">Nombre</th>
            <th className="adm-th">Fecha Solicitud</th>
            <th className="adm-th">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {tramites.map(t => (
            <TramiteRow key={t.id} tramite={t} />
          ))}
        </tbody>
      </table>
      <p className="adm-count">
        {tramites.length} registro{tramites.length !== 1 ? 's' : ''}
      </p>
    </div>
  );
};

export const AdministracionPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'solicitudes' | 'finalizados'>('solicitudes');

  const [busquedaFinalizados, setBusquedaFinalizados] = useState('');
  const [mostrarBusquedaAvanzada, setMostrarBusquedaAvanzada] = useState(false);
  const [filtroTipoTramite, setFiltroTipoTramite] = useState('');
  const [filtroFechaDesde, setFiltroFechaDesde] = useState('');
  const [filtroFechaHasta, setFiltroFechaHasta] = useState('');

  const { data: tramites = [], isLoading, isError } = useQuery({
    queryKey: ['aprobados'],
    queryFn: fetchAprobados,
    retry: false,
  });

  const activos     = tramites.filter(t => t.estado <= 3);
  const tramitando  = tramites.filter(t => t.estado === 4);
  const finalizados = tramites.filter(t => t.estado === 5);

  const tiposTramiteFinalizados = Array.from(
    new Map(finalizados.map(t => [t.tipoTramiteId, t.tipoTramiteNombre])).entries()
  ).sort((a, b) => a[1].localeCompare(b[1]));

  const hayFiltrosFinalizadosActivos =
    busquedaFinalizados.trim() !== '' ||
    filtroTipoTramite !== '' ||
    filtroFechaDesde !== '' ||
    filtroFechaHasta !== '';

  const finalizadosFiltrados = finalizados.filter(t => {
    if (busquedaFinalizados.trim()) {
      const q = busquedaFinalizados.trim().toLowerCase();
      const matchNombre = (t.userNombre ?? '').toLowerCase().includes(q);
      const matchCi = (t.userCi ?? '').toLowerCase().includes(q);
      if (!matchNombre && !matchCi) return false;
    }
    if (filtroTipoTramite && String(t.tipoTramiteId) !== filtroTipoTramite) return false;
    if (filtroFechaDesde) {
      const desde = new Date(filtroFechaDesde);
      desde.setHours(0, 0, 0, 0);
      if (new Date(t.updatedAt) < desde) return false;
    }
    if (filtroFechaHasta) {
      const hasta = new Date(filtroFechaHasta);
      hasta.setHours(23, 59, 59, 999);
      if (new Date(t.updatedAt) > hasta) return false;
    }
    return true;
  });

  const finalizadosOrdenados = [...finalizadosFiltrados].sort(
    (a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
  );

  const finalizadosMostrados = hayFiltrosFinalizadosActivos
    ? finalizadosOrdenados
    : finalizadosOrdenados.slice(0, 20);

  const limpiarFiltrosFinalizados = () => {
    setBusquedaFinalizados('');
    setFiltroTipoTramite('');
    setFiltroFechaDesde('');
    setFiltroFechaHasta('');
  };

  return (
    <div className="adm-page">
      <div className="adm-header">
        <h1>Administración</h1>
        <p className="adm-subtitle">Gestión y seguimiento de solicitudes.</p>
      </div>

      <div className="adm-tabs">
        <button
          className={`adm-tab-btn ${activeTab === 'solicitudes' ? 'active' : ''}`}
          onClick={() => setActiveTab('solicitudes')}
        >
          Solicitudes
          {tramites.filter(t => t.estado <= 4).length > 0 && (
            <span className="adm-tab-badge">{tramites.filter(t => t.estado <= 4).length}</span>
          )}
        </button>
        <button
          className={`adm-tab-btn ${activeTab === 'finalizados' ? 'active' : ''}`}
          onClick={() => setActiveTab('finalizados')}
        >
          Finalizados
          {finalizados.length > 0 && (
            <span className="adm-tab-badge adm-tab-badge--finalizados">{finalizados.length}</span>
          )}
        </button>
      </div>

      {isLoading && (
        <div className="adm-empty">
          <span className="adm-empty-icon">⏳</span>
          <p>Cargando registros...</p>
        </div>
      )}

      {isError && (
        <div className="adm-empty">
          <span className="adm-empty-icon">⚠️</span>
          <p>Error al cargar los registros.</p>
        </div>
      )}

      {!isLoading && !isError && (
        <div className="adm-tab-content">
          {activeTab === 'solicitudes' && (
            <>
              <div className="adm-section-label">Solicitudes activas</div>
              <TramiteTable tramites={activos} emptyText="No hay solicitudes activas." />

              <div className="adm-section-divider" />

              <div className="adm-section-label adm-section-label--tramitando">Tramitando</div>
              <TramiteTable tramites={tramitando} emptyText="No hay trámites en proceso." />
            </>
          )}

          {activeTab === 'finalizados' && (
            <>
              <div className="adm-section-label adm-section-label--finalizados">Finalizados</div>

              <div className="adm-search-bar">
                <input
                  type="text"
                  className="adm-search-input"
                  placeholder="Buscar por nombre y apellido o CI..."
                  value={busquedaFinalizados}
                  onChange={e => setBusquedaFinalizados(e.target.value)}
                />
                <button
                  type="button"
                  className={`adm-search-adv-toggle ${mostrarBusquedaAvanzada ? 'active' : ''}`}
                  onClick={() => setMostrarBusquedaAvanzada(v => !v)}
                >
                  Búsqueda avanzada {mostrarBusquedaAvanzada ? '▲' : '▼'}
                </button>
              </div>

              {mostrarBusquedaAvanzada && (
                <div className="adm-search-advanced">
                  <div className="adm-search-field">
                    <label>Tipo de trámite</label>
                    <select
                      value={filtroTipoTramite}
                      onChange={e => setFiltroTipoTramite(e.target.value)}
                    >
                      <option value="">Todos</option>
                      {tiposTramiteFinalizados.map(([id, nombre]) => (
                        <option key={id} value={String(id)}>{nombre}</option>
                      ))}
                    </select>
                  </div>
                  <div className="adm-search-field">
                    <label>Fecha finalización desde</label>
                    <input
                      type="date"
                      value={filtroFechaDesde}
                      onChange={e => setFiltroFechaDesde(e.target.value)}
                    />
                  </div>
                  <div className="adm-search-field">
                    <label>Fecha finalización hasta</label>
                    <input
                      type="date"
                      value={filtroFechaHasta}
                      onChange={e => setFiltroFechaHasta(e.target.value)}
                    />
                  </div>
                  <button
                    type="button"
                    className="adm-search-clear-btn"
                    onClick={limpiarFiltrosFinalizados}
                    disabled={!hayFiltrosFinalizadosActivos}
                  >
                    Limpiar filtros
                  </button>
                </div>
              )}

              <p className="adm-search-hint">
                {hayFiltrosFinalizadosActivos
                  ? `${finalizadosMostrados.length} resultado${finalizadosMostrados.length !== 1 ? 's' : ''} encontrado${finalizadosMostrados.length !== 1 ? 's' : ''}`
                  : `Mostrando los ${finalizadosMostrados.length} registros más recientes de ${finalizados.length}`}
              </p>

              <TramiteTable tramites={finalizadosMostrados} emptyText="No hay trámites finalizados." />
            </>
          )}
        </div>
      )}
    </div>
  );
};
