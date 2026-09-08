import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaSearch, FaTimes, FaGamepad, FaUser, FaBuilding, FaChevronRight } from 'react-icons/fa';
import api from '../../services/api';
import './SearchBar.css';

const SearchBar = ({ 
  onSearch, 
  suggestions = [], 
  onSelectSuggestion, 
  placeholder = "Buscar jogos, desenvolvedores ou usuários...",
  globalMode = false 
}) => {
  const [query, setQuery] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [loading, setLoading] = useState(false);
  const [globalResults, setGlobalResults] = useState({ jogos: [], empresas: [], usuarios: [] });
  const searchBarRef = useRef(null);
  const debounceTimerRef = useRef(null);
  const navigate = useNavigate();

  const fetchGlobalSuggestions = useCallback(async (searchTerm) => {
    if (!searchTerm || searchTerm.trim().length < 2) {
      setGlobalResults({ jogos: [], empresas: [], usuarios: [] });
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const termLower = searchTerm.toLowerCase();
      const [jogosRes, empresasRes, usuariosRes] = await Promise.allSettled([
        api.get('/Jogos'),
        api.get('/Empresas'),
        api.get('/Usuarios')
      ]);

      let jogosFiltrados = [];
      if (jogosRes.status === 'fulfilled' && Array.isArray(jogosRes.value.data)) {
        jogosFiltrados = jogosRes.value.data
          .filter(j => {
            const title = (j.titulo || j.nome || '').toLowerCase();
            const desc = (j.descricao || '').toLowerCase();
            const empresa = (j.nomeEmpresa || '').toLowerCase();
            return title.includes(termLower) || desc.includes(termLower) || empresa.includes(termLower);
          })
          .slice(0, 5);
      }

      let empresasFiltradas = [];
      if (empresasRes.status === 'fulfilled' && Array.isArray(empresasRes.value.data)) {
        empresasFiltradas = empresasRes.value.data
          .filter(e => {
            const name = (e.nomeEmpresa || e.nome || '').toLowerCase();
            return name.includes(termLower);
          })
          .slice(0, 3);
      }

      let usuariosFiltrados = [];
      if (usuariosRes.status === 'fulfilled' && Array.isArray(usuariosRes.value.data)) {
        usuariosFiltrados = usuariosRes.value.data
          .filter(u => {
            const name = (u.nomeUsuario || u.nome || '').toLowerCase();
            return name.includes(termLower);
          })
          .slice(0, 3);
      }

      setGlobalResults({ jogos: jogosFiltrados, empresas: empresasFiltradas, usuarios: usuariosFiltrados });
    } catch (err) {
      console.error("Erro na busca global:", err);
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
        setGlobalResults({ jogos: [], empresas: [], usuarios: [] });
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

  const handleSelectEmpresa = (empresa) => {
    setShowSuggestions(false);
    setQuery('');
    const id = empresa.empresaId || empresa.id;
    if (id) navigate(`/empresas/${id}`);
  };

  const handleSelectUser = (user) => {
    setShowSuggestions(false);
    setQuery('');
    const id = user.id || user.usuarioId;
    if (id) navigate(`/perfil/${id}`);
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
    setGlobalResults({ jogos: [], empresas: [], usuarios: [] });
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
    };
  }, []);

  const hasGlobalResults = 
    globalResults.jogos.length > 0 || 
    globalResults.empresas.length > 0 || 
    globalResults.usuarios.length > 0;

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

          {/* Modo Global: Categorias de Jogos, Empresas e Usuários */}
          {globalMode && !loading && (
            <>
              {globalResults.jogos.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaGamepad className="section-icon" />
                    <span>Jogos</span>
                  </div>
                  {globalResults.jogos.map((jogo) => {
                    const id = jogo.jogoId || jogo.id;
                    const title = jogo.titulo || jogo.nome || 'Jogo';
                    const year = jogo.dataLancamento ? String(jogo.dataLancamento).substring(0, 4) : '';
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

              {globalResults.empresas.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaBuilding className="section-icon" />
                    <span>Empresas & Estúdios</span>
                  </div>
                  {globalResults.empresas.map((empresa) => {
                    const id = empresa.empresaId || empresa.id;
                    const name = empresa.nomeEmpresa || empresa.nome || 'Empresa';

                    return (
                      <div 
                        key={`empresa-${id}`} 
                        className="suggestion-item empresa-item" 
                        onClick={() => handleSelectEmpresa(empresa)}
                      >
                        <div className="suggestion-empresa-fallback">
                          <FaBuilding />
                        </div>
                        <div className="suggestion-meta">
                          <span className="suggestion-title">{name}</span>
                          <span className="suggestion-sub">
                            {empresa.totalJogos !== undefined ? `${empresa.totalJogos} ${empresa.totalJogos === 1 ? 'jogo' : 'jogos'}` : 'Estúdio'}
                          </span>
                        </div>
                        <FaChevronRight className="suggestion-arrow" />
                      </div>
                    );
                  })}
                </div>
              )}

              {globalResults.usuarios.length > 0 && (
                <div className="suggestion-section">
                  <div className="suggestion-section-header">
                    <FaUser className="section-icon" />
                    <span>Usuários</span>
                  </div>
                  {globalResults.usuarios.map((usuario) => {
                    const id = usuario.id || usuario.usuarioId;
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
                          <span className="suggestion-title">{name}</span>
                          <span className="suggestion-sub">Perfil de Gamer</span>
                        </div>
                        <FaChevronRight className="suggestion-arrow" />
                      </div>
                    );
                  })}
                </div>
              )}

              {!hasGlobalResults && (
                <div className="suggestions-empty">
                  Nenhum jogo, estúdio ou usuário encontrado para "{query}".
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
