import { create } from 'zustand';
import { notificationService, type NotificationSummaryDTO } from '../services/notificationService';

const POLL_INTERVAL_MS = 60_000;

export interface NotificationState {
  summary: NotificationSummaryDTO | null;
  isLoading: boolean;
  fetchSummary: () => Promise<void>;
  markAsRead: (notificationId: number) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  startPolling: () => void;
  stopPolling: () => void;
}

let pollTimer: ReturnType<typeof setInterval> | null = null;

export const useNotificationStore = create<NotificationState>((set, get) => ({
  summary: null,
  isLoading: false,

  fetchSummary: async () => {
    set({ isLoading: true });
    try {
      const { data: res } = await notificationService.getSummary();
      if (res.success && res.data) {
        set({ summary: res.data });
      }
    } catch {
      // silencioso: el badge simplemente no se actualiza en este ciclo
    } finally {
      set({ isLoading: false });
    }
  },

  markAsRead: async (notificationId: number) => {
    const current = get().summary;
    if (current) {
      set({
        summary: {
          ...current,
          unreadCount: Math.max(0, current.unreadCount - 1),
          recent: current.recent.map((n) =>
            n.id === notificationId ? { ...n, isRead: true } : n
          ),
        },
      });
    }
    try {
      await notificationService.markAsRead(notificationId);
    } catch {
      await get().fetchSummary();
    }
  },

  markAllAsRead: async () => {
    const current = get().summary;
    if (current) {
      set({
        summary: {
          ...current,
          unreadCount: 0,
          recent: current.recent.map((n) => ({ ...n, isRead: true })),
        },
      });
    }
    try {
      await notificationService.markAllAsRead();
    } catch {
      await get().fetchSummary();
    }
  },

  startPolling: () => {
    if (pollTimer) return;
    get().fetchSummary();
    pollTimer = setInterval(() => get().fetchSummary(), POLL_INTERVAL_MS);
  },

  stopPolling: () => {
    if (pollTimer) {
      clearInterval(pollTimer);
      pollTimer = null;
    }
    set({ summary: null });
  },
}));
