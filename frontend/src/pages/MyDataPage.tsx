import React from 'react';
import { useAuthStore } from '../store/authStore';

export const MyDataPage: React.FC = () => {
  const { user } = useAuthStore();

  return (
    <div>
      <h1>MIS DATOS</h1>
      <p>Información personal y laboral registrada en el sistema de nómina</p>
      <div style={{ marginTop: '32px', background: 'var(--white)', padding: '24px', borderRadius: 'var(--radius-lg)', border: '1px solid var(--gray-100)' }}>
        <h3>Datos Personales</h3>
        <div style={{ marginTop: '16px' }}>
          <p><strong>Nombre:</strong> {user?.nombre}</p>
          <p><strong>Email:</strong> {user?.email}</p>
          <p><strong>Cargo:</strong> {user?.cargo}</p>
        </div>
      </div>
    </div>
  );
};
