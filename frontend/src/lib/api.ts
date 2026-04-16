import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { useAuthStore } from '../store/authStore';

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001/api';

export const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach JWT token
api.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle 401 and 403
// Solo hace logout+redirect si ya había una sesión activa.
// Un 401 durante el login es un fallo de credenciales, no de sesión.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthenticated = useAuthStore.getState().isAuthenticated;

    if (error.response?.status === 401 && isAuthenticated) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    } else if (error.response?.status === 403 && isAuthenticated) {
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
