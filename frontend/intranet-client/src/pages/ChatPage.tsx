import React, { useState, useRef, useEffect } from 'react';
import './ChatPage.css';

interface Message {
  id: number;
  sender: 'user' | 'bot';
  text: string;
  timestamp: Date;
}

export const ChatPage: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: 1,
      sender: 'bot',
      text: '¡Hola! Soy el asistente virtual de BIXA. ¿Cómo puedo ayudarte hoy?',
      timestamp: new Date(),
    },
  ]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const faqItems = [
    { id: 1, question: '¿Cómo solicitar vacaciones?', icon: '✈️' },
    { id: 2, question: '¿Cuál es el saldo de mis vacaciones?', icon: '📅' },
    { id: 3, question: '¿Cómo solicitar un préstamo?', icon: '💰' },
    { id: 4, question: '¿Cuáles son mis beneficios?', icon: '🎁' },
    { id: 5, question: '¿Cómo contactar a RRHH?', icon: '☎️' },
    { id: 6, question: '¿Dónde veo mis trámites?', icon: '📋' },
  ];

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSendMessage = (text: string = inputValue) => {
    if (!text.trim()) return;

    // Add user message
    const userMessage: Message = {
      id: messages.length + 1,
      sender: 'user',
      text: text.trim(),
      timestamp: new Date(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setInputValue('');
    setIsLoading(true);

    // Simulate bot response
    setTimeout(() => {
      const botResponses: { [key: string]: string } = {
        vacaciones:
          'Para solicitar vacaciones, ve a la sección "Solicitudes" y selecciona "Vacaciones". Necesitas indicar las fechas y una justificación. Tu líder debe aprobarlo.',
        saldo:
          'Tienes 15 días de vacaciones anuales. Ya has utilizado 8 días, por lo que te quedan 7 días disponibles.',
        préstamo:
          'Los préstamos de utilidades pueden solicitarse a través de "Solicitudes". El monto máximo es 3 veces tu sueldo mensual.',
        beneficios:
          'BIXA ofrece: cobertura médica, flexibilidad horaria, capacitación continua, bonificación anual y más. Ve a "Cultura & Beneficios" para detalles.',
        rrhh: 'Puedes contactar a RRHH en: teléfono (555) 123-4567 o email: rrhh@bixa.com. Horario: Lunes a Viernes, 8am-6pm.',
        trámites:
          'Tus trámites se encuentran en la sección "Mis Trámites" en el menú principal. Ahí puedes ver el estado de cada uno.',
      };

      let response = '¿Podrías ser más específico? Aquí hay algunas cosas en las que puedo ayudarte:';
      for (const [key, value] of Object.entries(botResponses)) {
        if (text.toLowerCase().includes(key)) {
          response = value;
          break;
        }
      }

      const botMessage: Message = {
        id: messages.length + 2,
        sender: 'bot',
        text: response,
        timestamp: new Date(),
      };

      setMessages((prev) => [...prev, botMessage]);
      setIsLoading(false);
    }, 800);
  };

  const handleFaqClick = (question: string) => {
    handleSendMessage(question);
  };

  return (
    <div className="chat-page">
      <div className="chat-container">
        {/* Chat Header */}
        <div className="chat-header">
          <div className="header-content">
            <h2>Asistente Virtual BIXA</h2>
            <p className="header-subtitle">Disponible 24/7 para ayudarte</p>
          </div>
          <div className="header-status">
            <span className="status-dot"></span>
            <span>En línea</span>
          </div>
        </div>

        {/* Messages Area */}
        <div className="messages-container">
          {messages.map((message) => (
            <div key={message.id} className={`message-group ${message.sender}`}>
              <div className="message">
                <p>{message.text}</p>
                <span className="timestamp">{message.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
              </div>
            </div>
          ))}
          {isLoading && (
            <div className="message-group bot">
              <div className="message">
                <div className="typing-indicator">
                  <span></span>
                  <span></span>
                  <span></span>
                </div>
              </div>
            </div>
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* FAQ Chips - Only show on first message */}
        {messages.length === 1 && !isLoading && (
          <div className="faq-section">
            <p className="faq-label">Preguntas frecuentes:</p>
            <div className="faq-chips">
              {faqItems.map((item) => (
                <button
                  key={item.id}
                  className="faq-chip"
                  onClick={() => handleFaqClick(item.question)}
                >
                  <span className="chip-icon">{item.icon}</span>
                  <span className="chip-text">{item.question}</span>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Input Area */}
        <div className="chat-input-section">
          <div className="input-wrapper">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
              placeholder="Escribe tu pregunta..."
              className="chat-input"
              disabled={isLoading}
            />
            <button
              className="send-btn"
              onClick={() => handleSendMessage()}
              disabled={!inputValue.trim() || isLoading}
            >
              ➤
            </button>
          </div>
          <p className="input-hint">Puedes preguntar sobre vacaciones, beneficios, solicitudes y más</p>
        </div>
      </div>
    </div>
  );
};
