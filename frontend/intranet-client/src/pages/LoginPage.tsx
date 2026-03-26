import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import api from '../lib/api';
import './LoginPage.css';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login } = useAuthStore();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const { data } = await api.post('/auth/login', { email, password });
      login(data.user, data.token);
      navigate('/home');
    } catch (err) {
      console.error(err);
      alert('Credenciales inválidas');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-container">
        <div className="login-brand">
          <div className="brand-logo">
            BIXA
            <span>PORTAL CORPORATIVO</span>
          </div>
          <div className="brand-tagline">
            <strong>Tu espacio de trabajo digital</strong>
            Accede a toda la información de tu gestión, solicitudes y herramientas en un solo lugar. Conectado. Ágil. Seguro.
          </div>
        </div>

        <div className="login-form-side">
          <h2>Bienvenido de vuelta</h2>
          <p>Ingresa tus credenciales corporativas para continuar</p>

          <form onSubmit={handleLogin}>
            <div className="form-group">
              <label className="form-label">Correo Corporativo</label>
              <input
                type="email"
                placeholder="Correo"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
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

            <button type="submit" className="btn-login" disabled={loading}>
              {loading ? 'Cargando...' : 'Ingresar al Portal →'}
            </button>
            <div className="forgot-link">¿Olvidaste tu contraseña? Solicitar restablecimiento</div>
          </form>
        </div>
      </div>
    </div>
  );
};
