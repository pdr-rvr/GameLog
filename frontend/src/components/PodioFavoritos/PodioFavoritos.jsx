import React from "react";
import { Link } from "react-router-dom";
import { FaTrophy, FaEdit, FaStar, FaCrown } from "react-icons/fa";
import "./PodioFavoritos.css";

// Podium layout order: [ 4º, 2º, 1º (centro elevado), 3º, 5º ]
const ORDEM_PODIO = [
  { posicao: 4, label: "4º", classe: "podio-4" },
  { posicao: 2, label: "2º", classe: "podio-2" },
  { posicao: 1, label: "1º", classe: "podio-1" },
  { posicao: 3, label: "3º", classe: "podio-3" },
  { posicao: 5, label: "5º", classe: "podio-5" },
];

const PodioFavoritos = ({ favoritos = [], isOwner = false, onEditar }) => {
  // Map of position -> favorite item
  const mapaFavoritos = {};
  if (Array.isArray(favoritos)) {
    favoritos.forEach((f) => {
      mapaFavoritos[f.posicao] = f;
    });
  }

  const temFavoritos = Object.keys(mapaFavoritos).length > 0;

  return (
    <section className="podio-favoritos-section">
      <div className="podio-favoritos-header">
        <div className="podio-title-wrap">
          <FaTrophy className="podio-trophy-icon" />
          <div>
            <h2 className="podio-section-title">Top 5 Jogos Favoritos</h2>
            <span className="podio-section-subtitle">
              Pódio dos títulos mais memoráveis
            </span>
          </div>
        </div>

        {isOwner && (
          <button
            type="button"
            className="btn-editar-podio"
            onClick={onEditar}
          >
            <FaEdit /> <span>Editar Top 5</span>
          </button>
        )}
      </div>

      {!temFavoritos && !isOwner ? (
        <div className="podio-vazio-visitante">
          <p>Nenhum jogo favorito foi destacado ainda neste perfil.</p>
        </div>
      ) : (
        <div className="podio-container">
          <div className="podio-cards-wrapper">
            {ORDEM_PODIO.map(({ posicao, label, classe }) => {
              const item = mapaFavoritos[posicao];

              if (!item) {
                return (
                  <div
                    key={posicao}
                    className={`podio-slot-card vazio ${classe} ${isOwner ? "clicavel" : ""}`}
                    onClick={isOwner ? onEditar : undefined}
                    title={isOwner ? `Definir ${label} lugar` : undefined}
                  >
                    <div className="podio-rank-badge">
                      {posicao === 1 && <FaCrown className="crown-icon" />}
                      <span>{label}</span>
                    </div>

                    <div className="podio-vazio-placeholder">
                      {isOwner ? (
                        <>
                          <span className="podio-plus-icon">+</span>
                          <span className="podio-add-text">Escolher {label}</span>
                        </>
                      ) : (
                        <span className="podio-vazio-text">-</span>
                      )}
                    </div>
                  </div>
                );
              }

              return (
                <div key={posicao} className={`podio-slot-card preenchido ${classe}`}>
                  <div className="podio-rank-badge">
                    {posicao === 1 && <FaCrown className="crown-icon" />}
                    <span>{label}</span>
                  </div>

                  <Link
                    to={`/jogos/${item.jogoId}`}
                    className="podio-card-link"
                    title={`${item.tituloJogo} (${label} Favorito)`}
                  >
                    <div className="podio-poster-wrap">
                      <img
                        src={item.imagemJogo || "/game-images/default_game_cover.png"}
                        alt={item.tituloJogo}
                        className="podio-poster-img"
                        loading="eager"
                        decoding="async"
                        onError={(e) => {
                          e.target.onerror = null;
                          e.target.src = "/game-images/default_game_cover.png";
                        }}
                      />
                      <div className="podio-poster-overlay">
                        <span className="overlay-titulo">{item.tituloJogo}</span>
                        {item.mediaAvaliacoes !== null && item.mediaAvaliacoes !== undefined && (
                          <span className="overlay-rating">
                            <FaStar className="star-icon" /> {item.mediaAvaliacoes.toFixed(1)}
                          </span>
                        )}
                      </div>
                    </div>
                  </Link>

                  <div className="podio-info-footer">
                    <Link to={`/jogos/${item.jogoId}`} className="podio-game-title">
                      {item.tituloJogo}
                    </Link>
                    <span className="podio-game-company">
                      {item.nomeEmpresa || "Game"}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </section>
  );
};

export default PodioFavoritos;
