import React, { useEffect, useRef, useState } from 'react';
import { Sidebar } from './Sidebar';
import { TopHeader } from './TopHeader';
import { Toast } from '../ui/Toast';
import { CelebrationModal } from '../ui/CelebrationModal';
import { useAuthStore } from '../../store/authStore';
import { useUserProfileStore } from '../../store/userProfileStore';
import { useNotificationStore } from '../../store/notificationStore';
import { userService } from '../../services/userService';
import { getCelebrationInfo, type CelebrationInfo } from '../../lib/celebrations';
import './AppLayout.css';

interface AppLayoutProps {
  children: React.ReactNode;
}

export const AppLayout: React.FC<AppLayoutProps> = ({ children }) => {
  const { user } = useAuthStore();
  const { startPolling, stopPolling } = useNotificationStore();
  const {
    profile,
    familyGroup,
    setProfile,
    setFamilyGroup,
    setFamilyGroupLoading,
    setLoading,
    setError,
  } = useUserProfileStore();

  const [celebration, setCelebration] = useState<CelebrationInfo | null>(null);
  const celebrationCheckedRef = useRef(false);

  useEffect(() => {
    startPolling();
    return () => stopPolling();
  }, [startPolling, stopPolling]);

  useEffect(() => {
    if (celebrationCheckedRef.current || !profile) return;
    celebrationCheckedRef.current = true;

    const info = getCelebrationInfo(profile.fechaNac, profile.fechaIng);
    if (info.esCumpleanos || info.esAniversario) {
      setCelebration(info);
    }
  }, [profile]);

  useEffect(() => {
    if (!user?.ci) return;

    // SnEmple: solo si no está en sessionStorage (primer login o sesión nueva)
    if (!profile) {
      setLoading(true);
      userService.getProfile(user.ci)
        .then(({ data: res }) => {
          if (res.success) setProfile(res.data);
          else setError(res.message);
        })
        .catch(() => setError('No se pudo cargar el perfil del empleado'));
    }

    // GrupoFa: siempre al montar (no persiste entre recargas)
    if (familyGroup.length === 0) {
      setFamilyGroupLoading(true);
      userService.getFamilyGroup(user.ci)
        .then(({ data: res }) => {
          setFamilyGroup(res.success ? (res.data ?? []) : []);
        })
        .catch(() => setFamilyGroup([]));
    }
  }, [user?.ci]);

  return (
    <div className="app-layout">
      <TopHeader />
      <Sidebar />
      <main className="main-content">
        {children}
      </main>
      <Toast />
      {celebration && profile && (
        <CelebrationModal
          nombre={profile.nombres ?? ''}
          esCumpleanos={celebration.esCumpleanos}
          esAniversario={celebration.esAniversario}
          aniosAniversario={celebration.aniosAniversario}
          onClose={() => setCelebration(null)}
        />
      )}
    </div>
  );
};
