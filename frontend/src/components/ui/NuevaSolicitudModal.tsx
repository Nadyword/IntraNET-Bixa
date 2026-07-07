import React, { useEffect, useState } from 'react';
import './NuevaSolicitudModal.css';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
}

interface Props {
  onClose: () => void;
  onSuccess?: () => void;
}

type TipoTramite = 'vacaciones' | 'diaEspecial' | 'utilidades' | 'prestamoPrestaciones';

interface TipoConfig {
  id: TipoTramite;
  label: string;
  icon: string;
  enumId?: number;
}

const TIPOS: TipoConfig[] = [
  { id: 'vacaciones',           label: 'Vacaciones',                      icon: '✈️', enumId: 4 },
  { id: 'diaEspecial',          label: 'Día Especial',                    icon: '📝', enumId: 5 },
  { id: 'utilidades',           label: 'Anticipo de Utilidades',          icon: '💳', enumId: 1 },
  { id: 'prestamoPrestaciones', label: 'Préstamo Prestaciones Sociales',  icon: '💰' },
];

type SubTipoPrestamo = 'prestamo' | 'sociales';

interface SubTipoConfig {
  id: SubTipoPrestamo;
  label: string;
  enumId: number;
}

const SUBTIPOS_PRESTAMO: SubTipoConfig[] = [
  { id: 'prestamo', label: 'Préstamo sobre Prestaciones Sociales', enumId: 3 },
  { id: 'sociales', label: 'Solicitud de Prestaciones Sociales',   enumId: 2 },
];

const DESTINOS_PRESTAMO: string[] = [
  'Construcción, Adquisición o Mejora de Vivienda',
  'Liberación de Hipoteca',
  'Pensiones Escolares (Art. 142 de la LOTTT, Parágrafo Segundo)',
  'Gastos por Atención Médica y Hospitalaria',
];

const MOTIVOS_DIA_ESPECIAL: string[] = [
  'Cédula de identidad',
  'Libreta militar',
  'Certificado de salud',
  'Licencia de conducir',
  'Pasaporte',
  'Citaciones judiciales, policiales o civiles',
  'Inscripción escolar hijos/trabajador',
  'Carta de soltería',
  'Constancia de concubinato',
  'Constancia de residencia',
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

const fetchDiasHabiles = async (desde: string, hasta: string): Promise<number> => {
  const res = await api.get<ApiResponse<number>>('/solicitudes/DiasHabiles', {
    params: { desde, hasta },
  });
  return res.data.data;
};

export const NuevaSolicitudModal: React.FC<Props> = ({ onClose, onSuccess }) => {
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
  const [subTipoPrestamo, setSubTipoPrestamo] = useState<SubTipoPrestamo | ''>('');
  const [destinoPrestamo, setDestinoPrestamo] = useState('');

  const [diasLoading, setDiasLoading] = useState(false);
  const [diasError, setDiasError] = useState<string | null>(null);

  const rangoInvalido = Boolean(
    vacaciones.fechaInicio && vacaciones.fechaFin && vacaciones.fechaFin < vacaciones.fechaInicio
  );

  useEffect(() => {
    if (tipo !== 'vacaciones' || !vacaciones.fechaInicio || !vacaciones.fechaFin) return;
    if (rangoInvalido) return;

    let cancelado = false;
    setDiasLoading(true);
    setDiasError(null);

    fetchDiasHabiles(vacaciones.fechaInicio, vacaciones.fechaFin)
      .then((dias) => {
        if (cancelado) return;
        setVacaciones((v) => ({ ...v, diasTotales: dias }));
      })
      .catch(() => {
        if (cancelado) return;
        setDiasError('No se pudo calcular los días hábiles. Intenta de nuevo.');
      })
      .finally(() => {
        if (!cancelado) setDiasLoading(false);
      });

    return () => { cancelado = true; };
  }, [tipo, vacaciones.fechaInicio, vacaciones.fechaFin, rangoInvalido]);

  const [diaEspecialChecking, setDiaEspecialChecking] = useState(false);
  const [diaEspecialFechaError, setDiaEspecialFechaError] = useState<string | null>(null);

  useEffect(() => {
    if (tipo !== 'diaEspecial' || !diaEspecial.fecha) return;

    let cancelado = false;
    setDiaEspecialChecking(true);
    setDiaEspecialFechaError(null);

    fetchDiasHabiles(diaEspecial.fecha, diaEspecial.fecha)
      .then((dias) => {
        if (cancelado) return;
        if (dias === 0) {
          setDiaEspecialFechaError('La fecha seleccionada es un feriado o día de descanso. Elige otra fecha.');
        }
      })
      .catch(() => {
        if (cancelado) return;
        setDiaEspecialFechaError('No se pudo validar la fecha. Intenta de nuevo.');
      })
      .finally(() => {
        if (!cancelado) setDiaEspecialChecking(false);
      });

    return () => { cancelado = true; };
  }, [tipo, diaEspecial.fecha]);

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
        errs.fechaFin = 'La fecha de inicio no puede ser posterior a la fecha de fin';
      else if (diasLoading) errs.fechaFin = 'Espera a que se calculen los días hábiles';
      else if (diasError) errs.fechaFin = diasError;
    }
    if (tipo === 'diaEspecial') {
      if (!diaEspecial.fecha) errs.fecha = 'Requerido';
      else if (diaEspecialChecking) errs.fecha = 'Espera a que se valide la fecha';
      else if (diaEspecialFechaError) errs.fecha = diaEspecialFechaError;
      if (!diaEspecial.motivo.trim()) errs.motivo = 'Requerido';
    }
    if (tipo === 'utilidades' || tipo === 'prestamoPrestaciones') {
      if (!monto.monto || Number(monto.monto) <= 0) errs.monto = 'Ingresa un monto válido';
    }
    if (tipo === 'prestamoPrestaciones') {
      if (!subTipoPrestamo) errs.subTipo = 'Requerido';
      if (!destinoPrestamo) errs.destino = 'Requerido';
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
          diasTotales: vacaciones.diasTotales,
          observaciones: vacaciones.observaciones || null,
        };
        await api.post('/solicitudes/Vacaciones', payload);
        setEnviado(true);
        onSuccess?.();
      } catch (err: any) {
        const msg = err?.response?.data?.message ?? 'Error al enviar la solicitud. Intenta de nuevo.';
        setSubmitError(msg);
      } finally {
        setIsLoading(false);
      }
      return;
    }

    if (tipo === 'diaEspecial') {
      setIsLoading(true);
      try {
        const payload = {
          ci: user?.ci ?? '',
          fecha: diaEspecial.fecha,
          motivo: diaEspecial.motivo,
        };
        await api.post('/solicitudes/DiaEspecial', payload);
        setEnviado(true);
        onSuccess?.();
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

  if (enviado) {
    const tipoConfig = TIPOS.find((t) => t.id === tipo)!;
    const tipoLabel = tipo === 'prestamoPrestaciones'
      ? SUBTIPOS_PRESTAMO.find((s) => s.id === subTipoPrestamo)?.label ?? tipoConfig.label
      : tipoConfig.label;
    return (
      <div className="ns-overlay" onClick={onClose}>
        <div className="ns-modal" onClick={(e) => e.stopPropagation()}>
          <div className="ns-success">
            <div className="ns-success-icon">✓</div>
            <h3>Solicitud enviada</h3>
            <p>Tu solicitud de <strong>{tipoLabel}</strong> fue registrada y está pendiente de revisión.</p>
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
                      max={vacaciones.fechaFin || undefined}
                      onChange={(e) => {
                        const val = e.target.value;
                        setVacaciones((v) => ({ ...v, fechaInicio: val }));
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
                        setVacaciones((v) => ({ ...v, fechaFin: val }));
                      }}
                      className={errors.fechaFin ? 'input-error' : ''}
                    />
                    {errors.fechaFin && <span className="ns-error">{errors.fechaFin}</span>}
                  </div>
                </div>

                {rangoInvalido && (
                  <p className="ns-error ns-submit-error">
                    ⚠ La fecha de inicio no puede ser posterior a la fecha de fin.
                  </p>
                )}

                {vacaciones.fechaInicio && vacaciones.fechaFin && !rangoInvalido && (
                  <div className="ns-dias-badge">
                    {diasLoading ? (
                      'Calculando días hábiles...'
                    ) : diasError ? (
                      <span className="ns-error">{diasError}</span>
                    ) : (
                      <>📅 Días de disfrute: <strong>{vacaciones.diasTotales}</strong></>
                    )}
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
                    className={errors.fecha || diaEspecialFechaError ? 'input-error' : ''}
                  />
                  {errors.fecha ? (
                    <span className="ns-error">{errors.fecha}</span>
                  ) : diaEspecial.fecha && (
                    <div className="ns-dias-badge">
                      {diaEspecialChecking ? (
                        'Validando fecha...'
                      ) : diaEspecialFechaError ? (
                        <span className="ns-error">{diaEspecialFechaError}</span>
                      ) : (
                        '✓ Fecha hábil'
                      )}
                    </div>
                  )}
                </div>
                <div className="ns-field">
                  <label>Motivo <span className="ns-required">*</span></label>
                  <select
                    value={diaEspecial.motivo}
                    onChange={(e) => setDiaEspecial((d) => ({ ...d, motivo: e.target.value }))}
                    className={errors.motivo ? 'input-error' : ''}
                  >
                    <option value="">Selecciona un motivo...</option>
                    {MOTIVOS_DIA_ESPECIAL.map((m) => (
                      <option key={m} value={m}>{m}</option>
                    ))}
                  </select>
                  {errors.motivo && <span className="ns-error">{errors.motivo}</span>}
                </div>
              </>
            )}

            {tipo === 'utilidades' && (
              <>
                <div className="ns-field">
                  <label>Monto solicitado<span className="ns-required">*</span></label>
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

            {tipo === 'prestamoPrestaciones' && (
              <>
                <div className="ns-field">
                  <label>Tipo de solicitud <span className="ns-required">*</span></label>
                  <select
                    value={subTipoPrestamo}
                    onChange={(e) => setSubTipoPrestamo(e.target.value as SubTipoPrestamo)}
                    className={errors.subTipo ? 'input-error' : ''}
                  >
                    <option value="">Selecciona el tipo de solicitud...</option>
                    {SUBTIPOS_PRESTAMO.map((s) => (
                      <option key={s.id} value={s.id}>{s.label}</option>
                    ))}
                  </select>
                  {errors.subTipo && <span className="ns-error">{errors.subTipo}</span>}
                </div>

                <div className="ns-field">
                  <label>Cantidad que será destinada para <span className="ns-required">*</span></label>
                  <select
                    value={destinoPrestamo}
                    onChange={(e) => setDestinoPrestamo(e.target.value)}
                    className={errors.destino ? 'input-error' : ''}
                  >
                    <option value="">Selecciona el destino...</option>
                    {DESTINOS_PRESTAMO.map((d) => (
                      <option key={d} value={d}>{d}</option>
                    ))}
                  </select>
                  {errors.destino && <span className="ns-error">{errors.destino}</span>}
                </div>

                <div className="ns-field">
                  <label>Monto solicitado<span className="ns-required">*</span></label>
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
              <button
                type="submit"
                className="ns-btn-primary"
                disabled={
                  isLoading ||
                  (tipo === 'vacaciones' && (diasLoading || rangoInvalido)) ||
                  (tipo === 'diaEspecial' && diaEspecialChecking)
                }
              >
                {isLoading ? 'Enviando...' : 'Enviar solicitud'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
