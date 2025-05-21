import React from 'react';
import './AvaliacaoCard.css';

const AvaliacaoCard = ({ avaliacao, jogo }) => {
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
        <span className="jogo-nome">{jogo?.titulo || 'Jogo desconhecido'}</span>
        <span className="avaliacao-data">
          {new Date(avaliacao.dataCriacao).toLocaleDateString()}
        </span>
      </div>
      <div className="card-estrelas">
        {renderEstrelas(avaliacao.nota)}
      </div>
      <div className="card-texto">
        {avaliacao.textoAvaliacao}
      </div>
      <div className="card-footer">
        <span className="usuario-nome">{avaliacao.usuarioNome || 'Anônimo'}</span>
      </div>
    </div>
  );
};

export default AvaliacaoCard;