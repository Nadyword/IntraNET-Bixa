import React from 'react';
import { createPortal } from 'react-dom';
import './ForgotPasswordModal.css';
import './CelebrationModal.css';

interface Props {
  nombre: string;
  esCumpleanos: boolean;
  esAniversario: boolean;
  aniosAniversario: number | null;
  onClose: () => void;
}

export const CelebrationModal: React.FC<Props> = ({
  nombre,
  esCumpleanos,
  esAniversario,
  aniosAniversario,
  onClose,
}) => {
  return createPortal(
    <div className="modal-overlay" onClick={onClose}>
      <div className="celebration-box" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>
        <div className="celebration-icon">🎉</div>

        {esCumpleanos && (
          <div className="celebration-message">
            <h2>¡Feliz cumpleaños, {nombre}!</h2>
            <p>Todo el equipo de Bixa te desea un día increíble.</p>
          </div>
        )}

        {esAniversario && (
          <div className="celebration-message">
            <h2>{esCumpleanos ? '¡Y feliz aniversario!' : `¡Feliz aniversario, ${nombre}!`}</h2>
            <p>
              Hoy cumples {aniosAniversario} {aniosAniversario === 1 ? 'año' : 'años'} en Bixa. ¡Gracias por tu dedicación!
            </p>
          </div>
        )}

        <button className="modal-btn-primary celebration-btn" onClick={onClose}>
          Cerrar
        </button>
      </div>
    </div>,
    document.body
  );
};
