import React, { useState, useEffect, useRef } from 'react';
import './FormAvaliacao.css';

const FormAvaliacao = ({ jogos, onSubmit, loading, error, onCancel, initialData = {}, isEditing = false }) => {
  const [avaliacao, setAvaliacao] = useState({
    jogoId: initialData.jogoId || '',
    nota: initialData.nota || 0,
    textoAvaliacao: initialData.textoAvaliacao || ''
  });
  const [searchTerm, setSearchTerm] = useState(initialData.tituloJogo || '');
  const [filteredJogos, setFilteredJogos] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const searchInputRef = useRef(null);

  useEffect(() => {
    if (isEditing) {
      setAvaliacao({
        jogoId: initialData.jogoId || '',
        nota: initialData.nota || 0,
        textoAvaliacao: initialData.textoAvaliacao || ''
      });
      setSearchTerm(initialData.tituloJogo || '');
    } else {
      setAvaliacao({ jogoId: '', nota: 0, textoAvaliacao: '' });
      setSearchTerm('');
      setFilteredJogos([]);
      setShowSuggestions(false);
    }
  }, [initialData, isEditing]);


  useEffect(() => {
    if (searchTerm && !isEditing) {
      const lowerCaseSearchTerm = searchTerm.toLowerCase();
      const suggestions = (jogos || []).filter(jogo =>
        jogo.titulo.toLowerCase().includes(lowerCaseSearchTerm)
      );
      setFilteredJogos(suggestions);
      setShowSuggestions(true);
    } else {
      setFilteredJogos([]);
      setShowSuggestions(false);
    }
  }, [searchTerm, jogos, isEditing]);

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

  const handleSearchChange = (e) => {
    if (!isEditing) {
      setSearchTerm(e.target.value);
      setAvaliacao(prev => ({ ...prev, jogoId: '' }));
    }
  };

  const handleSelectGame = (jogo) => {
    if (!isEditing) {
      setAvaliacao(prev => ({ ...prev, jogoId: jogo.jogoId }));
      setSearchTerm(jogo.titulo);
      setShowSuggestions(false);
    }
  };

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (searchInputRef.current && !searchInputRef.current.contains(event.target)) {
        setShowSuggestions(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <div className="form-avaliacao">
      <div className="form-header">
        <h3>{isEditing ? 'Editar Avaliação' : 'Criar Nova Avaliação'}</h3>
        {!isEditing && (
          <button
            className="close-button"
            onClick={onCancel}
            aria-label="Fechar formulário"
          >
            &times;
          </button>
        )}
      </div>

      <form onSubmit={handleSubmit}>
        <div className="form-group" ref={searchInputRef}>
          <label htmlFor="jogoId">Jogo:</label>
          <input
            type="text"
            id="jogoId"
            name="jogoId"
            value={searchTerm}
            onChange={handleSearchChange}
            placeholder="Digite o nome do jogo..."
            required
            disabled={loading || isEditing}
            onFocus={() => setShowSuggestions(!isEditing && true)}
            autoComplete="off"
          />
          {showSuggestions && filteredJogos.length > 0 && searchTerm && !isEditing && (
            <ul className="suggestions-list">
              {filteredJogos.map(jogo => (
                <li key={jogo.jogoId} onClick={() => handleSelectGame(jogo)}>
                  {jogo.titulo}
                </li>
              ))}
            </ul>
          )}
          {showSuggestions && filteredJogos.length === 0 && searchTerm && !isEditing && (
            <div className="no-suggestions">Nenhum jogo encontrado.</div>
          )}
          {!isEditing && !avaliacao.jogoId && searchTerm && filteredJogos.length > 0 && (
            <div className="select-game-message">Selecione um jogo da lista.</div>
          )}
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
                role="button"
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
          {onCancel && (
              <button
                type="button"
                className="cancel-button"
                onClick={onCancel}
                disabled={loading}
              >
                Cancelar
              </button>
          )}

          <button
            type="submit"
            className="submit-button"
            disabled={loading || avaliacao.nota === 0 || !avaliacao.textoAvaliacao.trim() || (!isEditing && !avaliacao.jogoId)}
          >
            {loading ? (
              <>
                <span className="spinner"></span>
                {isEditing ? 'Atualizando...' : 'Publicando...'}
              </>
            ) : (
              isEditing ? 'Salvar Alterações' : 'Publicar Avaliação'
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default FormAvaliacao;