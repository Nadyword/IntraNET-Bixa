import React from 'react';
import { useUIStore } from '../../store/uiStore';
import { useAuthStore } from '../../store/authStore';
import './Sidebar.css';

interface NavItem {
  label: string;
  icon: string;
  id: string;
  badge?: number;
  adminOnly?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Mis Datos', icon: '📄', id: 'mydata' },
  { label: 'Cultura & Beneficios', icon: '👥', id: 'culture' },
  { label: 'Consultas', icon: 'ℹ️', id: 'consultas' },
  { label: 'Solicitudes', icon: '📋', id: 'solicitudes'},
  { label: 'Mis Trámites', icon: '📊', id: 'tramites' },
  { label: '🔒 Portal del Líder', icon: '⭐', id: 'leader', adminOnly: true },
  { label: 'Soporte', icon: '💬', id: 'soporte' }
];

export const Sidebar: React.FC = () => {
  const { sidebarOpen, activeSection, setActiveSection } = useUIStore();
  const rolId = useAuthStore((state) => state.user?.rolId);
  const isAdmin = rolId === '1';
  const visibleItems = NAV_ITEMS.filter((item) => !item.adminOnly || isAdmin);

  return (
    <nav className={`sidebar ${sidebarOpen ? 'open' : ''}`}>
      {visibleItems.map((item) => (
        <div
          key={item.id}
          className={`nav-item ${activeSection === item.id ? 'active' : ''}`}
          onClick={() => setActiveSection(item.id)}
        >
          <span className="nav-icon">{item.icon}</span>
          <span className="nav-label">{item.label}</span>
          {item.badge && <span className="nav-badge">{item.badge}</span>}
        </div>
      ))}
    </nav>
  );
};
