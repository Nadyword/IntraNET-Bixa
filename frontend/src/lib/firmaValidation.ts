const REQUIRED_SIZE = 225;

/**
 * Valida en el cliente que el archivo sea un PNG de 225x225 píxeles.
 * Retorna un mensaje de error, o null si el archivo es válido.
 * La validación definitiva ocurre igualmente en el backend.
 */
export async function validateFirmaFile(file: File): Promise<string | null> {
  if (file.type !== 'image/png' && !file.name.toLowerCase().endsWith('.png')) {
    return 'La firma debe ser un archivo PNG.';
  }

  const url = URL.createObjectURL(file);
  try {
    const { width, height } = await new Promise<{ width: number; height: number }>((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve({ width: img.naturalWidth, height: img.naturalHeight });
      img.onerror = () => reject(new Error('No se pudo leer la imagen.'));
      img.src = url;
    });

    if (width !== REQUIRED_SIZE || height !== REQUIRED_SIZE) {
      return `La imagen debe medir exactamente ${REQUIRED_SIZE}x${REQUIRED_SIZE} píxeles (seleccionaste ${width}x${height}).`;
    }
    return null;
  } catch {
    return 'No se pudo leer la imagen seleccionada.';
  } finally {
    URL.revokeObjectURL(url);
  }
}
