import React from "react";
import { FaBookmark, FaGamepad, FaLayerGroup, FaStar, FaAward } from "react-icons/fa";

const PerfilStatsBar = ({ statsBiblioteca, totalColecoes, mediaNotas, topGeneros }) => {
  return (
    <section className="gamer-stats-bar">
      <div className="stat-card">
        <div className="stat-icon-wrapper purple">
          <FaBookmark />
        </div>
        <div className="stat-data">
          <span className="stat-value">{statsBiblioteca.totalJogos}</span>
          <span className="stat-label">Jogos na Biblioteca</span>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon-wrapper cyan">
          <FaGamepad />
        </div>
        <div className="stat-data">
          <span className="stat-value">{statsBiblioteca.totalZerados}</span>
          <span className="stat-label">Jogos Zerados</span>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon-wrapper green">
          <FaLayerGroup />
        </div>
        <div className="stat-data">
          <span className="stat-value">{totalColecoes}</span>
          <span className="stat-label">Coleções Criadas</span>
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-icon-wrapper gold">
          <FaStar />
        </div>
        <div className="stat-data">
          <span className="stat-value">{mediaNotas}</span>
          <span className="stat-label">Média das Avaliações</span>
        </div>
      </div>

      <div className="stat-card full-span">
        <div className="stat-icon-wrapper pink">
          <FaAward />
        </div>
        <div className="stat-data">
          <span className="stat-label">Gêneros Favoritos</span>
          <div className="genre-chips-wrapper">
            {topGeneros.length > 0 ? (
              topGeneros.map((g, idx) => (
                <span key={idx} className="genre-chip">
                  {g.genero || g.tituloGenero || g}
                </span>
              ))
            ) : (
              <span className="genre-chip empty">Gamer Eclético</span>
            )}
          </div>
        </div>
      </div>
    </section>
  );
};

export default PerfilStatsBar;
