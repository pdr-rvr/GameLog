import React, { useState } from "react";
import { Link } from "react-router-dom";
import { FaStar, FaHeart, FaRegHeart, FaCommentDots, FaArrowRight } from "react-icons/fa";
import "./ReviewCardV2.css";

const ReviewCardV2 = ({ avaliacao, onToggleCurtir }) => {
  const [avatarError, setAvatarError] = useState(false);
  const [coverError, setCoverError] = useState(false);

  if (!avaliacao) return null;

  const avaliacaoId = avaliacao.avaliacaoId || avaliacao.id;
  const usuarioId = avaliacao.usuarioId || avaliacao.autorId || avaliacao.idUsuario;
  const nomeUsuario = avaliacao.nomeUsuario || avaliacao.autorNome || avaliacao.usuarioNome || "Gamer";
  const fotoPerfilUsuario = avaliacao.fotoPerfilUsuario || avaliacao.autorFoto || avaliacao.usuarioFoto || avaliacao.fotoPerfil;
  const jogoId = avaliacao.jogoId || avaliacao.idJogo;
  const nomeJogo = avaliacao.nomeJogo || avaliacao.jogoTitulo || avaliacao.tituloJogo || (typeof avaliacao.jogo === "object" ? avaliacao.jogo?.titulo : null) || avaliacao.titulo || "Jogo";
  const imagemJogo = avaliacao.imagemJogo || avaliacao.jogoImagem || (typeof avaliacao.jogo === "object" ? avaliacao.jogo?.imagem : null) || avaliacao.imagem;
  const nomeEmpresa = avaliacao.nomeEmpresa || avaliacao.empresaNome || (typeof avaliacao.empresa === "object" ? avaliacao.empresa?.nomeEmpresa : avaliacao.empresa) || (typeof avaliacao.jogo?.empresa === "object" ? avaliacao.jogo?.empresa?.nomeEmpresa : null);
  const dataPublicacao = avaliacao.dataPublicacao || avaliacao.dataAtividade;
  const nota = avaliacao.nota || 0;
  const textoAvaliacao = avaliacao.textoAvaliacao || avaliacao.comentario || "";
  const totalCurtidas = avaliacao.totalCurtidas || 0;
  const curtidaPorMim = avaliacao.curtidaPorMim || false;
  const totalRespostas = avaliacao.totalRespostas || 0;

  const formatarData = (dt) => {
    if (!dt) return "";
    try {
      const d = new Date(dt);
      return d.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "short",
        year: "numeric"
      });
    } catch {
      return String(dt).substring(0, 10);
    }
  };

  return (
    <article className="review-card-v2" data-testid={`review-card-${avaliacaoId}`}>
      {/* Topo do Card: Informações do Autor e Data */}
      <div className="review-v2-author-bar">
        <Link to={`/perfil/${usuarioId}`} className="review-v2-author-info">
          {fotoPerfilUsuario && !avatarError ? (
            <img 
              src={fotoPerfilUsuario} 
              alt={nomeUsuario} 
              className="review-v2-avatar"
              onError={() => setAvatarError(true)}
            />
          ) : (
            <div className="review-v2-avatar-fallback">
              {nomeUsuario ? nomeUsuario.charAt(0).toUpperCase() : "G"}
            </div>
          )}
          <div>
            <h4 className="review-v2-author-name">{nomeUsuario}</h4>
            <span className="review-v2-date">{formatarData(dataPublicacao)}</span>
          </div>
        </Link>

        <div className="review-v2-rating-badge" data-testid="review-stars">
          {[1, 2, 3, 4, 5].map((star) => (
            <FaStar
              key={star}
              className={`review-star-icon ${star <= nota ? "filled" : ""}`}
            />
          ))}
          <span className="review-v2-rating-val">{nota}/5</span>
        </div>
      </div>

      {/* Meio do Card: Jogo e Texto da Análise */}
      <div className="review-v2-body">
        {imagemJogo && !coverError && (
          <Link to={`/jogos/${jogoId}`} className="review-v2-game-cover-link">
            <img 
              src={imagemJogo} 
              alt={nomeJogo} 
              className="review-v2-game-cover"
              onError={() => setCoverError(true)}
            />
          </Link>
        )}

        <div className="review-v2-content">
          <div className="review-v2-game-meta">
            <Link to={`/jogos/${jogoId}`} className="review-v2-game-title">
              {nomeJogo}
            </Link>
            {nomeEmpresa && <span className="review-v2-game-studio">{nomeEmpresa}</span>}
          </div>

          {textoAvaliacao && (
            <Link to={`/avaliacoes/${avaliacaoId}`} className="review-v2-text-link">
              <p className="review-v2-text">"{textoAvaliacao}"</p>
            </Link>
          )}
        </div>
      </div>

      {/* Rodapé: Ações Sociais */}
      <div className="review-v2-footer">
        <div className="review-v2-actions">
          <button
            type="button"
            className={`btn-review-like ${curtidaPorMim ? "liked" : ""}`}
            onClick={() => onToggleCurtir && onToggleCurtir(avaliacaoId)}
            data-testid="btn-like-review"
            aria-label={curtidaPorMim ? "Descurtir" : "Curtir"}
          >
            {curtidaPorMim ? <FaHeart className="like-icon" /> : <FaRegHeart className="like-icon" />}
            <span>{totalCurtidas}</span>
          </button>

          <Link to={`/avaliacoes/${avaliacaoId}`} className="btn-review-responses" title="Ver discussão">
            <FaCommentDots className="comment-icon" />
            <span>{totalRespostas} {totalRespostas === 1 ? "resposta" : "respostas"}</span>
          </Link>
        </div>

        <Link to={`/avaliacoes/${avaliacaoId}`} className="btn-review-details">
          <span>Ver Análise</span> <FaArrowRight />
        </Link>
      </div>
    </article>
  );
};

export default ReviewCardV2;
