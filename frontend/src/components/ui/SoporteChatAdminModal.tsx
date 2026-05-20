import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useAuthStore } from '../../store/authStore';
import { soporteService } from '../../services/soporteService';
import type { SolicitudChatDTO } from '../../services/soporteService';
import './SoporteChatAdminModal.css';

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

const formatTime = (date: Date) =>
  date.toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit' });

interface Props {
  chat: SolicitudChatDTO;
  onClose: () => void;
}

export const SoporteChatAdminModal: React.FC<Props> = ({ chat, onClose }) => {
  const user = useAuthStore((s) => s.user);
  const [messages, setMessages] = useState<{ id: number; content: string; isFromEmployee: boolean; time: Date }[]>([]);
  const [loading, setLoading] = useState(true);
  const [inputValue, setInputValue] = useState('');
  const [isSending, setIsSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const employeeCiNorm = normalizeCi(chat.userCi);

  const loadHistory = useCallback(async () => {
    setLoading(true);
    try {
      const { data: res } = await soporteService.getHistoriChat(chat.userCi);
      if (res.success && Array.isArray(res.data)) {
        setMessages(
          res.data.map((msg) => ({
            id: msg.id,
            content: msg.message ?? '',
            isFromEmployee: normalizeCi(msg.userCi) === employeeCiNorm,
            time: new Date(msg.createdAt),
          })),
        );
      }
    } catch {}
    finally { setLoading(false); }
  }, [chat.userCi, employeeCiNorm]);

  useEffect(() => {
    loadHistory();
    soporteService.setMessageStatus(chat.userCi).catch(() => {});
  }, [loadHistory, chat.userCi]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async () => {
    const text = inputValue.trim();
    if (!text || !user?.ci || isSending) return;

    setIsSending(true);
    setInputValue('');

    try {
      const { data: res } = await soporteService.sendAnswer({
        userCi: chat.userCi,
        message: text,
        respondidoPorCi: user.ci,
      });

      if (res.success) {
        await loadHistory();
      } else {
        setInputValue(text);
      }
    } catch {
      setInputValue(text);
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend(); }
  };

  const fullName = [chat.firstName, chat.lastName].filter(Boolean).join(' ') || chat.userCi;

  return (
    <div className="scam-overlay" onClick={onClose}>
      <div className="scam-container" onClick={(e) => e.stopPropagation()}>

        <div className="scam-header">
          <div className="support-avatar">💬</div>
          <div className="scam-info">
            <span className="scam-employee-name">{fullName}</span>
            <span className="scam-employee-ci">CI: {chat.userCi}</span>
          </div>
          <button className="scam-close-btn" onClick={onClose} aria-label="Cerrar">✕</button>
        </div>

        <div className="scam-messages-area messages-area">
          {loading ? (
            <div className="chat-loading">Cargando historial...</div>
          ) : messages.length === 0 ? (
            <div className="msg-row from-agent">
              <div className="msg-bubble">
                <p className="msg-text">Sin mensajes aún.</p>
              </div>
            </div>
          ) : (
            messages.map((msg) => (
              <div key={msg.id} className={`msg-row ${msg.isFromEmployee ? 'from-agent' : 'from-user'}`}>
                <div className="msg-bubble">
                  {msg.isFromEmployee && (
                    <span className="msg-agent-label">{fullName}</span>
                  )}
                  <p className="msg-text">{msg.content}</p>
                  <div className="msg-footer">
                    <span className="msg-time">{formatTime(msg.time)}</span>
                  </div>
                </div>
              </div>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        <div className="support-chat-input-area">
          <input
            type="text"
            className="support-msg-input"
            placeholder="Escribe una respuesta..."
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            disabled={isSending}
          />
          <button
            className="support-send-btn"
            onClick={handleSend}
            disabled={!inputValue.trim() || isSending}
            aria-label="Enviar respuesta"
          >
            <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
              <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
            </svg>
          </button>
        </div>

      </div>
    </div>
  );
};
