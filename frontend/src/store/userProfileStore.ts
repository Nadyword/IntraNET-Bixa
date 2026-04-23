import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { GrupoFamiliar } from '../services/userService';

export type { GrupoFamiliar };

export interface UserProfile {
  codEmp: string | null;
  nombres: string | null;
  apellidos: string | null;
  ci: string | null;
  rif: string | null;
  correoP: string | null;
  correoE: string | null;
  fechaNac: string | null;
  direccion: string | null;
  edad: number | null;
  sexo: string | null;
  timeAtEm: number | null;
  fechaIng: string | null;
  telefono: string | null;
  estadoCivil: string | null;
  desCargo: string | null;
  desDepart: string | null;
  desCont: string | null;
  desUbicacion: string | null;
  grupoSang: string | null;
  nacionalidad: string | null;
  cuentaBanc1: string | null;
}

interface UserProfileState {
  profile: UserProfile | null;
  familyGroup: GrupoFamiliar[];
  familyGroupLoading: boolean;
  loading: boolean;
  error: string | null;
  setProfile: (profile: UserProfile) => void;
  setFamilyGroup: (familyGroup: GrupoFamiliar[]) => void;
  setFamilyGroupLoading: (loading: boolean) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearProfile: () => void;
}

export const useUserProfileStore = create<UserProfileState>()(
  persist(
    (set) => ({
      profile: null,
      familyGroup: [],
      familyGroupLoading: false,
      loading: false,
      error: null,
      setProfile: (profile) => set({ profile, loading: false, error: null }),
      setFamilyGroup: (familyGroup) => set({ familyGroup, familyGroupLoading: false }),
      setFamilyGroupLoading: (familyGroupLoading) => set({ familyGroupLoading }),
      setLoading: (loading) => set({ loading }),
      setError: (error) => set({ error, loading: false }),
      clearProfile: () => set({ profile: null, familyGroup: [], familyGroupLoading: false, loading: false, error: null }),
    }),
    {
      name: 'bixa-user-profile',
      storage: createJSONStorage(() => sessionStorage),
      partialize: (state) => ({ profile: state.profile }),
    }
  )
);
