import { useUIStore } from '../store/uiStore';
import './ConsultasPage.css';

export const ConsultasPage: React.FC = () => {
  const { setActiveSection } = useUIStore();


  return (
    <div className="consultas-page">
      {/* Header */}
      <div className="consultas-header">
        <div className="consultas-header-text">
          <h1>Consultas</h1>
          <p>Consulta en tiempo real el estado de tus beneficios y conceptos laborales</p>
        </div>
      </div>

      <div className="stats-grid-3">
        <div className="stat-card">
          <div className="stat-icon red">🏖️</div>
          <div className="stat-info"><h3>15</h3><p>Días de vacaciones disponibles</p><div className="stat-sub">Período: 2024-2025</div></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon black">🗓️</div>
          <div className="stat-info"><h3>3</h3><p>Días de permiso especial</p><div className="stat-sub">Disponibles este año</div></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon green">💰</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>Bs.12.450</h3><p>Prestaciones acumuladas</p><div className="stat-sub">Corte: 31/05/2025</div></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">🏥</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>Bs.850</h3><p>Descuento HC mensual</p><div className="stat-sub">Titular + Cónyuge + 1 Hijo</div></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">📄</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>Bs.850</h3><p>ARC — Retención de ISLR:</p><div className="stat-sub">Titular + Cónyuge + 1 Hijo</div></div>
        </div>
        <div className="stat-card">
          <div className="stat-icon red">📈</div>
          <div className="stat-info"><h3 style={{ fontSize: '22px' }}>Bs.850</h3><p>Acumulado utilidades</p><div className="stat-sub">Titular + Cónyuge + 1 Hijo</div></div>
        </div>
      </div>

      <div className="support-section">
        <div className="support-box" style={{ marginTop: '30px' }}>
          <h3>¿No crees que está bien la información mostrata aquí?</h3>
          <p>
            Si tienes algún reclamo o duda sobre la información mostrada en esta pantalla, por favor crea un ticket detallando tu inquietud. Un supervisor revisará el caso y enviará la respuesta al correo electrónico asociado a tu cuenta, el cual puedes consultar en el apartado 'Mis datos, Correo personal'.
          </p>
          <a
            href="#"
            className="support-link"
            onClick={(e) => {
              e.preventDefault();
              setActiveSection('soporte');
            }}
          >
            📚 Asistencia/Soporte
          </a>
        </div>
      </div>
    </div>
  );
};
