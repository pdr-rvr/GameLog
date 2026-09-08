import React from "react";
import { Link } from "react-router-dom";
import { FaStar, FaGamepad, FaEdit, FaTrash } from "react-icons/fa";
import "./AvaliacaoCard.css";

const AvaliacaoCard = ({ avaliacao, onEdit, onDelete }) => {
  if (!avaliacao) return null;

  const jogoId = avaliacao.jogoId || avaliacao.idJogo;
  const nomeJogo = avaliacao.nomeJogo || avaliacao.tituloJogo || "Jogo";
  const nomeUsuario = avaliacao.nomeUsuario || "Gamer";
  const userInitial = nomeUsuario.charAt(0).toUpperCase() || "U";
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
    <article className="review-card">
      {/* Header: User Profile & Rating */}
      <div className="review-card-top">
        <div className="review-user-block">
          <div className="review-avatar">
            <span>{userInitial}</span>
          </div>
          <div className="review-user-meta">
            <span className="review-username" title={nomeUsuario}>
              {nomeUsuario}
            </span>
            {formattedDate && (
              <span className="review-timestamp">{formattedDate}</span>
            )}
          </div>
        </div>

        <div className="review-rating-stars" aria-label={`Nota ${nota} de 5`}>
          {[1, 2, 3, 4, 5].map((star) => (
            <FaStar
              key={star}
              className={`review-star ${star <= nota ? "filled" : "empty"}`}
            />
          ))}
        </div>
      </div>

      {/* Game Reference Badge */}
      <div className="review-game-row">
        {jogoId ? (
          <Link to={`/jogos/${jogoId}`} className="review-game-tag" title={`Ver ${nomeJogo}`}>
            <FaGamepad className="game-tag-icon" />
            <span className="game-tag-title">{nomeJogo}</span>
          </Link>
        ) : (
          <div className="review-game-tag">
            <FaGamepad className="game-tag-icon" />
            <span className="game-tag-title">{nomeJogo}</span>
          </div>
        )}

        {(onEdit || onDelete) && (
          <div className="review-card-actions">
            {onEdit && (
              <button
                type="button"
                className="review-btn edit"
                onClick={() => onEdit(avaliacao)}
                title="Editar"
              >
                <FaEdit />
              </button>
            )}
            {onDelete && (
              <button
                type="button"
                className="review-btn delete"
                onClick={() => onDelete(avaliacao.avaliacaoId || avaliacao.id)}
                title="Excluir"
              >
                <FaTrash />
              </button>
            )}
          </div>
        )}
      </div>

      {/* Review Text Content */}
      <div className="review-card-body">
        <p className="review-text">
          {avaliacao.textoAvaliacao || "Sem comentários."}
        </p>
      </div>
    </article>
  );
};

export default AvaliacaoCard;
