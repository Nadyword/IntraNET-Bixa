import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { useUserProfileStore } from '../store/userProfileStore';
import { userService } from '../services/userService';
import authService from '../services/authService';
import './FirstLoginPage.css';

export const FirstLoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useAuthStore();
  const { setProfile, setError: setProfileError } = useUserProfileStore();

  const token = searchParams.get('token') ?? '';
  const ci = searchParams.get('ci') ?? '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Si no llegan los parámetros necesarios, redirigir al login
  if (!token || !ci) {
    navigate('/login');
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Las contraseñas no coinciden');
      return;
    }
    if (newPassword.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres');
      return;
    }

    setLoading(true);
    try {
      const { data: response } = await authService.firstLogin(ci, newPassword, token);

      if (!response.success) {
        setError(response.message || 'No se pudo establecer la contraseña');
        return;
      }

      login(response.data.token, response.data.refreshToken);

      userService.getProfile(ci)
        .then(({ data: res }) => { if (res.success) setProfile(res.data); })
        .catch(() => setProfileError('No se pudo cargar el perfil del empleado'));

      navigate('/mydata');
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Error al establecer la contraseña';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="first-login-screen">
      <div className="first-login-container">
        <div className="first-login-brand">
          <div className="brand-logo">
            Comunik2
            <span>PORTAL CORPORATIVO</span>
          </div>
          <div className="brand-center-media">
            <img src="/Logo-login.webp" alt="BIXA" />
          </div>
          <div className="brand-tagline">
            <strong>Bienvenido a Comunik2</strong>
            Crea tu contraseña para acceder por primera vez al portal corporativo.
          </div>
        </div>

        <div className="first-login-form-side">
          <h2>Crear contraseña</h2>
          <p>Es tu primer acceso. Establece una contraseña segura para continuar.</p>

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Nueva contraseña</label>
              <div className="password-input-wrapper">
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Mínimo 8 caracteres"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  required
                />
                <button
                  type="button"
                  className="password-toggle-btn"
                  onClick={() => setShowPassword((p) => !p)}
                >
                  {showPassword ? 'Ocultar' : 'Mostrar'}
                </button>
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Confirmar contraseña</label>
              <input
                type={showPassword ? 'text' : 'password'}
                placeholder="Repite tu contraseña"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
            </div>

            {error && <div className="first-login-error">{error}</div>}

            <button type="submit" className="btn-first-login" disabled={loading}>
              {loading ? 'Guardando...' : 'Establecer contraseña →'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
