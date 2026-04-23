import api from '../lib/api';
import type { ApiResponse } from './authService';
import type { UserProfile } from '../store/userProfileStore';

export interface GrupoFamiliar {
  nombre: string | null;
  nacionalidad: string | null;
  edad: number | null;
  sexo: string | null;
  ocupacion: string | null;
  parentesco: string | null;
}

export const userService = {
  getProfile: (taxId: string) =>
    api.get<ApiResponse<UserProfile>>(`/usersProfit/${taxId}/SnEmple`),

  getFamilyGroup: (taxId: string) =>
    api.get<ApiResponse<GrupoFamiliar[]>>(`/usersProfit/${taxId}/GrupoFa`),
};