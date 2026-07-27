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
  getProfile: (ci: string) =>
    api.get<ApiResponse<UserProfile>>(`/usersProfit/${ci}/SnEmple`),

  getFamilyGroup: (ci: string) =>
    api.get<ApiResponse<GrupoFamiliar[]>>(`/usersProfit/${ci}/GrupoFa`),

  solicitarCorreccion: (comentario: string) =>
    api.post<ApiResponse<boolean>>('/users/SolicitarCorreccion', { comentario }),
};