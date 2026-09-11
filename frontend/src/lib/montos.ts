/**
 * Montos escritos en formato venezolano: "." separa los miles y "," los decimales (1.234,56).
 */

/** Convierte "1.234,56" en 1234.56. Una entrada inválida devuelve 0. */
export const parseMontoVE = (valor: string): number => {
  const limpio = valor.replace(/\s/g, '').replace(/\./g, '').replace(',', '.');
  if (!/^\d*\.?\d*$/.test(limpio) || !/\d/.test(limpio)) return 0;
  return Math.round(Number(limpio) * 100) / 100;
};

/** Convierte 1234.56 en "1.234,56" para precargar un input; 0 queda vacío. */
export const formatMontoInputVE = (monto: number): string => {
  if (!monto) return '';
  const [enteros, decimales] = monto.toFixed(2).split('.');
  return `${enteros.replace(/\B(?=(\d{3})+(?!\d))/g, '.')},${decimales}`;
};

/** Compara en céntimos para evitar errores de coma flotante (0.1 + 0.2 !== 0.3). */
export const montosIguales = (a: number, b: number): boolean =>
  Math.round(a * 100) === Math.round(b * 100);
