import React, { useState, useEffect } from 'react';
import api from '../lib/api';
import type { ApiResponse } from '../services/authService';
import type { UserProfile } from '../store/userProfileStore';
import { soporteService, type SolicitudChatDTO } from '../services/soporteService';
import { SoporteChatAdminModal } from '../components/ui/SoporteChatAdminModal';
import { CreateUserModal } from '../components/ui/CreateUserModal';
import { DeleteUserModal } from '../components/ui/DeleteUserModal';
import { EditUserModal } from '../components/ui/EditUserModal';
import { ResendWelcomeEmailModal } from '../components/ui/ResendWelcomeEmailModal';
import { EmployeeProfileModal } from '../components/ui/EmployeeProfileModal';
import './LeaderPage.css';

const PAGE_SIZE = 50;

interface ApprovalRequest {
  id: number;
  employeeName: string;
  type: string;
  description: string;
  dates?: string;
  amount?: number;
  submittedDate: string;
}

export const LeaderPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('aprobaciones');
  const [actionedRequests, setActionedRequests] = useState<number[]>([]);

  // Modales de gestión
  const [showCreateUserModal, setShowCreateUserModal] = useState(false);
  const [showDeleteUserModal, setShowDeleteUserModal] = useState(false);
  const [showEditUserModal, setShowEditUserModal] = useState(false);
  const [showResendWelcomeEmailModal, setShowResendWelcomeEmailModal] = useState(false);

  // Estado del equipo
  const [teamUsers, setTeamUsers] = useState<UserProfile[]>([]);
  const [teamLoading, setTeamLoading] = useState(false);
  const [teamError, setTeamError] = useState('');

  // Estado de chats de soporte
  const [chats, setChats] = useState<SolicitudChatDTO[]>([]);
  const [chatsLoading, setChatsLoading] = useState(false);
  const [chatsError, setChatsError] = useState('');
  const [selectedChat, setSelectedChat] = useState<SolicitudChatDTO | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [selectedCi, setSelectedCi] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    if (activeTab !== 'equipo') return;
    let cancelled = false;
    const load = async () => {
      setTeamLoading(true);
      setTeamError('');
      try {
        const res = await api.get<ApiResponse<UserProfile[]>>(`/users/${currentPage}/${PAGE_SIZE}`);
        if (!cancelled) {
          const data = res.data.data ?? [];
          setTeamUsers(data);
          setHasNextPage(data.length === PAGE_SIZE);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          const axiosError = err as { response?: { data?: { message?: string } } };
          setTeamError(axiosError?.response?.data?.message ?? 'Error al cargar los usuarios.');
        }
      } finally {
        if (!cancelled) setTeamLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [activeTab, currentPage]);

  useEffect(() => {
    if (activeTab !== 'chats') return;
    let cancelled = false;
    const load = async () => {
      setChatsLoading(true);
      setChatsError('');
      try {
        const res = await soporteService.getChatAbiertos();
        if (!cancelled) {
          const data = res.data.data ?? [];
          const sorted = [...data].sort((a, b) => a.respondido - b.respondido);
          setChats(sorted);
        }
      } catch {
        if (!cancelled) setChatsError('Sin chats de soporte activos.');
      } finally {
        if (!cancelled) setChatsLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [activeTab]);

  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    if (tab === 'equipo') setCurrentPage(1);
  };

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

  const handleApprove = (id: number) => setActionedRequests([...actionedRequests, id]);
  const handleReject  = (id: number) => setActionedRequests([...actionedRequests, id]);
  const pendingRequests = approvalRequests.filter((r) => !actionedRequests.includes(r.id));

  const filteredUsers = teamUsers.filter((u) => {
    const q = searchQuery.trim().toLowerCase();
    if (!q) return true;
    const fullName = `${u.nombres ?? ''} ${u.apellidos ?? ''}`.toLowerCase();
    const ci = (u.ci ?? '').toLowerCase();
    return fullName.includes(q) || ci.includes(q);
  });

  return (
    <>
    <div className="leader-page">
      {/* Header */}
      <div className="leader-header">
        <div>
          <h1>Portal del Líder</h1>
          <p>Gestiona tu equipo, aprueba solicitudes y monitorea el desempeño</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="leader-tabs">
        <button
          className={`tab-btn ${activeTab === 'aprobaciones' ? 'active' : ''}`}
          onClick={() => handleTabChange('aprobaciones')}
        >
          Procesos pendientes
        </button>
        <button
          className={`tab-btn ${activeTab === 'equipo' ? 'active' : ''}`}
          onClick={() => handleTabChange('equipo')}
        >
          Mi equipo
        </button>
        <button
          className={`tab-btn ${activeTab === 'chats' ? 'active' : ''}`}
          onClick={() => handleTabChange('chats')}
        >
          Chats de soporte
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
                      <button className="btn-approve" onClick={() => handleApprove(req.id)}>
                        ✓ Aprobar
                      </button>
                      <button className="btn-reject" onClick={() => handleReject(req.id)}>
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
          <div>
            <div style={{ display: 'flex', justifyContent: 'center', margin: '15px', flexWrap: 'wrap', gap: '8px' }}>
              <button className="btn-primary" onClick={() => setShowCreateUserModal(true)}>
                Crear usuario
              </button>
              <button className="btn-primary" onClick={() => setShowDeleteUserModal(true)}>
                Borrar usuario
              </button>
              <button className="btn-primary" onClick={() => setShowEditUserModal(true)}>
                Editar usuario
              </button>
              <button className="btn-primary" onClick={() => setShowResendWelcomeEmailModal(true)}>
                Reenviar correo de bienvenida
              </button>
            </div>

            <div className="team-search-bar">
              <input
                type="text"
                className="team-search-input"
                placeholder="Buscar por nombre o cédula..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>

            <div className="team-section">
              <table className="team-table">
                <thead>
                  <tr>
                    <th>Nombre y Apellido</th>
                    <th>CI</th>
                    <th>Departamento</th>
                    <th>Cargo</th>
                    <th>Acción</th>
                  </tr>
                </thead>
                <tbody>
                  {teamLoading && (
                    <tr>
                      <td colSpan={5} className="team-empty">Cargando usuarios...</td>
                    </tr>
                  )}
                  {!teamLoading && teamError && (
                    <tr>
                      <td colSpan={5} className="team-error">{teamError}</td>
                    </tr>
                  )}
                  {!teamLoading && !teamError && filteredUsers.length === 0 && (
                    <tr>
                      <td colSpan={5} className="team-empty">
                        {searchQuery.trim() ? 'No hay resultados para la búsqueda.' : 'No se encontraron usuarios.'}
                      </td>
                    </tr>
                  )}
                  {!teamLoading && !teamError && filteredUsers.map((user, i) => (
                    <tr key={user.ci ?? i}>
                      <td>
                        <strong>
                          {[user.nombres, user.apellidos].filter(Boolean).join(' ') || '—'}
                        </strong>
                      </td>
                      <td>{user.ci ?? '—'}</td>
                      <td>{user.desDepart ?? '—'}</td>
                      <td>{user.desCargo ?? '—'}</td>
                      <td>
                        <button
                          className="action-link-btn"
                          onClick={() => setSelectedCi(user.ci!)}
                        >
                          Ver perfil →
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              <div className="team-pagination">
                <button
                  disabled={currentPage === 1}
                  onClick={() => setCurrentPage((p) => p - 1)}
                >
                  ← Anterior
                </button>
                <span>Página {currentPage}</span>
                <button
                  disabled={!hasNextPage}
                  onClick={() => setCurrentPage((p) => p + 1)}
                >
                  Siguiente →
                </button>
              </div>
            </div>
          </div>
        )}
        {activeTab === 'chats' && (
          <div className="chats-section">
            {chatsLoading && <p className="team-empty">Cargando chats...</p>}
            {!chatsLoading && chatsError && <p className="team-error">{chatsError}</p>}
            {!chatsLoading && !chatsError && chats.length === 0 && (
              <div className="empty-state"><p>No hay chats de soporte activos.</p></div>
            )}
            {!chatsLoading && !chatsError && chats.length > 0 && (
              <table className="team-table">
                <thead>
                  <tr>
                    <th>Nombre y Apellido</th>
                    <th>CI</th>
                    <th>Estado</th>
                    <th>Acción</th>
                  </tr>
                </thead>
                <tbody>
                  {chats.map((chat) => (
                    <tr key={chat.userCi}>
                      <td><strong>{[chat.firstName, chat.lastName].filter(Boolean).join(' ') || '—'}</strong></td>
                      <td>{chat.userCi}</td>
                      <td>
                        <span className={`chat-status-badge ${chat.respondido === 0 ? 'badge-pending' : 'badge-answered'}`}>
                          {chat.respondido === 0 ? 'Sin responder' : 'Respondido'}
                        </span>
                      </td>
                      <td>
                        <button
                          className="action-link-btn"
                          onClick={() => setSelectedChat(chat)}
                        >
                          {chat.respondido === 0 ? 'Responder →' : 'Ver →'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
    </div>

      {showCreateUserModal && (
        <CreateUserModal onClose={() => setShowCreateUserModal(false)} />
      )}
      {showDeleteUserModal && (
        <DeleteUserModal onClose={() => setShowDeleteUserModal(false)} />
      )}
      {showEditUserModal && (
        <EditUserModal onClose={() => setShowEditUserModal(false)} />
      )}
      {showResendWelcomeEmailModal && (
        <ResendWelcomeEmailModal onClose={() => setShowResendWelcomeEmailModal(false)} />
      )}
      {selectedCi && (
        <EmployeeProfileModal ci={selectedCi} onClose={() => setSelectedCi(null)} />
      )}
      {selectedChat && (
        <SoporteChatAdminModal chat={selectedChat} onClose={() => setSelectedChat(null)} />
      )}
    </>
  );
};
