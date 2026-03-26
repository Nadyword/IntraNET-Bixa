import React from 'react';
import { Sidebar } from './Sidebar';
import { TopHeader } from './TopHeader';
import { Toast } from '../ui/Toast';
import './AppLayout.css';

interface AppLayoutProps {
  children: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  return (
    <div className="app-layout">
      <TopHeader />
      <Sidebar />
      <main className="main-content">
        {children}
      </main>
      <Toast />
    </div>
  );
};
