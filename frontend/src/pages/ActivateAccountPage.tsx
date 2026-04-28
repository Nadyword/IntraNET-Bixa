import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import type { ValidateTokenResponse } from '../services/authService';
import authService from '../services/authService';
import './ActivateAccountPage.css';

interface FormData {
  password: string;
  confirmPassword: string;
  telefono: string;
  fechaNacimiento: string;
  cedula: string;
  cargo: string;
}

interface FormErrors {
  [key: string]: string;
}

export const ActivateAccountPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { login } = useAuthStore();

  const [loading, setLoading] = useState(true);
  const [validatingToken, setValidatingToken] = useState(true);
  const [tokenData, setTokenData] = useState<ValidateTokenResponse | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<FormErrors>({});
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  const [formData, setFormData] = useState<FormData>({
    password: '',
    confirmPassword: '',
    telefono: '',
    fechaNacimiento: '',
    cedula: '',
    cargo: '',
  });

  useEffect(() => {
    const validateTokenAsync = async () => {
      const token = searchParams.get('token');
      if (!token) {
        setErrorMessage('Token no proporcionado. Por favor, usa el link del Usuario.');
        setValidatingToken(false);
        setLoading(false);
        return;
      }

      try {
        const result = await authService.validateToken(token);
        if (result.data.valid) {
          setTokenData(result.data);
          setFormData((prev) => ({
            ...prev,
            cedula: result.data.cedula || '',
            cargo: result.data.cargo || '',
          }));
        } else {
          setErrorMessage(
            result.data.message || 'Token inválido o expirado. Por favor, solicita un nuevo enlace.'
          );
        }
      } catch (error) {
        setErrorMessage('Error al validar el token.');
      } finally {
        setValidatingToken(false);
        setLoading(false);
      }
    };

    validateTokenAsync();
  }, [searchParams]);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.password) newErrors.password = 'La contraseña es requerida';
    if (formData.password.length < 8)
      newErrors.password = 'La contraseña debe tener al menos 8 caracteres';
    if (!formData.confirmPassword)
      newErrors.confirmPassword = 'Debe confirmar la contraseña';
    if (formData.password !== formData.confirmPassword)
      newErrors.confirmPassword = 'Las contraseñas no coinciden';
    if (!formData.telefono) newErrors.telefono = 'El teléfono es requerido';
    if (!formData.fechaNacimiento)
      newErrors.fechaNacimiento = 'La fecha de nacimiento es requerida';
    if (!formData.cedula) newErrors.cedula = 'La cédula es requerida';
    if (!formData.cargo) newErrors.cargo = 'El cargo es requerido';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    // Limpiar error del campo cuando el usuario empieza a escribir
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    const token = searchParams.get('token');
    if (!token) {
      setErrorMessage('Token no válido');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    setSuccessMessage('');

    try {
      const response = await authService.activateAccount({
        token,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
        telefono: formData.telefono,
        fechaNacimiento: new Date(formData.fechaNacimiento),
        cedula: formData.cedula,
        cargo: formData.cargo,
      });

      setSuccessMessage('¡Bienvenido! Iniciando sesión...');
      login(response.user, response.token);

      // Redirigir a mydata después de 1 segundo
      setTimeout(() => {
        navigate('/mydata');
      }, 1000);
    } catch (error: any) {
      const message =
        error.response?.data?.message ||
        error.response?.data?.errors?.[0] ||
        'Error al activar la cuenta';
      setErrorMessage(message);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="activate-page">
        <div className="activate-container">
          <div className="spinner">Validando...</div>
        </div>
      </div>
    );
  }

  if (!validatingToken && !tokenData?.valid) {
    return (
      <div className="activate-page">
        <div className="activate-container">
          <div className="error-section">
            <h2>❌ Token Inválido</h2>
            <p>{errorMessage}</p>
            <a href="/login" className="btn btn-primary">
              Volver al Login
            </a>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="activate-page">
      <div className="activate-container">
        <div className="activate-header">
          <h1>Completa tu Registro</h1>
          <p className="subtitle">¡Bienvenido a BIXA! Establece tu contraseña para activar tu cuenta.</p>
        </div>

        {errorMessage && <div className="alert alert-error">{errorMessage}</div>}
        {successMessage && <div className="alert alert-success">{successMessage}</div>}

        <form onSubmit={handleSubmit} className="activate-form">
          {/* Información del empleado */}
          <div className="form-section">
            <h3>Información Personal</h3>

            <div className="form-group">
              <label htmlFor="nombre">Nombre</label>
              <input
                type="text"
                id="nombre"
                value={tokenData?.nombre || ''}
                disabled
                className="form-input disabled"
              />
            </div>

            <div className="form-group">
              <label htmlFor="cedula">
                Cédula de Identidad
                <span className="required">*</span>
              </label>
              <input
                type="text"
                id="cedula"
                name="cedula"
                value={formData.cedula}
                onChange={handleInputChange}
                className={`form-input ${errors.cedula ? 'error' : ''}`}
              />
              {errors.cedula && <span className="error-text">{errors.cedula}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="cargo">
                Cargo
                <span className="required">*</span>
              </label>
              <input
                type="text"
                id="cargo"
                name="cargo"
                value={formData.cargo}
                onChange={handleInputChange}
                disabled
                className="form-input disabled"
              />
              {errors.cargo && <span className="error-text">{errors.cargo}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="telefono">
                Teléfono
                <span className="required">*</span>
              </label>
              <input
                type="tel"
                id="telefono"
                name="telefono"
                placeholder="Ej: +1 234 567 8900"
                value={formData.telefono}
                onChange={handleInputChange}
                className={`form-input ${errors.telefono ? 'error' : ''}`}
              />
              {errors.telefono && <span className="error-text">{errors.telefono}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="fechaNacimiento">
                Fecha de Nacimiento
                <span className="required">*</span>
              </label>
              <input
                type="date"
                id="fechaNacimiento"
                name="fechaNacimiento"
                value={formData.fechaNacimiento}
                onChange={handleInputChange}
                className={`form-input ${errors.fechaNacimiento ? 'error' : ''}`}
              />
              {errors.fechaNacimiento && (
                <span className="error-text">{errors.fechaNacimiento}</span>
              )}
            </div>
          </div>

          {/* Contraseña */}
          <div className="form-section">
            <h3>Seguridad de Cuenta</h3>

            <div className="form-group">
              <label htmlFor="password">
                Contraseña
                <span className="required">*</span>
              </label>
              <input
                type="password"
                id="password"
                name="password"
                placeholder="Mínimo 8 caracteres"
                value={formData.password}
                onChange={handleInputChange}
                className={`form-input ${errors.password ? 'error' : ''}`}
              />
              {errors.password && <span className="error-text">{errors.password}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">
                Confirmar Contraseña
                <span className="required">*</span>
              </label>
              <input
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                placeholder="Repite tu contraseña"
                value={formData.confirmPassword}
                onChange={handleInputChange}
                className={`form-input ${errors.confirmPassword ? 'error' : ''}`}
              />
              {errors.confirmPassword && (
                <span className="error-text">{errors.confirmPassword}</span>
              )}
            </div>
          </div>

          <button
            type="submit"
            disabled={submitting}
            className="btn btn-primary btn-submit"
          >
            {submitting ? 'Activando...' : 'Activar Cuenta'}
          </button>
        </form>

        <div className="activate-footer">
          <p>
            ¿Tienes problemas? Contacta a{' '}
            <a href="mailto:onboarding@bixa.com">onboarding@bixa.com</a>
          </p>
        </div>
      </div>
    </div>
  );
};
