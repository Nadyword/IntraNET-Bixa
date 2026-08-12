import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import api from '../lib/api';
import { PasswordInput } from '../components/ui/PasswordInput';
import './LoginPage.css';
import './ResetPasswordPage.css';

export const ResetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token') ?? '';
  const ci = searchParams.get('ci') ?? '';

  const [validating, setValidating] = useState(true);
  const [tokenValid, setTokenValid] = useState(false);
  const [tokenError, setTokenError] = useState('');

  const [NewPassword, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (!token) {
      setTokenError('Enlace inválido.');
      setValidating(false);
      return;
    }

    api
      .post('/Login/validateToken', { token })
      .then(({ data }) => {
        if (data.valid) {
          setTokenValid(true);
        } else {
          setTokenError(data.message ?? 'Enlace inválido o expirado.');
        }
      })
      .catch(() => {
        setTokenError('No se pudo validar el enlace. Intenta de nuevo.');
      })
      .finally(() => setValidating(false));
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (NewPassword !== confirmPassword) {
      setError('Las contraseñas no coinciden.');
      return;
    }

    if (NewPassword.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }

    setLoading(true);
    try {
      await api.post('/Login/ChangePassword', { ci, NewPassword, token });
      setSuccess(true);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string; errors?: string[] } } };
      const msg =
        axiosErr.response?.data?.message ||
        axiosErr.response?.data?.errors?.[0] ||
        'Ocurrió un error. Intenta de nuevo.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  if (validating) {
    return (
      <div className="login-screen">
        <div className="reset-card">
          <p className="reset-validating">Validando enlace...</p>
        </div>
      </div>
    );
  }

  if (!tokenValid) {
    return (
      <div className="login-screen">
        <div className="reset-card">
          <div className="reset-icon reset-icon--error">✕</div>
          <h2>Enlace inválido</h2>
          <p>{tokenError}</p>
          <button className="btn-login" onClick={() => navigate('/login')}>
            Volver al inicio de sesión
          </button>
        </div>
      </div>
    );
  }

  if (success) {
    return (
      <div className="login-screen">
        <div className="reset-card">
          <div className="reset-icon reset-icon--success">✓</div>
          <h2>Contraseña actualizada</h2>
          <p>Tu contraseña fue cambiada correctamente. Ya puedes iniciar sesión con tu nueva contraseña.</p>
          <button className="btn-login" onClick={() => navigate('/login')}>
            Ir al inicio de sesión
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="login-screen">
      <div className="reset-card">
        <div className="reset-header">
          <div className="reset-logo">BIXA</div>
          <h2>Nueva contraseña</h2>
          <p>Ingresa y confirma tu nueva contraseña para acceder al portal.</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Nueva Contraseña</label>
            <PasswordInput
              placeholder="Mínimo 8 caracteres"
              value={NewPassword}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Confirmar Contraseña</label>
            <PasswordInput
              placeholder="Repite la contraseña"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
          </div>

          {error && <p className="reset-error">{error}</p>}

          <button type="submit" className="btn-login" disabled={loading}>
            {loading ? 'Guardando...' : 'Guardar nueva contraseña →'}
          </button>
        </form>
      </div>
    </div>
  );
};
