import React from 'react';
import { useUIStore } from '../../store/uiStore';
import { useAuthStore } from '../../store/authStore';
import { useNotificationStore } from '../../store/notificationStore';
import './Sidebar.css';

interface NavItem {
  label: string;
  icon: string;
  id: string;
  adminOnly?: boolean;
  nonAdminOnly?: boolean;
  nonEmployeeOnly?: boolean;
  supervisorOnly?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Mis Datos',           icon: '📄', id: 'mydata' },
  { label: 'Cultura & Beneficios', icon: '👥', id: 'culture' },
  { label: 'Consultas',            icon: 'ℹ️', id: 'consultas' },
  { label: 'Mis Trámites',         icon: '📊', id: 'tramites' },
  { label: 'Solicitudes',          icon: '📋', id: 'solicitudes', nonEmployeeOnly: true },
  { label: '🔒 Portal del Líder',  icon: '⭐', id: 'leader',          adminOnly: true },
  { label: 'Mi Equipo',            icon: '🧑‍🤝‍🧑', id: 'miequipo',       supervisorOnly: true },
  { label: 'Administración',       icon: '🗂️', id: 'administracion',  adminOnly: true },
  { label: 'Soporte',              icon: '💬', id: 'soporte',          nonAdminOnly: true },
];

export const Sidebar: React.FC = () => {
  const { sidebarOpen, activeSection, setActiveSection } = useUIStore();
  const rolId = useAuthStore((state) => state.user?.rolId);
  const { summary } = useNotificationStore();

  const isAdmin      = rolId === '1';
  const isSupervisor = rolId === '2';
  const isEmployee   = rolId === '3';

  const badgesById: Record<string, number> = summary
    ? {
        solicitudes: summary.pendingApprovals,
        administracion: summary.pendingAdminApproval + summary.pendingArchive,
      }
    : {};

  const visibleItems = NAV_ITEMS.filter((item) => {
    if (item.adminOnly && !isAdmin) return false;
    if (item.nonAdminOnly && isAdmin) return false;
    if (item.nonEmployeeOnly && isEmployee) return false;
    if (item.supervisorOnly && !isSupervisor) return false;
    return true;
  });

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
          {badgesById[item.id] > 0 && <span className="nav-badge">{badgesById[item.id]}</span>}
        </div>
      ))}
    </nav>
  );
};
