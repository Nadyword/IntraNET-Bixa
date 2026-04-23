import React from 'react';
import { useAuthStore } from '../../store/authStore';
import { useUserProfileStore } from '../../store/userProfileStore';
import { useUIStore } from '../../store/uiStore';
import './TopHeader.css';

export const TopHeader: React.FC = () => {
  const { logout } = useAuthStore();
  const { profile } = useUserProfileStore();
  const { toggleSidebar, toggleNotifPanel } = useUIStore();

  const inicial = profile?.nombres?.charAt(0) ?? 'U';

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


      <div className="header-actions">
        <button className="notif-btn" onClick={toggleNotifPanel}>
          🔔
          <span className="notif-badge"></span>
        </button>
        <div className="user-avatar">{inicial}</div>
        <button className="btn btn-sm btn-outline" onClick={logout}>
          Salir
        </button>
      </div>
    </header>
  );
};
