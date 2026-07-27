import { useState } from 'react';
import { useUserProfileStore } from '../store/userProfileStore';
import { api } from '../lib/api';
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

const formatFecha = (fecha: string | null): string => {
  if (!fecha) return '—';
  const d = new Date(fecha);
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('es-VE', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

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

  const ultimoDisponible = vacaciones !== null && vacaciones.length > 0
    ? vacaciones[vacaciones.length - 1].disponibleAcumulado
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
          <div className="stat-icon red">🏖️</div>
          <div className="stat-info">
            <h3>{ultimoDisponible !== null ? ultimoDisponible : '—'}</h3>
            <p>Días de vacaciones disponibles</p>
            <div className="stat-sub">
              {vacaciones === null
                ? 'Sin consultar'
                : vacaciones.length === 0
                ? 'Sin registros'
                : 'Disponible acumulado'}
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
          <div className="stat-icon black">🗓️</div>
          <div className="stat-info">
            <h3>{diasEspDisponibles !== null ? diasEspDisponibles : '—'}</h3>
            <p>Días de permiso especial</p>
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

        <div className="stat-card">
          <div className="stat-icon green">💰</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>*En desarrollo*</h3><p>Prestaciones acumuladas</p></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">🏥</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>*En desarrollo*</h3><p>Descuento HC mensual</p></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">📄</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>*En desarrollo*</h3><p>ARC — Retención de ISLR:</p></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">📈</div>
          <div className="stat-info">
            <h3>
              {typeof montoUtilidades === 'number'
                ? `$${montoUtilidades.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                : '—'}
            </h3>
            <p>Utilidades disponibles</p>
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
    </div>
  );
};
