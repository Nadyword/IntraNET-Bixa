import React, { useState } from 'react';
import './OnboardingPage.css';

interface OnboardingStep {
  id: number;
  title: string;
  description: string;
  completed: boolean;
  icon: string;
  estimatedTime: string;
  tips?: string[];
}

export const OnboardingPage: React.FC = () => {
  const [steps, setSteps] = useState<OnboardingStep[]>([
    {
      id: 1,
      title: 'Completar Perfil',
      description: 'Actualiza tu información personal, foto de perfil y datos de contacto',
      completed: true,
      icon: '👤',
      estimatedTime: '10 min',
      tips: ['Usa una foto profesional', 'Verifica tu número de teléfono'],
    },
    {
      id: 2,
      title: 'Revisar Políticas Corporativas',
      description: 'Lee el manual de políticas, código de conducta y procedimientos',
      completed: true,
      icon: '📖',
      estimatedTime: '30 min',
      tips: ['Presta atención a las políticas de confidencialidad', 'Conserva una copia'],
    },
    {
      id: 3,
      title: 'Configurar Herramientas',
      description: 'Instala y configura las aplicaciones necesarias para tu rol',
      completed: false,
      icon: '⚙️',
      estimatedTime: '20 min',
      tips: ['Descarga Slack, Teams y VPN', 'Solicita credenciales al IT'],
    },
    {
      id: 4,
      title: 'Conocer al Equipo',
      description: 'Participa en la sesión de bienvenida y conoce a tus compañeros',
      completed: false,
      icon: '🤝',
      estimatedTime: '1 hora',
      tips: ['Prepara preguntas sobre el equipo', 'Llega 5 min antes'],
    },
    {
      id: 5,
      title: 'Primera Reunión con Líder',
      description: 'Planifica tus primeras semanas y aclara dudas sobre tu rol',
      completed: false,
      icon: '💼',
      estimatedTime: '45 min',
      tips: ['Prepara preguntas sobre expectativas', 'Lleva libreta'],
    },
    {
      id: 6,
      title: 'Tour por las Oficinas',
      description: 'Conoce las instalaciones, áreas comunes y políticas de uso',
      completed: false,
      icon: '🏢',
      estimatedTime: '30 min',
      tips: ['Pregunta sobre estacionamiento', 'Conoce dónde queda cada área'],
    },
    {
      id: 7,
      title: 'Completar Documentación Legal',
      description: 'Firma contrato, documentos fiscales y formularios de beneficios',
      completed: false,
      icon: '📝',
      estimatedTime: '45 min',
      tips: ['Lee todo cuidadosamente', 'Guarda copias'],
    },
    {
      id: 8,
      title: 'Evaluación de Capacitación',
      description: 'Completa la evaluación de capacitación inicial',
      completed: false,
      icon: '✅',
      estimatedTime: '20 min',
      tips: ['Revisa el material antes', 'Sin presión, puedes reintentar'],
    },
  ]);

  const toggleStep = (id: number) => {
    setSteps((prev) =>
      prev.map((step) =>
        step.id === id ? { ...step, completed: !step.completed } : step
      )
    );
  };

  const completedCount = steps.filter((s) => s.completed).length;
  const progressPercent = Math.round((completedCount / steps.length) * 100);

  return (
    <div className="onboarding-page">
      {/* Hero Section */}
      <div className="onboarding-hero">
        <h1>¡Bienvenido a BIXA!</h1>
        <p>Te ayudaremos a integrarte rápidamente a nuestro equipo</p>
      </div>

      {/* Progress Section */}
      <div className="progress-section">
        <div className="progress-header">
          <div>
            <h2>Tu Progreso de Incorporación</h2>
            <p>{completedCount} de {steps.length} pasos completados</p>
          </div>
          <div className="progress-number">{progressPercent}%</div>
        </div>

        <div className="progress-bar-container">
          <div className="progress-bar">
            <div className="progress-fill" style={{ width: `${progressPercent}%` }}></div>
          </div>
          <p className="progress-text">
            {progressPercent === 100
              ? '¡Felicidades! Has completado tu onboarding'
              : `Completa ${steps.length - completedCount} paso(s) más`}
          </p>
        </div>
      </div>

      {/* Checklist Section */}
      <div className="checklist-section">
        <div className="checklist-grid">
          {steps.map((step) => (
            <div
              key={step.id}
              className={`checklist-card ${step.completed ? 'completed' : ''}`}
            >
              <div className="card-header">
                <div className="step-number">{step.id}</div>
                <div className="step-icon">{step.icon}</div>
                <button
                  className="checkbox"
                  onClick={() => toggleStep(step.id)}
                  aria-label={`Toggle step ${step.id}`}
                >
                  {step.completed ? '✓' : ''}
                </button>
              </div>

              <div className="card-body">
                <h3>{step.title}</h3>
                <p className="description">{step.description}</p>

                {step.tips && step.tips.length > 0 && !step.completed && (
                  <div className="tips-section">
                    <p className="tips-label">💡 Tips:</p>
                    <ul className="tips-list">
                      {step.tips.map((tip, idx) => (
                        <li key={idx}>{tip}</li>
                      ))}
                    </ul>
                  </div>
                )}

                <div className="card-footer">
                  <span className="estimated-time">⏱️ {step.estimatedTime}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Support Section */}
      <div className="support-section">
        <div className="support-box">
          <h3>¿Necesitas Ayuda?</h3>
          <p>
            Si tienes dudas sobre el proceso de incorporación, contacta a nuestro equipo de RRHH
          </p>
          <a href="mailto:onboarding@bixa.com" className="support-link">
            📧 onboarding@bixa.com
          </a>
        </div>
        <div className="support-box">
          <h3>Recursos Útiles</h3>
          <p>Accede a documentos, guías y videos de capacitación</p>
          <a href="#" className="support-link">
            📚 Ver recursos
          </a>
        </div>
      </div>
    </div>
  );
};
