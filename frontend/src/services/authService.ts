import api from '../lib/api';

export interface LoginDTO {
  token: string;
  refreshToken: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
}

export const authService = {
  login: (taxId: string, password: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/AuthenticateLogin', { taxId, password }),

  firstLogin: (taxId: string, newPassword: string, token: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/FirstLogin', { taxId, newPassword, token }),

  validateToken: (token: string) =>
    api.post<{ valid: boolean; message?: string }>('/login/validateToken', { token }),

  refreshToken: (refreshToken: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/RefreshToken', { refreshToken }),
};

export default authService;
