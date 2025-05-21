import React, { useState } from 'react';
import './FormAvaliacao.css';

const FormAvaliacao = ({ jogos, onSubmit, loading, error, onCancel }) => {
  const [avaliacao, setAvaliacao] = useState({
    jogoId: '',
    nota: 0,
    textoAvaliacao: ''
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(avaliacao);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setAvaliacao(prev => ({ ...prev, [name]: value }));
  };

  const handleNotaChange = (nota) => {
    setAvaliacao(prev => ({ ...prev, nota }));
  };

  return (
    <div className="form-avaliacao">
      <div className="form-header">
        <h3>Criar Nova Avaliação</h3>
        <button 
          className="close-button" 
          onClick={onCancel}
          aria-label="Fechar formulário"
        >
          &times;
        </button>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="jogoId">Selecione um jogo:</label>
          <select
            id="jogoId"
            name="jogoId"
            value={avaliacao.jogoId}
            onChange={handleChange}
            required
            disabled={loading}
          >
            <option value="">Selecione um jogo</option>
            {Array.isArray(jogos) && jogos.map(jogo => (
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
                onClick={() => !loading && handleNotaChange(i)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    !loading && handleNotaChange(i);
                  }
                }}
                tabIndex={0}
                aria-label={`Nota ${i}`}
              >
                {i <= avaliacao.nota ? '★' : '☆'}
              </span>
            ))}
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="textoAvaliacao">Escreva sua avaliação:</label>
          <textarea
            id="textoAvaliacao"
            name="textoAvaliacao"
            placeholder="Digite sua avaliação (máx. 500 caracteres)"
            value={avaliacao.textoAvaliacao}
            onChange={(e) => {
              if (e.target.value.length <= 500) {
                handleChange(e);
              }
            }}
            maxLength="500"
            required
            disabled={loading}
          />
          <div className="contador-caracteres">
            {avaliacao.textoAvaliacao.length}/500 caracteres
          </div>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="form-actions">
          <button
            type="submit"
            className="submit-button"
            disabled={loading || !avaliacao.jogoId || !avaliacao.textoAvaliacao}
          >
            {loading ? (
              <>
                <span className="spinner"></span>
                Publicando...
              </>
            ) : (
              'Publicar Avaliação'
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default FormAvaliacao;