import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaSearch, FaTimes, FaGamepad, FaUser, FaLayerGroup, FaChevronRight } from 'react-icons/fa';
import api from '../../services/api';
import './SearchBar.css';

const SearchBar = ({ 
  onSearch, 
  suggestions = [], 
  onSelectSuggestion, 
  placeholder = "Buscar jogos, jogadores ou coleções...",
  globalMode = false 
}) => {
  const [query, setQuery] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [loading, setLoading] = useState(false);
  const [globalResults, setGlobalResults] = useState({ jogos: [], usuarios: [], listas: [] });
  const searchBarRef = useRef(null);
  const debounceTimerRef = useRef(null);
  const abortControllerRef = useRef(null);
  const navigate = useNavigate();

  const fetchGlobalSuggestions = useCallback(async (searchTerm) => {
    if (!searchTerm || searchTerm.trim().length < 2) {
      setGlobalResults({ jogos: [], usuarios: [], listas: [] });
      setLoading(false);
      return;
    }

    // Cancelar requisição anterior em andamento
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    setLoading(true);
    try {
      const response = await api.get(`/BuscaGlobal?q=${encodeURIComponent(searchTerm.trim())}&limite=5`, {
        signal: abortControllerRef.current.signal
      });

      const data = response.data || {};
      setGlobalResults({
        jogos: data.jogos || [],
        usuarios: data.usuarios || [],
        listas: data.listas || []
      });
    } catch (err) {
      if (err.name !== 'CanceledError' && err.code !== 'ERR_CANCELED') {
        console.error("Erro na busca global:", err);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  const handleChange = (e) => {
    const value = e.target.value;
    setQuery(value);

    if (onSearch) {
      onSearch(value);
    }

    if (globalMode) {
      if (debounceTimerRef.current) clearTimeout(debounceTimerRef.current);
      if (value.trim().length >= 2) {
        setShowSuggestions(true);
        debounceTimerRef.current = setTimeout(() => {
          fetchGlobalSuggestions(value);
        }, 250);
      } else {
        setShowSuggestions(false);
        setGlobalResults({ jogos: [], usuarios: [], listas: [] });
      }
    } else {
      setShowSuggestions(value.trim().length > 0);
    }
  };

  const handleSelectGame = (jogo) => {
    setShowSuggestions(false);
    setQuery('');
    if (onSelectSuggestion) {
      onSelectSuggestion(jogo);
    } else {
      const id = jogo.jogoId || jogo.id;
      if (id) navigate(`/jogos/${id}`);
    }
  };

  const handleSelectUser = (user) => {
    setShowSuggestions(false);
    setQuery('');
    const id = user.usuarioId || user.id;
    if (id) navigate(`/perfil/${id}`);
  };

  const handleSelectList = (lista) => {
    setShowSuggestions(false);
    setQuery('');
    const id = lista.listaId || lista.id;
    if (id) navigate(`/listas/${id}`);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && query.trim().length > 0) {
      setShowSuggestions(false);
      if (globalMode) {
        navigate(`/jogos?q=${encodeURIComponent(query.trim())}`);
      }
    }
  };

  const handleClear = () => {
    setQuery('');
    setGlobalResults({ jogos: [], usuarios: [], listas: [] });
    if (onSearch) onSearch('');
    setShowSuggestions(false);
  };

  const handleClickOutside = (event) => {
    if (searchBarRef.current && !searchBarRef.current.contains(event.target)) {
      setShowSuggestions(false);
    }
  };

  useEffect(() => {
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      if (debounceTimerRef.current) clearTimeout(debounceTimerRef.current);
      if (abortControllerRef.current) abortControllerRef.current.abort();
    };
  }, []);

  const hasGlobalResults = 
    globalResults.jogos.length > 0 || 
    globalResults.usuarios.length > 0 || 
    globalResults.listas.length > 0;

  return (
    <div className="search-bar-container" ref={searchBarRef}>
      <div className="search-input-wrapper">
        <FaSearch className="search-input-icon" />
        <input
          type="text"
          placeholder={placeholder}
          value={query}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          onFocus={() => {
            if (globalMode && query.trim().length >= 2) setShowSuggestions(true);
            else if (!globalMode && query.trim().length > 0) setShowSuggestions(true);
          }}
          className="search-input"
          autoComplete="off"
        />
        {query && (
          <button 
            type="button" 
            className="search-clear-btn" 
            onClick={handleClear}
            aria-label="Limpar busca"
          >
            <FaTimes />
          </button>
        )}
      </div>

      {showSuggestions && (
        <div className="suggestions-dropdown">
          {loading && (
            <div className="suggestions-loading">
              <div className="suggestions-spinner"></div>
              <span>Buscando resultados...</span>
            </div>
          )}

          {/* Modo Global: Categorias de Jogos, Listas/Coleções e Usuários */}
          {globalMode && !loading && (
            <>
              {/* Jogos */}
              {globalResults.jogos.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaGamepad className="section-icon" />
                    <span>Jogos</span>
                  </div>
                  {globalResults.jogos.map((jogo) => {
                    const id = jogo.jogoId || jogo.id;
                    const title = jogo.titulo || jogo.nome || 'Jogo';
                    const year = jogo.anoLancamento || (jogo.dataLancamento ? String(jogo.dataLancamento).substring(0, 4) : '');
                    const image = jogo.imagem || jogo.foto;

                    return (
                      <div 
                        key={`game-${id}`} 
                        className="suggestion-item" 
                        onClick={() => handleSelectGame(jogo)}
                      >
                        {image ? (
                          <img src={image} alt={title} className="suggestion-thumb" />
                        ) : (
                          <div className="suggestion-thumb-fallback">
                            <FaGamepad />
                          </div>
                        )}
                        <div className="suggestion-meta">
                          <span className="suggestion-title">{title}</span>
                          <span className="suggestion-sub">
                            {year} {jogo.nomeEmpresa ? `• ${jogo.nomeEmpresa}` : ''}
                          </span>
                        </div>
                        <FaChevronRight className="suggestion-arrow" />
                      </div>
                    );
                  })}
                </div>
              )}

              {/* Coleções / Listas */}
              {globalResults.listas.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaLayerGroup className="section-icon" />
                    <span>Coleções & Listas</span>
                  </div>
                  {globalResults.listas.map((lista) => {
                    const id = lista.listaId || lista.id;
                    const title = lista.titulo || 'Coleção';
                    const creator = lista.nomeCriador ? `por @${lista.nomeCriador}` : '';
                    const count = `${lista.totalJogos || 0} ${lista.totalJogos === 1 ? 'jogo' : 'jogos'}`;

                    return (
                      <div 
                        key={`lista-${id}`} 
                        className="suggestion-item" 
                        onClick={() => handleSelectList(lista)}
                      >
                        <div className="suggestion-lista-fallback">
                          <FaLayerGroup />
                        </div>
                        <div className="suggestion-meta">
                          <span className="suggestion-title">{title}</span>
                          <span className="suggestion-sub">
                            {count} {creator ? `• ${creator}` : ''}
                          </span>
                        </div>
                        <FaChevronRight className="suggestion-arrow" />
                      </div>
                    );
                  })}
                </div>
              )}

              {/* Jogadores / Usuários */}
              {globalResults.usuarios.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaUser className="section-icon" />
                    <span>Jogadores</span>
                  </div>
                  {globalResults.usuarios.map((usuario) => {
                    const id = usuario.usuarioId || usuario.id;
                    const name = usuario.nomeUsuario || usuario.nome || 'Gamer';
                    const initial = name.charAt(0).toUpperCase();

                    return (
                      <div 
                        key={`user-${id}`} 
                        className="suggestion-item user-item" 
                        onClick={() => handleSelectUser(usuario)}
                      >
                        {usuario.fotoDePerfil ? (
                          <img src={usuario.fotoDePerfil} alt={name} className="suggestion-avatar-img" />
                        ) : (
                          <div className="suggestion-avatar-fallback">
                            <span>{initial}</span>
                          </div>
                        )}
                        <div className="suggestion-meta">
                          <span className="suggestion-title">@{name}</span>
                          <span className="suggestion-sub">
                            {usuario.bio ? usuario.bio.substring(0, 45) + (usuario.bio.length > 45 ? '...' : '') : 'Membro GameLog'}
                          </span>
                        </div>
                        <FaChevronRight className="suggestion-arrow" />
                      </div>
                    );
                  })}
                </div>
              )}

              {!hasGlobalResults && (
                <div className="suggestions-empty">
                  Nenhum jogo, coleção ou jogador encontrado para "{query}".
                </div>
              )}
            </>
          )}

          {/* Modo Local (Custom Suggestions) */}
          {!globalMode && !loading && suggestions.length > 0 && (
            suggestions.map((jogo) => {
              const id = jogo.jogoId || jogo.id;
              const title = jogo.titulo || jogo.nome || 'Jogo';
              const year = jogo.dataLancamento ? String(jogo.dataLancamento).substring(0, 4) : '';
              const image = jogo.imagem || jogo.foto;

              return (
                <div 
                  key={id} 
                  className="suggestion-item" 
                  onClick={() => handleSelectGame(jogo)}
                >
                  {image ? (
                    <img src={image} alt={title} className="suggestion-thumb" />
                  ) : (
                    <div className="suggestion-thumb-fallback">
                      <FaGamepad />
                    </div>
                  )}
                  <div className="suggestion-meta">
                    <span className="suggestion-title">{title}</span>
                    {year && <span className="suggestion-sub">{year}</span>}
                  </div>
                  <FaChevronRight className="suggestion-arrow" />
                </div>
              );
            })
          )}
        </div>
      )}
    </div>
  );
};

export default SearchBar;