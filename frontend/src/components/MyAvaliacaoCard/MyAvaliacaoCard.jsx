import React from "react";
import { Link } from "react-router-dom";
import { FaEdit, FaTrash, FaStar, FaGamepad } from "react-icons/fa";
import "./MyAvaliacaoCard.css";

const MyAvaliacaoCard = ({ avaliacao, onEdit, onDelete }) => {
  if (!avaliacao) return null;

  const id = avaliacao.avaliacaoId || avaliacao.id;
  const jogoId = avaliacao.jogoId || avaliacao.idJogo;
  const nomeJogo = avaliacao.nomeJogo || avaliacao.tituloJogo || "Jogo";
  const nota = Math.min(5, Math.max(1, Number(avaliacao.nota) || 5));

  let formattedDate = "";
  if (avaliacao.dataPublicacao) {
    try {
      const d = new Date(avaliacao.dataPublicacao);
      formattedDate = d.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "short",
        year: "numeric"
      });
    } catch {
      formattedDate = String(avaliacao.dataPublicacao).substring(0, 10);
    }
  }

  return (
    <article className="my-review-card">
      {/* Header: Game Title/Tag & Action Buttons */}
      <div className="my-review-header">
        <div className="my-review-game-block">
          {jogoId ? (
            <Link to={`/jogos/${jogoId}`} className="my-review-game-title" title={`Ver ${nomeJogo}`}>
              <FaGamepad className="my-review-game-icon" />
              <span>{nomeJogo}</span>
            </Link>
          ) : (
            <div className="my-review-game-title">
              <FaGamepad className="my-review-game-icon" />
              <span>{nomeJogo}</span>
            </div>
          )}
        </div>

        <div className="my-review-actions">
          {onEdit && (
            <button
              type="button"
              className="my-review-btn edit"
              onClick={() => onEdit(id || avaliacao)}
              title="Editar Avaliação"
              aria-label="Editar Avaliação"
            >
              <FaEdit />
            </button>
          )}
          {onDelete && (
            <button
              type="button"
              className="my-review-btn delete"
              onClick={() => onDelete(id)}
              title="Excluir Avaliação"
              aria-label="Excluir Avaliação"
            >
              <FaTrash />
            </button>
          )}
        </div>
      </div>

      {/* Meta: Stars Rating & Publication Date */}
      <div className="my-review-meta">
        <div className="my-review-rating-stars" aria-label={`Nota ${nota} de 5`}>
          {[1, 2, 3, 4, 5].map((star) => (
            <FaStar
              key={star}
              className={`my-review-star ${star <= nota ? "filled" : "empty"}`}
            />
          ))}
          <span className="my-review-rating-value">{nota}/5</span>
        </div>

        {formattedDate && (
          <span className="my-review-date">{formattedDate}</span>
        )}
      </div>

      {/* Body: Clamped Review Text with Quote Accent */}
      <div className="my-review-body">
        <p className="my-review-text">
          {avaliacao.textoAvaliacao || "Sem comentários."}
        </p>
      </div>
    </article>
  );
};

export default MyAvaliacaoCard;

