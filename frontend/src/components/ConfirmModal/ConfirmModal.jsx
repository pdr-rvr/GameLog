import React, { useEffect } from "react";
import { FaExclamationTriangle, FaTimes, FaTrash } from "react-icons/fa";
import "./ConfirmModal.css";

const ConfirmModal = ({
  isOpen,
  title = "Confirmar Ação",
  message = "Tem certeza que deseja prosseguir com esta ação? Esta operação não pode ser desfeita.",
  confirmText = "Excluir",
  cancelText = "Cancelar",
  confirmVariant = "danger",
  onConfirm,
  onCancel,
  loading = false,
}) => {
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === "Escape" && onCancel && !loading) {
        onCancel();
      }
    };

    if (isOpen) {
      window.addEventListener("keydown", handleKeyDown);
    }
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, onCancel, loading]);

  if (!isOpen) return null;

  return (
    <div 
      className="confirm-modal-backdrop" 
      onClick={(e) => {
        if (e.target === e.currentTarget && onCancel && !loading) onCancel();
      }}
    >
      <div className="confirm-modal-card" role="dialog" aria-modal="true">
        <div className="confirm-modal-header">
          <div className={`confirm-modal-icon-badge ${confirmVariant}`}>
            <FaExclamationTriangle />
          </div>
          <div className="confirm-modal-title-group">
            <h3>{title}</h3>
          </div>
          <button 
            type="button" 
            className="confirm-modal-close" 
            onClick={onCancel}
            disabled={loading}
            aria-label="Fechar modal"
          >
            <FaTimes />
          </button>
        </div>

        <div className="confirm-modal-body">
          <p>{message}</p>
        </div>

        <div className="confirm-modal-actions">
          <button
            type="button"
            className="btn-confirm-cancel"
            onClick={onCancel}
            disabled={loading}
          >
            {cancelText}
          </button>
          <button
            type="button"
            className={`btn-confirm-action ${confirmVariant}`}
            onClick={onConfirm}
            disabled={loading}
          >
            {confirmVariant === "danger" && <FaTrash className="btn-icon" />}
            {loading ? "Processando..." : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmModal;
