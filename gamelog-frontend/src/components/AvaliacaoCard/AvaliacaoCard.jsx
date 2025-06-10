import React from 'react';
import { FaEdit, FaTrashAlt, FaStar } from 'react-icons/fa';
import './AvaliacaoCard.css';

const AvaliacaoCard = ({ avaliacao, onEdit, onDelete, currentUserId }) => {
  const reviewDate = avaliacao.dataPublicacao ? new Date(avaliacao.dataPublicacao).toLocaleDateString('pt-BR') : 'Data Indisponível';

  const isAuthor = currentUserId && avaliacao.usuarioId === currentUserId;

  return (
    <div className="avaliacao-card">
      <div className="avaliacao-card-header">
        <h3 className="game-title">{avaliacao.nomeJogo || 'Jogo Desconhecido'}</h3>
        <div className="review-actions">
          {isAuthor && (
            <>
              <button className="edit-button" onClick={() => onEdit(avaliacao.avaliacaoId)} title="Editar Avaliação">
                <FaEdit />
              </button>
              <button className="delete-button" onClick={() => onDelete(avaliacao.avaliacaoId)} title="Excluir Avaliação">
                <FaTrashAlt />
              </button>
            </>
          )}
        </div>
      </div>
      <div className="avaliacao-card-body">
        <div className="review-content">
          <div className="review-rating">
            {[...Array(5)].map((_, i) => (
              <FaStar key={i} className={i < avaliacao.nota ? 'star-filled' : 'star-empty'} />
            ))}
            <span className="rating-text">{avaliacao.nota}/5</span>
          </div>
          <p className="review-comment">{avaliacao.textoAvaliacao || 'Sem comentário.'}</p>
          <span className="review-author">Por: {avaliacao.usuarioNome || 'Usuário Anônimo'}</span>
          <span className="review-date">Avaliado em: {reviewDate}</span>
        </div>
      </div>
    </div>
  );
};

export default AvaliacaoCard;