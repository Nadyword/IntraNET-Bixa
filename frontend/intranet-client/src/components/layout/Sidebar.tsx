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
  { label: 'Inicio', icon: '🏠', id: 'home' },
  { label: 'Mis Datos', icon: '👤', id: 'mydata' },
  { label: 'Cultura & Beneficios', icon: '👥', id: 'culture' },
  { label: 'Consultas', icon: 'ℹ️', id: 'consultas' },
  { label: 'Solicitudes', icon: '📋', id: 'solicitudes', badge: 2 },
  { label: 'Mis Trámites', icon: '📊', id: 'tramites' },
  { label: '🔒 Portal del Líder', icon: '⭐', id: 'leader', badge: 5 },
  { label: 'Asistente/Soporte', icon: '💬', id: 'chatbot' },
  { label: 'Onboarding', icon: '🛡️', id: 'onboarding' },
];

export const Sidebar: React.FC = () => {
  const { sidebarOpen, setActiveSection } = useUIStore();

  return (
    <nav className={`sidebar ${sidebarOpen ? 'open' : ''}`}>
      {NAV_ITEMS.map((item) => (
        <div
          key={item.id}
          className="nav-item"
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
