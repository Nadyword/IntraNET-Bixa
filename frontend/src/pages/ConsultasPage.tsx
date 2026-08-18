import { useState } from 'react';
import { useUserProfileStore } from '../store/userProfileStore';
import { api } from '../lib/api';
import { ajustarSaldoVacaciones } from '../lib/vacaciones';
import { ButtonSpinner } from '../components/ui/ButtonSpinner';
import './ConsultasPage.css';

interface Vacacion {
  codEmp: string;
  nombre: string;
  desde: string | null;
  hasta: string | null;
  dias: number | null;
  disponibleAcumulado: number | null;
}

interface DiaEspecial {
  desNovedadDia: string | null;
  fechaRegistro: string | null;
  autorisadoPor: string | null;
  desde: string | null;
  hasta: string | null;
  dias: number | null;
  comentario: string | null;
}

interface ConsultaHc {
  nombreCompleto: string | null;
  parentesco: string | null;
  primaTrimBs: number | null;
  pagoBixaTrim: number | null;
  mes1E071: number | null;
  mes2E071: number | null;
  mes3E071: number | null;
}

interface HcMesRegistro {
  ci: string;
  nombreCompleto: string | null;
  mes1: number;
  mes2: number;
  mes3: number;
  primaTrimBs: number;
  updatedAt: string;
  modifiedByCi: string | null;
}

interface ConsultaArc {
  mes: number | null;
  remuneracion: number | null;
  porcentRetencion: number | null;
  impuestoRetenido: number | null;
  remuneracionAcumulada: number | null;
  impuestoRetenidoAcum: number | null;
}

const NOMBRES_MESES = [
  'ENERO', 'FEBRERO', 'MARZO', 'ABRIL', 'MAYO', 'JUNIO',
  'JULIO', 'AGOSTO', 'SEPTIEMBRE', 'OCTUBRE', 'NOVIEMBRE', 'DICIEMBRE',
];

const formatFecha = (fecha: string | null): string => {
  if (!fecha) return '—';
  const d = new Date(fecha);
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('es-VE', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatMonto = (monto: number | null | undefined): string =>
  typeof monto === 'number'
    ? monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    : '—';

export const ConsultasPage: React.FC = () => {

  const { profile } = useUserProfileStore();

  // null = aún no consultado, [] = consultado sin registros, [...] = con datos
  const [vacaciones, setVacaciones] = useState<Vacacion[] | null>(null);
  const [loadingVacaciones, setLoadingVacaciones] = useState(false);
  const [modalVacaciones, setModalVacaciones] = useState(false);
  const [errorVacaciones, setErrorVacaciones] = useState<string | null>(null);

  const [diasEspeciales, setDiasEspeciales] = useState<DiaEspecial[] | null>(null);
  const [loadingDiasEsp, setLoadingDiasEsp] = useState(false);
  const [modalDiasEsp, setModalDiasEsp] = useState(false);
  const [errorDiasEsp, setErrorDiasEsp] = useState<string | null>(null);

  // null = aún no consultado, undefined = consultado sin registros, number = monto disponible
  const [montoUtilidades, setMontoUtilidades] = useState<number | null | undefined>(null);
  const [loadingUtilidades, setLoadingUtilidades] = useState(false);
  const [errorUtilidades, setErrorUtilidades] = useState<string | null>(null);

  // null = aún no consultado, undefined = consultado sin registros, number = monto disponible
  const [montoPrestaciones, setMontoPrestaciones] = useState<number | null | undefined>(null);
  const [loadingPrestaciones, setLoadingPrestaciones] = useState(false);
  const [errorPrestaciones, setErrorPrestaciones] = useState<string | null>(null);

  // null = aún no consultado, undefined = consultado sin registros, ConsultaHc = con datos
  const [hcCobertura1, setHcCobertura1] = useState<ConsultaHc | null | undefined>(null);
  // null = aún no consultado, undefined = consultado sin registros, ConsultaHc[] = con datos (puede haber varios familiares)
  const [hcCobertura2, setHcCobertura2] = useState<ConsultaHc[] | null | undefined>(null);
  const [loadingHc, setLoadingHc] = useState(false);
  const [modalHc, setModalHc] = useState(false);
  const [errorHc, setErrorHc] = useState<string | null>(null);

  const [mes1Input, setMes1Input] = useState('');
  const [mes2Input, setMes2Input] = useState('');
  const [mes3Input, setMes3Input] = useState('');
  const [hcSaving, setHcSaving] = useState(false);
  const [hcSaveError, setHcSaveError] = useState<string | null>(null);
  const [hcSaveSuccess, setHcSaveSuccess] = useState(false);

  // null = aún no consultado, undefined = consultado sin registros, [...] = con datos
  const [arcData, setArcData] = useState<ConsultaArc[] | null | undefined>(null);
  const [loadingArc, setLoadingArc] = useState(false);
  const [modalArc, setModalArc] = useState(false);
  const [errorArc, setErrorArc] = useState<string | null>(null);
  const [anioArc, setAnioArc] = useState(new Date().getFullYear());
  const [descargandoArc, setDescargandoArc] = useState(false);
  const [errorDescargaArc, setErrorDescargaArc] = useState<string | null>(null);

  const ultimoDisponibleReal = vacaciones !== null && vacaciones.length > 0
    ? vacaciones[vacaciones.length - 1].disponibleAcumulado
    : null;
  const ultimoDisponible = ultimoDisponibleReal !== null
    ? ajustarSaldoVacaciones(ultimoDisponibleReal)
    : null;

  const diasEspUsados = diasEspeciales !== null
    ? diasEspeciales.reduce((sum, d) => sum + (d.dias ?? 0), 0)
    : null;
  const diasEspDisponibles = diasEspUsados !== null ? 4 - diasEspUsados : null;

  const handleConsultarVacaciones = async () => {
    if (!profile?.codEmp) return;
    setLoadingVacaciones(true);
    setErrorVacaciones(null);
    try {
      const { data } = await api.get(`/usersProfit/${profile.codEmp}/Vacaciones`);
      if (data.success) {
        setVacaciones(data.data);
        setModalVacaciones(true);
      } else {
        setErrorVacaciones(data.message || 'No se encontró información');
      }
    } catch (err: any) {
      if (err.response?.status === 404) {
        setVacaciones([]);
      } else {
        setErrorVacaciones('Error al consultar vacaciones');
      }
    } finally {
      setLoadingVacaciones(false);
    }
  };

  const handleConsultarDiasEsp = async () => {
    if (!profile?.codEmp) return;
    setLoadingDiasEsp(true);
    setErrorDiasEsp(null);
    try {
      const { data } = await api.get(`/usersProfit/${profile.codEmp}/DiasEspeciales`);
      if (data.success) {
        setDiasEspeciales(data.data);
        setModalDiasEsp(true);
      } else {
        setErrorDiasEsp(data.message || 'No se encontró información');
      }
    } catch (err: any) {
      if (err.response?.status === 404) {
        setDiasEspeciales([]);
      } else {
        setErrorDiasEsp('Error al consultar días especiales');
      }
    } finally {
      setLoadingDiasEsp(false);
    }
  };

  const handleConsultarUtilidades = async () => {
    if (!profile?.ci) return;
    setLoadingUtilidades(true);
    setErrorUtilidades(null);
    try {
      const { data } = await api.get(`/usersProfit/${profile.ci}/Utilidades`);
      if (data.success) {
        setMontoUtilidades(data.data);
      } else {
        setErrorUtilidades(data.message || 'No se encontró información');
      }
    } catch (err: any) {
      if (err.response?.status === 404) {
        setMontoUtilidades(undefined);
      } else {
        setErrorUtilidades('Error al consultar utilidades');
      }
    } finally {
      setLoadingUtilidades(false);
    }
  };

  const handleConsultarPrestaciones = async () => {
    if (!profile?.ci) return;
    setLoadingPrestaciones(true);
    setErrorPrestaciones(null);
    try {
      const { data } = await api.get(`/usersProfit/${profile.ci}/PrestacionesSociales`);
      if (data.success) {
        setMontoPrestaciones(data.data);
      } else {
        setErrorPrestaciones(data.message || 'No se encontró información');
      }
    } catch (err: any) {
      if (err.response?.status === 404) {
        setMontoPrestaciones(undefined);
      } else {
        setErrorPrestaciones('Error al consultar prestaciones sociales');
      }
    } finally {
      setLoadingPrestaciones(false);
    }
  };

  const handleConsultarHc = async () => {
    if (!profile?.ci) return;
    setLoadingHc(true);
    setErrorHc(null);
    setHcSaveError(null);
    setHcSaveSuccess(false);
    try {
      const normalizedCi = profile.ci;
      const [cob1, cob2, registro] = await Promise.allSettled([
        api.get(`/usersProfit/${normalizedCi}/ConsultaHc/Cobertura1`),
        api.get(`/usersProfit/${normalizedCi}/ConsultaHc/Cobertura2`),
        api.get(`/hc/${normalizedCi}`),
      ]);

      if (cob1.status === 'fulfilled' && cob1.value.data.success) {
        setHcCobertura1(cob1.value.data.data);
      } else {
        setHcCobertura1(undefined);
      }

      if (cob2.status === 'fulfilled' && cob2.value.data.success) {
        setHcCobertura2(cob2.value.data.data);
      } else {
        setHcCobertura2(undefined);
      }

      if (registro.status === 'fulfilled' && registro.value.data.success) {
        const r: HcMesRegistro = registro.value.data.data;
        setMes1Input(r.mes1 ? String(r.mes1) : '');
        setMes2Input(r.mes2 ? String(r.mes2) : '');
        setMes3Input(r.mes3 ? String(r.mes3) : '');
      } else {
        setMes1Input('');
        setMes2Input('');
        setMes3Input('');
      }

      setModalHc(true);
    } catch {
      setErrorHc('Error al consultar HC');
    } finally {
      setLoadingHc(false);
    }
  };

  const parseMonto = (valor: string): number => {
    const parsed = parseFloat(valor.replace(',', '.'));
    return Number.isFinite(parsed) ? parsed : 0;
  };

  const sumaMeses = parseMonto(mes1Input) + parseMonto(mes2Input) + parseMonto(mes3Input);
  const primaTrimBsHc = hcCobertura1?.primaTrimBs ?? 0;
  const sumaNoCoincideHc = sumaMeses !== primaTrimBsHc;

  const handleGuardarHc = async () => {
    if (!profile?.ci || sumaNoCoincideHc) return;
    setHcSaving(true);
    setHcSaveError(null);
    setHcSaveSuccess(false);
    try {
      const { data } = await api.put(`/hc/${profile.ci}`, {
        mes1: parseMonto(mes1Input),
        mes2: parseMonto(mes2Input),
        mes3: parseMonto(mes3Input),
      });
      if (data.success) {
        setHcSaveSuccess(true);
      } else {
        setHcSaveError(data.message || 'No se pudo guardar la información.');
      }
    } catch (err: any) {
      setHcSaveError(err.response?.data?.message || 'Error al guardar los valores.');
    } finally {
      setHcSaving(false);
    }
  };

  const handleConsultarArc = async (anio: number = new Date().getFullYear()) => {
    if (!profile?.ci) return;
    setLoadingArc(true);
    setErrorArc(null);
    try {
      const { data } = await api.get(`/usersProfit/${profile.ci}/ConsultaArc`, { params: { anio } });
      setAnioArc(anio);
      if (data.success) {
        setArcData(data.data);
        setModalArc(true);
      } else {
        setArcData(undefined);
        setModalArc(true);
      }
    } catch (err: any) {
      if (err.response?.status === 404) {
        setAnioArc(anio);
        setArcData(undefined);
        setModalArc(true);
      } else {
        setErrorArc('Error al consultar ARC');
      }
    } finally {
      setLoadingArc(false);
    }
  };

  const handleToggleAnioArc = () => {
    const anioActual = new Date().getFullYear();
    handleConsultarArc(anioArc === anioActual ? anioActual - 1 : anioActual);
  };

  const handleDescargarArcReporte = async () => {
    if (!profile?.ci) return;
    setDescargandoArc(true);
    setErrorDescargaArc(null);
    try {
      const response = await api.get(`/usersProfit/${profile.ci}/ConsultaArc/Reporte`, {
        params: { anio: anioArc },
        responseType: 'blob',
      });
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
      const link = document.createElement('a');
      link.href = url;
      link.download = `ARC_${profile.ci}_${anioArc}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch {
      setErrorDescargaArc('Error al generar el reporte de ARC');
    } finally {
      setDescargandoArc(false);
    }
  };

  return (
    <div className="consultas-page">
      <div className="consultas-header">
        <div className="consultas-header-text">
          <h1>Consultas</h1>
          <p>Consulta en tiempo real el estado de tus beneficios y conceptos laborales</p>
        </div>
      </div>

      <div className="stats-grid-3">
        {/* Vacaciones */}
        <div className="stat-card">
          <h4 className="stat-title">Vacaciones</h4>
          <div className="stat-icon red">🏖️</div>
          <div className="stat-info">
            <div className="stat-sub">
              {vacaciones === null ? (
                'Sin consultar'
              ) : vacaciones.length === 0 ? (
                'Sin registros'
              ) : (
                <>
                  <div>Días de vacaciones disponibles: <strong>{ultimoDisponibleReal}</strong></div>
                  <div>Días disponibles menos vacaciones colectivas: <strong>{ultimoDisponible}</strong></div>
                </>
              )}
            </div>
            {errorVacaciones && <div className="stat-error">{errorVacaciones}</div>}
            <button
              className="consultar-btn"
              onClick={handleConsultarVacaciones}
              disabled={loadingVacaciones}
            >
              {loadingVacaciones ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
        </div>

        {/* Días de permiso especial */}
        <div className="stat-card">
          <h4 className="stat-title">Días de permiso especial</h4>
          <div className="stat-icon black">🗓️</div>
          <div className="stat-info">
            <h3>{diasEspDisponibles !== null ? diasEspDisponibles : '—'}</h3>
            <div className="stat-sub">
              {diasEspeciales === null
                ? 'Sin consultar'
                : `${diasEspUsados} de 4 días usados este año`}
            </div>
            {errorDiasEsp && <div className="stat-error">{errorDiasEsp}</div>}
            <button
              className="consultar-btn"
              onClick={handleConsultarDiasEsp}
              disabled={loadingDiasEsp}
            >
              {loadingDiasEsp ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
        </div>

        {/* Consulta de HC */}
        <div className="stat-card">
          <h4 className="stat-title">HC</h4>
          <div className="stat-icon red">🏥</div>
          <div className="stat-info">
            <p>Prima trimestral y pago a Bixa</p>
            <div className="stat-sub">
              {hcCobertura1 === null && hcCobertura2 === null
                ? 'Sin consultar'
                : !hcCobertura1 && !hcCobertura2
                ? 'Sin registros'
                : 'Consultado'}
            </div>
            {errorHc && <div className="stat-error">{errorHc}</div>}
            <button
              className="consultar-btn"
              onClick={handleConsultarHc}
              disabled={loadingHc}
            >
              {loadingHc ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
        </div>

        {/* Consulta de Prestaciones Sociales */}
        <div className="stat-card">
          <h4 className="stat-title">Prestaciones Sociales</h4>
          <div className="stat-icon green">💰</div>
          <div className="stat-info">
            <h3>
              {typeof montoPrestaciones === 'number'
                ? `Bs. ${montoPrestaciones.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                : '—'}
            </h3>
            <div className="stat-sub">
              {montoPrestaciones === null
                ? 'Sin consultar'
                : montoPrestaciones === undefined
                ? 'Sin registros'
                : 'Monto disponible'}
            </div>
            {errorPrestaciones && <div className="stat-error">{errorPrestaciones}</div>}
            <button
              className="consultar-btn"
              onClick={handleConsultarPrestaciones}
              disabled={loadingPrestaciones}
            >
              {loadingPrestaciones ? <><ButtonSpinner /> Consultando...</> : 'Consultar'}
            </button>
          </div>
        </div>
        {/* Consulta ARC */}
        <div className="stat-card">
          <h4 className="stat-title">ARC</h4>
          <div className="stat-icon red">📄</div>
          <div className="stat-info">
            <p>Retención de ISLR</p>
            <div className="stat-sub">
              {arcData === null
                ? 'Sin consultar'
                : !arcData
                ? 'Sin registros'
                : 'Consultado'}
            </div>
            {errorArc && <div className="stat-error">{errorArc}</div>}
            <button
              className="consultar-btn"
              onClick={() => handleConsultarArc()}
              disabled={loadingArc}
            >
              {loadingArc ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
        </div>
        <div className="stat-card">
          <h4 className="stat-title">Utilidades</h4>
          <div className="stat-icon red">📈</div>
          <div className="stat-info">
            <h3>
              {typeof montoUtilidades === 'number'
                ? `Bs. ${montoUtilidades.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                : '—'}
            </h3>
            <div className="stat-sub">
              {montoUtilidades === null
                ? 'Sin consultar'
                : montoUtilidades === undefined
                ? 'Sin registros'
                : 'Monto disponible'}
            </div>
            {errorUtilidades && <div className="stat-error">{errorUtilidades}</div>}
            <button
              className="consultar-btn"
              onClick={handleConsultarUtilidades}
              disabled={loadingUtilidades}
            >
              {loadingUtilidades ? 'Consultando...' : 'Consultar'}
            </button>
          </div>
        </div>
      </div>

      {/* Modal vacaciones */}
      {modalVacaciones && vacaciones && vacaciones.length > 0 && (
        <div className="modal-overlay" onClick={() => setModalVacaciones(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>Historial de Vacaciones</h2>
                {vacaciones[0]?.nombre && (
                  <p className="modal-subtitle">{vacaciones[0].nombre}</p>
                )}
              </div>
              <button className="modal-close" onClick={() => setModalVacaciones(false)}>✕</button>
            </div>
            <div className="modal-body">
              <table className="vacaciones-table">
                <thead>
                  <tr>
                    <th>Desde</th>
                    <th>Hasta</th>
                    <th>Días</th>
                    <th>Disponible acumulado</th>
                  </tr>
                </thead>
                <tbody>
                  {vacaciones.map((v, i) => (
                    <tr key={i} className={i === vacaciones.length - 1 ? 'vacaciones-last-row' : ''}>
                      <td>{formatFecha(v.desde)}</td>
                      <td>{formatFecha(v.hasta)}</td>
                      <td>{v.dias ?? '—'}</td>
                      <td><strong>{v.disponibleAcumulado ?? '—'}</strong></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Modal días especiales */}
      {modalDiasEsp && diasEspeciales && diasEspeciales.length > 0 && (
        <div className="modal-overlay" onClick={() => setModalDiasEsp(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>Días de Permiso Especial</h2>
                <p className="modal-subtitle">{diasEspUsados} de 4 días usados — {diasEspDisponibles} disponibles</p>
              </div>
              <button className="modal-close" onClick={() => setModalDiasEsp(false)}>✕</button>
            </div>
            <div className="modal-body">
              <table className="vacaciones-table">
                <thead>
                  <tr>
                    <th>Descripción</th>
                    <th>Desde</th>
                    <th>Hasta</th>
                    <th>Días</th>
                    <th>Autorizado por</th>
                    <th>Comentario</th>
                  </tr>
                </thead>
                <tbody>
                  {diasEspeciales.map((d, i) => (
                    <tr key={i}>
                      <td>{d.desNovedadDia ?? '—'}</td>
                      <td>{formatFecha(d.desde)}</td>
                      <td>{formatFecha(d.hasta)}</td>
                      <td>{d.dias ?? '—'}</td>
                      <td>{d.autorisadoPor ?? '—'}</td>
                      <td>{d.comentario ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Modal HC */}
      {modalHc && (hcCobertura1 || hcCobertura2) && (
        <div className="modal-overlay" onClick={() => setModalHc(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>Consulta de HC</h2>
              </div>
              <button className="modal-close" onClick={() => setModalHc(false)}>✕</button>
            </div>
            <div className="modal-body">
              <p className="modal-subtitle">Cobertura 1</p>
              {hcCobertura1 ? (
                <table className="vacaciones-table">
                  <thead>
                    <tr>
                      <th>Nombre del asegurado</th>
                      <th>Prima trim en Bs. Factura</th>
                      <th>Mes 1</th>
                      <th>Mes 2</th>
                      <th>Mes 3</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>{hcCobertura1.nombreCompleto ?? '—'}</td>
                      <td>{formatMonto(hcCobertura1.primaTrimBs)}</td>
                      <td>
                        <input
                          type="text"
                          inputMode="decimal"
                          className="hc-mes-input"
                          value={mes1Input}
                          onChange={(e) => setMes1Input(e.target.value)}
                          placeholder="0,00"
                        />
                      </td>
                      <td>
                        <input
                          type="text"
                          inputMode="decimal"
                          className="hc-mes-input"
                          value={mes2Input}
                          onChange={(e) => setMes2Input(e.target.value)}
                          placeholder="0,00"
                        />
                      </td>
                      <td>
                        <input
                          type="text"
                          inputMode="decimal"
                          className="hc-mes-input"
                          value={mes3Input}
                          onChange={(e) => setMes3Input(e.target.value)}
                          placeholder="0,00"
                        />
                      </td>
                    </tr>
                  </tbody>
                </table>
              ) : (
                <p className="stat-sub">Sin registros</p>
              )}

              {hcCobertura1 && (
                <div className="hc-mes-actions">
                  <div className="hc-mes-summary">
                    Suma: {formatMonto(sumaMeses)} / {formatMonto(primaTrimBsHc)}
                    {sumaNoCoincideHc && (
                      <span className="hc-mes-error"> — La suma debe ser igual a la Prima trim en Bs. Factura</span>
                    )}
                  </div>
                  <button
                    className="consultar-btn"
                    onClick={handleGuardarHc}
                    disabled={hcSaving || sumaNoCoincideHc}
                  >
                    {hcSaving ? 'Guardando...' : 'Guardar'}
                  </button>
                  {hcSaveSuccess && <span className="hc-mes-success">Guardado correctamente</span>}
                  {hcSaveError && <span className="hc-mes-error">{hcSaveError}</span>}
                </div>
              )}

              <p className="modal-subtitle" style={{ marginTop: '20px' }}>Cobertura 2</p>
              {hcCobertura2 && hcCobertura2.length > 0 ? (
                <table className="vacaciones-table">
                  <thead>
                    <tr>
                      <th>Nombre del asegurado</th>
                      <th>Parentesco</th>
                      <th>Pago a Bixa Trim $</th>
                    </tr>
                  </thead>
                  <tbody>
                    {hcCobertura2.map((f, i) => (
                      <tr key={i}>
                        <td>{f.nombreCompleto ?? '—'}</td>
                        <td>{f.parentesco ?? '—'}</td>
                        <td>{formatMonto(f.pagoBixaTrim)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="stat-sub">Sin registros</p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Modal ARC */}
      {modalArc && (
        <div className="modal-overlay" onClick={() => setModalArc(false)}>
          <div className="modal-box" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div>
                <h2>ARC — Retención de ISLR ({anioArc})</h2>
              </div>
              <button className="modal-close" onClick={() => setModalArc(false)}>✕</button>
            </div>
            <div className="modal-body">
              <div style={{ marginBottom: '12px', display: 'flex', gap: '10px', flexWrap: 'wrap', alignItems: 'center' }}>
                <button
                  className="btn-secondary"
                  onClick={handleToggleAnioArc}
                  disabled={loadingArc}
                >
                  {loadingArc
                    ? 'Consultando...'
                    : anioArc === new Date().getFullYear()
                      ? 'Ver año anterior'
                      : 'Ver año actual'}
                </button>
                {arcData && arcData.length > 0 && (
                  <button
                    className="btn-secondary"
                    onClick={handleDescargarArcReporte}
                    disabled={descargandoArc}
                  >
                    {descargandoArc ? 'Generando...' : '⬇ Descargar reporte'}
                  </button>
                )}
                {errorDescargaArc && <span className="hc-mes-error">{errorDescargaArc}</span>}
              </div>
              {arcData && arcData.length > 0 ? (
                <table className="vacaciones-table">
                  <thead>
                    <tr>
                      <th>Meses</th>
                      <th>Remuneraciones pagadas abonadas en cuentas</th>
                      <th>Porcentaje de retención</th>
                      <th>Impuesto retenido</th>
                      <th>Remuneraciones pagadas o abonadas en cuentas acumuladas</th>
                      <th>Impuesto retenido acumulado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {arcData.map((a, i) => (
                      <tr key={i}>
                        <td>{a.mes ? NOMBRES_MESES[a.mes - 1] : '—'}</td>
                        <td>{formatMonto(a.remuneracion)}</td>
                        <td>{formatMonto(a.porcentRetencion)}</td>
                        <td>{formatMonto(a.impuestoRetenido)}</td>
                        <td>{formatMonto(a.remuneracionAcumulada)}</td>
                        <td>{formatMonto(a.impuestoRetenidoAcum)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="stat-sub">Sin registros</p>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
