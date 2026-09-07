import api from '../lib/api';
import type { ApiResponse } from './authService';

/** Un firmante dentro de la cadena de aprobación, en la posición en que le toca firmar. */
export interface FirmanteDTO {
  ci: string;
  nombre: string;
  /** Posición en la que firma; 1 es el primero. */
  orden: number;
  /** `true` si viene de la jerarquía de supervisores de Profit; `false` si lo agregó un ajuste. */
  desdeProfit: boolean;
}

/** Cadena de firmantes de un empleado: la de Profit y la que se usará realmente. */
export interface AjusteFirmantesDTO {
  ci: string;
  nombreCompleto: string | null;
  tieneAjuste: boolean;
  /** Cadena de supervisores tal como la devuelve Profit, sin ajustes. */
  original: FirmanteDTO[];
  /** Cadena que firmará realmente: la de Profit con los ajustes aplicados. */
  efectiva: FirmanteDTO[];
}

/** Usuario de la intranet que puede añadirse como firmante. */
export interface CandidatoFirmanteDTO {
  ci: string;
  nombre: string;
}

export const firmantesService = {
  /** Cadena de firmantes del empleado, con y sin ajustes. */
  getByCi: (ci: string) =>
    api.get<ApiResponse<AjusteFirmantesDTO>>(`/firmantes/${ci}`),

  /**
   * Guarda la cadena en el orden indicado. El backend la compara contra la de Profit y persiste
   * solo las diferencias, así que enviar la original equivale a no tener ajuste.
   */
  guardar: (ci: string, firmantes: string[]) =>
    api.put<ApiResponse<AjusteFirmantesDTO>>(`/firmantes/${ci}`, { firmantes }),

  /** Elimina el ajuste: el empleado vuelve a firmar según la jerarquía de Profit. */
  restablecer: (ci: string) =>
    api.delete<ApiResponse<AjusteFirmantesDTO>>(`/firmantes/${ci}`),

  /** Usuarios activos que pueden añadirse como firmantes. */
  getCandidatos: (q: string) =>
    api.get<ApiResponse<CandidatoFirmanteDTO[]>>('/firmantes/candidatos', { params: { q } }),

  /** CIs de los empleados que tienen un ajuste guardado. */
  getAjustados: () =>
    api.get<ApiResponse<string[]>>('/firmantes/ajustados'),
};
