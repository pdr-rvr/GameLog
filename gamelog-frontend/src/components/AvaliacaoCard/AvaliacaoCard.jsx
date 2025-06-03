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
      <div className="card-header">
        <div className="card-jogo-info">
          <span className="jogo-nome">
            {avaliacao.nomeJogo || 'Jogo desconhecido'}
          </span>
          <span className="avaliacao-data">
            {new Date(avaliacao.dataPublicacao).toLocaleDateString('pt-BR')}
          </span>
        </div>
        <div className="card-usuario-info">
          <span className="usuario-nome">Por: {avaliacao.nomeUsuario}</span>
        </div>
      </div>
      
      <div className="card-estrelas">
        {renderEstrelas(avaliacao.nota)}
      </div>
      
      <div className="card-texto">
        {avaliacao.textoAvaliacao}
      </div>
      
      <div className="card-footer">
        <span className="curtidas">
          {avaliacao.totalCurtidas || 0} curtida{avaliacao.totalCurtidas !== 1 ? 's' : ''}
        </span>
      </div>
    </div>
  );
};

export default AvaliacaoCard;