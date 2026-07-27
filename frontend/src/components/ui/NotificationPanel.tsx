import React from 'react';
import { useAuthStore } from '../../store/authStore';
import { useUIStore } from '../../store/uiStore';
import { useNotificationStore } from '../../store/notificationStore';
import type { NotificationDTO } from '../../services/notificationService';
import './NotificationPanel.css';

interface NotificationPanelProps {
  onClose: () => void;
}

function timeAgo(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime();
  const minutes = Math.floor(diffMs / 60_000);
  if (minutes < 1) return 'ahora';
  if (minutes < 60) return `hace ${minutes} min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `hace ${hours} h`;
  const days = Math.floor(hours / 24);
  return `hace ${days} d`;
}

export const NotificationPanel: React.FC<NotificationPanelProps> = ({ onClose }) => {
  const user = useAuthStore((state) => state.user);
  const { setActiveSection } = useUIStore();
  const { summary, markAsRead, markAllAsRead } = useNotificationStore();

  const isAdmin = user?.rolId === '1';
  const isEmployee = user?.rolId === '3';

  const goTo = (section: string) => {
    setActiveSection(section);
    onClose();
  };

  const handleNotificationClick = (n: NotificationDTO) => {
    if (!n.isRead) markAsRead(n.id);

    if (n.referenceType === 'Tramite') {
      goTo('tramites');
    } else if (n.referenceType === 'Chat') {
      goTo(isEmployee ? 'soporte' : 'leader');
    } else {
      onClose();
    }
  };

  const pendingItems: { label: string; section: string }[] = [];
  if (summary) {
    if (summary.pendingApprovals > 0) {
      pendingItems.push({
        label: `Tienes ${summary.pendingApprovals} solicitud(es) pendiente(s) de tu firma`,
        section: 'solicitudes',
      });
    }
    if (isAdmin && summary.pendingAdminApproval > 0) {
      pendingItems.push({
        label: `${summary.pendingAdminApproval} solicitud(es) esperando tu aprobación final`,
        section: 'administracion',
      });
    }
    if (isAdmin && summary.pendingArchive > 0) {
      pendingItems.push({
        label: `${summary.pendingArchive} solicitud(es) lista(s) para archivar`,
        section: 'administracion',
      });
    }
  }

  const recent = summary?.recent ?? [];

  return (
    <div className="notif-panel">
      <div className="notif-panel-header">
        <span>Notificaciones</span>
        {summary && summary.unreadCount > 0 && (
          <button className="notif-panel-markall" onClick={() => markAllAsRead()}>
            Marcar todas leídas
          </button>
        )}
      </div>

      {pendingItems.length > 0 && (
        <div className="notif-panel-section">
          <div className="notif-panel-section-title">Pendientes de acción</div>
          {pendingItems.map((item) => (
            <button
              key={item.label}
              className="notif-panel-pending-item"
              onClick={() => goTo(item.section)}
            >
              {item.label}
            </button>
          ))}
        </div>
      )}

      <div className="notif-panel-section">
        <div className="notif-panel-section-title">Recientes</div>
        {recent.length === 0 && (
          <div className="notif-panel-empty">No tienes notificaciones.</div>
        )}
        {recent.map((n) => (
          <button
            key={n.id}
            className={`notif-panel-item${n.isRead ? '' : ' notif-panel-item--unread'}`}
            onClick={() => handleNotificationClick(n)}
          >
            {!n.isRead && <span className="notif-panel-dot" />}
            <div className="notif-panel-item-body">
              <div className="notif-panel-item-title">{n.title}</div>
              <div className="notif-panel-item-message">{n.message}</div>
              <div className="notif-panel-item-time">{timeAgo(n.createdAt)}</div>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
};
