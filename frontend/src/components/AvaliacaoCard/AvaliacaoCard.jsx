import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { 
  FaStar, 
  FaGamepad, 
  FaEdit, 
  FaTrash, 
  FaHeart, 
  FaRegHeart, 
  FaCommentAlt
} from "react-icons/fa";
import { AuthService } from "../../services/authService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { useToast } from "../../context/ToastContext";
import "./AvaliacaoCard.css";

const AvaliacaoCard = ({ avaliacao, onEdit, onDelete }) => {
  const navigate = useNavigate();
  const { warning, error } = useToast();
  const currentUser = AuthService.getCurrentUser();
  const isAuth = AuthService.isAuthenticated();

  const id = avaliacao?.avaliacaoId || avaliacao?.id;
  const jogoId = avaliacao?.jogoId || avaliacao?.idJogo;
  const nomeJogo = avaliacao?.nomeJogo || avaliacao?.tituloJogo || "Jogo";
  const usuarioId = avaliacao?.usuarioId;
  const nomeUsuario = avaliacao?.nomeUsuario || "Gamer";
  const userInitial = nomeUsuario.charAt(0).toUpperCase() || "U";
  const userAvatar = avaliacao?.fotoPerfilUsuario || avaliacao?.fotoDePerfil;
  const nota = Math.min(5, Math.max(1, Number(avaliacao?.nota) || 5));

  const isOwnReview = currentUser && usuarioId && currentUser.usuarioId === usuarioId;

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
      className="review-card" 
      onClick={handleIrParaDiscussao}
      title="Ver discussão e avaliação completa"
    >
      {/* Header: User Profile & Rating */}
      <div className="review-card-top">
        <div className="review-user-block">
          <div className="review-avatar">
            {userAvatar ? (
              <img src={userAvatar} alt={nomeUsuario} className="review-avatar-img" />
            ) : (
              <span>{userInitial}</span>
            )}
          </div>
          <div className="review-user-meta">
            {usuarioId ? (
              <Link 
                to={`/perfil/${usuarioId}`} 
                className="review-username-link" 
                title={`Perfil de ${nomeUsuario}`}
                onClick={(e) => e.stopPropagation()}
              >
                {nomeUsuario}
              </Link>
            ) : (
              <span className="review-username" title={nomeUsuario}>
                {nomeUsuario}
              </span>
            )}
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
          <Link 
            to={`/jogos/${jogoId}`} 
            className="review-game-tag" 
            title={`Ver ${nomeJogo}`}
            onClick={(e) => e.stopPropagation()}
          >
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
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit(avaliacao);
                }}
                title="Editar Avaliação"
              >
                <FaEdit />
              </button>
            )}
            {onDelete && (
              <button
                type="button"
                className="review-btn delete"
                onClick={(e) => {
                  e.stopPropagation();
                  onDelete(id);
                }}
                title="Excluir Avaliação"
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

      {/* Social Actions Bar */}
      <div className="review-social-bar">
        <button
          type="button"
          className={`review-social-btn like-btn ${curtido ? "liked" : ""} ${isOwnReview ? "disabled-self" : ""}`}
          onClick={handleToggleCurtir}
          disabled={isLiking}
          title={isOwnReview ? "Você não pode curtir sua própria avaliação" : curtido ? "Descurtir" : "Curtir"}
          aria-label={curtido ? "Descurtir avaliação" : "Curtir avaliação"}
        >
          {curtido ? <FaHeart className="heart-icon active" /> : <FaRegHeart className="heart-icon" />}
          <span className="social-count">{totalCurtidas}</span>
        </button>

        <button
          type="button"
          className="review-social-btn comment-btn"
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

export default AvaliacaoCard;


