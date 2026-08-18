import React, { useEffect, useState } from 'react';
import api from '../../lib/api';
import type { ApiResponse } from '../../services/authService';
import type { UserProfile, GrupoFamiliar } from '../../store/userProfileStore';
import { ajustarSaldoVacaciones } from '../../lib/vacaciones';
import { ButtonSpinner } from './ButtonSpinner';
import './ForgotPasswordModal.css';

interface Props {
  ci: string;
  onClose: () => void;
}

interface Vacacion {
  nombre: string | null;
  desde: string | null;
  hasta: string | null;
  dias: number | null;
  disponibleAcumulado: number | null;
}

interface DiaEspecial {
  desNovedadDia: string | null;
  desde: string | null;
  hasta: string | null;
  dias: number | null;
  autorisadoPor: string | null;
  comentario: string | null;
}

const formatMonto = (monto: number | null | undefined): string =>
  typeof monto === 'number'
    ? `Bs. ${monto.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
    : '—';

function isNotFoundError(err: unknown): boolean {
  return (err as { response?: { status?: number } })?.response?.status === 404;
}

function formatDate(value: string | null | undefined): string {
  if (!value) return '—';
  try {
    return new Date(value).toLocaleDateString('es-VE', { day: '2-digit', month: '2-digit', year: 'numeric' });
  } catch {
    return value;
  }
}

function calcTimeAtCompany(fechaIng: string | null | undefined): string | null {
  if (!fechaIng) return null;
  const start = new Date(fechaIng);
  if (isNaN(start.getTime())) return null;
  const now = new Date();
  let years = now.getFullYear() - start.getFullYear();
  let months = now.getMonth() - start.getMonth();
  if (months < 0) { years--; months += 12; }
  const parts: string[] = [];
  if (years > 0) parts.push(`${years} ${years === 1 ? 'año' : 'años'}`);
  if (months > 0) parts.push(`${months} ${months === 1 ? 'mes' : 'meses'}`);
  return parts.length ? parts.join(', ') : 'Menos de un mes';
}

const Badge: React.FC<{ color: string; bg: string; children: React.ReactNode }> = ({ color, bg, children }) => (
  <span style={{
    display: 'inline-flex', alignItems: 'center', padding: '3px 10px',
    borderRadius: '999px', fontSize: '12px', fontWeight: 600, color, background: bg,
  }}>
    {children}
  </span>
);

const Field: React.FC<{ label: string; value: string | number | null | undefined }> = ({ label, value }) => (
  <div style={{ marginBottom: '12px' }}>
    <span style={{ fontWeight: 600, color: 'var(--gray-600)', fontSize: '12px', display: 'block', marginBottom: '2px', textTransform: 'uppercase', letterSpacing: '0.4px' }}>
      {label}
    </span>
    <span style={{ color: 'var(--gray-900)', fontSize: '14px' }}>
      {value ?? '—'}
    </span>
  </div>
);

const Section: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <div style={{
    background: 'var(--gray-50)', padding: '20px 24px', borderRadius: 'var(--radius-lg)',
    border: '1.5px solid var(--gray-100)', marginBottom: '12px',
  }}>
    <h4 style={{ margin: '0 0 16px', color: 'var(--gray-700)', fontSize: '13px', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
      {title}
    </h4>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '4px 24px' }}>
      {children}
    </div>
  </div>
);

const FAMILY_COLS: { key: keyof GrupoFamiliar; label: string }[] = [
  { key: 'nombre',       label: 'Nombre'       },
  { key: 'parentesco',   label: 'Parentesco'   },
  { key: 'edad',         label: 'Edad'         },
  { key: 'sexo',         label: 'Sexo'         },
  { key: 'ocupacion',    label: 'Ocupación'    },
  { key: 'nacionalidad', label: 'Nacionalidad' },
];

const nestedOverlayStyle: React.CSSProperties = {
  position: 'fixed', inset: 0, background: 'rgba(0, 0, 0, 0.55)', display: 'flex',
  alignItems: 'center', justifyContent: 'center', zIndex: 10001,
};

const nestedBoxStyle: React.CSSProperties = {
  background: 'var(--white)', borderRadius: 'var(--radius-lg)', boxShadow: '0 24px 60px rgba(0, 0, 0, 0.3)',
  width: 'min(680px, 92vw)', maxHeight: '80vh', display: 'flex', flexDirection: 'column', overflow: 'hidden',
};

const nestedHeaderStyle: React.CSSProperties = {
  display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between',
  padding: '20px 24px', borderBottom: '1px solid var(--gray-100)',
};

const nestedBodyStyle: React.CSSProperties = { padding: '24px', overflowY: 'auto' };

const ConsultaCard: React.FC<{
  title: string;
  icon: string;
  loading: boolean;
  error: string | null;
  onConsultar: () => void;
  children: React.ReactNode;
}> = ({ title, icon, loading, error, onConsultar, children }) => (
  <div style={{
    background: 'var(--white)', padding: '16px 18px', borderRadius: 'var(--radius)',
    border: '1.5px solid var(--gray-100)', display: 'flex', flexDirection: 'column', gap: '8px',
  }}>
    <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
      <span style={{ fontSize: '20px' }}>{icon}</span>
      <span style={{ fontWeight: 700, fontSize: '12px', color: 'var(--gray-700)', textTransform: 'uppercase', letterSpacing: '0.4px' }}>
        {title}
      </span>
    </div>
    <div style={{ fontSize: '13px', color: 'var(--gray-600)', minHeight: '18px' }}>{children}</div>
    {error && <div style={{ color: 'var(--red)', fontSize: '12px' }}>{error}</div>}
    <button
      onClick={onConsultar}
      disabled={loading}
      style={{
        marginTop: '2px', background: 'var(--red)', color: 'var(--white)', border: '2px solid var(--red)',
        padding: '8px 14px', borderRadius: 'var(--radius)', fontSize: '13px', fontWeight: 600,
        cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.6 : 1,
        display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px',
      }}
    >
      {loading ? <><ButtonSpinner /> Consultando...</> : 'Consultar'}
    </button>
  </div>
);

const thStyle: React.CSSProperties = {
  textAlign: 'left', padding: '8px 14px', color: 'var(--gray-600)',
  fontWeight: 600, fontSize: '12px', borderBottom: '1px solid var(--gray-100)', whiteSpace: 'nowrap',
  textTransform: 'uppercase', letterSpacing: '0.4px',
};
const tdStyle: React.CSSProperties = {
  padding: '10px 14px', color: 'var(--gray-900)', fontSize: '13px', borderBottom: '1px solid var(--gray-100)',
};

export const EmployeeProfileModal: React.FC<Props> = ({ ci, onClose }) => {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [familyGroup, setFamilyGroup] = useState<GrupoFamiliar[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

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

  const [montoPrestaciones, setMontoPrestaciones] = useState<number | null | undefined>(null);
  const [loadingPrestaciones, setLoadingPrestaciones] = useState(false);
  const [errorPrestaciones, setErrorPrestaciones] = useState<string | null>(null);

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
    } catch (err: unknown) {
      if (isNotFoundError(err)) {
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
    } catch (err: unknown) {
      if (isNotFoundError(err)) {
        setDiasEspeciales([]);
      } else {
        setErrorDiasEsp('Error al consultar días especiales');
      }
    } finally {
      setLoadingDiasEsp(false);
    }
  };

  const handleConsultarUtilidades = async () => {
    setLoadingUtilidades(true);
    setErrorUtilidades(null);
    try {
      const { data } = await api.get(`/usersProfit/${ci}/Utilidades`);
      if (data.success) {
        setMontoUtilidades(data.data);
      } else {
        setErrorUtilidades(data.message || 'No se encontró información');
      }
    } catch (err: unknown) {
      if (isNotFoundError(err)) {
        setMontoUtilidades(undefined);
      } else {
        setErrorUtilidades('Error al consultar utilidades');
      }
    } finally {
      setLoadingUtilidades(false);
    }
  };

  const handleConsultarPrestaciones = async () => {
    setLoadingPrestaciones(true);
    setErrorPrestaciones(null);
    try {
      const { data } = await api.get(`/usersProfit/${ci}/PrestacionesSociales`);
      if (data.success) {
        setMontoPrestaciones(data.data);
      } else {
        setErrorPrestaciones(data.message || 'No se encontró información');
      }
    } catch (err: unknown) {
      if (isNotFoundError(err)) {
        setMontoPrestaciones(undefined);
      } else {
        setErrorPrestaciones('Error al consultar prestaciones sociales');
      }
    } finally {
      setLoadingPrestaciones(false);
    }
  };

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const [profileRes, familyRes] = await Promise.all([
          api.get<ApiResponse<UserProfile>>(`/usersProfit/${ci}/SnEmple`),
          api.get<ApiResponse<GrupoFamiliar[]>>(`/usersProfit/${ci}/GrupoFa`),
        ]);
        if (!cancelled) {
          setProfile(profileRes.data.data);
          setFamilyGroup(familyRes.data.data ?? []);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          const axiosError = err as { response?: { data?: { message?: string } } };
          setError(axiosError?.response?.data?.message ?? 'Error al cargar el perfil.');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [ci]);

  const fullName = profile
    ? `${profile.nombres ?? ''} ${profile.apellidos ?? ''}`.trim() || '—'
    : '—';

  return (
    <>
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box-wide" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>

        {loading && (
          <div className="modal-loading">
            <div className="modal-spinner" />
            Cargando perfil...
          </div>
        )}

        {!loading && error && (
          <div style={{ padding: '40px 0', textAlign: 'center' }}>
            <p style={{ color: 'var(--red)', marginBottom: '20px' }}>{error}</p>
            <button className="modal-btn-secondary" onClick={onClose}>Cerrar</button>
          </div>
        )}

        {!loading && !error && profile && (
          <>
            {/* Encabezado */}
            <div style={{
              background: 'linear-gradient(135deg, var(--red) 0%, #ff5555 100%)',
              padding: '24px', borderRadius: 'var(--radius-lg)', marginBottom: '16px',
              color: 'var(--white)',
            }}>
              <div style={{ fontSize: '22px', fontWeight: 700, marginBottom: '4px' }}>
                {fullName}
              </div>
              <div style={{ fontSize: '13px', opacity: 0.85, marginBottom: '12px' }}>
                {[profile.desCargo, profile.desDepart].filter(Boolean).join(' · ') || 'Sin cargo asignado'}
              </div>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                <Badge color="#166534" bg="#dcfce7">Activo</Badge>
                {profile.desCont && (
                  <Badge color="#1e40af" bg="#dbeafe">{profile.desCont}</Badge>
                )}
                {calcTimeAtCompany(profile.fechaIng) && (
                  <Badge color="#374151" bg="#f3f4f6">{calcTimeAtCompany(profile.fechaIng)}</Badge>
                )}
              </div>
            </div>

            {/* Datos personales */}
            <Section title="Datos personales">
              <Field label="Nombres" value={profile.nombres} />
              <Field label="Apellidos" value={profile.apellidos} />
              <Field label="Cédula de Identidad" value={profile.ci} />
              <Field label="Fecha de nacimiento" value={formatDate(profile.fechaNac)} />
              <Field label="Edad" value={profile.edad != null ? `${profile.edad} años` : null} />
              <Field label="Sexo" value={profile.sexo} />
              <Field label="Estado civil" value={profile.estadoCivil} />
              <Field label="Nacionalidad" value={profile.nacionalidad} />
              <Field label="Grupo sanguíneo" value={profile.grupoSang} />
              <Field label="Teléfono" value={profile.telefono} />
              <Field label="Dirección" value={profile.direccion} />
              <Field label="Correo personal" value={profile.correoP} />
            </Section>

            {/* Datos laborales */}
            <Section title="Datos laborales">
              <Field label="Cargo" value={profile.desCargo} />
              <Field label="Departamento" value={profile.desDepart} />
              <Field label="Tipo de contrato" value={profile.desCont} />
              <Field label="Ubicación" value={profile.desUbicacion} />
              <Field label="Fecha de ingreso" value={formatDate(profile.fechaIng)} />
              <Field label="RIF" value={profile.rif} />
              <Field label="Cuenta bancaria" value={profile.cuentaBanc1} />
              <Field label="Correo corporativo" value={profile.correoE} />
            </Section>

            {/* Grupo familiar */}
            <div style={{
              background: 'var(--gray-50)', padding: '20px 24px', borderRadius: 'var(--radius-lg)',
              border: '1.5px solid var(--gray-100)',
            }}>
              <h4 style={{ margin: '0 0 16px', color: 'var(--gray-700)', fontSize: '13px', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                Grupo Familiar
              </h4>
              {familyGroup.length === 0 ? (
                <p style={{ color: 'var(--gray-500)', margin: 0, fontSize: '14px' }}>
                  No hay integrantes registrados.
                </p>
              ) : (
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                      <tr>
                        {FAMILY_COLS.map(col => <th key={col.key} style={thStyle}>{col.label}</th>)}
                      </tr>
                    </thead>
                    <tbody>
                      {familyGroup.map((member, i) => (
                        <tr key={i}>
                          {FAMILY_COLS.map(col => (
                            <td key={col.key} style={tdStyle}>{member[col.key] ?? '—'}</td>
                          ))}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* Consultas */}
            <div style={{
              background: 'var(--gray-50)', padding: '20px 24px', borderRadius: 'var(--radius-lg)',
              border: '1.5px solid var(--gray-100)', marginTop: '12px',
            }}>
              <h4 style={{ margin: '0 0 16px', color: 'var(--gray-700)', fontSize: '13px', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.5px' }}>
                Consultas
              </h4>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', gap: '12px' }}>
                <ConsultaCard
                  title="Vacaciones"
                  icon="🏖️"
                  loading={loadingVacaciones}
                  error={errorVacaciones}
                  onConsultar={handleConsultarVacaciones}
                >
                  {vacaciones === null
                    ? 'Sin consultar'
                    : vacaciones.length === 0
                    ? 'Sin registros'
                    : `${ultimoDisponible} días disponibles`}
                </ConsultaCard>

                <ConsultaCard
                  title="Días de permiso especial"
                  icon="🗓️"
                  loading={loadingDiasEsp}
                  error={errorDiasEsp}
                  onConsultar={handleConsultarDiasEsp}
                >
                  {diasEspeciales === null
                    ? 'Sin consultar'
                    : `${diasEspDisponibles} de 4 días disponibles`}
                </ConsultaCard>

                <ConsultaCard
                  title="Prestaciones sociales"
                  icon="💰"
                  loading={loadingPrestaciones}
                  error={errorPrestaciones}
                  onConsultar={handleConsultarPrestaciones}
                >
                  {montoPrestaciones === null
                    ? 'Sin consultar'
                    : montoPrestaciones === undefined
                    ? 'Sin registros'
                    : formatMonto(montoPrestaciones)}
                </ConsultaCard>

                <ConsultaCard
                  title="Utilidades"
                  icon="📈"
                  loading={loadingUtilidades}
                  error={errorUtilidades}
                  onConsultar={handleConsultarUtilidades}
                >
                  {montoUtilidades === null
                    ? 'Sin consultar'
                    : montoUtilidades === undefined
                    ? 'Sin registros'
                    : formatMonto(montoUtilidades)}
                </ConsultaCard>
              </div>
            </div>
          </>
        )}
      </div>
    </div>

    {modalVacaciones && vacaciones && vacaciones.length > 0 && (
      <div style={nestedOverlayStyle} onClick={() => setModalVacaciones(false)}>
        <div style={nestedBoxStyle} onClick={(e) => e.stopPropagation()}>
          <div style={nestedHeaderStyle}>
            <div>
              <h2 style={{ fontSize: '20px', margin: '0 0 4px', color: 'var(--black)' }}>Historial de Vacaciones</h2>
              {vacaciones[0]?.nombre && (
                <p style={{ fontSize: '14px', color: 'var(--gray-500)', margin: 0 }}>{vacaciones[0].nombre}</p>
              )}
            </div>
            <button className="modal-close" style={{ position: 'static' }} onClick={() => setModalVacaciones(false)}>✕</button>
          </div>
          <div style={nestedBodyStyle}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={thStyle}>Desde</th>
                  <th style={thStyle}>Hasta</th>
                  <th style={thStyle}>Días</th>
                  <th style={thStyle}>Disponible acumulado</th>
                </tr>
              </thead>
              <tbody>
                {vacaciones.map((v, i) => (
                  <tr key={i}>
                    <td style={tdStyle}>{formatDate(v.desde)}</td>
                    <td style={tdStyle}>{formatDate(v.hasta)}</td>
                    <td style={tdStyle}>{v.dias ?? '—'}</td>
                    <td style={tdStyle}><strong>{v.disponibleAcumulado ?? '—'}</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    )}

    {modalDiasEsp && diasEspeciales && diasEspeciales.length > 0 && (
      <div style={nestedOverlayStyle} onClick={() => setModalDiasEsp(false)}>
        <div style={nestedBoxStyle} onClick={(e) => e.stopPropagation()}>
          <div style={nestedHeaderStyle}>
            <div>
              <h2 style={{ fontSize: '20px', margin: '0 0 4px', color: 'var(--black)' }}>Días de Permiso Especial</h2>
              <p style={{ fontSize: '14px', color: 'var(--gray-500)', margin: 0 }}>
                {diasEspUsados} de 4 días usados — {diasEspDisponibles} disponibles
              </p>
            </div>
            <button className="modal-close" style={{ position: 'static' }} onClick={() => setModalDiasEsp(false)}>✕</button>
          </div>
          <div style={nestedBodyStyle}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={thStyle}>Descripción</th>
                  <th style={thStyle}>Desde</th>
                  <th style={thStyle}>Hasta</th>
                  <th style={thStyle}>Días</th>
                  <th style={thStyle}>Autorizado por</th>
                  <th style={thStyle}>Comentario</th>
                </tr>
              </thead>
              <tbody>
                {diasEspeciales.map((d, i) => (
                  <tr key={i}>
                    <td style={tdStyle}>{d.desNovedadDia ?? '—'}</td>
                    <td style={tdStyle}>{formatDate(d.desde)}</td>
                    <td style={tdStyle}>{formatDate(d.hasta)}</td>
                    <td style={tdStyle}>{d.dias ?? '—'}</td>
                    <td style={tdStyle}>{d.autorisadoPor ?? '—'}</td>
                    <td style={tdStyle}>{d.comentario ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    )}
    </>
  );
};
