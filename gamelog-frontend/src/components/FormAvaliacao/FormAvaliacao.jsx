import React, { useState } from 'react';
import './FormAvaliacao.css';

const FormAvaliacao = ({ jogos, onSubmit, loading, error }) => {
  const [avaliacao, setAvaliacao] = useState({
    jogoId: '',
    nota: 0,
    textoAvaliacao: ''
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(avaliacao);
  };

  return (
    <div className="form-avaliacao">
      <h3>Criar Nova Avaliação</h3>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <select
            value={avaliacao.jogoId}
            onChange={(e) => setAvaliacao({...avaliacao, jogoId: e.target.value})}
            required
          >
            <option value="">Selecione um jogo</option>
            {jogos.map(jogo => (
              <option key={jogo.jogoId} value={jogo.jogoId}>
                {jogo.titulo}
              </option>
            ))}
          </select>
        </div>
        
        <div className="form-group">
          <label>Nota:</label>
          <div className="estrelas-container">
            {[1, 2, 3, 4, 5].map(i => (
              <span 
                key={i}
                className={`estrela ${i <= avaliacao.nota ? 'preenchida' : ''}`}
                onClick={() => setAvaliacao({...avaliacao, nota: i})}
              >
                {i <= avaliacao.nota ? '★' : '☆'}
              </span>
            ))}
          </div>
        </div>
        
        <div className="form-group">
          <textarea
            placeholder="Escreva sua avaliação (máx. 500 caracteres)"
            value={avaliacao.textoAvaliacao}
            onChange={(e) => {
              if (e.target.value.length <= 500) {
                setAvaliacao({...avaliacao, textoAvaliacao: e.target.value});
              }
            }}
            maxLength="500"
            required
          />
          <small>{avaliacao.textoAvaliacao.length}/500 caracteres</small>
        </div>
        
        <button type="submit" disabled={loading}>
          {loading ? 'Publicando...' : 'Publicar Avaliação'}
        </button>
        {error && <div className="error-message">{error}</div>}
      </form>
    </div>
  );
};

export default FormAvaliacao;