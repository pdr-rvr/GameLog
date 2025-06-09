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
              src={recomendacao.capaUrl || 'https://via.placeholder.com/150'} 
              alt={recomendacao.titulo} 
              className="recomendacao-image" 
            />
            <h3 className="recomendacao-title">{recomendacao.titulo}</h3>
            {recomendacao.generos && recomendacao.generos.length > 0 && (
              <p className="recomendacao-genres">{recomendacao.generos.join(', ')}</p>
            )}
          </div>
        ))}
      </div>
    </section>
  );
};

export default RecomendacoesCarrossel;