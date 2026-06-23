import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';
import './AdministracionPage.css';

// ─── Types ────────────────────────────────────────────────────────────────────

interface VacacionesDetalle {
  desde: string;
  hasta: string;
  diasTotales: number;
  observaciones?: string;
}

interface TramiteDTO {
  id: number;
  tipoTramiteId: number;
  tipoTramiteNombre: string;
  userCi: string;
  fechaSolicitud: string;
  updatedAt: string;
  estado: number;
  motivoRechazo?: string;
  vacaciones?: VacacionesDetalle;
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
};

const ESTADO_LABEL: Record<number, { label: string; className: string }> = {
  1: { label: 'Creado',      className: 'badge-creado'     },
  2: { label: 'En Revisión', className: 'badge-revision'   },
  3: { label: 'Aprobado',    className: 'badge-aprobado'   },
  4: { label: 'Archivado',   className: 'badge-archivado'  },
  5: { label: 'Finalizado',  className: 'badge-finalizado' },
  6: { label: 'Rechazado',   className: 'badge-rechazado'  },
};

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchAprobados = async (): Promise<TramiteDTO[]> => {
  const res = await api.get<ApiResponse<TramiteDTO[]>>('/solicitudes/Aprobados');
  return res.data.data;
};

// ─── Helpers ──────────────────────────────────────────────────────────────────

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString('es-VE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

// ─── Row ──────────────────────────────────────────────────────────────────────

const TramiteRow: React.FC<{ tramite: TramiteDTO }> = ({ tramite }) => {
  const [downloading, setDownloading] = useState(false);

  const icon   = TIPO_TRAMITE_ICON[tramite.tipoTramiteId] ?? '📄';
  const estado = ESTADO_LABEL[tramite.estado] ?? { label: String(tramite.estado), className: '' };

  const detalle = tramite.vacaciones
    ? `${formatDate(tramite.vacaciones.desde)} → ${formatDate(tramite.vacaciones.hasta)} (${tramite.vacaciones.diasTotales} días)`
    : '—';

  const handleReporte = async () => {
    setDownloading(true);
    try {
      const response = await api.get(`/solicitudes/Reporte/Vacaciones/${tramite.id}`, {
        responseType: 'blob',
      });
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
      const a = document.createElement('a');
      a.href = url;
      a.download = `reporte_vacaciones_${tramite.id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setDownloading(false);
    }
  };

  return (
    <tr className="adm-row">
      <td className="adm-td adm-td-id">#{tramite.id}</td>
      <td className="adm-td adm-td-tipo">
        <span className="adm-tipo-icon">{icon}</span>
        <span className="adm-tipo-nombre">{tramite.tipoTramiteNombre}</span>
      </td>
      <td className="adm-td adm-td-ci">{tramite.userCi}</td>
      <td className="adm-td adm-td-fecha">{formatDate(tramite.fechaSolicitud)}</td>
      <td className="adm-td adm-td-estado">
        <span className={`adm-badge ${estado.className}`}>{estado.label}</span>
      </td>
      <td className="adm-td adm-td-detalle">
        {tramite.motivoRechazo ? (
          <span className="adm-motivo-rechazo" title={tramite.motivoRechazo}>
            {tramite.motivoRechazo}
          </span>
        ) : (
          <span className="adm-detalle-text">{detalle}</span>
        )}
      </td>
      <td className="adm-td adm-td-acciones">
        <button
          className="adm-btn adm-btn-reporte"
          disabled={downloading || !tramite.vacaciones}
          onClick={handleReporte}
        >
          {downloading ? '⏳ Generando...' : '📄 Reporte'}
        </button>
        <button className="adm-btn adm-btn-archivar" >
          📦 Archivar
        </button>
      </td>
    </tr>
  );
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export const AdministracionPage: React.FC = () => {
  const { data: tramites = [], isLoading, isError } = useQuery({
    queryKey: ['aprobados'],
    queryFn: fetchAprobados,
    retry: false,
  });

  return (
    <div className="adm-page">
      <div className="adm-header">
        <h1>Administración</h1>
        <p className="adm-subtitle">Registro de solicitudes finalizadas.</p>
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

      {!isLoading && !isError && tramites.length === 0 && (
        <div className="adm-empty">
          <span className="adm-empty-icon">📭</span>
          <p>No hay solicitudes finalizadas.</p>
        </div>
      )}

      {!isLoading && !isError && tramites.length > 0 && (
        <div className="adm-table-wrapper">
          <table className="adm-table">
            <thead>
              <tr>
                <th className="adm-th">#</th>
                <th className="adm-th">Tipo de Trámite</th>
                <th className="adm-th">CI Empleado</th>
                <th className="adm-th">Fecha Solicitud</th>
                <th className="adm-th">Estado</th>
                <th className="adm-th">Detalle / Motivo</th>
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
      )}
    </div>
  );
};
