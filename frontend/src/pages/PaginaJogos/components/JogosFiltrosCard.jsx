import React from "react";
import { 
  FaGamepad, 
  FaSearch, 
  FaCalendarAlt, 
  FaStar, 
  FaSortAmountDown, 
  FaTimes 
} from "react-icons/fa";

const JogosFiltrosCard = ({
  termoPesquisa,
  handleSearchChange,
  handleLimparBusca,
  generosDisponiveis,
  generosSelecionados,
  handleToggleGenero,
  handleRemoverGenero,
  handleLimparGeneros,
  anoInput,
  handleAnoChange,
  handleLimparAno,
  notaSelecionada,
  handleNotaChange,
  ordenacao,
  handleOrdenacaoChange,
  temFiltrosAtivos,
  limparFiltros
}) => {
  return (
    <div className="filtros-card">
      {/* Barra de Busca */}
      <div className="filtro-busca-wrapper">
        <FaSearch className="filtro-busca-icon" />
        <input
          type="text"
          placeholder="Buscar por título, franquia ou tema..."
          value={termoPesquisa}
          onChange={handleSearchChange}
          className="filtro-input-busca"
        />
        {termoPesquisa && (
          <button
            type="button"
            className="btn-limpar-busca"
            onClick={handleLimparBusca}
            aria-label="Limpar busca"
          >
            <FaTimes />
          </button>
        )}
      </div>

      {/* Grid de Seletores e Filtros */}
      <div className="filtros-seletores-grid">
        <div className="filtro-select-group">
          <label><FaGamepad /> Gêneros / Categorias</label>
          <select
            value=""
            onChange={(e) => {
              if (e.target.value) {
                handleToggleGenero(e.target.value);
              }
            }}
            className="filtro-select-modern"
          >
            <option value="">+ Adicionar Gênero...</option>
            {generosDisponiveis.map(genero => (
              <option key={genero} value={genero} disabled={generosSelecionados.includes(genero)}>
                {genero} {generosSelecionados.includes(genero) ? "✓" : ""}
              </option>
            ))}
          </select>
        </div>

        <div className="filtro-select-group">
          <label><FaCalendarAlt /> Ano</label>
          <div className="filtro-input-ano-wrapper">
            <input
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={4}
              placeholder="Ex: 2024"
              value={anoInput}
              onChange={handleAnoChange}
              className="filtro-input-ano"
            />
            {anoInput && (
              <button
                type="button"
                className="btn-limpar-ano"
                onClick={handleLimparAno}
                title="Limpar ano"
                aria-label="Limpar ano"
              >
                <FaTimes />
              </button>
            )}
          </div>
        </div>

        {/* Filtro de Nota (1 a 5 estrelas) */}
        <div className="filtro-select-group">
          <label><FaStar /> Nota</label>
          <select
            value={notaSelecionada}
            onChange={(e) => handleNotaChange(e.target.value)}
            className="filtro-select-modern"
          >
            <option value="">Todas as Notas</option>
            <option value="5">★ 5 Estrelas</option>
            <option value="4">★ 4+ Estrelas</option>
            <option value="3">★ 3+ Estrelas</option>
            <option value="2">★ 2+ Estrelas</option>
            <option value="1">★ 1+ Estrela</option>
          </select>
        </div>

        <div className="filtro-select-group">
          <label><FaSortAmountDown /> Ordenar por</label>
          <select
            value={ordenacao}
            onChange={(e) => handleOrdenacaoChange(e.target.value)}
            className="filtro-select-modern"
          >
            <option value="melhores">Melhor Avaliados</option>
            <option value="recentes">Mais Recentes</option>
            <option value="antigos">Mais Antigos</option>
            <option value="az">Alfabética (A-Z)</option>
            <option value="za">Alfabética (Z-A)</option>
          </select>
        </div>
      </div>

      {/* Chips de Gêneros Selecionados */}
      {generosSelecionados.length > 0 && (
        <div className="generos-chips-container">
          <span className="chips-label">Gêneros selecionados:</span>
          <div className="generos-chips-list">
            {generosSelecionados.map(g => (
              <span key={g} className="genero-chip">
                <span>{g}</span>
                <button
                  type="button"
                  className="btn-remove-chip"
                  onClick={() => handleRemoverGenero(g)}
                  title={`Remover ${g}`}
                  aria-label={`Remover ${g}`}
                >
                  <FaTimes />
                </button>
              </span>
            ))}
            <button
              type="button"
              className="btn-limpar-chips"
              onClick={handleLimparGeneros}
            >
              Limpar Gêneros
            </button>
          </div>
        </div>
      )}

      {/* Botão de Limpar Filtros */}
      {temFiltrosAtivos && (
        <div className="filtros-actions-bar">
          <button
            type="button"
            className="btn-limpar-filtros-global"
            onClick={limparFiltros}
          >
            <FaTimes /> <span>Limpar Todos os Filtros</span>
          </button>
        </div>
      )}
    </div>
  );
};

export default JogosFiltrosCard;
