import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { ForgotPasswordModal } from '../components/ui/ForgotPasswordModal';
import authService from '../services/authService';
import './LoginPage.css';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuthStore();
  const [ci, setCi] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showForgot, setShowForgot] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const { data: response } = await authService.login(ci, password);

      if (!response.success) {
        setError(response.message || 'Credenciales inválidas');
        return;
      }

      const { token, refreshToken } = response.data;

      // Primer login: el backend devuelve refreshToken = "FIRSTLOGIN"
      if (refreshToken === 'FIRSTLOGIN') {
        navigate(`/first-login?token=${encodeURIComponent(token)}&ci=${encodeURIComponent(ci)}`);
        return;
      }

      login(token, refreshToken);
      navigate('/mydata');
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Credenciales inválidas';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-container">
        <div className="login-brand">
          <div className="brand-logo">
            Comunik2
            <span>PORTAL CORPORATIVO</span>
          </div>

          <div className="brand-center-media">
            <img src="Logo-login.webp" alt="BIXA" />
          </div>

          <div className="brand-tagline">
            <strong>Tu espacio de trabajo digital</strong>
            Accede a toda la información de tu gestión, solicitudes y herramientas en un solo lugar, fácil, ágil y seguro.
          </div>
        </div>

        <div className="login-form-side">
          <h2>Bienvenido de vuelta</h2>
          <p>Ingresa tus credenciales corporativas para continuar</p>

          <form onSubmit={handleLogin}>
            <div className="form-group">
              <label className="form-label">Cedula</label>
              <input
                type="text"
                placeholder="Cedula"
                value={ci}
                onChange={(e) => setCi(e.target.value)}
                required
              />
            </div>

            <div className="form-group">
              <label className="form-label">Contraseña</label>
              <div className="password-input-wrapper">
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Clave"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <button
                  type="button"
                  className="password-toggle-btn"
                  onClick={() => setShowPassword((prev) => !prev)}
                >
                  {showPassword ? 'Ocultar' : 'Mostrar'}
                </button>
              </div>
            </div>

            {error && <div className="login-error">{error}</div>}

            <button type="submit" className="btn-login" disabled={loading}>
              {loading ? 'Cargando...' : 'Ingresar al Portal →'}
            </button>

            <div className="forgot-link" onClick={() => setShowForgot(true)}>
              ¿Olvidaste tu contraseña?
            </div>
          </form>
        </div>
      </div>

      {showForgot && <ForgotPasswordModal onClose={() => setShowForgot(false)} />}
    </div>
  );
};
