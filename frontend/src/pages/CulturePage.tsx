import React, { useState } from 'react';
import './CulturePage.css';

export const CulturePage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('cultura');

  const values = [
    { icon: '🎯', title: 'Excelencia', desc: 'Buscamos la calidad en cada detalle' },
    { icon: '🤝', title: 'Colaboración', desc: 'Juntos alcanzamos más' },
    { icon: '🚀', title: 'Innovación', desc: 'Transformamos ideas en realidad' },
    { icon: '💪', title: 'Integridad', desc: 'Actuamos con ética y transparencia' },
  ];

  const benefits = [
    { icon: '🏥', title: 'Cobertura Médica', desc: 'Plan integral para ti y tu familia' },
    { icon: '✈️', title: 'Flexibilidad Horaria', desc: 'Equilibrio entre trabajo y vida personal' },
    { icon: '📚', title: 'Capacitación Continua', desc: 'Programas de desarrollo profesional' },
    { icon: '🎉', title: 'Eventos Corporativos', desc: 'Actividades de integración y esparcimiento' },
    { icon: '💰', title: 'Bonificación Anual', desc: 'Reconocimiento al desempeño' },
    { icon: '🏠', title: 'Préstamo Vivienda', desc: 'Apoyo para tu proyecto inmobiliario' },
  ];

  const orgChart = [
    { role: 'CEO', name: 'Carlos Mendoza', dept: 'Dirección Ejecutiva' },
    { role: 'COO', name: 'María González', dept: 'Operaciones' },
    { role: 'CTO', name: 'Juan Pérez', dept: 'Tecnología' },
    { role: 'CHRO', name: 'Laura Rodríguez', dept: 'Recursos Humanos' },
  ];

  return (
    <div className="culture-page">
      {/* Hero Section */}
      <div className="culture-hero">
        <h1>Nuestra Cultura</h1>
        <p>Somos una organización comprometida con la excelencia, la innovación y el bienestar de nuestro equipo</p>
      </div>

      {/* Tabs */}
      <div className="culture-tabs">
        <button
          className={`tab-btn ${activeTab === 'cultura' ? 'active' : ''}`}
          onClick={() => setActiveTab('cultura')}
        >
          Misión, Visión & Valores
        </button>
        <button
          className={`tab-btn ${activeTab === 'beneficios' ? 'active' : ''}`}
          onClick={() => setActiveTab('beneficios')}
        >
          Beneficios
        </button>
        <button
          className={`tab-btn ${activeTab === 'liderazgo' ? 'active' : ''}`}
          onClick={() => setActiveTab('liderazgo')}
        >
          Liderazgo
        </button>
      </div>

      {/* Content */}
      <div className="culture-content">
        {activeTab === 'cultura' && (
          <>
            {/* MVV Cards */}
            <div className="mvv-section">
              <div className="mvv-card">
                <div className="mvv-icon">🎯</div>
                <h3>Misión</h3>
                <p>Proporcionar soluciones innovadoras que empoderen a las organizaciones para alcanzar su máximo potencial</p>
              </div>
              <div className="mvv-card">
                <div className="mvv-icon">🌟</div>
                <h3>Visión</h3>
                <p>Ser la empresa líder en tecnología y recursos humanos reconocida por transformar el futuro del trabajo</p>
              </div>
            </div>

            {/* Values Grid */}
            <div className="values-section">
              <h2>Nuestros Valores</h2>
              <div className="values-grid">
                {values.map((value, idx) => (
                  <div key={idx} className="value-card">
                    <div className="value-icon">{value.icon}</div>
                    <h4>{value.title}</h4>
                    <p>{value.desc}</p>
                  </div>
                ))}
              </div>
            </div>
          </>
        )}

        {activeTab === 'beneficios' && (
          <div className="benefits-section">
            <h2>Nuestros Beneficios</h2>
            <div className="benefits-grid">
              {benefits.map((benefit, idx) => (
                <div key={idx} className="benefit-card">
                  <div className="benefit-icon">{benefit.icon}</div>
                  <h4>{benefit.title}</h4>
                  <p>{benefit.desc}</p>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'liderazgo' && (
          <div className="leadership-section">
            <h2>Nuestro Equipo Directivo</h2>
            <div className="org-chart">
              {orgChart.map((person, idx) => (
                <div key={idx} className="org-card">
                  <div className="org-avatar">{person.name[0]}</div>
                  <h4>{person.name}</h4>
                  <p className="org-role">{person.role}</p>
                  <p className="org-dept">{person.dept}</p>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
