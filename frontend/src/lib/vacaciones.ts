const MES_DICIEMBRE = 11; // Date.getMonth() es 0-indexado: enero=0 ... diciembre=11

export const DIAS_RESERVADOS_VACACIONES_COLECTIVAS = 15;

/**
 * Cada diciembre la empresa toma vacaciones colectivas, así que de enero a noviembre
 * se reservan 15 días del saldo del empleado para ese período; desde el 1 de diciembre
 * el saldo completo vuelve a quedar disponible porque ya se está en el mes de disfrute colectivo.
 */
export function ajustarSaldoVacaciones(disponibleAcumulado: number): number {
  const enPeriodoDeReserva = new Date().getMonth() < MES_DICIEMBRE;
  if (!enPeriodoDeReserva) return disponibleAcumulado;
  return Math.max(0, disponibleAcumulado - DIAS_RESERVADOS_VACACIONES_COLECTIVAS);
}
