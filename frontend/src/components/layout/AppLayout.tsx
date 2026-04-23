import React, { useEffect } from 'react';
import { Sidebar } from './Sidebar';
import { TopHeader } from './TopHeader';
import { Toast } from '../ui/Toast';
import { useAuthStore } from '../../store/authStore';
import { useUserProfileStore } from '../../store/userProfileStore';
import { userService } from '../../services/userService';
import './AppLayout.css';

interface AppLayoutProps {
  children: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const { user } = useAuthStore();
  const {
    profile,
    familyGroup,
    setProfile,
    setFamilyGroup,
    setFamilyGroupLoading,
    setLoading,
    setError,
  } = useUserProfileStore();

  useEffect(() => {
    if (!user?.taxId) return;

    // SnEmple: solo si no está en sessionStorage (primer login o sesión nueva)
    if (!profile) {
      setLoading(true);
      userService.getProfile(user.taxId)
        .then(({ data: res }) => {
          if (res.success) setProfile(res.data);
          else setError(res.message);
        })
        .catch(() => setError('No se pudo cargar el perfil del empleado'));
    }

    // GrupoFa: siempre al montar (no persiste entre recargas)
    if (familyGroup.length === 0) {
      setFamilyGroupLoading(true);
      userService.getFamilyGroup(user.taxId)
        .then(({ data: res }) => {
          setFamilyGroup(res.success ? (res.data ?? []) : []);
        })
        .catch(() => setFamilyGroup([]));
    }
  }, [user?.taxId]);

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
