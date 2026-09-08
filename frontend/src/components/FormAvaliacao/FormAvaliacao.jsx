import React, { useState, useEffect, useRef } from 'react';
import { FaStar, FaTimes, FaGamepad, FaSearch } from 'react-icons/fa';
import './FormAvaliacao.css';

const FormAvaliacao = ({ isOpen, onClose, onCancel, onSubmit, loading, error, jogos = [], initialData = {}, isEditing = false }) => {
  const [avaliacao, setAvaliacao] = useState({
    jogoId: initialData.jogoId || '',
    nota: initialData.nota || 0,
    textoAvaliacao: initialData.textoAvaliacao || ''
  });
  const [hoverRating, setHoverRating] = useState(0);
  const [searchTerm, setSearchTerm] = useState(initialData.tituloJogo || '');
  const [filteredJogos, setFilteredJogos] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const searchInputRef = useRef(null);

  const handleClose = onClose || onCancel;

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
  }, [initialData, isEditing, isOpen]);

  useEffect(() => {
    if (searchTerm && !isEditing && !avaliacao.jogoId) {
      const lowerCase = searchTerm.toLowerCase();
      const suggestions = (jogos || []).filter(jogo =>
        jogo.titulo.toLowerCase().includes(lowerCase)
      );
      setFilteredJogos(suggestions);
      setShowSuggestions(true);
    } else {
      setFilteredJogos([]);
      setShowSuggestions(false);
    }
  }, [searchTerm, jogos, isEditing, avaliacao.jogoId]);

  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === 'Escape' && handleClose) {
        handleClose();
      }
    };

    if (isOpen) {
      window.addEventListener('keydown', handleKeyDown);
    }
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, handleClose]);

  if (!isOpen) return null;

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!avaliacao.jogoId && !isEditing) return;
    if (avaliacao.nota === 0) return;
    if (!avaliacao.textoAvaliacao.trim()) return;

    onSubmit(avaliacao);
  };

  const handleSelectGame = (jogo) => {
    setAvaliacao(prev => ({ ...prev, jogoId: jogo.jogoId }));
    setSearchTerm(jogo.titulo);
    setShowSuggestions(false);
  };

  const ratingDescriptions = ["", "Péssimo", "Ruim", "Regular", "Muito Bom", "Obra-Prima"];

  return (
    <div className="modal-backdrop" onClick={handleClose}>
      <div className="modal-content-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div className="modal-title-group">
            <div className="modal-icon-badge">
              <FaGamepad />
            </div>
            <div>
              <h3>{isEditing ? 'Editar Avaliação' : 'Publicar Avaliação'}</h3>
              <p className="modal-subtitle">Compartilhe sua experiência de jogo com a comunidade</p>
            </div>
          </div>
          <button className="modal-close-btn" onClick={handleClose} aria-label="Fechar modal">
            <FaTimes />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          {/* Seleção do Jogo */}
          {!isEditing ? (
            <div className="modal-form-group" ref={searchInputRef}>
              <label htmlFor="jogoSearch">Selecionar Jogo</label>
              <div className="input-search-wrapper">
                <FaSearch className="search-icon" />
                <input
                  type="text"
                  id="jogoSearch"
                  placeholder="Pesquise o nome do jogo..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setAvaliacao(prev => ({ ...prev, jogoId: '' }));
                  }}
                  onFocus={() => !avaliacao.jogoId && setShowSuggestions(true)}
                  autoComplete="off"
                  required
                />
              </div>

              {showSuggestions && filteredJogos.length > 0 && (
                <ul className="modal-suggestions-list">
                  {filteredJogos.map((jogo) => (
                    <li key={jogo.jogoId} onClick={() => handleSelectGame(jogo)}>
                      <img 
                        src={jogo.imagem || "/game-images/default_game_cover.png"} 
                        alt={jogo.titulo}
                        className="suggestion-thumb" 
                      />
                      <div className="suggestion-info">
                        <strong>{jogo.titulo}</strong>
                        <span>{jogo.dataLancamento ? jogo.dataLancamento.toString().substring(0, 4) : ""} • {jogo.nomeEmpresa || "Game"}</span>
                      </div>
                    </li>
                  ))}
                </ul>
              )}

              {showSuggestions && filteredJogos.length === 0 && searchTerm && (
                <div className="modal-no-suggestions">Nenhum jogo encontrado com esse nome.</div>
              )}
            </div>
          ) : (
            <div className="modal-form-group">
              <label>Jogo</label>
              <input type="text" value={searchTerm} disabled className="input-disabled" />
            </div>
          )}

          {/* Seleção de Nota */}
          <div className="modal-form-group">
            <label>Sua Nota</label>
            <div className="modal-stars-wrapper">
              <div className="modal-stars-container">
                {[1, 2, 3, 4, 5].map((star) => (
                  <FaStar
                    key={star}
                    className={`modal-star-icon ${(hoverRating || avaliacao.nota) >= star ? 'active' : ''}`}
                    onClick={() => setAvaliacao(prev => ({ ...prev, nota: star }))}
                    onMouseEnter={() => setHoverRating(star)}
                    onMouseLeave={() => setHoverRating(0)}
                  />
                ))}
              </div>
              <span className="rating-label">
                {avaliacao.nota > 0 ? `${avaliacao.nota}/5 (${ratingDescriptions[avaliacao.nota]})` : "Selecione uma nota"}
              </span>
            </div>
          </div>

          {/* Comentário */}
          <div className="modal-form-group">
            <label htmlFor="textoAvaliacao">Opinião / Análise</label>
            <textarea
              id="textoAvaliacao"
              placeholder="O que você achou da jogabilidade, história, gráficos e trilha sonora? (Máximo 500 caracteres)"
              value={avaliacao.textoAvaliacao}
              onChange={(e) => setAvaliacao(prev => ({ ...prev, textoAvaliacao: e.target.value }))}
              maxLength={500}
              rows={5}
              required
            />
            <div className="char-counter">
              {avaliacao.textoAvaliacao.length} / 500 caracteres
            </div>
          </div>

          {error && <div className="modal-error-alert">{error}</div>}

          {/* Ações */}
          <div className="modal-actions">
            <button type="button" className="btn-modal-cancel" onClick={handleClose} disabled={loading}>
              Cancelar
            </button>
            <button 
              type="submit" 
              className="btn-modal-submit" 
              disabled={loading || avaliacao.nota === 0 || !avaliacao.textoAvaliacao.trim() || (!isEditing && !avaliacao.jogoId)}
            >
              {loading ? 'Salvando...' : (isEditing ? 'Salvar Alterações' : 'Publicar Avaliação')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FormAvaliacao;
