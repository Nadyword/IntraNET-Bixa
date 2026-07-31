import React, { useEffect, useState } from 'react';
import './NuevaSolicitudModal.css';
import api from '../../lib/api';
import { useAuthStore } from '../../store/authStore';
import { useUserProfileStore } from '../../store/userProfileStore';
import { ButtonSpinner } from './ButtonSpinner';
import { ajustarSaldoVacaciones } from '../../lib/vacaciones';

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

type TipoTramite = 'vacaciones' | 'diaEspecial' | 'utilidades' | 'prestamoPrestaciones' | 'constanciaTrabajo';

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
  { id: 'prestamoPrestaciones', label: 'Prestaciones Sociales',  icon: '💰' },
  { id: 'constanciaTrabajo',    label: 'Constancia de Trabajo',           icon: '📃', enumId: 6 },
];

type SubTipoPrestamo = 'prestamo' | 'sociales';

interface SubTipoConfig {
  id: SubTipoPrestamo;
  label: string;
  enumId: number;
}

const SUBTIPOS_PRESTAMO: SubTipoConfig[] = [
  { id: 'prestamo', label: 'Préstamo sobre Prestaciones Sociales', enumId: 3 },
  { id: 'sociales', label: 'Anticipo de Prestaciones Sociales',   enumId: 2 },
];

interface DestinoConfig {
  id: number;
  label: string;
}

const DESTINOS_PRESTAMO: DestinoConfig[] = [
  { id: 1, label: 'Construcción, Adquisición o Mejora de Vivienda' },
  { id: 2, label: 'Liberación de Hipoteca' },
  { id: 3, label: 'Pensiones Escolares' },
  { id: 4, label: 'Gastos por Atención Médica y Hospitalaria' },
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
  cuotas: string;
  archivo: File | null;
}

interface FormUtilidades {
  monto: string;
  motivo: string;
}

interface FormConstanciaTrabajo {
  conSueldo: '' | 'con' | 'sin';
  dirigidoAEspecifico: boolean;
  dirigidoA: string;
}

const CUOTAS_MAXIMAS = 52;
const ARCHIVO_MAX_BYTES = 3 * 1024 * 1024;
const ARCHIVO_EXTENSIONES_PERMITIDAS = ['.pdf', '.jpg', '.jpeg', '.png'];

function validarArchivoAdjunto(archivo: File): string | null {
  const extension = archivo.name.slice(archivo.name.lastIndexOf('.')).toLowerCase();
  if (!ARCHIVO_EXTENSIONES_PERMITIDAS.includes(extension)) {
    return 'El archivo debe ser PDF, JPG o PNG.';
  }
  if (archivo.size > ARCHIVO_MAX_BYTES) {
    return 'El archivo no puede superar los 3 MB.';
  }
  return null;
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
  const profile = useUserProfileStore((s) => s.profile);
  const [tipo, setTipo] = useState<TipoTramite | null>(null);
  const [enviado, setEnviado] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const [vacaciones, setVacaciones] = useState<FormVacaciones>({
    fechaInicio: '', fechaFin: '', diasTotales: 0, observaciones: '',
  });
  const [diaEspecial, setDiaEspecial] = useState<FormDiaEspecial>({ fecha: '', motivo: '' });
  const [monto, setMonto] = useState<FormMonto>({ monto: '', observaciones: '', cuotas: '', archivo: null });
  const [utilidades, setUtilidades] = useState<FormUtilidades>({ monto: '', motivo: '' });
  const [constanciaTrabajo, setConstanciaTrabajo] = useState<FormConstanciaTrabajo>({
    conSueldo: '', dirigidoAEspecifico: false, dirigidoA: '',
  });
  const [subTipoPrestamo, setSubTipoPrestamo] = useState<SubTipoPrestamo | ''>('');
  const [destinoPrestamo, setDestinoPrestamo] = useState('');

  const [montoDisponibleUtilidades, setMontoDisponibleUtilidades] = useState<number | null>(null);
  const [utilidadesLoading, setUtilidadesLoading] = useState(false);
  const [utilidadesError, setUtilidadesError] = useState<string | null>(null);

  useEffect(() => {
    if (tipo !== 'utilidades' || !user?.ci) return;

    let cancelado = false;
    setUtilidadesLoading(true);
    setUtilidadesError(null);

    api.get<ApiResponse<number | null>>(`/usersProfit/${user.ci}/Utilidades`)
      .then((res) => {
        if (cancelado) return;
        if (res.data.success) {
          setMontoDisponibleUtilidades(res.data.data ?? 0);
        } else {
          setUtilidadesError('No se pudo obtener el monto disponible.');
        }
      })
      .catch((err) => {
        if (cancelado) return;
        if (err.response?.status === 404) {
          setMontoDisponibleUtilidades(0);
        } else {
          setUtilidadesError('No se pudo obtener el monto disponible.');
        }
      })
      .finally(() => {
        if (!cancelado) setUtilidadesLoading(false);
      });

    return () => { cancelado = true; };
  }, [tipo, user?.ci]);

  const [diasDisponiblesVacaciones, setDiasDisponiblesVacaciones] = useState<number | null>(null);
  const [vacacionesSaldoLoading, setVacacionesSaldoLoading] = useState(false);
  const [vacacionesSaldoError, setVacacionesSaldoError] = useState<string | null>(null);

  useEffect(() => {
    if (tipo !== 'vacaciones' || !profile?.codEmp) return;

    let cancelado = false;
    setVacacionesSaldoLoading(true);
    setVacacionesSaldoError(null);

    api.get<ApiResponse<{ disponibleAcumulado: number | null }[]>>(`/usersProfit/${profile.codEmp}/Vacaciones`)
      .then((res) => {
        if (cancelado) return;
        if (res.data.success) {
          const registros = res.data.data ?? [];
          const disponibleReal = registros.length > 0 ? registros[registros.length - 1].disponibleAcumulado ?? 0 : 0;
          setDiasDisponiblesVacaciones(ajustarSaldoVacaciones(disponibleReal));
        } else {
          setVacacionesSaldoError('No se pudo obtener el saldo de vacaciones disponible.');
        }
      })
      .catch((err) => {
        if (cancelado) return;
        if (err.response?.status === 404) {
          setDiasDisponiblesVacaciones(0);
        } else {
          setVacacionesSaldoError('No se pudo obtener el saldo de vacaciones disponible.');
        }
      })
      .finally(() => {
        if (!cancelado) setVacacionesSaldoLoading(false);
      });

    return () => { cancelado = true; };
  }, [tipo, profile?.codEmp]);

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
      if (!errs.fechaFin && !diasLoading && !diasError && vacaciones.fechaInicio && vacaciones.fechaFin) {
        if (vacacionesSaldoLoading) errs.fechaFin = 'Espera a que se consulte tu saldo de vacaciones disponible';
        else if (vacacionesSaldoError) errs.fechaFin = vacacionesSaldoError;
        else if (diasDisponiblesVacaciones === null) errs.fechaFin = 'No se pudo determinar tu saldo disponible, intenta de nuevo';
        else if (vacaciones.diasTotales > diasDisponiblesVacaciones)
          errs.fechaFin = `Solo tienes ${diasDisponiblesVacaciones} día(s) disponible(s) para solicitar`;
      }
    }
    if (tipo === 'diaEspecial') {
      if (!diaEspecial.fecha) errs.fecha = 'Requerido';
      else if (diaEspecialChecking) errs.fecha = 'Espera a que se valide la fecha';
      else if (diaEspecialFechaError) errs.fecha = diaEspecialFechaError;
      if (!diaEspecial.motivo.trim()) errs.motivo = 'Requerido';
    }
    if (tipo === 'prestamoPrestaciones') {
      if (!monto.monto || Number(monto.monto) <= 0) errs.monto = 'Ingresa un monto válido';
      if (!subTipoPrestamo) errs.subTipo = 'Requerido';
      if (!destinoPrestamo) errs.destino = 'Requerido';
      if (subTipoPrestamo === 'prestamo') {
        const cuotasNum = Number(monto.cuotas);
        if (!monto.cuotas || cuotasNum <= 0 || cuotasNum > CUOTAS_MAXIMAS) {
          errs.cuotas = `Ingresa una cantidad de cuotas entre 1 y ${CUOTAS_MAXIMAS}`;
        }
      }
      if (monto.archivo) {
        const archivoError = validarArchivoAdjunto(monto.archivo);
        if (archivoError) errs.archivo = archivoError;
      }
    }
    if (tipo === 'utilidades') {
      if (!utilidades.monto || Number(utilidades.monto) <= 0) errs.monto = 'Ingresa un monto válido';
      else if (montoDisponibleUtilidades === null) errs.monto = 'No se pudo determinar el monto disponible, intenta de nuevo';
      else if (Number(utilidades.monto) > montoDisponibleUtilidades) errs.monto = `El monto no puede superar $${montoDisponibleUtilidades.toLocaleString('es-VE')}`;
      if (!utilidades.motivo.trim()) errs.motivo = 'Requerido';
    }
    if (tipo === 'constanciaTrabajo') {
      if (!constanciaTrabajo.conSueldo) errs.conSueldo = 'Requerido';
      if (constanciaTrabajo.dirigidoAEspecifico && !constanciaTrabajo.dirigidoA.trim()) errs.dirigidoA = 'Requerido';
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

    if (tipo === 'utilidades') {
      setIsLoading(true);
      try {
        const payload = {
          ci: user?.ci ?? '',
          monto: Number(utilidades.monto),
          motivo: utilidades.motivo,
        };
        await api.post('/solicitudes/Utilidades', payload);
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

    if (tipo === 'constanciaTrabajo') {
      setIsLoading(true);
      try {
        const payload = {
          ci: user?.ci ?? '',
          conSueldo: constanciaTrabajo.conSueldo === 'con',
          dirigidoAEspecifico: constanciaTrabajo.dirigidoAEspecifico,
          dirigidoA: constanciaTrabajo.dirigidoAEspecifico ? constanciaTrabajo.dirigidoA : null,
        };
        await api.post('/solicitudes/ConstanciaTrabajo', payload);
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

    if (tipo === 'prestamoPrestaciones') {
      setIsLoading(true);
      try {
        const subTipo = SUBTIPOS_PRESTAMO.find((s) => s.id === subTipoPrestamo)!;
        const formData = new FormData();
        formData.append('ci', user?.ci ?? '');
        formData.append('tipoTramiteId', String(subTipo.enumId));
        formData.append('monto', String(Number(monto.monto)));
        formData.append('destino', String(Number(destinoPrestamo)));
        if (monto.observaciones) formData.append('observaciones', monto.observaciones);
        if (subTipo.id === 'prestamo' && monto.cuotas) formData.append('cuotas', monto.cuotas);
        if (monto.archivo) formData.append('archivo', monto.archivo);

        await api.post('/solicitudes/Prestaciones', formData);
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

                <div className="ns-dias-badge">
                  {vacacionesSaldoLoading
                    ? 'Consultando saldo disponible...'
                    : vacacionesSaldoError
                    ? <span className="ns-error">{vacacionesSaldoError}</span>
                    : diasDisponiblesVacaciones !== null
                    ? <>🏖️ Días disponibles para solicitar: <strong>{diasDisponiblesVacaciones}</strong></>
                    : ''}
                </div>

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
                  <label>Monto solicitado <span className="ns-required">*</span></label>
                  <div className="ns-monto-wrapper">
                    <span className="ns-monto-prefix">Bs</span>
                    <input
                      type="number"
                      min="1"
                      max={montoDisponibleUtilidades ?? undefined}
                      value={utilidades.monto}
                      onChange={(e) => setUtilidades((u) => ({ ...u, monto: e.target.value }))}
                      className={errors.monto ? 'input-error' : ''}
                    />
                  </div>
                  <span className="ns-dias-badge">
                    {utilidadesLoading
                      ? 'Consultando monto disponible...'
                      : utilidadesError
                      ? utilidadesError
                      : montoDisponibleUtilidades !== null
                      ? `Monto máximo disponible: $${montoDisponibleUtilidades.toLocaleString('es-VE')}`
                      : ''}
                  </span>
                  {errors.monto && <span className="ns-error">{errors.monto}</span>}
                </div>
                <div className="ns-field">
                  <label>Motivo de la solicitud <span className="ns-required">*</span></label>
                  <textarea
                    rows={3}
                    placeholder="Describe el motivo de tu solicitud"
                    value={utilidades.motivo}
                    onChange={(e) => setUtilidades((u) => ({ ...u, motivo: e.target.value }))}
                    className={errors.motivo ? 'input-error' : ''}
                  />
                  {errors.motivo && <span className="ns-error">{errors.motivo}</span>}
                </div>
              </>
            )}

            {tipo === 'constanciaTrabajo' && (
              <>
                <div className="ns-field">
                  <label>Tipo de constancia <span className="ns-required">*</span></label>
                  <div className="ns-radio-group">
                    <label className="ns-radio-option">
                      <input
                        type="radio"
                        name="conSueldo"
                        checked={constanciaTrabajo.conSueldo === 'con'}
                        onChange={() => setConstanciaTrabajo((c) => ({ ...c, conSueldo: 'con' }))}
                      />
                      Con sueldo
                    </label>
                    <label className="ns-radio-option">
                      <input
                        type="radio"
                        name="conSueldo"
                        checked={constanciaTrabajo.conSueldo === 'sin'}
                        onChange={() => setConstanciaTrabajo((c) => ({ ...c, conSueldo: 'sin' }))}
                      />
                      Sin sueldo
                    </label>
                  </div>
                  {errors.conSueldo && <span className="ns-error">{errors.conSueldo}</span>}
                </div>

                <div className="ns-field">
                  <label className="ns-checkbox-row">
                    <input
                      type="checkbox"
                      checked={constanciaTrabajo.dirigidoAEspecifico}
                      onChange={(e) => {
                        const checked = e.target.checked;
                        setConstanciaTrabajo((c) => ({ ...c, dirigidoAEspecifico: checked, dirigidoA: checked ? c.dirigidoA : '' }));
                      }}
                    />
                    Dirigir a una persona o entidad en específico
                  </label>
                </div>

                {constanciaTrabajo.dirigidoAEspecifico && (
                  <div className="ns-field">
                    <label>Dirigido a <span className="ns-required">*</span></label>
                    <input
                      type="text"
                      placeholder="Ej. A quien pueda interesar"
                      value={constanciaTrabajo.dirigidoA}
                      onChange={(e) => setConstanciaTrabajo((c) => ({ ...c, dirigidoA: e.target.value }))}
                      className={errors.dirigidoA ? 'input-error' : ''}
                    />
                    {errors.dirigidoA && <span className="ns-error">{errors.dirigidoA}</span>}
                  </div>
                )}
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
                      <option key={d.id} value={d.id}>{d.label}</option>
                    ))}
                  </select>
                  {errors.destino && <span className="ns-error">{errors.destino}</span>}
                </div>

                <div className="ns-field">
                  <label>Monto solicitado<span className="ns-required">*</span></label>
                  <div className="ns-monto-wrapper">
                    <span className="ns-monto-prefix">Bs</span>
                    <input
                      type="number"
                      min="1"
                      value={monto.monto}
                      onChange={(e) => setMonto((m) => ({ ...m, monto: e.target.value }))}
                      className={errors.monto ? 'input-error' : ''}
                    />
                  </div>
                  {errors.monto && <span className="ns-error">{errors.monto}</span>}
                </div>

                {subTipoPrestamo === 'prestamo' && (
                  <div className="ns-field">
                    <label>Cuotas <span className="ns-required">*</span></label>
                    <input
                      type="number"
                      min="1"
                      max={CUOTAS_MAXIMAS}
                      placeholder={`Máximo ${CUOTAS_MAXIMAS} cuotas`}
                      value={monto.cuotas}
                      onChange={(e) => setMonto((m) => ({ ...m, cuotas: e.target.value }))}
                      className={errors.cuotas ? 'input-error' : ''}
                    />
                    {errors.cuotas && <span className="ns-error">{errors.cuotas}</span>}
                    {!errors.cuotas && monto.monto && Number(monto.monto) > 0 && monto.cuotas && Number(monto.cuotas) > 0 && (
                      <span className="ns-dias-badge">
                        💰 {monto.cuotas} cuotas de Bs {(Number(monto.monto) / Number(monto.cuotas)).toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} c/u
                      </span>
                    )}
                  </div>
                )}

                <div className="ns-field">
                  <label>Observaciones</label>
                  <textarea
                    rows={3}
                    placeholder="Información adicional (opcional)"
                    value={monto.observaciones}
                    onChange={(e) => setMonto((m) => ({ ...m, observaciones: e.target.value }))}
                  />
                </div>

                <div className="ns-field">
                  <label>Adjuntar archivo (opcional)</label>
                  <input
                    type="file"
                    accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
                    onChange={(e) => {
                      const archivo = e.target.files?.[0] ?? null;
                      setMonto((m) => ({ ...m, archivo }));
                    }}
                    className={errors.archivo ? 'input-error' : ''}
                  />
                  <span className="ns-hint">PDF, JPG o PNG · máximo 3 MB</span>
                  {errors.archivo && <span className="ns-error">{errors.archivo}</span>}
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
                {isLoading ? <><ButtonSpinner /> Enviando...</> : 'Enviar solicitud'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
