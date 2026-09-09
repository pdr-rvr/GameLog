import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { 
  FaEdit, 
  FaTrash, 
  FaStar, 
  FaGamepad, 
  FaHeart, 
  FaRegHeart, 
  FaCommentAlt
} from "react-icons/fa";
import { AuthService } from "../../services/authService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { useToast } from "../../context/ToastContext";
import "./MyAvaliacaoCard.css";

const MyAvaliacaoCard = ({ avaliacao, onEdit, onDelete }) => {
  const navigate = useNavigate();
  const { warning, error } = useToast();
  const isAuth = AuthService.isAuthenticated();

  const id = avaliacao?.avaliacaoId || avaliacao?.id;
  const jogoId = avaliacao?.jogoId || avaliacao?.idJogo;
  const nomeJogo = avaliacao?.nomeJogo || avaliacao?.tituloJogo || "Jogo";
  const nota = Math.min(5, Math.max(1, Number(avaliacao?.nota) || 5));

  // MyAvaliacaoCard is always the user's own review
  const isOwnReview = true;

  // Social state
  const [curtido, setCurtido] = useState(Boolean(avaliacao?.curtidaPorMim));
  const [totalCurtidas, setTotalCurtidas] = useState(Number(avaliacao?.totalCurtidas) || 0);
  const [isLiking, setIsLiking] = useState(false);
  const totalRespostas = Number(avaliacao?.totalRespostas) || 0;

  if (!avaliacao) return null;

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

  const handleToggleCurtir = async (e) => {
    e.stopPropagation();
    if (!isAuth) {
      navigate("/login");
      return;
    }

    if (isOwnReview) {
      warning("Você não pode curtir sua própria avaliação.");
      return;
    }

    if (isLiking || !id) return;

    setIsLiking(true);
    const anteriorCurtido = curtido;
    const anteriorTotal = totalCurtidas;

    const novoCurtido = !anteriorCurtido;
    setCurtido(novoCurtido);
    setTotalCurtidas(novoCurtido ? anteriorTotal + 1 : Math.max(0, anteriorTotal - 1));

    try {
      const res = await AvaliacaoService.toggleCurtir(id);
      if (res && typeof res.curtido === "boolean") {
        setCurtido(res.curtido);
        setTotalCurtidas(res.totalCurtidas);
      }
    } catch (err) {
      console.error("Erro ao alternar curtida:", err);
      setCurtido(anteriorCurtido);
      setTotalCurtidas(anteriorTotal);
      error(err.response?.data?.message || "Erro ao curtir avaliação.");
    } finally {
      setIsLiking(false);
    }
  };

  const handleIrParaDiscussao = (e) => {
    e.stopPropagation();
    if (id) {
      navigate(`/avaliacoes/${id}`);
    }
  };

  return (
    <article 
      className="my-review-card" 
      onClick={handleIrParaDiscussao}
      title="Ver discussão e avaliação completa"
    >
      {/* Header: Game Title/Tag & Action Buttons */}
      <div className="my-review-header">
        <div className="my-review-game-block">
          {jogoId ? (
            <Link 
              to={`/jogos/${jogoId}`} 
              className="my-review-game-title" 
              title={`Ver ${nomeJogo}`}
              onClick={(e) => e.stopPropagation()}
            >
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
              onClick={(e) => {
                e.stopPropagation();
                onEdit(id || avaliacao);
              }}
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
              onClick={(e) => {
                e.stopPropagation();
                onDelete(id);
              }}
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

      {/* Social Actions Bar */}
      <div className="my-review-social-bar">
        <button
          type="button"
          className={`my-review-social-btn like-btn ${curtido ? "liked" : ""} disabled-self`}
          onClick={handleToggleCurtir}
          disabled={isLiking}
          title="Você não pode curtir sua própria avaliação"
          aria-label="Curtir avaliação"
        >
          {curtido ? <FaHeart className="heart-icon active" /> : <FaRegHeart className="heart-icon" />}
          <span className="social-count">{totalCurtidas}</span>
        </button>

        <button
          type="button"
          className="my-review-social-btn comment-btn"
          onClick={handleIrParaDiscussao}
          title="Ver discussão e comentários da avaliação"
          aria-label="Ver discussão da avaliação"
        >
          <FaCommentAlt className="comment-icon" />
          <span className="social-count">{totalRespostas}</span>
          <span className="social-label">{totalRespostas === 1 ? "comentário" : "comentários"}</span>
        </button>
      </div>
    </article>
  );
};

export default MyAvaliacaoCard;


