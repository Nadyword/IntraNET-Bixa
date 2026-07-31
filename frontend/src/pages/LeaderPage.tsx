import React, { useState, useEffect } from 'react';
import api from '../lib/api';
import type { ApiResponse } from '../services/authService';
import type { UserProfile } from '../store/userProfileStore';
import { useAuthStore } from '../store/authStore';
import { soporteService, type SolicitudChatDTO, type FAQsDTO } from '../services/soporteService';
import { SoporteChatAdminModal } from '../components/ui/SoporteChatAdminModal';
import { CreateUserModal } from '../components/ui/CreateUserModal';
import { DeleteUserModal } from '../components/ui/DeleteUserModal';
import { EditUserModal } from '../components/ui/EditUserModal';
import { ResendWelcomeEmailModal } from '../components/ui/ResendWelcomeEmailModal';
import { EmployeeProfileModal } from '../components/ui/EmployeeProfileModal';
import './LeaderPage.css';

const PAGE_SIZE = 50;


export const LeaderPage: React.FC = () => {
  const isAdmin = useAuthStore((state) => state.user?.rolId === '1');
  const [activeTab, setActiveTab] = useState(isAdmin ? 'equipo' : 'chats');

  // Modales de gestión
  const [showCreateUserModal, setShowCreateUserModal] = useState(false);
  const [showDeleteUserModal, setShowDeleteUserModal] = useState(false);
  const [showEditUserModal, setShowEditUserModal] = useState(false);
  const [showResendWelcomeEmailModal, setShowResendWelcomeEmailModal] = useState(false);

  // Estado del equipo
  const [teamUsers, setTeamUsers] = useState<UserProfile[]>([]);
  const [teamLoading, setTeamLoading] = useState(false);
  const [teamError, setTeamError] = useState('');

  // Estado de preguntas frecuentes
  const [faqs, setFaqs] = useState<FAQsDTO[]>([]);
  const [faqsLoading, setFaqsLoading] = useState(false);
  const [faqsError, setFaqsError] = useState('');
  const [faqSaving, setFaqSaving] = useState(false);
  const [faqExpandedId, setFaqExpandedId] = useState<number | null>(null);
  const [showFaqCreateForm, setShowFaqCreateForm] = useState(false);
  const [faqCreateForm, setFaqCreateForm] = useState({ question: '', response: '' });
  const [faqEditingId, setFaqEditingId] = useState<number | null>(null);
  const [faqEditForm, setFaqEditForm] = useState({ question: '', response: '' });
  const [faqDeleteConfirmId, setFaqDeleteConfirmId] = useState<number | null>(null);

  const handleFaqCreate = async () => {
    if (!faqCreateForm.question.trim() || !faqCreateForm.response.trim()) return;
    setFaqSaving(true);
    try {
      await soporteService.createFAQ({ id: 0, ...faqCreateForm });
      const res = await soporteService.getFAQs();
      setFaqs(res.data.data ?? []);
      setFaqCreateForm({ question: '', response: '' });
      setShowFaqCreateForm(false);
    } finally {
      setFaqSaving(false);
    }
  };

  const handleFaqEditStart = (faq: FAQsDTO) => {
    setFaqEditingId(faq.id);
    setFaqEditForm({ question: faq.question, response: faq.response });
    setFaqExpandedId(null);
  };

  const handleFaqEditSave = async () => {
    if (!faqEditForm.question.trim() || !faqEditForm.response.trim()) return;
    setFaqSaving(true);
    try {
      await soporteService.updateFAQ({ id: faqEditingId!, ...faqEditForm });
      const res = await soporteService.getFAQs();
      setFaqs(res.data.data ?? []);
      setFaqEditingId(null);
    } finally {
      setFaqSaving(false);
    }
  };

  const handleFaqDelete = async () => {
    setFaqSaving(true);
    try {
      await soporteService.deleteFAQ(faqDeleteConfirmId!);
      setFaqs(faqs.filter((f) => f.id !== faqDeleteConfirmId));
      setFaqDeleteConfirmId(null);
    } finally {
      setFaqSaving(false);
    }
  };

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
    if (activeTab !== 'faqs') return;
    let cancelled = false;
    const load = async () => {
      setFaqsLoading(true);
      setFaqsError('');
      try {
        const res = await soporteService.getFAQs();
        if (!cancelled) setFaqs(res.data.data ?? []);
      } catch {
        if (!cancelled) setFaqsError('Error al cargar las preguntas frecuentes.');
      } finally {
        if (!cancelled) setFaqsLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [activeTab]);

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
          <p>{isAdmin ? 'Gestiona tu equipo, aprueba solicitudes y monitorea el desempeño' : 'Responde los chats de soporte de tu equipo'}</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="leader-tabs">
        {isAdmin && (
          <button
            className={`tab-btn ${activeTab === 'equipo' ? 'active' : ''}`}
            onClick={() => handleTabChange('equipo')}
          >
            Usuarios
          </button>
        )}
        <button
          className={`tab-btn ${activeTab === 'chats' ? 'active' : ''}`}
          onClick={() => handleTabChange('chats')}
        >
          Chats de soporte
        </button>
        {isAdmin && (
          <button
            className={`tab-btn ${activeTab === 'faqs' ? 'active' : ''}`}
            onClick={() => handleTabChange('faqs')}
          >
            Preguntas frecuentes
          </button>
        )}
      </div>

      {/* Tab Content */}
      <div className="tab-content">
        {isAdmin && activeTab === 'equipo' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'center', margin: '15px', flexWrap: 'wrap', gap: '8px' }}>
              <button className="tramites-nueva-btn" onClick={() => setShowCreateUserModal(true)}>
                Crear usuario
              </button>
              <button className="tramites-nueva-btn" onClick={() => setShowDeleteUserModal(true)}>
                Borrar usuario
              </button>
              <button className="tramites-nueva-btn" onClick={() => setShowEditUserModal(true)}>
                Editar usuario
              </button>
              <button className="tramites-nueva-btn" onClick={() => setShowResendWelcomeEmailModal(true)}>
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

        {isAdmin && activeTab === 'faqs' && (
          <div className="faq-section">
            <div className="faq-header-row">
              <button
                className="tramites-nueva-btn"
                onClick={() => { setShowFaqCreateForm(true); setFaqEditingId(null); }}
                disabled={showFaqCreateForm || faqsLoading}
              >
                + Nueva pregunta frecuente
              </button>
            </div>

            {showFaqCreateForm && (
              <div className="faq-card faq-card-form">
                <p className="faq-form-title">Nueva pregunta frecuente</p>
                <div className="faq-form-field">
                  <label>Pregunta</label>
                  <input
                    type="text"
                    placeholder="¿Cuál es la pregunta?"
                    value={faqCreateForm.question}
                    onChange={(e) => setFaqCreateForm({ ...faqCreateForm, question: e.target.value })}
                  />
                </div>
                <div className="faq-form-field">
                  <label>Respuesta</label>
                  <textarea
                    placeholder="Escribe la respuesta..."
                    rows={4}
                    value={faqCreateForm.response}
                    onChange={(e) => setFaqCreateForm({ ...faqCreateForm, response: e.target.value })}
                  />
                </div>
                <div className="faq-form-actions">
                  <button className="faq-btn-save" onClick={handleFaqCreate} disabled={faqSaving}>
                    {faqSaving ? 'Guardando...' : 'Guardar'}
                  </button>
                  <button className="faq-btn-cancel" disabled={faqSaving} onClick={() => { setShowFaqCreateForm(false); setFaqCreateForm({ question: '', response: '' }); }}>Cancelar</button>
                </div>
              </div>
            )}

            <div className="faq-list">
              {faqsLoading && <p className="team-empty">Cargando preguntas frecuentes...</p>}
              {!faqsLoading && faqsError && <p className="team-error">{faqsError}</p>}
              {!faqsLoading && !faqsError && faqs.length === 0 && (
                <div className="empty-state"><p>No hay preguntas frecuentes registradas.</p></div>
              )}
              {!faqsLoading && !faqsError && faqs.map((faq) => (
                <div key={faq.id} className={`faq-card${faqEditingId === faq.id ? ' faq-card-form' : ''}`}>
                  {faqEditingId === faq.id ? (
                    <>
                      <p className="faq-form-title">Editar pregunta frecuente</p>
                      <div className="faq-form-field">
                        <label>Pregunta</label>
                        <input
                          type="text"
                          value={faqEditForm.question}
                          onChange={(e) => setFaqEditForm({ ...faqEditForm, question: e.target.value })}
                        />
                      </div>
                      <div className="faq-form-field">
                        <label>Respuesta</label>
                        <textarea
                          rows={4}
                          value={faqEditForm.response}
                          onChange={(e) => setFaqEditForm({ ...faqEditForm, response: e.target.value })}
                        />
                      </div>
                      <div className="faq-form-actions">
                        <button className="faq-btn-save" onClick={handleFaqEditSave} disabled={faqSaving}>
                          {faqSaving ? 'Guardando...' : 'Guardar'}
                        </button>
                        <button className="faq-btn-cancel" disabled={faqSaving} onClick={() => setFaqEditingId(null)}>Cancelar</button>
                      </div>
                    </>
                  ) : (
                    <>
                      <div
                        className="faq-card-header"
                        onClick={() => setFaqExpandedId(faqExpandedId === faq.id ? null : faq.id)}
                      >
                        <span className="faq-expand-icon">{faqExpandedId === faq.id ? '▼' : '►'}</span>
                        <span className="faq-question-text">{faq.question}</span>
                        <div className="faq-actions" onClick={(e) => e.stopPropagation()}>
                          <button className="faq-action-btn faq-btn-edit" onClick={() => handleFaqEditStart(faq)}>
                            Editar
                          </button>
                          <button className="faq-action-btn faq-btn-delete" onClick={() => setFaqDeleteConfirmId(faq.id)}>
                            Borrar
                          </button>
                        </div>
                      </div>
                      {faqExpandedId === faq.id && (
                        <div className="faq-answer">
                          <p>{faq.response}</p>
                        </div>
                      )}
                    </>
                  )}
                </div>
              ))}
            </div>

            {faqDeleteConfirmId !== null && (
              <div className="faq-delete-overlay">
                <div className="faq-delete-modal">
                  <p className="faq-delete-title">¿Eliminar pregunta?</p>
                  <p className="faq-delete-desc">
                    Esta acción no se puede deshacer. ¿Confirmas que deseas eliminar esta pregunta frecuente?
                  </p>
                  <div className="faq-delete-actions">
                    <button className="faq-btn-delete-confirm" onClick={handleFaqDelete} disabled={faqSaving}>
                      {faqSaving ? 'Eliminando...' : 'Sí, eliminar'}
                    </button>
                    <button className="faq-btn-cancel" disabled={faqSaving} onClick={() => setFaqDeleteConfirmId(null)}>Cancelar</button>
                  </div>
                </div>
              </div>
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
