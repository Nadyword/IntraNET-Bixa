import React, { useState } from 'react';
import './NuevaSolicitudModal.css';

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

function calcularDias(inicio: string, fin: string): number {
  if (!inicio || !fin) return 0;
  const d1 = new Date(inicio);
  const d2 = new Date(fin);
  const diff = Math.ceil((d2.getTime() - d1.getTime()) / (1000 * 60 * 60 * 24)) + 1;
  return diff > 0 ? diff : 0;
}

export const NuevaSolicitudModal: React.FC<Props> = ({ onClose }) => {
  const [tipo, setTipo] = useState<TipoTramite | null>(null);
  const [enviado, setEnviado] = useState(false);
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
      if (!vacaciones.fechaInicio) errs.fechaInicio = 'Requerido';
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

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!tipo || !validate()) return;

    const tipoConfig = TIPOS.find((t) => t.id === tipo)!;

    // TODO: conectar con backend — POST /api/tramites
    const payload = {
      tipoTramiteId: tipoConfig.enumId,
      ...(tipo === 'vacaciones' && {
        fechaInicio: vacaciones.fechaInicio,
        fechaFin: vacaciones.fechaFin,
        diasTotales: vacaciones.diasTotales,
        observaciones: vacaciones.observaciones || null,
      }),
      ...(tipo === 'diaEspecial' && {
        fecha: diaEspecial.fecha,
        motivo: diaEspecial.motivo,
      }),
      ...((tipo === 'utilidades' || tipo === 'sociales' || tipo === 'prestaciones') && {
        monto: Number(monto.monto),
        observaciones: monto.observaciones || null,
      }),
    };
    console.log('[NuevaSolicitud] payload:', payload);

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

                {dias > 0 && (
                  <div className="ns-dias-badge">
                    📅 {dias} {dias === 1 ? 'día' : 'días'} solicitados
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

            <div className="ns-actions">
              <button type="button" className="ns-btn-secondary" onClick={onClose}>
                Cancelar
              </button>
              <button type="submit" className="ns-btn-primary">
                Enviar solicitud
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
