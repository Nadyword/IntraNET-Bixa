import api from '../lib/api';
import type { ApiResponse } from './authService';

export interface FAQsDTO {
  id: number;
  question: string;
  response: string;
  /** Posición en la que se muestra la pregunta. Menor valor = se muestra primero. */
  displayOrder: number;
}

/** Ordena las preguntas frecuentes por el orden definido en el portal del líder. */
export const ordenarFAQs = (faqs: FAQsDTO[]): FAQsDTO[] =>
  [...faqs].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0) || a.id - b.id);

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

export interface SolicitudChatDTO {
  userCi: string;
  firstName: string;
  lastName: string;
  respondido: number;
}

export const soporteService = {
  getHistoriChat: (ci: string) =>
    api.get<ApiResponse<SoporteChatMessageDTO[]>>(`/soporte/HistoriChat/${ci}`),

  sendMessage: (payload: SoporteChatMDTO) =>
    api.post<ApiResponse<null>>('/soporte/NewMessage', payload),

  sendAnswer: (payload: SoporteChatRDTO) =>
    api.post<ApiResponse<null>>('/soporte/NewAnswer', payload),

  getChatAbiertos: () =>
    api.get<ApiResponse<SolicitudChatDTO[]>>('/soporte/ChatAbiertos'),

  setMessageStatus: (ci: string) =>
    api.put<ApiResponse<boolean>>(`/soporte/SetMessageStatus/${ci}`),

  getFAQs: () =>
    api.get<ApiResponse<FAQsDTO[]>>('/soporte/FAQ'),

  createFAQ: (payload: FAQsDTO) =>
    api.post<ApiResponse<boolean>>('/soporte/CreateFAQ', payload),

  updateFAQ: (payload: FAQsDTO) =>
    api.put<ApiResponse<boolean>>('/soporte/UpdateFAQ', payload),

  deleteFAQ: (id: number) =>
    api.delete<ApiResponse<boolean>>(`/soporte/DeleteFAQ/${id}`),
};
