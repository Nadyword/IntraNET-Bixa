import React, { useCallback, useEffect, useState } from 'react';
import {
  firmantesService,
  type AjusteFirmantesDTO,
  type CandidatoFirmanteDTO,
  type FirmanteDTO,
} from '../../services/firmantesService';
import './ForgotPasswordModal.css';
import './AjustarFirmaModal.css';

interface Props {
  ci: string;
  nombreCompleto?: string | null;
  onClose: () => void;
  /** Se invoca al guardar o restablecer, para refrescar quién tiene ajuste en el listado. */
  onSaved: () => void;
}

/** Los firmantes se comparan por CI en todo el modal. */
const mismosFirmantes = (a: FirmanteDTO[], b: FirmanteDTO[]): boolean =>
  a.length === b.length && a.every((f, i) => f.ci === b[i].ci);

export const AjustarFirmaModal: React.FC<Props> = ({ ci, nombreCompleto, onClose, onSaved }) => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  const [ajuste, setAjuste] = useState<AjusteFirmantesDTO | null>(null);
  /** Cadena que se está editando; es lo que se enviará al guardar. */
  const [firmantes, setFirmantes] = useState<FirmanteDTO[]>([]);

  const [busqueda, setBusqueda] = useState('');
  const [candidatos, setCandidatos] = useState<CandidatoFirmanteDTO[]>([]);
  const [buscando, setBuscando] = useState(false);

  const cargar = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await firmantesService.getByCi(ci);
      if (res.data.success) {
        setAjuste(res.data.data);
        setFirmantes(res.data.data.efectiva);
      } else {
        setError(res.data.message || 'No se pudo cargar la cadena de firmantes.');
      }
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'No se pudo cargar la cadena de firmantes.');
    } finally {
      setLoading(false);
    }
  }, [ci]);

  useEffect(() => { cargar(); }, [cargar]);

  // El buscador de firmantes por agregar espera a que el administrador deje de escribir.
  useEffect(() => {
    const termino = busqueda.trim();
    if (!termino) {
      setCandidatos([]);
      return;
    }

    let cancelled = false;
    setBuscando(true);
    const timeout = setTimeout(async () => {
      try {
        const res = await firmantesService.getCandidatos(termino);
        if (!cancelled) setCandidatos(res.data.data ?? []);
      } catch {
        if (!cancelled) setCandidatos([]);
      } finally {
        if (!cancelled) setBuscando(false);
      }
    }, 300);

    return () => { cancelled = true; clearTimeout(timeout); };
  }, [busqueda]);

  const mover = (desde: number, hacia: number) => {
    if (hacia < 0 || hacia >= firmantes.length) return;
    const siguiente = [...firmantes];
    const [movido] = siguiente.splice(desde, 1);
    siguiente.splice(hacia, 0, movido);
    setFirmantes(siguiente);
    setSuccessMessage('');
  };

  const quitar = (ciFirmante: string) => {
    setFirmantes((actuales) => actuales.filter((f) => f.ci !== ciFirmante));
    setSuccessMessage('');
  };

  const agregar = (candidato: CandidatoFirmanteDTO) => {
    if (candidato.ci === ci) {
      setError('El empleado no puede firmar su propia solicitud.');
      return;
    }
    if (firmantes.some((f) => f.ci === candidato.ci)) {
      setError('Ese firmante ya está en la cadena.');
      return;
    }

    setError('');
    setSuccessMessage('');
    setFirmantes((actuales) => [
      ...actuales,
      { ci: candidato.ci, nombre: candidato.nombre, orden: actuales.length + 1, desdeProfit: false },
    ]);
    setBusqueda('');
    setCandidatos([]);
  };

  const guardar = async () => {
    if (firmantes.length === 0) {
      setError('La cadena debe tener al menos un firmante.');
      return;
    }

    setSaving(true);
    setError('');
    setSuccessMessage('');
    try {
      const res = await firmantesService.guardar(ci, firmantes.map((f) => f.ci));
      if (res.data.success) {
        setAjuste(res.data.data);
        setFirmantes(res.data.data.efectiva);
        setSuccessMessage('Firmantes guardados correctamente.');
        onSaved();
      } else {
        setError(res.data.message || 'No se pudo guardar.');
      }
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Error al guardar los firmantes.');
    } finally {
      setSaving(false);
    }
  };

  const restablecer = async () => {
    setSaving(true);
    setError('');
    setSuccessMessage('');
    try {
      const res = await firmantesService.restablecer(ci);
      if (res.data.success) {
        setAjuste(res.data.data);
        setFirmantes(res.data.data.efectiva);
        setSuccessMessage('Se restableció la cadena de firmantes de Profit.');
        onSaved();
      } else {
        setError(res.data.message || 'No se pudo restablecer.');
      }
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Error al restablecer los firmantes.');
    } finally {
      setSaving(false);
    }
  };

  const sinCambios = ajuste ? mismosFirmantes(firmantes, ajuste.efectiva) : true;
  const igualAProfit = ajuste ? mismosFirmantes(firmantes, ajuste.original) : true;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box-wide" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>
        <h3>Ajustar firma — {ajuste?.nombreCompleto ?? nombreCompleto ?? ci}</h3>
        <p>CI: {ci}</p>

        {loading ? (
          <p className="firma-vacio">Cargando cadena de firmantes...</p>
        ) : (
          <>
            <p className="firma-ayuda">
              Los firmantes aparecen en el orden en que deben firmar. Se guardan solo las diferencias
              respecto de la jerarquía de supervisores de Profit, así que los cambios en esa jerarquía
              se siguen reflejando aquí.
            </p>

            <ol className="firma-lista">
              {firmantes.length === 0 && (
                <li className="firma-vacio">No hay firmantes en la cadena.</li>
              )}
              {firmantes.map((firmante, i) => (
                <li key={firmante.ci} className="firma-item">
                  <span className="firma-orden">{i + 1}</span>
                  <span className="firma-datos">
                    <strong>{firmante.nombre || firmante.ci}</strong>
                    <small>
                      {firmante.ci}
                      {!firmante.desdeProfit && <em className="firma-etiqueta">Agregado</em>}
                    </small>
                  </span>
                  <span className="firma-acciones">
                    <button
                      type="button"
                      title="Subir"
                      onClick={() => mover(i, i - 1)}
                      disabled={saving || i === 0}
                    >
                      ↑
                    </button>
                    <button
                      type="button"
                      title="Bajar"
                      onClick={() => mover(i, i + 1)}
                      disabled={saving || i === firmantes.length - 1}
                    >
                      ↓
                    </button>
                    <button
                      type="button"
                      title="Quitar"
                      className="firma-quitar"
                      onClick={() => quitar(firmante.ci)}
                      disabled={saving}
                    >
                      ✕
                    </button>
                  </span>
                </li>
              ))}
            </ol>

            <div className="firma-agregar">
              <label htmlFor="firma-buscar">Agregar firmante</label>
              <input
                id="firma-buscar"
                type="text"
                placeholder="Buscar usuario por nombre o cédula..."
                value={busqueda}
                onChange={(e) => setBusqueda(e.target.value)}
                disabled={saving}
              />
              {busqueda.trim() && (
                <ul className="firma-candidatos">
                  {buscando && <li className="firma-vacio">Buscando...</li>}
                  {!buscando && candidatos.length === 0 && (
                    <li className="firma-vacio">Sin usuarios activos para esa búsqueda.</li>
                  )}
                  {!buscando && candidatos.map((candidato) => (
                    <li key={candidato.ci}>
                      <button type="button" onClick={() => agregar(candidato)} disabled={saving}>
                        <strong>{candidato.nombre}</strong>
                        <small>{candidato.ci}</small>
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            {igualAProfit && firmantes.length > 0 && (
              <p className="firma-nota">
                La cadena coincide con la de Profit: al guardar no queda ningún ajuste.
              </p>
            )}
            {error && <p className="modal-error">{error}</p>}
            {successMessage && <p className="firma-exito">{successMessage}</p>}

            <div className="modal-actions">
              <button
                className="modal-btn-secondary"
                onClick={restablecer}
                disabled={saving || !ajuste?.tieneAjuste}
                title={ajuste?.tieneAjuste ? 'Volver a la cadena de Profit' : 'Este empleado no tiene ajustes'}
              >
                Restablecer
              </button>
              <button className="modal-btn-secondary" onClick={onClose} disabled={saving}>
                Cerrar
              </button>
              <button className="modal-btn-primary" onClick={guardar} disabled={saving || sinCambios}>
                {saving ? 'Guardando...' : 'Guardar'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};
