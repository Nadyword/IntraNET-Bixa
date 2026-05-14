import React, { useState } from 'react';
import './CulturePage.css';

export const CulturePage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('cultura');

  const values = [
    { icon: '🏠', title: 'Sentido de Pertenencia', desc: 'Sentirse parte de la familia Bixa, cuidando la marca y sus raíces como si fueran propias.' },
    { icon: '🤝', title: 'Orientación al Servicio', desc: 'La disposición constante de ayudar y superar las expectativas de clientes y compañeros con calidez.' },
    { icon: '⭐', title: 'Búsqueda de la Excelencia', desc: 'El esfuerzo diario por innovar y entregar productos de la más alta calidad, sin conformarse con lo ordinario.' },
    { icon: '💍', title: 'Compromiso', desc: 'La firme determinación de cumplir con nuestra misión y ser leales a los objetivos de la organización.' },
    { icon: '❤️', title: 'Empatía', desc: 'La capacidad de conectar con las necesidades del otro, entendiendo que detrás de cada producto hay personas.' },
    { icon: '✅', title: 'Responsabilidad', desc: 'Asumir con integridad cada tarea y decisión, garantizando la confianza que el mercado deposita en nosotros.' },
  ];

  const benefits = [
    { icon: '🏥', title: 'Cobertura Médica', desc: 'Plan integral para ti y tu familia' },
    { icon: '✈️', title: 'Flexibilidad Horaria', desc: 'Equilibrio entre trabajo y vida personal' },
    { icon: '📚', title: 'Capacitación Continua', desc: 'Programas de desarrollo profesional' },
    { icon: '🎉', title: 'Eventos Corporativos', desc: 'Actividades de integración y esparcimiento' },
    { icon: '💰', title: 'Bonificación Anual', desc: 'Reconocimiento al desempeño' },
    { icon: '🏠', title: 'Préstamo Vivienda', desc: 'Apoyo para tu proyecto inmobiliario' },
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
                <p>Ser un aliado estratégico en la provisión de ingredientes para la industria alimentaria, destacándonos por nuestro compromiso con la innovación y la excelencia en calidad.</p>
              </div>
              <div className="mvv-card">
                <div className="mvv-icon">🌟</div>
                <h3>Visión</h3>
                <p>Ser reconocido como el proveedor líder en la fabricación de ingredientes para las empresas alimentarias en el mercado nacional, creando nuevos productos, conquistando mercados internacionales y adaptándonos a futuros cambios.</p>
              </div>
              <div className="mvv-card">
                <div className="mvv-icon">📜</div>
                <h3>Política</h3>
                <p>Productos Bixa se compromete a garantizar la seguridad alimentaria y la satisfacción al cliente, cumpliendo con las normativas vigentes, promoviendo la mejora continua en todos nuestros procesos y fomentando el bienestar de sus colaboradores.</p>
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
      </div>
    </div>
  );
};
