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

export interface ValidateTokenResponse {
  valid: boolean;
  message?: string;
  nombre?: string;
  cedula?: string;
  cargo?: string;
}

export interface ActivateAccountRequest {
  token: string;
  password: string;
  confirmPassword: string;
  telefono: string;
  fechaNacimiento: Date;
  cedula: string;
  cargo: string;
}

export interface ActivateAccountResponse {
  user: string;
  token: string;
}

export const authService = {
  login: (ci: string, password: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/AuthenticateLogin', { ci, password }),

  firstLogin: (ci: string, newPassword: string, token: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/FirstLogin', { ci, newPassword, token }),

  validateToken: (token: string) =>
    api.post<ValidateTokenResponse>('/login/validateToken', { token }),

  refreshToken: (refreshToken: string) =>
    api.post<ApiResponse<LoginDTO>>('/login/RefreshToken', { refreshToken }),

  activateAccount: (data: ActivateAccountRequest) =>
    api.post<ActivateAccountResponse>('/login/ActivateAccount', data).then((r) => r.data),
};

export default authService;
