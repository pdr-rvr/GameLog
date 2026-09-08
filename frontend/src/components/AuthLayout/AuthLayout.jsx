import React from 'react';
import { Link } from 'react-router-dom';
import { FaGamepad, FaStar, FaCompass, FaLayerGroup } from 'react-icons/fa';
import './AuthLayout.css';

const AuthLayout = ({ children, title = "GameLog", subtitle = "Seu universo gamer, organizado e avaliado." }) => {
  return (
    <div className="auth-layout-container">
      {/* Lado Esquerdo - Branding Gamer */}
      <div className="auth-layout-left">
        <div className="auth-brand-content">
          <Link to="/" className="auth-brand-logo">
            <div className="auth-logo-icon">
              <FaGamepad />
            </div>
            <span className="auth-logo-text">Game<span>Log</span></span>
          </Link>
          
          <h1 className="auth-brand-title">
            Descubra, avalie e compartilhe seus jogos favoritos.
          </h1>
          <p className="auth-brand-subtitle">
            {subtitle}
          </p>

          <div className="auth-features-list">
            <div className="auth-feature-item">
              <div className="feature-icon"><FaCompass /></div>
              <div className="feature-text">
                <strong>Catálogo Completo</strong>
                <span>Navegue por dezenas de jogos e filtre por gêneros.</span>
              </div>
            </div>

            <div className="auth-feature-item">
              <div className="feature-icon"><FaStar /></div>
              <div className="feature-text">
                <strong>Avaliações da Comunidade</strong>
                <span>Deixe suas notas e leia a opinião de outros jogadores.</span>
              </div>
            </div>

            <div className="auth-feature-item">
              <div className="feature-icon"><FaLayerGroup /></div>
              <div className="feature-text">
                <strong>Recomendações Inteligentes</strong>
                <span>Sugestões feitas sob medida com base no seu gosto.</span>
              </div>
            </div>
          </div>
        </div>

        {/* Glows Decorativos de Fundo */}
        <div className="auth-bg-glow glow-1"></div>
        <div className="auth-bg-glow glow-2"></div>
      </div>

      {/* Lado Direito - Formulário */}
      <div className="auth-layout-right">
        <div className="auth-card-wrapper">
          {children}
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
