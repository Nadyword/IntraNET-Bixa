import React from 'react';
import { useAuthStore } from '../../store/authStore';
import { useUIStore } from '../../store/uiStore';
import './TopHeader.css';

export const TopHeader: React.FC = () => {
  const { user, logout } = useAuthStore();
  const { toggleSidebar, toggleNotifPanel } = useUIStore();

  return (
    <header className="top-header">
      <button className="hamburger" onClick={toggleSidebar}>
        <span></span>
        <span></span>
        <span></span>
      </button>

      <div className="header-logo">
        BIXA<span> · INTRANET</span>
      </div>

      <div className="header-search">
        <input
          type="text"
          placeholder="Buscar personas, documentos, solicitudes…"
        />
      </div>

      <div className="header-actions">
        <button className="notif-btn" onClick={toggleNotifPanel}>
          🔔
          <span className="notif-badge"></span>
        </button>
        <div className="user-avatar">{user?.nombre?.charAt(0) || 'U'}</div>
        <button className="btn btn-sm btn-outline" onClick={logout}>
          Salir
        </button>
      </div>
    </header>
  );
};
