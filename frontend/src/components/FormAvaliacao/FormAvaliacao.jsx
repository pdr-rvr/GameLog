import React, { useState, useEffect, useRef } from 'react';
import { FaStar, FaTimes, FaGamepad, FaSearch, FaCheckCircle, FaExchangeAlt } from 'react-icons/fa';
import './FormAvaliacao.css';

const FormAvaliacao = ({ 
  isOpen, 
  onClose, 
  onCancel, 
  onSubmit, 
  loading = false, 
  error = null, 
  jogos = [], 
  initialData = null, 
  isEditing = false,
  lockGame = false
}) => {
  const [avaliacao, setAvaliacao] = useState({
    jogoId: '',
    nota: 0,
    textoAvaliacao: ''
  });
  const [hoverRating, setHoverRating] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [selectedGame, setSelectedGame] = useState(null);

  const searchContainerRef = useRef(null);
  const handleClose = onClose || onCancel;

  // Initialize or reset state only when modal opens or initialData changes
  useEffect(() => {
    if (isOpen) {
      if (initialData || lockGame || isEditing) {
        const currentJogoId = initialData?.jogoId || initialData?.id || (jogos?.length === 1 ? (jogos[0].jogoId || jogos[0].id) : '');
        const currentNota = initialData?.nota || 0;
        const currentTexto = initialData?.textoAvaliacao || '';
        const gameObj = (jogos || []).find(j => Number(j.jogoId || j.id) === Number(currentJogoId)) || (jogos?.length === 1 ? jogos[0] : null);

        setAvaliacao({
          jogoId: currentJogoId,
          nota: currentNota,
          textoAvaliacao: currentTexto
        });
        setSearchTerm(initialData?.tituloJogo || initialData?.nomeJogo || gameObj?.titulo || gameObj?.nome || '');
        setSelectedGame(gameObj || null);
      } else {
        setAvaliacao({
          jogoId: '',
          nota: 0,
          textoAvaliacao: ''
        });
        setSearchTerm('');
        setSelectedGame(null);
      }
      setHoverRating(0);
      setShowSuggestions(false);
    }
  }, [isOpen, isEditing, lockGame, initialData?.avaliacaoId, initialData?.jogoId]);

  // Close suggestions when clicking outside
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (searchContainerRef.current && !searchContainerRef.current.contains(e.target)) {
        setShowSuggestions(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  // Close on Escape
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

  // Compute filtered games
  const safeJogos = Array.isArray(jogos) ? jogos : [];
  const filteredJogos = searchTerm.trim()
    ? safeJogos.filter(jogo => {
        const title = (jogo.titulo || jogo.nome || '').toLowerCase();
        return title.includes(searchTerm.toLowerCase());
      })
    : safeJogos;

  const handleSelectGame = (jogo) => {
    const id = jogo.jogoId || jogo.id;
    const title = jogo.titulo || jogo.nome || '';
    setAvaliacao(prev => ({ ...prev, jogoId: id }));
    setSelectedGame(jogo);
    setSearchTerm(title);
    setShowSuggestions(false);
  };

  const handleClearSelectedGame = () => {
    if (lockGame) return;
    setAvaliacao(prev => ({ ...prev, jogoId: '' }));
    setSelectedGame(null);
    setSearchTerm('');
    setShowSuggestions(true);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!isEditing && !lockGame && !avaliacao.jogoId) return;
    if (avaliacao.nota <= 0) return;
    if (!avaliacao.textoAvaliacao.trim()) return;

    onSubmit({
      jogoId: parseInt(avaliacao.jogoId, 10),
      nota: Number(avaliacao.nota),
      textoAvaliacao: avaliacao.textoAvaliacao.trim()
    });
  };

  const currentDisplayRating = hoverRating || avaliacao.nota;

  return (
    <div 
      className="modal-backdrop" 
      onClick={(e) => {
        if (e.target === e.currentTarget && handleClose) handleClose();
      }}
    >
      <div className="modal-content-card">
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
          <button 
            type="button" 
            className="modal-close-btn" 
            onClick={handleClose} 
            aria-label="Fechar modal"
          >
            <FaTimes />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          {/* Seleção do Jogo */}
          <div className="modal-form-group" ref={searchContainerRef}>
            <label htmlFor="jogoSearch">Jogo Avaliado</label>

            {/* Jogo Pré-selecionado / Travado ou Selecionado */}
            {(lockGame || isEditing || (avaliacao.jogoId && selectedGame) || (searchTerm && !showSuggestions)) ? (
              <div className="selected-game-banner">
                <img 
                  src={selectedGame?.imagem || selectedGame?.foto || "/game-images/default_game_cover.png"} 
                  alt={selectedGame?.titulo || selectedGame?.nome || searchTerm || "Jogo"}
                  className="selected-game-thumb"
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = "/game-images/default_game_cover.png";
                  }}
                />
                <div className="selected-game-info">
                  <span className="selected-game-title">
                    <FaCheckCircle className="check-icon" /> {selectedGame?.titulo || selectedGame?.nome || searchTerm || "Jogo Selecionado"}
                  </span>
                  <span className="selected-game-meta">
                    {selectedGame?.dataLancamento ? String(selectedGame.dataLancamento).substring(0, 4) : ''} 
                    {selectedGame?.nomeEmpresa ? ` • ${selectedGame.nomeEmpresa}` : ''}
                    {lockGame && " • Página do Jogo"}
                  </span>
                </div>
                {!lockGame && !isEditing && (
                  <button 
                    type="button" 
                    className="btn-change-game" 
                    onClick={handleClearSelectedGame}
                    title="Trocar jogo"
                  >
                    <FaExchangeAlt /> <span>Trocar</span>
                  </button>
                )}
              </div>
            ) : (
              /* Campo de Busca de Jogo (Apenas quando não estiver travado) */
              <div className="input-search-wrapper">
                <FaSearch className="search-icon" />
                <input
                  type="text"
                  id="jogoSearch"
                  placeholder="Pesquise o jogo para avaliar..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setShowSuggestions(true);
                  }}
                  onFocus={() => setShowSuggestions(true)}
                  autoComplete="off"
                  required={!avaliacao.jogoId}
                />
                {searchTerm && (
                  <button 
                    type="button" 
                    className="search-clear-btn" 
                    onClick={() => setSearchTerm('')}
                  >
                    <FaTimes />
                  </button>
                )}
              </div>
            )}

            {/* Lista de Sugestões / Dropdown (apenas no modo busca livre) */}
            {!lockGame && !isEditing && !avaliacao.jogoId && showSuggestions && (
              <ul className="modal-suggestions-list">
                {filteredJogos.length > 0 ? (
                  filteredJogos.map((jogo) => (
                    <li 
                      key={jogo.jogoId || jogo.id} 
                      onClick={() => handleSelectGame(jogo)}
                      className="suggestion-item"
                    >
                      <img 
                        src={jogo.imagem || jogo.foto || "/game-images/default_game_cover.png"} 
                        alt={jogo.titulo || jogo.nome}
                        className="suggestion-thumb" 
                        onError={(e) => {
                          e.target.onerror = null;
                          e.target.src = "/game-images/default_game_cover.png";
                        }}
                      />
                      <div className="suggestion-info">
                        <strong>{jogo.titulo || jogo.nome}</strong>
                        <span>
                          {jogo.dataLancamento ? String(jogo.dataLancamento).substring(0, 4) : ''} 
                          {jogo.nomeEmpresa ? ` • ${jogo.nomeEmpresa}` : ''}
                        </span>
                      </div>
                    </li>
                  ))
                ) : (
                  <li className="modal-no-suggestions">
                    Nenhum jogo encontrado com "{searchTerm}".
                  </li>
                )}
              </ul>
            )}
          </div>

          {/* Seleção de Nota */}
          <div className="modal-form-group">
            <label>Sua Nota</label>
            <div className="modal-stars-wrapper">
              <div className="modal-stars-container">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    type="button"
                    key={star}
                    className={`modal-star-btn ${currentDisplayRating >= star ? 'active' : ''}`}
                    onClick={() => setAvaliacao(prev => ({ ...prev, nota: star }))}
                    onMouseEnter={() => setHoverRating(star)}
                    onMouseLeave={() => setHoverRating(0)}
                    aria-label={`Nota ${star}`}
                  >
                    <FaStar className="modal-star-icon" />
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Comentário */}
          <div className="modal-form-group">
            <label htmlFor="textoAvaliacao">Review</label>
            <textarea
              id="textoAvaliacao"
              placeholder="O que você achou da jogabilidade, enredo, gráficos e desempenho? (Máximo 500 caracteres)"
              value={avaliacao.textoAvaliacao}
              onChange={(e) => setAvaliacao(prev => ({ ...prev, textoAvaliacao: e.target.value }))}
              maxLength={500}
              rows={5}
              required
            />
            <div className="char-counter">
              <span className={avaliacao.textoAvaliacao.length > 450 ? 'near-limit' : ''}>
                {avaliacao.textoAvaliacao.length}
              </span> / 500 caracteres
            </div>
          </div>

          {error && <div className="modal-error-alert">{error}</div>}

          {/* Ações */}
          <div className="modal-actions">
            <button 
              type="button" 
              className="btn-modal-cancel" 
              onClick={handleClose} 
              disabled={loading}
            >
              Cancelar
            </button>
            <button 
              type="submit" 
              className="btn-modal-submit" 
              disabled={
                loading || 
                avaliacao.nota === 0 || 
                !avaliacao.textoAvaliacao.trim() || 
                (!isEditing && !avaliacao.jogoId)
              }
            >
              {loading ? 'Publicando...' : (isEditing ? 'Salvar Alterações' : 'Publicar Avaliação')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FormAvaliacao;
