/**
 * Formulario de la planilla AR-I.
 *
 * El resto de la planilla (identidad, U.T., carga familiar, remuneraciones estimadas) lo resuelve
 * la consulta a Profit en el backend; aquí solo vive lo que el empleado decide.
 *
 * Vive fuera del modal porque la página que lo abre sostiene su estado, de modo que lo escrito
 * sigue disponible al cerrar y reabrir el modal mientras el usuario no salga de la sección.
 */
export interface FormAri {
  mes: string;
  desgravamenTipo: '' | 'unico' | 'detallado';
  institutosDocentes: string;
  primasSeguro: string;
  serviciosMedicos: string;
  interesesVivienda: string;
}

export const ARI_FORM_INICIAL: FormAri = {
  mes: '',
  desgravamenTipo: '',
  institutosDocentes: '',
  primasSeguro: '',
  serviciosMedicos: '',
  interesesVivienda: '',
};
