import React from "react";
import { Link } from "react-router-dom";
import { FaStar, FaTrash } from "react-icons/fa";
import { STATUS_LABELS } from "../../services/bibliotecaService";
import "./BibliotecaCard.css";

const STATUS_CLASSES = {
  1: "status-quero-jogar",
  2: "status-jogando",
  3: "status-zerado",
  4: "status-pausado",
  5: "status-abandonado",
};

const BibliotecaCard = ({ item, isOwner = false, onRemover }) => {
  if (!item) return null;

  const statusClasse = STATUS_CLASSES[item.status] || "status-default";
  const statusTexto = item.statusNome || STATUS_LABELS[item.status] || "Biblioteca";

  return (
    <div className="biblioteca-game-card">
      <Link to={`/jogos/${item.jogoId}`} className="biblioteca-card-cover-link">
        <div className="biblioteca-cover-wrapper">
          <img
            src={item.imagemJogo || "/game-images/default_game_cover.png"}
            alt={item.tituloJogo}
            className="biblioteca-cover-img"
            loading="lazy"
            decoding="async"
          />
          <div className="biblioteca-cover-overlay">
            <span className="cover-view-text">Ver Detalhes</span>
          </div>
        </div>
      </Link>

      <div className="biblioteca-card-content">
        <div className="biblioteca-card-tags">
          <span className={`biblioteca-status-badge ${statusClasse}`}>
            {statusTexto}
          </span>
        </div>

        <Link to={`/jogos/${item.jogoId}`} className="biblioteca-game-title" title={item.tituloJogo}>
          {item.tituloJogo}
        </Link>

        <span className="biblioteca-game-company">
          {item.nomeEmpresa || "Game"}
        </span>

        <div className="biblioteca-card-ratings">
          {item.minhaNota !== null && item.minhaNota !== undefined ? (
            <div className="my-rating-tag" title="Sua Avaliação">
              <FaStar className="star-gold" />
              <span>Sua Nota: <strong>{item.minhaNota}/10</strong></span>
            </div>
          ) : item.mediaAvaliacoes !== null && item.mediaAvaliacoes !== undefined ? (
            <div className="avg-rating-tag" title="Média Geral da Comunidade">
              <FaStar className="star-gold" />
              <span>{item.mediaAvaliacoes.toFixed(1)}</span>
            </div>
          ) : (
            <span className="no-rating-tag">Sem nota</span>
          )}
        </div>

        {isOwner && onRemover && (
          <div className="biblioteca-card-actions">
            <button
              type="button"
              className="btn-card-remove"
              onClick={() => onRemover(item.jogoId)}
              title="Remover da biblioteca"
            >
              <FaTrash /> <span>Remover</span>
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default BibliotecaCard;
