import React from 'react';
import { useUIStore } from '../../store/uiStore';
import './Sidebar.css';

interface NavItem {
  label: string;
  icon: string;
  id: string;
  badge?: number;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Mis Datos', icon: '📄', id: 'mydata' },
  { label: 'Cultura & Beneficios', icon: '👥', id: 'culture' },
  { label: 'Consultas', icon: 'ℹ️', id: 'consultas' },
  { label: 'Solicitudes', icon: '📋', id: 'solicitudes'},
  { label: 'Mis Trámites', icon: '📊', id: 'tramites' },
  { label: '🔒 Portal del Líder', icon: '⭐', id: 'leader'},
  { label: 'Soporte', icon: '💬', id: 'soporte' }
];

export const Sidebar: React.FC = () => {
  const { sidebarOpen, activeSection, setActiveSection } = useUIStore();

  return (
    <nav className={`sidebar ${sidebarOpen ? 'open' : ''}`}>
      {NAV_ITEMS.map((item) => (
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
