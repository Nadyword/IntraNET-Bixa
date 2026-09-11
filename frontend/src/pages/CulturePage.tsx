import React, { useState } from 'react';
import './CulturePage.css';

export const CulturePage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('cultura');

  const values = [
    { icon: '🏠', title: 'Sentido de Pertenencia', desc: 'Sentirse parte de Bixa, cuidando la marca y sus raíces como si fueran propias.' },
    { icon: '🤝', title: 'Orientación al Servicio', desc: 'La disposición constante de ayudar y superar las expectativas de clientes y compañeros con calidez.' },
    { icon: '⭐', title: 'Búsqueda de la Excelencia', desc: 'El esfuerzo diario por innovar y entregar productos de la más alta calidad, sin conformarse con lo ordinario.' },
    { icon: '💍', title: 'Compromiso', desc: 'La firme determinación de cumplir con nuestra misión y ser leales a los objetivos de la organización.' },
    { icon: '❤️', title: 'Empatía', desc: 'La capacidad de conectar con las necesidades del otro, entendiendo que detrás de cada producto hay personas.' },
    { icon: '✅', title: 'Responsabilidad', desc: 'Asumir con integridad cada tarea y decisión, garantizando la confianza que el mercado deposita en nosotros.' },
  ];

  const benefits = [
    {
      title: 'Utilidades',
      detail: '100 días. (Anticipo de 30 días en el mes de Marzo). Período para el pago: Noviembre - Octubre.',
    },
    {
      title: 'Vacaciones',
      detail: 'De Ley. (Disfrute Colectivo en el mes de Diciembre).',
    },
    {
      title: 'Juguete Navideño (hasta los 12 años)',
      detail: 'Contribución para la compra de un juguete para los hijos de los colaboradores.',
    },
    {
      title: 'Bonificación y Permiso por Matrimonio',
      detail: 'Pago = 45 días de sueldo. Permiso 7 días continuos (Incluyendo Sábado y Domingo).',
    },
    {
      title: 'Bonificación y Permiso por Muerte de Familiar',
      detail: 'Pago = 45 días de sueldo. Permiso 7 días continuos (Trabajador, Cónyuge, Padres e Hijos).',
    },
    {
      title: 'Bonificación y Permiso por Nacimiento de Hijos',
      detail: 'Pago = 45 días de sueldo. Permiso establecido en la LOTTT.',
    },
    {
      title: 'Cumpleaños del Trabajador',
      detail: 'Disfrute del día libre si coincide con día hábil. Contribución por cumpleaños.',
    },
    {
      title: 'Útiles Escolares',
      detail: 'Ayuda para la compra de útiles escolares.',
    },
    {
      title: 'Cesta Navideña',
      detail: 'Contribución para la adquisición de alimentos para la época decembrina.',
    },
    {
      title: 'Semana Santa',
      detail: 'Disfrute con pago remunerado de Lunes, Martes y Miércoles de Semana Santa y cancelación del beneficio de alimentación.',
    },
    {
      title: 'Tasa de Interés Sobre Prestaciones Sociales',
      detail: 'Dos puntos por encima de la tasa promedio entre la activa y la pasiva determinada por el BCV.',
    },
    {
      title: 'Permiso para la Obtención de Documentos',
      detail: 'Se concede cuatro (04) días de permiso al año remunerados a salario básico para la tramitación de cédula de identidad, libreta militar, certificado de salud, licencia de conducir, pasaporte, atender citaciones de autoridades judiciales, policiales o civiles, inscripción escolar de sus hijos, carta de soltería, concubinato y constancia de residencia. El trabajador presentará constancia de la gestión realizada. Adicionalmente se cancelará el beneficio de alimentación. Período (Junio - Mayo).',
    },
    {
      title: 'Sábados y Domingos en Feriados',
      detail: 'Cancelación de un día adicional si el feriado coincide con un día sábado o domingo. Se considerarán los días feriados establecidos en el Art. 184 de la LOTTT.',
    },
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
            <div className="benefits-table-wrapper">
              <table className="benefits-table">
                <thead>
                  <tr>
                    <th className="col-num">N°</th>
                    <th className="col-benefit">Beneficio</th>
                    <th className="col-detail">Detalle</th>
                  </tr>
                </thead>
                <tbody>
                  {benefits.map((benefit, idx) => (
                    <tr key={idx}>
                      <td className="col-num">{idx + 1}</td>
                      <td className="col-benefit">{benefit.title}</td>
                      <td className="col-detail">{benefit.detail}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
