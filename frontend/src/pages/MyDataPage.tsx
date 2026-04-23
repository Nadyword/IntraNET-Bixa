import React from 'react';
import { useUserProfileStore, type GrupoFamiliar } from '../store/userProfileStore';

const Field: React.FC<{ label: string; value: string | number | null | undefined }> = ({ label, value }) => (
  <div style={{ marginBottom: '12px' }}>
    <span style={{ fontWeight: 600, color: 'var(--gray-600)', fontSize: '13px', display: 'block', marginBottom: '2px' }}>
      {label}
    </span>
    <span style={{ color: 'var(--gray-900)', fontSize: '15px' }}>
      {value ?? '—'}
    </span>
  </div>
);

const Section: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <div style={{
    background: 'var(--white)',
    padding: '24px',
    borderRadius: 'var(--radius-lg)',
    border: '1px solid var(--gray-100)',
    marginBottom: '16px',
  }}>
    <h3 style={{ marginTop: 0, marginBottom: '20px', color: 'var(--gray-800)', fontSize: '16px' }}>{title}</h3>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '8px 24px' }}>
      {children}
    </div>
  </div>
);

function formatDate(value: string | null | undefined): string | null {
  if (!value) return null;
  try {
    return new Date(value).toLocaleDateString('es-VE', { day: '2-digit', month: '2-digit', year: 'numeric' });
  } catch {
    return value;
  }
}

const FAMILY_COLS: { key: keyof GrupoFamiliar; label: string }[] = [
  { key: 'nombre',       label: 'Nombre'       },
  { key: 'parentesco',   label: 'Parentesco'   },
  { key: 'edad',         label: 'Edad'         },
  { key: 'sexo',         label: 'Sexo'         },
  { key: 'ocupacion',    label: 'Ocupación'    },
  { key: 'nacionalidad', label: 'Nacionalidad' },
];

const thStyle: React.CSSProperties = {
  textAlign: 'left',
  padding: '8px 14px',
  color: 'var(--gray-600)',
  fontWeight: 600,
  fontSize: '13px',
  borderBottom: '1px solid var(--gray-100)',
  whiteSpace: 'nowrap',
};

const tdStyle: React.CSSProperties = {
  padding: '10px 14px',
  color: 'var(--gray-900)',
  fontSize: '14px',
  borderBottom: '1px solid var(--gray-100)',
};

export const MyDataPage: React.FC = () => {
  const { profile, familyGroup, familyGroupLoading, loading, error } = useUserProfileStore();

  if (loading) {
    return (
      <div>
        <h1>Mis Datos</h1>
        <p style={{ color: 'var(--gray-500)' }}>Cargando información...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <h1>Mis Datos</h1>
        <p style={{ color: 'var(--error, #dc2626)' }}>{error}</p>
      </div>
    );
  }

  if (!profile) {
    return (
      <div>
        <h1>Mis Datos</h1>
        <p style={{ color: 'var(--gray-500)' }}>No se encontró información del empleado.</p>
      </div>
    );
  }

  return (
    <div>
      <h1 style={{ marginBottom: '6px' }}>Mis Datos</h1>
      <p style={{ color: 'var(--gray-500)', marginBottom: '24px' }}>
        Información personal y laboral registrada en el sistema de nómina
      </p>

      <Section title="Datos Personales">
        <Field label="Nombres" value={profile.nombres} />
        <Field label="Apellidos" value={profile.apellidos} />
        <Field label="Cédula de Identidad" value={profile.ci} />
        <Field label="Dirección" value={profile.direccion} />
        <Field label="Teléfono" value={profile.telefono} />
        <Field label="Correo personal" value={profile.correoP} />
        <Field label="Correo corporativo" value={profile.correoE} />
        <Field label="Cargo" value={profile.desCargo} />
        <Field label="Fecha de ingreso" value={formatDate(profile.fechaIng)} />
        <Field label="Departamento" value={profile.desDepart} />
        <Field label="Fecha de nacimiento" value={formatDate(profile.fechaNac)} />
        <Field label="RIF" value={profile.rif} />
        <Field label="Cuenta Provincial" value={profile.cuentaBanc1} />
      </Section>

      <div style={{
        background: 'var(--white)',
        padding: '24px',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--gray-100)',
        marginBottom: '16px',
      }}>
        <h3 style={{ marginTop: 0, marginBottom: '20px', color: 'var(--gray-800)', fontSize: '16px' }}>
          Grupo Familiar
        </h3>

        {familyGroupLoading ? (
          <p style={{ color: 'var(--gray-500)', margin: 0 }}>Cargando grupo familiar...</p>
        ) : familyGroup.length === 0 ? (
          <p style={{ color: 'var(--gray-500)', margin: 0 }}>No hay integrantes registrados.</p>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  {FAMILY_COLS.map(col => (
                    <th key={col.key} style={thStyle}>{col.label}</th>
                  ))}
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
    </div>
  );
};
