import api from '../lib/api';

export interface ValidateTokenResponse {
  valid: boolean;
  email?: string;
  nombre?: string;
  cedula?: string;
  cargo?: string;
  message?: string;
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

export interface AuthResponse {
  token: string;
  user: {
    id: string;
    email: string;
    nombre: string;
    cargo: string;
  };
}

export const authService = {
  validateToken: async (token: string): Promise<ValidateTokenResponse> => {
    try {
      const response = await api.get<ValidateTokenResponse>(
        `/auth/validate-token?token=${token}`
      );
      return response.data;
    } catch (error) {
      return { valid: false, message: 'Error validando token' };
    }
  },

  activateAccount: async (
    data: ActivateAccountRequest
  ): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/activate-account', data);
    return response.data;
  },

  createEmployee: async (data: {
    email: string;
    nombre: string;
    cedula: string;
    cargo: string;
  }) => {
    const response = await api.post('/auth/create-employee', data);
    return response.data;
  },
};

export default authService;
