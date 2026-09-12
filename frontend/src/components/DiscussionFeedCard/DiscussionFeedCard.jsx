import React, { useState } from "react";
import { Link } from "react-router-dom";
import { FaCommentDots, FaHeart, FaRegHeart, FaArrowRight, FaGamepad } from "react-icons/fa";
import "./DiscussionFeedCard.css";

const DiscussionFeedCard = ({ item, onToggleCurtir }) => {
  const [avatarError, setAvatarError] = useState(false);
  const [coverError, setCoverError] = useState(false);

  if (!item) return null;

  const autorId = item.autorId || item.usuarioId;
  const autorNome = item.autorNome || item.usuarioNome || item.nomeUsuario || "Gamer";
  const autorFoto = item.autorFoto || item.fotoPerfilUsuario || item.usuarioFoto;

  const autorRespondidoId = item.autorAvaliacaoRespondidaId;
  const autorRespondidoNome = item.autorAvaliacaoRespondidaNome || "outro jogador";

  const avaliacaoId = item.avaliacaoId;
  const avaliacaoOriginalTexto = item.avaliacaoOriginalTexto;
  const comentarioTexto = item.comentarioTexto || item.textoCurto || "";

  const jogoId = item.jogoId;
  const jogoTitulo = item.jogoTitulo || item.nomeJogo || "Jogo";
  const jogoImagem = item.jogoImagem || item.imagemJogo;
  const nomeEmpresa = item.nomeEmpresa || item.empresa;

  const dataAtividade = item.dataAtividade || item.dataCriacao;
  const totalCurtidas = item.totalCurtidas || 0;
  const curtidaPorMim = item.curtidaPorMim || false;

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
    <article className="discussion-feed-card" data-testid={`discussion-card-${item.id || avaliacaoId}`}>
      {/* Topo: Autor e Ação */}
      <div className="discussion-header">
        <div className="discussion-author-row">
          <Link to={`/perfil/${autorId}`} className="discussion-author-link">
            {autorFoto && !avatarError ? (
              <img 
                src={autorFoto} 
                alt={autorNome} 
                className="discussion-avatar"
                onError={() => setAvatarError(true)}
              />
            ) : (
              <div className="discussion-avatar-fallback">
                {autorNome ? autorNome.charAt(0).toUpperCase() : "G"}
              </div>
            )}
          </Link>

          <div className="discussion-meta-info">
            <div className="discussion-action-line">
              <Link to={`/perfil/${autorId}`} className="discussion-author-name">
                {autorNome}
              </Link>
              <span className="discussion-action-text">
                comentou na análise de{" "}
                {autorRespondidoId ? (
                  <Link to={`/perfil/${autorRespondidoId}`} className="discussion-target-user">
                    @{autorRespondidoNome}
                  </Link>
                ) : (
                  <span className="discussion-target-user">@{autorRespondidoNome}</span>
                )}
              </span>
            </div>
            <span className="discussion-date">{formatarData(dataAtividade)}</span>
          </div>
        </div>

        <div className="discussion-badge">
          <FaCommentDots />
          <span>Discussão</span>
        </div>
      </div>

      {/* Contexto do Jogo */}
      <div className="discussion-game-context">
        {jogoImagem && !coverError ? (
          <Link to={`/jogos/${jogoId}`} className="discussion-game-thumb-link">
            <img 
              src={jogoImagem} 
              alt={jogoTitulo} 
              className="discussion-game-thumb"
              onError={() => setCoverError(true)}
            />
          </Link>
        ) : (
          <div className="discussion-game-thumb-fallback">
            <FaGamepad />
          </div>
        )}

        <div className="discussion-game-meta">
          <Link to={`/jogos/${jogoId}`} className="discussion-game-title">
            {jogoTitulo}
          </Link>
          {nomeEmpresa && <span className="discussion-game-studio">{nomeEmpresa}</span>}
        </div>
      </div>

      {/* Bloco de Comentário e Contexto da Resenha Original */}
      <div className="discussion-content-box">
        {avaliacaoOriginalTexto && (
          <div className="discussion-original-quote">
            <span className="quote-label">Em resposta à resenha:</span>
            <p className="quote-text">
              {`"${avaliacaoOriginalTexto.length > 120 
                ? avaliacaoOriginalTexto.substring(0, 120) + "..." 
                : avaliacaoOriginalTexto}"`}
            </p>
          </div>
        )}

        {comentarioTexto && (
          <p className="discussion-comment-text">{`"${comentarioTexto}"`}</p>
        )}
      </div>

      {/* Rodapé: Ações */}
      <div className="discussion-footer">
        <button
          type="button"
          className={`btn-discussion-like ${curtidaPorMim ? "liked" : ""}`}
          onClick={() => onToggleCurtir && onToggleCurtir(avaliacaoId)}
          data-testid="btn-like-discussion"
          aria-label={curtidaPorMim ? "Descurtir comentário" : "Curtir comentário"}
        >
          {curtidaPorMim ? <FaHeart className="like-icon" /> : <FaRegHeart className="like-icon" />}
          <span>{totalCurtidas}</span>
        </button>

        {avaliacaoId && (
          <Link to={`/avaliacoes/${avaliacaoId}`} className="btn-discussion-thread">
            <span>Ver Discussão Completa</span>
            <FaArrowRight />
          </Link>
        )}
      </div>
    </article>
  );
};

export default DiscussionFeedCard;
