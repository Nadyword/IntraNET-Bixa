const REQUIRED_RATIO = 2;

/**
 * Valida en el cliente que el archivo sea un PNG con proporción ancho:alto de 2:1.
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

    if (width !== height * REQUIRED_RATIO) {
      return `La imagen debe tener una proporción de 2:1 (ancho el doble del alto). Seleccionaste ${width}x${height}.`;
    }
    return null;
  } catch {
    return 'No se pudo leer la imagen seleccionada.';
  } finally {
    URL.revokeObjectURL(url);
  }
}
