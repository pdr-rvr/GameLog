import React from 'react';
import './RecomendacoesCarrossel.css';

const RecomendacoesCarrossel = ({ title, recomendacoes }) => {
  if (!recomendacoes || recomendacoes.length === 0) {
    return null;
  }

  return (
    <section className="carrossel-section">
      <h2 className="carrossel-title">{title}</h2>
      <div className="recomendacoes-carrossel-container">
        {recomendacoes.map(recomendacao => (
          <div key={recomendacao.jogoId || recomendacao.id} className="recomendacao-card">
            <img
              src={recomendacao.imagem || '/game-images/default_game_cover.png'} 
              alt={recomendacao.titulo}
              className="recomendacao-image"
              onError={(e) => { 
                e.target.onerror = null; 
                e.target.src = '/game-images/default_game_cover.png'; 
              }}
            />
            <h3 className="recomendacao-title">{recomendacao.titulo}</h3>
          </div>
        ))}
      </div>
    </section>
  );
};

export default RecomendacoesCarrossel;