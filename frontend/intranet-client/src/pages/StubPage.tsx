import React from 'react';

export const StubPage: React.FC<{ title: string }> = ({ title }) => {
  return (
    <div>
      <h1 style={{ fontFamily: 'var(--font-display)', fontSize: '32px', marginBottom: '12px' }}>{title}</h1>
      <p style={{ color: 'var(--gray-500)', marginBottom: '32px' }}>Esta sección está en desarrollo...</p>
      <div style={{
        background: 'var(--white)',
        padding: '48px',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--gray-100)',
        textAlign: 'center',
        color: 'var(--gray-500)',
      }}>
        <div style={{ fontSize: '48px', marginBottom: '12px' }}>🚀</div>
        <p>Pronto tendrás acceso a todas las funcionalidades aquí.</p>
      </div>
    </div>
  );
};
