import React, { useEffect, useState } from 'react';
import api from '../../lib/api';
import type { ApiResponse } from '../../services/authService';
import type { UserProfile, GrupoFamiliar } from '../../store/userProfileStore';
import './ForgotPasswordModal.css';

interface Props {
  ci: string;
  onClose: () => void;
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
          </>
        )}
      </div>
    </div>
  );
};
