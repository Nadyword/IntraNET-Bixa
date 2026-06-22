import React, { useState } from 'react';
import './NuevaSolicitudModal.css';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';

interface Props {
  onClose: () => void;
}

type TipoTramite = 'vacaciones' | 'diaEspecial' | 'utilidades' | 'sociales' | 'prestaciones';

interface TipoConfig {
  id: TipoTramite;
  label: string;
  icon: string;
  enumId: number;
}

const TIPOS: TipoConfig[] = [
  { id: 'vacaciones',   label: 'Vacaciones',             icon: '✈️', enumId: 4 },
  { id: 'diaEspecial',  label: 'Día Especial',            icon: '📝', enumId: 5 },
  { id: 'utilidades',   label: 'Anticipo de Utilidades',  icon: '💳', enumId: 1 },
  { id: 'sociales',     label: 'Prestaciones Sociales',   icon: '📋', enumId: 2 },
  { id: 'prestaciones', label: 'Préstamo Prestaciones',   icon: '💰', enumId: 3 },
];

interface FormVacaciones {
  fechaInicio: string;
  fechaFin: string;
  diasTotales: number;
  observaciones: string;
}

interface FormDiaEspecial {
  fecha: string;
  motivo: string;
}

interface FormMonto {
  monto: string;
  observaciones: string;
}

function fechaLocalHoy(): string {
  const d = new Date();
  return [
    d.getFullYear(),
    String(d.getMonth() + 1).padStart(2, '0'),
    String(d.getDate()).padStart(2, '0'),
  ].join('-');
}

function calcularDias(inicio: string, fin: string): number {
  if (!inicio || !fin) return 0;
  const d1 = new Date(inicio + 'T00:00:00');
  const d2 = new Date(fin + 'T00:00:00');
  if (d2 < d1) return 0;
  let count = 0;
  const current = new Date(d1);
  while (current <= d2) {
    const day = current.getDay();
    if (day !== 0 && day !== 6) count++;
    current.setDate(current.getDate() + 1);
  }
  return count;
}

export const NuevaSolicitudModal: React.FC<Props> = ({ onClose }) => {
  const user = useAuthStore((s) => s.user);
  const [tipo, setTipo] = useState<TipoTramite | null>(null);
  const [enviado, setEnviado] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const [vacaciones, setVacaciones] = useState<FormVacaciones>({
    fechaInicio: '', fechaFin: '', diasTotales: 0, observaciones: '',
  });
  const [diaEspecial, setDiaEspecial] = useState<FormDiaEspecial>({ fecha: '', motivo: '' });
  const [monto, setMonto] = useState<FormMonto>({ monto: '', observaciones: '' });

  const handleSelectTipo = (t: TipoTramite) => {
    setTipo(t);
    setErrors({});
  };

  const validate = (): boolean => {
    const errs: Record<string, string> = {};

    if (tipo === 'vacaciones') {
      const hoy = fechaLocalHoy();
      if (!vacaciones.fechaInicio) errs.fechaInicio = 'Requerido';
      else if (vacaciones.fechaInicio < hoy) errs.fechaInicio = 'La fecha de inicio no puede ser anterior a hoy';
      if (!vacaciones.fechaFin) errs.fechaFin = 'Requerido';
      if (vacaciones.fechaInicio && vacaciones.fechaFin && vacaciones.fechaFin < vacaciones.fechaInicio)
        errs.fechaFin = 'La fecha de fin debe ser posterior al inicio';
    }
    if (tipo === 'diaEspecial') {
      if (!diaEspecial.fecha) errs.fecha = 'Requerido';
      if (!diaEspecial.motivo.trim()) errs.motivo = 'Requerido';
    }
    if (tipo === 'utilidades' || tipo === 'sociales' || tipo === 'prestaciones') {
      if (!monto.monto || Number(monto.monto) <= 0) errs.monto = 'Ingresa un monto válido';
    }

    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!tipo || !validate()) return;

    setSubmitError(null);

    if (tipo === 'vacaciones') {
      setIsLoading(true);
      try {
        const payload = {
          ci: user?.ci ?? '',
          fechaInicio: vacaciones.fechaInicio,
          fechaFin: vacaciones.fechaFin,
          diasTotales: calcularDias(vacaciones.fechaInicio, vacaciones.fechaFin),
          observaciones: vacaciones.observaciones || null,
        };
        await api.post('/solicitudes/Vacaciones', payload);
        setEnviado(true);
      } catch (err: any) {
        const msg = err?.response?.data?.message ?? 'Error al enviar la solicitud. Intenta de nuevo.';
        setSubmitError(msg);
      } finally {
        setIsLoading(false);
      }
      return;
    }

    // Resto de tipos — pendientes de conectar
    setEnviado(true);
  };

  const dias = vacaciones.fechaInicio && vacaciones.fechaFin
    ? calcularDias(vacaciones.fechaInicio, vacaciones.fechaFin)
    : 0;

  if (enviado) {
    const tipoConfig = TIPOS.find((t) => t.id === tipo)!;
    return (
      <div className="ns-overlay" onClick={onClose}>
        <div className="ns-modal" onClick={(e) => e.stopPropagation()}>
          <div className="ns-success">
            <div className="ns-success-icon">✓</div>
            <h3>Solicitud enviada</h3>
            <p>Tu solicitud de <strong>{tipoConfig.label}</strong> fue registrada y está pendiente de revisión.</p>
            <button className="ns-btn-primary" onClick={onClose}>Entendido</button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="ns-overlay" onClick={onClose}>
      <div className="ns-modal" onClick={(e) => e.stopPropagation()}>
        <button className="ns-close" onClick={onClose}>✕</button>
        <h2 className="ns-title">Nueva Solicitud</h2>
        <p className="ns-subtitle">Selecciona el tipo de trámite que deseas solicitar</p>

        {/* Selector de tipo */}
        <div className="ns-tipos">
          {TIPOS.map((t) => (
            <button
              key={t.id}
              type="button"
              className={`ns-tipo-btn ${tipo === t.id ? 'active' : ''}`}
              onClick={() => handleSelectTipo(t.id)}
            >
              <span className="ns-tipo-icon">{t.icon}</span>
              <span className="ns-tipo-label">{t.label}</span>
            </button>
          ))}
        </div>

        {/* Campos dinámicos */}
        {tipo && (
          <form className="ns-form" onSubmit={handleSubmit}>
            <div className="ns-form-divider" />

            {tipo === 'vacaciones' && (
              <>
                <div className="ns-row">
                  <div className="ns-field">
                    <label>Fecha inicio <span className="ns-required">*</span></label>
                    <input
                      type="date"
                      value={vacaciones.fechaInicio}
                      min={fechaLocalHoy()}
                      onChange={(e) => {
                        const val = e.target.value;
                        setVacaciones((v) => ({
                          ...v,
                          fechaInicio: val,
                          diasTotales: calcularDias(val, v.fechaFin),
                        }));
                      }}
                      className={errors.fechaInicio ? 'input-error' : ''}
                    />
                    {errors.fechaInicio && <span className="ns-error">{errors.fechaInicio}</span>}
                  </div>
                  <div className="ns-field">
                    <label>Fecha fin <span className="ns-required">*</span></label>
                    <input
                      type="date"
                      value={vacaciones.fechaFin}
                      min={vacaciones.fechaInicio || undefined}
                      onChange={(e) => {
                        const val = e.target.value;
                        setVacaciones((v) => ({
                          ...v,
                          fechaFin: val,
                          diasTotales: calcularDias(v.fechaInicio, val),
                        }));
                      }}
                      className={errors.fechaFin ? 'input-error' : ''}
                    />
                    {errors.fechaFin && <span className="ns-error">{errors.fechaFin}</span>}
                  </div>
                </div>

                {vacaciones.fechaInicio && vacaciones.fechaFin && (
                  <div className="ns-dias-badge">
                    📅 Días de disfrute: <strong>{dias}</strong>
                  </div>
                )}

                <div className="ns-field">
                  <label>Observaciones</label>
                  <textarea
                    rows={3}
                    placeholder="Información adicional (opcional)"
                    value={vacaciones.observaciones}
                    onChange={(e) => setVacaciones((v) => ({ ...v, observaciones: e.target.value }))}
                  />
                </div>
              </>
            )}

            {tipo === 'diaEspecial' && (
              <>
                <div className="ns-field">
                  <label>Fecha <span className="ns-required">*</span></label>
                  <input
                    type="date"
                    value={diaEspecial.fecha}
                    onChange={(e) => setDiaEspecial((d) => ({ ...d, fecha: e.target.value }))}
                    className={errors.fecha ? 'input-error' : ''}
                  />
                  {errors.fecha && <span className="ns-error">{errors.fecha}</span>}
                </div>
                <div className="ns-field">
                  <label>Motivo <span className="ns-required">*</span></label>
                  <textarea
                    rows={4}
                    placeholder="Describe el motivo del día especial..."
                    value={diaEspecial.motivo}
                    onChange={(e) => setDiaEspecial((d) => ({ ...d, motivo: e.target.value }))}
                    className={errors.motivo ? 'input-error' : ''}
                  />
                  {errors.motivo && <span className="ns-error">{errors.motivo}</span>}
                </div>
              </>
            )}

            {(tipo === 'utilidades' || tipo === 'sociales' || tipo === 'prestaciones') && (
              <>
                <div className="ns-field">
                  <label>Monto solicitado (USD) <span className="ns-required">*</span></label>
                  <div className="ns-monto-wrapper">
                    <span className="ns-monto-prefix">$</span>
                    <input
                      type="number"
                      min="1"
                      step="0.01"
                      placeholder="0.00"
                      value={monto.monto}
                      onChange={(e) => setMonto((m) => ({ ...m, monto: e.target.value }))}
                      className={errors.monto ? 'input-error' : ''}
                    />
                  </div>
                  {errors.monto && <span className="ns-error">{errors.monto}</span>}
                </div>
                <div className="ns-field">
                  <label>Observaciones</label>
                  <textarea
                    rows={3}
                    placeholder="Información adicional (opcional)"
                    value={monto.observaciones}
                    onChange={(e) => setMonto((m) => ({ ...m, observaciones: e.target.value }))}
                  />
                </div>
              </>
            )}

            {submitError && <p className="ns-error ns-submit-error">{submitError}</p>}

            <div className="ns-actions">
              <button type="button" className="ns-btn-secondary" onClick={onClose} disabled={isLoading}>
                Cancelar
              </button>
              <button type="submit" className="ns-btn-primary" disabled={isLoading}>
                {isLoading ? 'Enviando...' : 'Enviar solicitud'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
