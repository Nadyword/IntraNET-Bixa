import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import './SolicitudesPage.css';

interface RequestFormData {
  type: string;
  description: string;
  startDate?: string;
  endDate?: string;
  amount?: number;
}

export const SolicitudesPage: React.FC = () => {
  const [showModal, setShowModal] = useState(false);
  const [selectedType, setSelectedType] = useState<string | null>(null);
  const { register, handleSubmit, reset } = useForm<RequestFormData>();

  const requestTypes = [
    { id: 'vacaciones', label: 'Vacaciones', icon: '✈️', desc: 'Solicitar días de descanso' },
    { id: 'permiso', label: 'Permiso Especial', icon: '📝', desc: 'Solicitar permiso puntual' },
    { id: 'prestamo', label: 'Préstamo Utilidades', icon: '💳', desc: 'Solicitar adelanto de utilidades' },
    { id: 'certificado', label: 'Certificado Laboral', icon: '📄', desc: 'Obtener certificado' },
    { id: 'cambio', label: 'Cambio de Departamento', icon: '🔄', desc: 'Solicitar transferencia' },
    { id: 'otro', label: 'Otra Solicitud', icon: '❓', desc: 'Solicitud personalizada' },
  ];

  const existingRequests = [
    { id: 1, type: 'Vacaciones', date: '15-26 Abril', status: 'Aprobada' },
    { id: 2, type: 'Permiso Especial', date: '22 Marzo (2h)', status: 'Pendiente' },
    { id: 3, type: 'Préstamo Utilidades', amount: 1000, status: 'Aprobado' },
  ];

  const handleTypeSelect = (typeId: string) => {
    setSelectedType(typeId);
    reset();
  };

  const onSubmit = async (data: RequestFormData) => {
    console.log({ ...data, type: selectedType });
    alert('Solicitud creada exitosamente');
    setShowModal(false);
    setSelectedType(null);
  };

  return (
    <div className="solicitudes-page">
      {/* Header */}
      <div className="solicitudes-header">
        <h1>Mis Solicitudes</h1>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          + Nueva Solicitud
        </button>
      </div>

      {/* Existing Requests */}
      <div className="requests-container">
        <h2>Solicitudes Activas</h2>
        <div className="requests-grid">
          {existingRequests.map((req) => (
            <div key={req.id} className="request-card">
              <div className="request-header">
                <h3>{req.type}</h3>
                <span className={`status-badge ${req.status.toLowerCase()}`}>{req.status}</span>
              </div>
              <div className="request-body">
                <p>
                  {'amount' in req ? `Monto: $${req.amount}` : `Fechas: ${req.date}`}
                </p>
              </div>
              <div className="request-footer">
                <a href="#" className="link">
                  Ver detalles →
                </a>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Nueva Solicitud</h2>
              <button className="close-btn" onClick={() => setShowModal(false)}>
                ✕
              </button>
            </div>

            {!selectedType ? (
              <div className="modal-body">
                <p className="modal-subtitle">Selecciona el tipo de solicitud</p>
                <div className="request-types-grid">
                  {requestTypes.map((type) => (
                    <button
                      key={type.id}
                      className="type-card"
                      onClick={() => handleTypeSelect(type.id)}
                    >
                      <div className="type-icon">{type.icon}</div>
                      <h4>{type.label}</h4>
                      <p>{type.desc}</p>
                    </button>
                  ))}
                </div>
              </div>
            ) : (
              <div className="modal-body">
                <button className="back-btn" onClick={() => setSelectedType(null)}>
                  ← Atrás
                </button>

                <form onSubmit={handleSubmit(onSubmit)} className="request-form">
                  <div className="form-group">
                    <label>Tipo de Solicitud</label>
                    <input
                      type="hidden"
                      {...register('type')}
                      value={selectedType}
                    />
                    <input
                      type="text"
                      disabled
                      value={requestTypes.find((t) => t.id === selectedType)?.label || ''}
                      className="form-input disabled"
                    />
                  </div>

                  {/* Conditional fields based on type */}
                  {(selectedType === 'vacaciones' || selectedType === 'permiso') && (
                    <>
                      <div className="form-group">
                        <label>Fecha de Inicio</label>
                        <input
                          type="date"
                          {...register('startDate', { required: true })}
                          className="form-input"
                        />
                      </div>
                      <div className="form-group">
                        <label>Fecha de Fin</label>
                        <input
                          type="date"
                          {...register('endDate', { required: true })}
                          className="form-input"
                        />
                      </div>
                    </>
                  )}

                  {selectedType === 'prestamo' && (
                    <div className="form-group">
                      <label>Monto Solicitado ($)</label>
                      <input
                        type="number"
                        {...register('amount', { required: true, min: 100 })}
                        placeholder="100"
                        className="form-input"
                      />
                    </div>
                  )}

                  <div className="form-group">
                    <label>Descripción / Motivo</label>
                    <textarea
                      {...register('description', { required: true })}
                      placeholder="Explica brevemente el motivo de tu solicitud..."
                      rows={4}
                      className="form-input"
                    ></textarea>
                  </div>

                  <div className="form-actions">
                    <button
                      type="button"
                      className="btn-secondary"
                      onClick={() => setShowModal(false)}
                    >
                      Cancelar
                    </button>
                    <button type="submit" className="btn-primary">
                      Enviar Solicitud
                    </button>
                  </div>
                </form>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
