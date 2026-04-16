import React, { useState } from 'react';
import './LeaderPage.css';

interface ApprovalRequest {
  id: number;
  employeeName: string;
  type: string;
  description: string;
  dates?: string;
  amount?: number;
  submittedDate: string;
}

interface TeamMember {
  id: number;
  name: string;
  role: string;
  department: string;
  status: 'activo' | 'vacaciones' | 'permiso';
  performanceScore: number;
}

export const LeaderPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('aprobaciones');
  const [actionedRequests, setActionedRequests] = useState<number[]>([]);

  const approvalRequests: ApprovalRequest[] = [
    {
      id: 1,
      employeeName: 'Juan Rodríguez',
      type: 'Vacaciones',
      description: 'Solicitud de 10 días de vacaciones',
      dates: '20-30 Abril',
      submittedDate: '15 Marzo 2026',
    },
    {
      id: 2,
      employeeName: 'María Gonzalez',
      type: 'Permiso Especial',
      description: 'Permiso de 2 horas - Cita médica',
      dates: '22 Marzo',
      submittedDate: '20 Marzo 2026',
    },
    {
      id: 3,
      employeeName: 'Carlos Mendez',
      type: 'Préstamo Utilidades',
      description: 'Solicitud de adelanto de utilidades',
      amount: 2000,
      submittedDate: '18 Marzo 2026',
    },
  ];

  const teamMembers: TeamMember[] = [
    {
      id: 1,
      name: 'Juan Rodríguez',
      role: 'Desarrollador Senior',
      department: 'Tecnología',
      status: 'activo',
      performanceScore: 92,
    },
    {
      id: 2,
      name: 'María Gonzalez',
      role: 'Especialista en QA',
      department: 'Calidad',
      status: 'vacaciones',
      performanceScore: 88,
    },
    {
      id: 3,
      name: 'Carlos Mendez',
      role: 'Analista de Negocios',
      department: 'Operaciones',
      status: 'activo',
      performanceScore: 85,
    },
    {
      id: 4,
      name: 'Laura Pérez',
      role: 'Diseñadora UX',
      department: 'Producto',
      status: 'activo',
      performanceScore: 90,
    },
  ];

  const stats = [
    { label: 'Miembros del equipo', value: 12, icon: '👥' },
    { label: 'Solicitudes pendientes', value: 3, icon: '📋' },
    { label: 'Desempeño promedio', value: '8.8/10', icon: '⭐' },
    { label: 'Ausencias este mes', value: 2, icon: '📅' },
  ];

  const handleApprove = (id: number) => {
    setActionedRequests([...actionedRequests, id]);
  };

  const handleReject = (id: number) => {
    setActionedRequests([...actionedRequests, id]);
  };

  const pendingRequests = approvalRequests.filter((r) => !actionedRequests.includes(r.id));

  return (
    <div className="leader-page">
      {/* Header */}
      <div className="leader-header">
        <div>
          <h1>Portal del Líder</h1>
          <p>Gestiona tu equipo, aprueba solicitudes y monitorea el desempeño</p>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="stats-grid">
        {stats.map((stat, idx) => (
          <div key={idx} className="stat-box">
            <div className="stat-icon">{stat.icon}</div>
            <h3>{stat.value}</h3>
            <p>{stat.label}</p>
          </div>
        ))}
      </div>

      {/* Tabs */}
      <div className="leader-tabs">
        <button
          className={`tab-btn ${activeTab === 'aprobaciones' ? 'active' : ''}`}
          onClick={() => setActiveTab('aprobaciones')}
        >
          Aprobaciones Pendientes
        </button>
        <button
          className={`tab-btn ${activeTab === 'equipo' ? 'active' : ''}`}
          onClick={() => setActiveTab('equipo')}
        >
          Mi Equipo
        </button>
        <button
          className={`tab-btn ${activeTab === 'alertas' ? 'active' : ''}`}
          onClick={() => setActiveTab('alertas')}
        >
          Alertas
        </button>
      </div>

      {/* Tab Content */}
      <div className="tab-content">
        {activeTab === 'aprobaciones' && (
          <div className="approvals-section">
            {pendingRequests.length > 0 ? (
              <div className="approvals-list">
                {pendingRequests.map((req) => (
                  <div key={req.id} className="approval-card">
                    <div className="approval-left">
                      <div className="employee-info">
                        <h4>{req.employeeName}</h4>
                        <p className="request-type">{req.type}</p>
                        <p className="request-desc">{req.description}</p>
                        {req.dates && <p className="request-detail">Fechas: {req.dates}</p>}
                        {req.amount && <p className="request-detail">Monto: ${req.amount}</p>}
                      </div>
                      <div className="submitted-date">Solicitado: {req.submittedDate}</div>
                    </div>
                    <div className="approval-actions">
                      <button
                        className="btn-approve"
                        onClick={() => handleApprove(req.id)}
                      >
                        ✓ Aprobar
                      </button>
                      <button
                        className="btn-reject"
                        onClick={() => handleReject(req.id)}
                      >
                        ✕ Rechazar
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="empty-state">
                <p>✓ No hay solicitudes pendientes</p>
              </div>
            )}
          </div>
        )}

        {activeTab === 'equipo' && (
          <div className="team-section">
            <table className="team-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Rol</th>
                  <th>Departamento</th>
                  <th>Estado</th>
                  <th>Desempeño</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                {teamMembers.map((member) => (
                  <tr key={member.id}>
                    <td>
                      <strong>{member.name}</strong>
                    </td>
                    <td>{member.role}</td>
                    <td>{member.department}</td>
                    <td>
                      <span className={`status-badge ${member.status}`}>
                        {member.status === 'activo' && '🟢 Activo'}
                        {member.status === 'vacaciones' && '🏖️ Vacaciones'}
                        {member.status === 'permiso' && '📝 Permiso'}
                      </span>
                    </td>
                    <td>
                      <div className="performance-bar">
                        <div
                          className="performance-fill"
                          style={{ width: `${member.performanceScore}%` }}
                        ></div>
                        <span className="performance-text">{member.performanceScore}%</span>
                      </div>
                    </td>
                    <td>
                      <a href="#" className="action-link">
                        Ver perfil →
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {activeTab === 'alertas' && (
          <div className="alerts-section">
            <div className="alert-item alert-warning">
              <div className="alert-icon">⚠️</div>
              <div className="alert-content">
                <h4>Ausencia No Comunicada</h4>
                <p>Carlos Mendez no registró entrada hoy. Verifica su estado.</p>
                <span className="alert-time">Hace 2 horas</span>
              </div>
            </div>
            <div className="alert-item alert-info">
              <div className="alert-icon">ℹ️</div>
              <div className="alert-content">
                <h4>Aniversario</h4>
                <p>Juan Rodríguez cumple 5 años en la empresa el 25 de marzo.</p>
                <span className="alert-time">Hace 1 día</span>
              </div>
            </div>
            <div className="alert-item alert-success">
              <div className="alert-icon">✓</div>
              <div className="alert-content">
                <h4>Evaluación Completada</h4>
                <p>La evaluación de desempeño de María Gonzalez fue completada.</p>
                <span className="alert-time">Hace 3 días</span>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
