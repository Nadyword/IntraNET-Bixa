import api from '../lib/api';
import type { ApiResponse } from './authService';

export interface SoporteChatMessageDTO {
  id: number;
  userCi: string;
  message: string | null;
  isRead: boolean;
  respondidoPorCi: string | null;
  createdAt: string;
}

export interface SoporteChatMDTO {
  userCi: string;
  message: string;
}

export interface SoporteChatRDTO {
  userCi: string;
  message: string;
  respondidoPorCi?: string;
}

export const soporteService = {
  getHistoriChat: (ci: string) =>
    api.get<ApiResponse<SoporteChatMessageDTO[]>>(`/soporte/HistoriChat/${ci}`),

  sendMessage: (payload: SoporteChatMDTO) =>
    api.post<ApiResponse<null>>('/soporte/NewMessage', payload),

  sendAnswer: (payload: SoporteChatRDTO) =>
    api.post<ApiResponse<null>>('/soporte/NewAnswer', payload),
};
