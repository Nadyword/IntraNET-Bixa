import React, { useEffect, useState } from 'react';
import api from '../../lib/api';
import type { ApiResponse } from '../../services/authService';
import { formatMontoInputVE, montosIguales, parseMontoVE as parseMonto } from '../../lib/montos';
import './ForgotPasswordModal.css';

interface HcMesRegistroDTO {
  ci: string;
  nombreCompleto: string | null;
  mes1: number;
  mes2: number;
  mes3: number;
  primaTrimBs: number;
  updatedAt: string;
  modifiedByCi: string | null;
}

interface Props {
  ci: string;
  nombreCompleto?: string | null;
  onClose: () => void;
  onSaved: () => void;
}

const formatMonto = (n: number): string =>
  n.toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export const HcEditModal: React.FC<Props> = ({ ci, nombreCompleto, onClose, onSaved }) => {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [primaTrimBs, setPrimaTrimBs] = useState(0);
  const [mes1, setMes1] = useState('');
  const [mes2, setMes2] = useState('');
  const [mes3, setMes3] = useState('');

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const res = await api.get<ApiResponse<HcMesRegistroDTO>>(`/hc/${ci}`);
        if (!cancelled && res.data.success) {
          const r = res.data.data;
          setPrimaTrimBs(r.primaTrimBs);
          setMes1(formatMontoInputVE(r.mes1));
          setMes2(formatMontoInputVE(r.mes2));
          setMes3(formatMontoInputVE(r.mes3));
        }
      } catch {
        if (!cancelled) setError('No se pudo cargar el registro.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [ci]);

  const suma = parseMonto(mes1) + parseMonto(mes2) + parseMonto(mes3);
  const noCoincide = !montosIguales(suma, primaTrimBs);

  const handleSave = async () => {
    if (noCoincide) return;
    setSaving(true);
    setError('');
    setSuccessMessage('');
    try {
      const res = await api.put<ApiResponse<HcMesRegistroDTO>>(`/hc/${ci}`, {
        mes1: parseMonto(mes1),
        mes2: parseMonto(mes2),
        mes3: parseMonto(mes3),
      });
      if (res.data.success) {
        setSuccessMessage('Registro guardado correctamente.');
        onSaved();
      } else {
        setError(res.data.message || 'No se pudo guardar.');
      }
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: { message?: string } } };
      setError(axiosError?.response?.data?.message ?? 'Error al guardar los valores.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>
        <h3>HC — {nombreCompleto ?? ci}</h3>
        <p>CI: {ci}</p>

        {loading ? (
          <p>Cargando...</p>
        ) : (
          <>
            <p style={{ fontSize: 13, color: 'var(--gray-500)', marginTop: '-8px' }}>
              Prima trim en Bs. Factura: <strong>{formatMonto(primaTrimBs)}</strong>
            </p>

            <div className="modal-field">
              <label>Mes 1</label>
              <input type="text" inputMode="decimal" value={mes1} onChange={(e) => setMes1(e.target.value)} placeholder="0,00" disabled={saving} />
            </div>
            <div className="modal-field">
              <label>Mes 2</label>
              <input type="text" inputMode="decimal" value={mes2} onChange={(e) => setMes2(e.target.value)} placeholder="0,00" disabled={saving} />
            </div>
            <div className="modal-field">
              <label>Mes 3</label>
              <input type="text" inputMode="decimal" value={mes3} onChange={(e) => setMes3(e.target.value)} placeholder="0,00" disabled={saving} />
            </div>

            <p style={{ fontSize: 13, color: 'var(--gray-500)' }}>
              Suma: {formatMonto(suma)} / {formatMonto(primaTrimBs)}
            </p>

            {noCoincide && <p className="modal-error">La suma debe ser igual a la Prima trim en Bs. Factura.</p>}
            {error && <p className="modal-error">{error}</p>}
            {successMessage && <p style={{ color: '#1e8449', fontSize: 13, marginTop: '-12px', marginBottom: '16px' }}>{successMessage}</p>}

            <div className="modal-actions">
              <button className="modal-btn-secondary" onClick={onClose} disabled={saving}>Cerrar</button>
              <button className="modal-btn-primary" onClick={handleSave} disabled={saving || noCoincide}>
                {saving ? 'Guardando...' : 'Guardar'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};
