import React, { useState } from 'react';
import api from '../../lib/api';
import './ForgotPasswordModal.css';

interface Props {
  onClose: () => void;
}

export const ForgotPasswordModal: React.FC<Props> = ({ onClose }) => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/auth/forgot-password', { email });
      setSent(true);
    } catch {
      setError('Ocurrió un error. Intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>

        {sent ? (
          <div className="modal-success">
            <div className="modal-success-icon">✉</div>
            <h3>Usuario enviado</h3>
            <p>
              Si el Cedula está registrado, recibirás un enlace para restablecer
              tu contraseña. Revisa también la bandeja de spam.
            </p>
            <button className="modal-btn-primary" onClick={onClose}>
              Entendido
            </button>
          </div>
        ) : (
          <>
            <h3>Recuperar contraseña</h3>
            <p>Ingresa tu Cedula y te enviaremos un enlace para restablecer tu contraseña.</p>

            <form onSubmit={handleSubmit}>
              <div className="modal-field">
                <label>Cedula</label>
                <input
                  type="text"
                  placeholder="Cedula a recuperar"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoFocus
                />
              </div>

              {error && <p className="modal-error">{error}</p>}

              <div className="modal-actions">
                <button type="button" className="modal-btn-secondary" onClick={onClose}>
                  Cancelar
                </button>
                <button type="submit" className="modal-btn-primary" disabled={loading}>
                  {loading ? 'Enviando...' : 'Enviar enlace'}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
};
