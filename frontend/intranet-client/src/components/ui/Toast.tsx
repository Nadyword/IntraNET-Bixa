import React from 'react';
import { useUIStore } from '../../store/uiStore';
import './Toast.css';

export const Toast: React.FC = () => {
  const { toasts, removeToast } = useUIStore();

  return (
    <div className="toast-container">
      {toasts.map((toast) => (
        <div key={toast.id} className={`toast toast-${toast.type}`}>
          <span>{toast.message}</span>
          <button onClick={() => removeToast(toast.id)}>×</button>
        </div>
      ))}
    </div>
  );
};
