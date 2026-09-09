import React from "react";
import { Link } from "react-router-dom";
import { FaStar, FaCalendarAlt, FaBuilding } from "react-icons/fa";
import "./JogoCard.css";

const JogoCard = ({ jogo }) => {
  if (!jogo) return null;

  const jogoId = jogo.jogoId || jogo.id;
  const titulo = jogo.titulo || jogo.nome || "Jogo sem título";
  const imagem = jogo.imagem || jogo.foto || "/game-images/default_game_cover.png";
  const empresa = jogo.nomeEmpresa || jogo.empresa || "";
  const genero = jogo.genero || jogo.generoFavorito || (Array.isArray(jogo.generos) && jogo.generos[0]) || "";
  
  let anoLancamento = null;
  if (jogo.dataLancamento) {
    const ano = String(jogo.dataLancamento).substring(0, 4);
    if (!isNaN(parseInt(ano, 10))) {
      anoLancamento = ano;
    }
  }

  const mediaNum = Number(jogo.mediaAvaliacoes);
  const media = jogo.mediaAvaliacoes !== null && jogo.mediaAvaliacoes !== undefined && !isNaN(mediaNum) && mediaNum > 0
    ? mediaNum.toFixed(1)
    : null;

  return (
    <Link to={`/jogos/${jogoId}`} className="jogo-card-link">
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
            {empresa && (
              <span className="jogo-card-company" title={empresa}>
                <FaBuilding className="meta-icon" />
                <span className="meta-text">{empresa}</span>
              </span>
            )}

            {anoLancamento && (
              <span className="jogo-card-year">
                <FaCalendarAlt className="meta-icon" />
                <span>{anoLancamento}</span>
              </span>
            )}
          </div>
        </div>

        {/* Glow Accent Bar */}
        <div className="jogo-card-bottom-glow" />
      </article>
    </Link>
  );
};

export default JogoCard;
