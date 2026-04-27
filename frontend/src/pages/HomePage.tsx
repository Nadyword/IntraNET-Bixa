import React from 'react';
import { useUserProfileStore } from '../store/userProfileStore';

export const HomePage: React.FC = () => {
  const { profile } = useUserProfileStore();

  return (
    <div>
      <div style={{
        background: 'linear-gradient(135deg, var(--black) 0%, var(--gray-900) 50%, #1a0000 100%)',
        borderRadius: 'var(--radius-lg)',
        padding: '48px 40px',
        marginBottom: '28px',
        color: 'var(--white)',
        minHeight: '200px',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between',
      }}>
        <div>
          <div style={{ fontSize: '13px', letterSpacing: '2px', textTransform: 'uppercase', opacity: 0.5, marginBottom: '8px' }}>
            Bienvenida de nuevo
          </div>
          <div style={{ fontFamily: 'var(--font-display)', fontSize: '48px', letterSpacing: '2px', marginBottom: '12px' }}>
            {profile?.nombres?.split(' ')[0] || 'USUARIO'} <span style={{ color: 'var(--red)' }}>{profile?.apellidos?.split(' ')[0] || ''}</span>
          </div>
        </div>
        <div style={{ display: 'flex', gap: '24px' }}>
          <div style={{ fontSize: '13px' }}>
            <strong style={{ display: 'block', color: 'var(--white)', fontSize: '15px', marginBottom: '2px' }}>
              {profile?.desCargo || 'Cargo'}
            </strong>
            <span style={{ opacity: 0.6 }}>{profile?.desDepart || 'Departamento'}</span>
          </div>
          <div style={{ fontSize: '13px' }}>
            <strong style={{ display: 'block', color: 'var(--white)', fontSize: '15px', marginBottom: '2px' }}>
              15 días
            </strong>
            <span style={{ opacity: 0.6 }}>Vacaciones disponibles</span>
          </div>
        </div>
      </div>

      <h2 style={{ fontSize: '18px', fontWeight: 700, marginBottom: '16px' }}>Mi Resumen de Gestión</h2>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px' }}>
        <div style={{
          background: 'var(--white)',
          borderRadius: 'var(--radius-lg)',
          border: '1px solid var(--gray-100)',
          padding: '24px',
          boxShadow: 'var(--shadow-sm)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingBottom: '10px', borderBottom: '1px solid var(--gray-50)' }}>
            <span>🌴 Vacaciones Disponibles</span>
            <strong style={{ color: 'var(--red)', fontSize: '16px' }}>15 días</strong>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 0', borderBottom: '1px solid var(--gray-50)' }}>
            <span>📋 Solicitudes Activas</span>
            <strong>2</strong>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 0', borderBottom: '1px solid var(--gray-50)' }}>
            <span>🏦 Prestaciones Acumuladas</span>
            <strong>Bs. 12,450</strong>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 0' }}>
            <span>📅 Próximo cumpleaños de colega</span>
            <strong style={{ color: 'var(--gray-500)', fontSize: '14px' }}>Carlos M. — 18 Jun</strong>
          </div>
        </div>
      </div>
    </div>
  );
};
