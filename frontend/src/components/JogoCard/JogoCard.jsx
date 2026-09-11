import React from "react";
import { Link } from "react-router-dom";
import { FaStar, FaCalendarAlt, FaBuilding } from "react-icons/fa";
import ClassificacaoBadge from "../ClassificacaoBadge/ClassificacaoBadge";
import "./JogoCard.css";

const JogoCard = ({ jogo, onClick }) => {
  if (!jogo) return null;

  const jogoId = jogo.jogoId || jogo.id;
  const titulo = jogo.titulo || jogo.nome || "Jogo sem título";
  const imagem = jogo.imagem || jogo.foto || "/game-images/default_game_cover.png";
  
  // Desenvolvedora e Publicadora
  const desenvolvedora = jogo.nomeDesenvolvedora || jogo.nomeEmpresa || jogo.empresa || "";
  const publicadora = jogo.nomePublicadora || "";
  const temPublicadoraDiferente = Boolean(
    publicadora && 
    desenvolvedora && 
    publicadora.trim().toLowerCase() !== desenvolvedora.trim().toLowerCase()
  );

  const empresaTitle = temPublicadoraDiferente 
    ? `Desenvolvido por: ${desenvolvedora} • Publicado por: ${publicadora}`
    : `Desenvolvimento e Publicação: ${desenvolvedora || publicadora}`;

  const genero = jogo.genero || jogo.generoFavorito || (Array.isArray(jogo.generos) && jogo.generos[0]) || "";
  
  let anoLancamento = null;
  if (jogo.dataLancamento) {
    const ano = String(jogo.dataLancamento).substring(0, 4);
    if (!isNaN(parseInt(ano, 10))) {
      anoLancamento = ano;
    }
  }

  const mediaNum = Number(jogo.mediaAvaliacoes);
  const totalAvaliacoes = Number(jogo.totalAvaliacoes) || 0;
  const isGuid = typeof jogoId === "string" && jogoId.length === 36 && jogoId !== "00000000-0000-0000-0000-000000000000";
  const ehExterno = Boolean(jogo.ehExterno || (jogo.rawgId && !isGuid));

  const media = !ehExterno &&
    jogo.mediaAvaliacoes !== null && 
    jogo.mediaAvaliacoes !== undefined && 
    !isNaN(mediaNum) && 
    mediaNum > 0 && 
    totalAvaliacoes > 0
    ? mediaNum.toFixed(1)
    : null;

  const targetUrl = (isGuid && !ehExterno)
    ? `/jogos/${jogoId}`
    : (jogo.rawgId ? `/jogos/rawg-${jogo.rawgId}` : `/jogos/${jogoId || 0}`);

  const handleLinkClick = (e) => {
    if (onClick) {
      e.preventDefault();
      onClick(jogo);
    }
  };

  return (
    <Link to={targetUrl} onClick={handleLinkClick} className="jogo-card-link">
      <article className="jogo-card">
        {/* Container da Capa */}
        <div className="jogo-card-image-container">
          <img
            src={imagem}
            alt={titulo}
            className="jogo-card-image"
            loading="eager"
            decoding="async"
            onError={(e) => {
              e.target.onerror = null;
              e.target.src = "/game-images/default_game_cover.png";
            }}
          />
          <div className="jogo-card-overlay-gradient" />

          {/* Badge de Nota Média */}
          {media && (
            <div className="jogo-card-rating-badge">
              <FaStar className="star-icon" />
              <span>{media}</span>
            </div>
          )}

          {/* Badge de Gênero */}
          {genero && (
            <span className="jogo-card-genre-badge">
              {genero}
            </span>
          )}
        </div>

        {/* Detalhes do Jogo */}
        <div className="jogo-card-details">
          <h3 className="jogo-card-title" title={titulo}>
            {titulo}
          </h3>

          <div className="jogo-card-meta">
            <div className="jogo-card-companies-box" title={empresaTitle}>
              {desenvolvedora && (
                <span className="jogo-card-company">
                  <FaBuilding className="meta-icon" />
                  <span className="meta-text">{desenvolvedora}</span>
                </span>
              )}
              {temPublicadoraDiferente && (
                <span className="jogo-card-publisher-badge">
                  <span className="pub-tag-label">Pub:</span>
                  <span className="pub-tag-name">{publicadora}</span>
                </span>
              )}
            </div>

            <div className="jogo-card-meta-right">
              {anoLancamento && (
                <span className="jogo-card-year">
                  <FaCalendarAlt className="meta-icon" />
                  <span>{anoLancamento}</span>
                </span>
              )}
              <ClassificacaoBadge classificacao={jogo.classificacaoIndicativa} size="sm" />
            </div>
          </div>
        </div>

        {/* Glow Accent Bar */}
        <div className="jogo-card-bottom-glow" />
      </article>
    </Link>
  );
};

export default JogoCard;
