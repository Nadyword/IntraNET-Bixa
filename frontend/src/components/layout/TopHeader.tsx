import React, { useState, useRef, useEffect } from 'react';
import { useAuthStore } from '../../store/authStore';
import { useUserProfileStore } from '../../store/userProfileStore';
import { useUIStore } from '../../store/uiStore';
import { ChangePasswordModal } from '../ui/ChangePasswordModal';
import './TopHeader.css';

export const TopHeader: React.FC = () => {
  const { logout } = useAuthStore();
  const { profile } = useUserProfileStore();
  const { toggleSidebar, toggleNotifPanel } = useUIStore();

  const [menuOpen, setMenuOpen] = useState(false);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const inicial = profile?.nombres?.charAt(0) ?? 'U';

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    if (menuOpen) document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [menuOpen]);

  return (
    <>
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

          <div className="user-menu-wrapper" ref={menuRef}>
            <div
              className={`user-avatar${menuOpen ? ' user-avatar--active' : ''}`}
              onClick={() => setMenuOpen((prev) => !prev)}
            >
              {inicial}
            </div>

            {menuOpen && (
              <div className="user-dropdown">
                <button
                  className="user-dropdown-item"
                  onClick={() => { setMenuOpen(false); setShowChangePassword(true); }}
                >
                  🔑 Cambiar clave
                </button>
              </div>
            )}
          </div>

          <button className="btn btn-sm btn-outline" onClick={logout}>
            Salir
          </button>
        </div>
      </header>

      {showChangePassword && (
        <ChangePasswordModal onClose={() => setShowChangePassword(false)} />
      )}
    </>
  );
};
