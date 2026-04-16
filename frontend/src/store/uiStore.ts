import { create } from 'zustand';

export interface Toast {
  message: string;
  type: 'success' | 'error' | 'info';
  id: string;
}

export interface UIState {
  sidebarOpen: boolean;
  notifPanelOpen: boolean;
  toasts: Toast[];
  activeSection: string;
  toggleSidebar: () => void;
  toggleNotifPanel: () => void;
  showToast: (message: string, type?: 'success' | 'error' | 'info') => void;
  removeToast: (id: string) => void;
  setActiveSection: (section: string) => void;
}

export const useUIStore = create<UIState>((set) => ({
  sidebarOpen: false,
  notifPanelOpen: false,
  toasts: [],
  activeSection: 'home',

  toggleSidebar: () =>
    set((state) => ({ sidebarOpen: !state.sidebarOpen })),

  toggleNotifPanel: () =>
    set((state) => ({ notifPanelOpen: !state.notifPanelOpen })),

  showToast: (message, type = 'info') => {
    const id = Date.now().toString();
    set((state) => ({
      toasts: [...state.toasts, { message, type, id }],
    }));
    setTimeout(() => {
      set((state) => ({
        toasts: state.toasts.filter((t) => t.id !== id),
      }));
    }, 3000);
  },

  removeToast: (id) =>
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    })),

  setActiveSection: (section) => set({ activeSection: section }),
}));
