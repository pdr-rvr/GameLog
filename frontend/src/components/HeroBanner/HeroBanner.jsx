import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { FaStar, FaChevronLeft, FaChevronRight, FaGamepad, FaInfoCircle } from "react-icons/fa";
import "./HeroBanner.css";

const HeroBanner = ({ jogos = [] }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isPaused, setIsPaused] = useState(false);

  useEffect(() => {
    if (!jogos || jogos.length <= 1 || isPaused) return;

    const timer = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % jogos.length);
    }, 7000);

    return () => clearInterval(timer);
  }, [jogos, isPaused]);

  if (!jogos || jogos.length === 0) {
    return null;
  }

  const currentGame = jogos[currentIndex];

  const handlePrev = () => {
    setCurrentIndex((prev) => (prev - 1 + jogos.length) % jogos.length);
  };

  const handleNext = () => {
    setCurrentIndex((prev) => (prev + 1) % jogos.length);
  };

  const ano = currentGame.dataLancamento
    ? new Date(currentGame.dataLancamento).getFullYear()
    : null;

  return (
    <div 
      className="hero-banner-container"
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
      data-testid="hero-banner"
    >
      <div 
        className="hero-banner-backdrop"
        style={{ backgroundImage: `url(${currentGame.imagem || ""})` }}
      >
        <div className="hero-banner-gradient"></div>
      </div>

      <div className="hero-banner-content">
        <div className="hero-badges-row">
          {currentGame.mediaAvaliacoes && (
            <span className="hero-rating-badge" data-testid="hero-rating">
              <FaStar className="star-icon" />
              <span>{currentGame.mediaAvaliacoes.toFixed(1)}</span>
            </span>
          )}
          {ano && <span className="hero-meta-pill">{ano}</span>}
          {currentGame.nomeEmpresa && (
            <span className="hero-meta-pill studio">{currentGame.nomeEmpresa}</span>
          )}
        </div>

        <h1 className="hero-game-title" data-testid="hero-title">{currentGame.titulo}</h1>

        {currentGame.generos && currentGame.generos.length > 0 && (
          <div className="hero-genres-list">
            {currentGame.generos.slice(0, 3).map((g, idx) => (
              <span key={idx} className="hero-genre-tag">{g}</span>
            ))}
          </div>
        )}

        <p className="hero-game-description" data-testid="hero-description">
          {currentGame.descricao || "Explore um universo fantástico e compartilhe suas avaliações com a comunidade GameLog."}
        </p>

        <div className="hero-actions-row">
          <Link to={`/jogos/${currentGame.jogoId}`} className="btn-hero-primary" data-testid="hero-link">
            <FaInfoCircle /> <span>Ver Detalhes</span>
          </Link>
          <Link to="/jogos" className="btn-hero-secondary">
            <FaGamepad /> <span>Explorar Catálogo</span>
          </Link>
        </div>
      </div>

      {jogos.length > 1 && (
        <>
          <button 
            type="button" 
            className="hero-nav-arrow left" 
            onClick={handlePrev}
            aria-label="Destaque anterior"
          >
            <FaChevronLeft />
          </button>
          <button 
            type="button" 
            className="hero-nav-arrow right" 
            onClick={handleNext}
            aria-label="Próximo destaque"
          >
            <FaChevronRight />
          </button>

          <div className="hero-pagination-dots">
            {jogos.map((_, idx) => (
              <button
                key={idx}
                type="button"
                className={`hero-dot ${idx === currentIndex ? "active" : ""}`}
                onClick={() => setCurrentIndex(idx)}
                aria-label={`Ir para destaque ${idx + 1}`}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default HeroBanner;
