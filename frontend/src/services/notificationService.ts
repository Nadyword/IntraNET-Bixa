import api from '../lib/api';
import type { ApiResponse } from './authService';

export interface NotificationDTO {
  id: number;
  notificationType: string;
  title: string;
  message: string;
  isRead: boolean;
  referenceType: 'Tramite' | 'Chat' | string | null;
  referenceId: number | null;
  readAt: string | null;
  createdAt: string;
}

export interface NotificationSummaryDTO {
  unreadCount: number;
  pendingApprovals: number;
  pendingAdminApproval: number;
  pendingArchive: number;
  recent: NotificationDTO[];
}

export const notificationService = {
  getSummary: () =>
    api.get<ApiResponse<NotificationSummaryDTO>>('/notificationsApi/Summary'),

  markAsRead: (notificationId: number) =>
    api.put<ApiResponse<boolean>>(`/notificationsApi/${notificationId}/MarkAsRead`),

  markAllAsRead: () =>
    api.put<ApiResponse<number>>('/notificationsApi/MarkAllAsRead'),
};
