import React, { useState } from 'react';
import { useUserProfileStore, type GrupoFamiliar } from '../store/userProfileStore';

// ─── utilidades ───────────────────────────────────────────────────────────────

function formatDate(value: string | null | undefined): string | null {
  if (!value) return null;
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

// ─── componentes base ─────────────────────────────────────────────────────────

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
    border: '3px solid var(--gray-700)',
    marginBottom: '16px',
  }}>
    <h3 style={{ marginTop: 0, marginBottom: '20px', color: 'var(--gray-800)', fontSize: '16px' }}>{title}</h3>
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '8px 24px' }}>
      {children}
    </div>
  </div>
);

// ─── tabla grupo familiar ─────────────────────────────────────────────────────

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
  fontWeight: 600, fontSize: '13px', borderBottom: '1px solid var(--gray-100)', whiteSpace: 'nowrap',
};
const tdStyle: React.CSSProperties = {
  padding: '10px 14px', color: 'var(--gray-900)', fontSize: '14px', borderBottom: '1px solid var(--gray-100)',
};

// ─── modal de corrección ───────────────────────────────────────────────────────

const CorrectionModal: React.FC<{ onClose: () => void }> = ({ onClose }) => {
  const [text, setText] = useState('');

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.45)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000,
    }}>
      <div style={{
        background: 'var(--white)', borderRadius: 'var(--radius-lg)', padding: '32px',
        width: '100%', maxWidth: '480px', boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
      }}>
        <h3 style={{ margin: '0 0 8px', color: 'var(--gray-900)', fontSize: '18px' }}>
          Solicitar Corrección
        </h3>
        <p style={{ margin: '0 0 20px', color: 'var(--gray-500)', fontSize: '13px' }}>
          Indique qué dato desea corregir y la información correcta.
        </p>

        <textarea
          value={text}
          onChange={e => setText(e.target.value)}
          placeholder="Favor comente que tipo de cambio quiere realizar y agregue la información para reemplazar"
          rows={6}
          style={{
            width: '100%', boxSizing: 'border-box', padding: '12px',
            border: '1px solid var(--gray-900)', borderRadius: 'var(--radius-lg)',
            fontSize: '14px', color: 'var(--gray-900)', resize: 'vertical',
            fontFamily: 'inherit', outline: 'none', 
          }}
        />

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '20px' }}>
          <button
            onClick={onClose}
            style={{
              padding: '8px 18px', borderRadius: 'var(--radius-lg)',
              border: '1px solid var(--gray-700)', background: 'var(----gray-800)',
              color: 'var(--gray-800)', fontSize: '14px', cursor: 'pointer', fontWeight: 500,
            }}
          >
            Cerrar
          </button>
          <button
            style={{
              padding: '8px 18px', borderRadius: 'var(--radius-md)',
              border: 'none', background: 'var(--primary, #2563eb)',
              color: '#fff', fontSize: '14px', cursor: 'pointer', fontWeight: 500,
              opacity: text.trim() ? 1 : 0.5,
            }}
            disabled={!text.trim()}
          >
            Enviar Solicitud
          </button>
        </div>
      </div>
    </div>
  );
};

// ─── tarjeta de perfil ────────────────────────────────────────────────────────

const Badge: React.FC<{ color: string; bg: string; children: React.ReactNode }> = ({ color, bg, children }) => (
  <span style={{
    display: 'inline-flex', alignItems: 'center', padding: '3px 10px',
    borderRadius: '999px', fontSize: '12px', fontWeight: 600,
    color, background: bg,
  }}>
    {children}
  </span>
);

interface ProfileHeaderProps {
  nombres: string | null;
  apellidos: string | null;
  desCargo: string | null;
  desDepart: string | null;
  desCont: string | null;
  fechaIng: string | null;
  onRequestCorrection: () => void;
}

const ProfileHeader: React.FC<ProfileHeaderProps> = ({
  nombres, apellidos, desCargo, desDepart, desCont, fechaIng, onRequestCorrection,
}) => {
  const initials = [nombres?.[0], apellidos?.[0]].filter(Boolean).join('').toUpperCase() || '?';
  const fullName = [nombres, apellidos].filter(Boolean).join(' ') || '—';
  const subtitle = [desCargo, desDepart].filter(Boolean).join(' · ') || null;
  const timeLabel = calcTimeAtCompany(fechaIng);

  return (
    <div style={{
      background: 'var(--black)', padding: '24px 28px', borderRadius: 'var(--radius-lg)',
      border:'3px solid var(--red)', marginBottom: '16px',
      display: 'flex', alignItems: 'center', gap: '20px',
    }}>
      {/* Avatar */}
      <div style={{
        width: '58px', height: '58px', borderRadius: '50%', flexShrink: 0,
        background: '#7c6355', display: 'flex', alignItems: 'center', justifyContent: 'center',
        color: '#fff', fontSize: '20px', fontWeight: 700, letterSpacing: '1px',
      }}>
        {initials}
      </div>

      {/* Info */}
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontSize: '18px', fontWeight: 700, color: 'var(--white)', marginBottom: '3px' }}>
          {fullName}
        </div>
        {subtitle && (
          <div style={{ fontSize: '13px', color: 'var(--gray-500)', marginBottom: '10px' }}>
            {subtitle}
          </div>
        )}
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
          <Badge color="#166534" bg="#dcfce7">Activo</Badge>
          {desCont && <Badge color="#1e40af" bg="#dbeafe">{desCont}</Badge>}
          {timeLabel && <Badge color="#374151" bg="#f3f4f6">{timeLabel}</Badge>}
        </div>
      </div>

      {/* Botón corrección */}
      <button
        onClick={onRequestCorrection}
        style={{
          flexShrink: 0, display: 'flex', alignItems: 'center', gap: '6px',
          padding: '7px 14px', borderRadius: 'var(--radius-lg)',
          border: '2px solid var(--red)', background: 'var(--white)',
          color: 'var(--gray-600)', fontSize: '13px', fontWeight: 500,
          cursor: 'pointer', whiteSpace: 'nowrap' 
        }}
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
        </svg>
        Solicitar Corrección
      </button>
    </div>
  );
};

// ─── página principal ─────────────────────────────────────────────────────────

export const MyDataPage: React.FC = () => {
  const { profile, familyGroup, familyGroupLoading, loading, error } = useUserProfileStore();
  const [modalOpen, setModalOpen] = useState(false);

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
    <>
      <div>
        <h1 style={{ marginBottom: '6px' }}>Mis Datos</h1>
        <p style={{ color: 'var(--gray-500)', marginBottom: '24px' }}>
          Información personal y laboral registrada en el sistema de nómina
        </p>

        <ProfileHeader
          nombres={profile.nombres}
          apellidos={profile.apellidos}
          desCargo={profile.desCargo}
          desDepart={profile.desDepart}
          desCont={profile.desCont}
          fechaIng={profile.fechaIng}
          onRequestCorrection={() => setModalOpen(true)}
        />

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
          background: 'var(--white)', padding: '24px', borderRadius: 'var(--radius-lg)',
          border:'3px solid var(--gray-700)', marginBottom: '16px',
        }}>
          <h3 style={{ marginTop: 0, marginBottom: '20px', color: 'var(--gray-800)', fontSize: '16px' }}>
            Grupo Familiar
          </h3>

          {familyGroupLoading ? (
            <p style={{ color: 'var(--gray-500)', margin: 0 }}>Cargando grupo familiar...</p>
          ) : familyGroup.length === 0 ? (
            <p style={{ color: 'var(--gray-500)', margin: 0 }}>No hay integrantes registrados.</p>
          ) : (
            <div style={{ overflowX: 'auto'}}>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr>{FAMILY_COLS.map(col => <th key={col.key} style={thStyle}>{col.label}</th>)}</tr>
                </thead>
                <tbody>
                  {familyGroup.map((member, i) => (
                    <tr key={i}>
                      {FAMILY_COLS.map(col => <td key={col.key} style={tdStyle}>{member[col.key] ?? '—'}</td>)}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {modalOpen && <CorrectionModal onClose={() => setModalOpen(false)} />}
    </>
  );
};
