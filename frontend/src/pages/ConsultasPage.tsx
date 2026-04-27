import React, { useState } from 'react';
import './ConsultasPage.css';

export const ConsultasPage: React.FC = () => {
  const [expandedCard, setExpandedCard] = useState<string | null>(null);

  const vacationDays = {
    total: 15,
    used: 8,
    pending: 7,
    info: 'Tienes 15 días de vacaciones al año según tu contrato. Puedes solicitarlos en cualquier momento con 10 días de anticipación.',
  };

  const accumulatedBenefits = {
    amount: 2450.50,
    currency: 'USD',
    info: 'Monto acumulado de tus prestaciones y bonificaciones. Puedes consultar el desglose de cada concepto a continuación.',
  };

  const activeRequests = {
    count: 3,
    items: [
      { id: 1, type: 'Vacaciones', dates: '15-26 Abril', status: 'En aprobación' },
      { id: 2, type: 'Préstamo Utilidades', amount: 1000, status: 'Aprobado' },
      { id: 3, type: 'Permiso Especial', dates: '22 Marzo', status: 'Pendiente' },
    ],
    info: 'Tus solicitudes activas se muestran a continuación. Puedes seguir el estado en tiempo real.',
  };

  const benefitsBreakdown = [
    { label: 'Bonificación Anual', amount: 1200 },
    { label: 'Aguinaldo', amount: 800 },
    { label: 'Horas Extras', amount: 450.50 },
  ];

  return (
    <div className="consultas-page">
      {/* Header */}
      <div className="consultas-header">
        <h1>Consultas de tramites y solicitudes</h1>
        <p>Aquí encontrás toda la información necesaria para poder realizar cualquier tipo de trámite permitido en la empresa</p>
      </div>

      {/* Stats Cards */}
      <div className="stats-section">
        {/* Vacation Days Card */}
        <div
          className={`stat-card ${expandedCard === 'vacations' ? 'expanded' : ''}`}
          onClick={() => setExpandedCard(expandedCard === 'vacations' ? null : 'vacations')}
        >
          <div className="stat-header">
            <div className="stat-icon">📅</div>
            <div className="stat-info">
              <h3>Días de Vacaciones</h3>
              <p className="stat-value">
                <span className="used">{vacationDays.used}</span>
                <span className="separator">/</span>
                <span className="total">{vacationDays.total}</span>
              </p>
            </div>
          </div>
          {expandedCard === 'vacations' && (
            <div className="stat-details">
              <p>{vacationDays.info}</p>
              <div className="progress-bar">
                <div
                  className="progress-fill"
                  style={{ width: `${(vacationDays.used / vacationDays.total) * 100}%` }}
                ></div>
              </div>
              <div className="detail-row">
                <span>Disponibles:</span>
                <strong>{vacationDays.pending} días</strong>
              </div>
            </div>
          )}
        </div>

        {/* Accumulated Benefits Card */}
        <div
          className={`stat-card ${expandedCard === 'benefits' ? 'expanded' : ''}`}
          onClick={() => setExpandedCard(expandedCard === 'benefits' ? null : 'benefits')}
        >
          <div className="stat-header">
            <div className="stat-icon">💰</div>
            <div className="stat-info">
              <h3>Prestaciones Acumuladas</h3>
              <p className="stat-value">
                ${accumulatedBenefits.amount.toFixed(2)} {accumulatedBenefits.currency}
              </p>
            </div>
          </div>
          {expandedCard === 'benefits' && (
            <div className="stat-details">
              <p>{accumulatedBenefits.info}</p>
              <div className="benefits-breakdown">
                {benefitsBreakdown.map((benefit, idx) => (
                  <div key={idx} className="breakdown-row">
                    <span>{benefit.label}</span>
                    <strong>${benefit.amount.toFixed(2)}</strong>
                  </div>
                ))}
              </div>
              <button className="btn-secondary">Solicitar Pago</button>
            </div>
          )}
        </div>

        {/* Active Requests Card */}
        <div
          className={`stat-card ${expandedCard === 'requests' ? 'expanded' : ''}`}
          onClick={() => setExpandedCard(expandedCard === 'requests' ? null : 'requests')}
        >
          <div className="stat-header">
            <div className="stat-icon">📋</div>
            <div className="stat-info">
              <h3>Solicitudes Activas</h3>
              <p className="stat-value">{activeRequests.count}</p>
            </div>
          </div>
          {expandedCard === 'requests' && (
            <div className="stat-details">
              <p>{activeRequests.info}</p>
              <div className="requests-list">
                {activeRequests.items.map((req) => (
                  <div key={req.id} className="request-item">
                    <div className="request-left">
                      <h5>{req.type}</h5>
                      <p className="request-dates">
                        {'dates' in req ? req.dates : `$${req.amount}`}
                      </p>
                    </div>
                    <div
                      className={`request-badge ${req.status.toLowerCase().replace(/\s+/g, '-')}`}
                    >
                      {req.status}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Info Section */}
    </div>
  );
};
