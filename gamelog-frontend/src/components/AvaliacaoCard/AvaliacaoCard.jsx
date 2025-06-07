import React from 'react';
import './AvaliacaoCard.css';

const AvaliacaoCard = ({ avaliacao }) => {
  const renderEstrelas = (nota) => {
    return [1, 2, 3, 4, 5].map(i => (
      <span key={i} className={`estrela ${i <= nota ? 'preenchida' : ''}`}>
        {i <= nota ? '★' : '☆'}
      </span>
    ));
  };

  return (
    <div className="avaliacao-card">
      {/* img de avaliação ????? */}
      <div className="card-image-placeholder">
        {/* Aqui você pode adicionar uma imagem padrão ou um ícone para a avaliação */}
        <span className="material-icons-outlined">rate_review</span> {/* Exemplo de ícone */}
      </div>
      <div className="card-content">
        <h4 className="card-title">
          {avaliacao.nomeJogo || 'Jogo desconhecido'}
        </h4>
        <div className="card-meta">
          <span className="card-user">Por: {avaliacao.nomeUsuario}</span>
          <span className="card-date">
            {new Date(avaliacao.dataPublicacao).toLocaleDateString('pt-BR')}
          </span>
        </div>
        <div className="card-rating">
          {renderEstrelas(avaliacao.nota)}
        </div>
        <p className="card-description">
          {avaliacao.textoAvaliacao}
        </p>
        <div className="card-footer">
          <span className="card-likes">
            {avaliacao.totalCurtidas || 0} curtida{avaliacao.totalCurtidas !== 1 ? 's' : ''}
          </span>
        </div>
      </div>
    </div>
  );
};

export default AvaliacaoCard;