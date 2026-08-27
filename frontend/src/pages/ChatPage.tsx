import React, { useState, useRef, useEffect } from 'react';
import { useAuthStore } from '../store/authStore';
import { soporteService, ordenarFAQs, type FAQsDTO } from '../services/soporteService';
import './ChatPage.css';

function normalizeCi(ci: string): string {
  if (!ci?.trim()) return '';
  const trimmed = ci.trim();
  const hasE = trimmed.toUpperCase().endsWith('-E');
  const digits = trimmed.replace(/\D/g, '');
  if (!digits) return '';
  const d = digits.length > 9 ? digits.slice(-9) : digits;
  const parts: string[] = [];
  const rem = d.length % 3;
  if (rem > 0) parts.push(d.slice(0, rem));
  for (let i = rem; i < d.length; i += 3) parts.push(d.slice(i, i + 3));
  return parts.join('.') + (hasE ? '-E' : '');
}

export type MessageStatus = 'sending' | 'sent' | 'delivered' | 'read';

export interface ChatMessage {
  id: number;
  content: string;
  isFromUser: boolean;
  status: MessageStatus;
  timestamp: Date;
  agentName?: string;
}

const MessageTicks: React.FC<{ status: MessageStatus }> = ({ status }) => {
  const strokeProps = {
    stroke: 'currentColor',
    strokeWidth: '1.7',
    strokeLinecap: 'round' as const,
    strokeLinejoin: 'round' as const,
    fill: 'none',
  };

  if (status === 'sending') {
    return (
      <span className="msg-ticks status-sending">
        <svg width="12" height="9" viewBox="0 0 12 9">
          <path d="M1.5 4.5 L4.5 7.5 L10.5 1.5" {...strokeProps} />
        </svg>
      </span>
    );
  }
  if (status === 'sent') {
    return (
      <span className="msg-ticks status-sent">
        <svg width="12" height="9" viewBox="0 0 12 9">
          <path d="M1.5 4.5 L4.5 7.5 L10.5 1.5" {...strokeProps} />
        </svg>
      </span>
    );
  }
  if (status === 'delivered') {
    return (
      <span className="msg-ticks status-delivered">
        <svg width="17" height="9" viewBox="0 0 17 9">
          <path d="M1.5 4.5 L4.5 7.5 L10.5 1.5" {...strokeProps} />
          <path d="M6.5 4.5 L9.5 7.5 L15.5 1.5" {...strokeProps} />
        </svg>
      </span>
    );
  }
  if (status === 'read') {
    return (
      <span className="msg-ticks status-read">
        <svg width="17" height="9" viewBox="0 0 17 9">
          <path d="M1.5 4.5 L4.5 7.5 L10.5 1.5" {...strokeProps} />
          <path d="M6.5 4.5 L9.5 7.5 L15.5 1.5" {...strokeProps} />
        </svg>
      </span>
    );
  }
  return null;
};

const formatTime = (date: Date) =>
  date.toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit' });

const formatDateLabel = (date: Date): string => {
  const today = new Date();
  const msgDate = new Date(date);
  today.setHours(0, 0, 0, 0);
  msgDate.setHours(0, 0, 0, 0);
  const diffDays = Math.round((today.getTime() - msgDate.getTime()) / 86400000);
  if (diffDays === 0) return 'Hoy';
  if (diffDays === 1) return 'Ayer';
  return date.toLocaleDateString('es-VE', { day: 'numeric', month: 'long', year: 'numeric' });
};

const shouldShowDate = (messages: ChatMessage[], index: number): boolean => {
  if (index === 0) return true;
  return messages[index - 1].timestamp.toDateString() !== messages[index].timestamp.toDateString();
};

export const ChatPage: React.FC = () => {
  const user = useAuthStore((state) => state.user);

  // Estado FAQs
  const [faqs, setFaqs] = useState<FAQsDTO[]>([]);
  const [faqsLoading, setFaqsLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  // Estado chat popup
  const [showChat, setShowChat] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const nextIdRef = useRef(1);

  useEffect(() => {
    soporteService.getFAQs()
      .then(({ data: res }) => {
        if (res.success) setFaqs(ordenarFAQs(res.data ?? []));
      })
      .catch(() => {})
      .finally(() => setFaqsLoading(false));
  }, []);

  useEffect(() => {
    if (!showChat || !user?.ci) return;
    const userCiNorm = normalizeCi(user.ci);
    setIsLoading(true);
    soporteService.getHistoriChat(user.ci)
      .then(({ data: res }) => {
        if (res.success && Array.isArray(res.data)) {
          const loaded: ChatMessage[] = res.data.map(msg => {
            const esMensajeDelUsuario = normalizeCi(msg.userCi) === userCiNorm;
            return {
              id: msg.id,
              content: msg.message ?? '',
              isFromUser: esMensajeDelUsuario,
              status: msg.isRead ? 'read' : 'delivered',
              timestamp: new Date(msg.createdAt),
              agentName: esMensajeDelUsuario ? undefined : 'Soporte BIXA',
            };
          });
          const maxId = loaded.reduce((acc, m) => Math.max(acc, m.id), 0);
          nextIdRef.current = maxId + 1;
          setMessages(loaded);
        }
      })
      .catch(() => {})
      .finally(() => setIsLoading(false));
  }, [showChat, user?.ci]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async () => {
    const text = inputValue.trim();
    if (!text || !user?.ci || isSending) return;
    setIsSending(true);
    setInputValue('');
    const msgId = nextIdRef.current++;
    setMessages(prev => [...prev, {
      id: msgId,
      content: text,
      isFromUser: true,
      status: 'sending',
      timestamp: new Date(),
    }]);
    try {
      const { data: res } = await soporteService.sendMessage({ userCi: user.ci, message: text });
      if (res.success) {
        setMessages(prev => prev.map(m => m.id === msgId ? { ...m, status: 'delivered' } : m));
      } else {
        setMessages(prev => prev.filter(m => m.id !== msgId));
        setInputValue(text);
      }
    } catch {
      setMessages(prev => prev.filter(m => m.id !== msgId));
      setInputValue(text);
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const filteredFaqs = faqs.filter(faq =>
    faq.question.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="support-page">

      {/* ── Encabezado ── */}
      <div className="support-page-header">
        <div className="support-page-title">
          <h1>Centro de Ayuda</h1>
          <p>Encuentra respuestas a las preguntas más frecuentes</p>
        </div>
        <button className="support-open-chat-btn" onClick={() => setShowChat(true)}>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"/>
          </svg>
          Hablar con soporte
        </button>
      </div>

      {/* ── Buscador ── */}
      <div className="support-search-wrap">
        <svg className="support-search-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <input
          type="text"
          className="support-search-input"
          placeholder="Buscar pregunta..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
      </div>

      {/* ── Grid de FAQs ── */}
      {faqsLoading && <p className="support-faq-loading">Cargando preguntas frecuentes...</p>}

      {!faqsLoading && filteredFaqs.length === 0 && (
        <p className="support-faq-empty">
          {searchQuery.trim() ? 'No se encontraron preguntas para esa búsqueda.' : 'No hay preguntas frecuentes registradas.'}
        </p>
      )}

      {!faqsLoading && filteredFaqs.length > 0 && (
        <div className="support-faq-grid">
          {filteredFaqs.map((faq) => (
            <div key={faq.id} className="support-faq-card">
              <p className="support-faq-question">{faq.question}</p>
              <p className="support-faq-response">{faq.response}</p>
            </div>
          ))}
        </div>
      )}

      {/* ── Popup del chat ── */}
      {showChat && (
        <div className="support-chat-overlay" onClick={(e) => { if (e.target === e.currentTarget) setShowChat(false); }}>
          <div className="support-chat-container">

            <div className="support-chat-header">
              <div className="support-avatar">💬</div>
              <div className="support-info">
                <span className="support-name-description">Todos los mensajes podrán ser leídos y respondidos por cualquier miembro del equipo de Soporte.</span>
                <span className="support-name">Soporte BIXA</span>
                <span className="support-status">
                  <span className="online-dot" />En línea
                </span>
              </div>
              <button className="support-chat-close" onClick={() => setShowChat(false)} aria-label="Cerrar chat">✕</button>
            </div>

            <div className="messages-area">
              {isLoading ? (
                <div className="chat-loading">Cargando historial...</div>
              ) : messages.length === 0 ? (
                <div className="msg-row from-agent">
                  <div className="msg-bubble">
                    <span className="msg-agent-label">Soporte BIXA</span>
                    <p className="msg-text">¡Hola! Bienvenido al canal de Soporte BIXA. Este es un canal de mensajería con el equipo de Soporte BIXA.</p>
                    <div className="msg-footer">
                      <span className="msg-time">{formatTime(new Date())}</span>
                    </div>
                  </div>
                </div>
              ) : (
                messages.map((msg, idx) => (
                  <React.Fragment key={msg.id}>
                    {shouldShowDate(messages, idx) && (
                      <div className="date-separator">
                        <span>{formatDateLabel(msg.timestamp)}</span>
                      </div>
                    )}
                    <div className={`msg-row ${msg.isFromUser ? 'from-user' : 'from-agent'}`}>
                      <div className="msg-bubble">
                        {!msg.isFromUser && <span className="msg-agent-label">{msg.agentName}</span>}
                        <p className="msg-text">{msg.content}</p>
                        <div className="msg-footer">
                          <span className="msg-time">{formatTime(msg.timestamp)}</span>
                          {msg.isFromUser && <MessageTicks status={msg.status} />}
                        </div>
                      </div>
                    </div>
                  </React.Fragment>
                ))
              )}
              <div ref={messagesEndRef} />
            </div>

            <div className="support-chat-input-area">
              <input
                type="text"
                className="support-msg-input"
                placeholder="Escribe un mensaje..."
                value={inputValue}
                onChange={e => setInputValue(e.target.value)}
                onKeyDown={handleKeyDown}
              />
              <button
                className="support-send-btn"
                onClick={handleSend}
                disabled={!inputValue.trim() || isSending}
                aria-label="Enviar mensaje"
              >
                <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
                  <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
                </svg>
              </button>
            </div>

          </div>
        </div>
      )}

    </div>
  );
};
