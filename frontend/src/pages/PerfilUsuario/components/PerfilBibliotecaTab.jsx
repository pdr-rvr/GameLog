import React from "react";
import { Link } from "react-router-dom";
import { FaBookmark, FaSearch, FaTimes } from "react-icons/fa";
import BibliotecaCard from "../../../components/BibliotecaCard/BibliotecaCard";
import { STATUS_JOGO } from "../../../services/bibliotecaService";

const PerfilBibliotecaTab = ({
  perfil,
  isOwner,
  statsBiblioteca,
  statusFiltro,
  setStatusFiltro,
  buscaBiblioteca,
  setBuscaBiblioteca,
  loadingBiblioteca,
  itensBiblioteca,
  onRemoverJogo
}) => {
  const tabsFiltroBiblioteca = [
    { status: null, label: "Todos", count: statsBiblioteca.totalJogos },
    { status: STATUS_JOGO.QUERO_JOGAR, label: "Quero Jogar", count: statsBiblioteca.totalQueroJogar },
    { status: STATUS_JOGO.JOGANDO, label: "Jogando", count: statsBiblioteca.totalJogando },
    { status: STATUS_JOGO.ZERADO, label: "Zerado", count: statsBiblioteca.totalZerados },
    { status: STATUS_JOGO.PAUSADO, label: "Pausado", count: statsBiblioteca.totalPausados },
    { status: STATUS_JOGO.ABANDONADO, label: "Abandonado", count: statsBiblioteca.totalAbandonados },
  ];

  return (
    <section className="gamer-library-section">
      <div className="library-section-header">
        <div>
          <h2 className="section-title">
            Biblioteca de {isOwner ? "Você" : perfil?.nomeUsuario}
          </h2>
          <span className="section-subtitle">Coleção e status de progresso dos jogos</span>
        </div>
      </div>

      <div className="library-toolbar">
        <div className="library-tabs-row">
          {tabsFiltroBiblioteca.map((tab, idx) => {
            const isAtivo = statusFiltro === tab.status;
            return (
              <button
                key={idx}
                type="button"
                className={`library-tab-btn ${isAtivo ? "ativo" : ""}`}
                onClick={() => setStatusFiltro(tab.status)}
              >
                <span className="tab-label">{tab.label}</span>
                <span className="tab-count">{tab.count}</span>
              </button>
            );
          })}
        </div>

        <div className="library-search-input-wrap">
          <FaSearch className="search-icon" />
          <input
            type="text"
            placeholder="Buscar na biblioteca..."
            value={buscaBiblioteca}
            onChange={(e) => setBuscaBiblioteca(e.target.value)}
          />
          {buscaBiblioteca && (
            <button
              type="button"
              className="btn-clear-library-search"
              onClick={() => setBuscaBiblioteca("")}
            >
              <FaTimes />
            </button>
          )}
        </div>
      </div>

      {loadingBiblioteca ? (
        <div className="library-loading-state">
          <div className="perfil-spinner small"></div>
          <span>Atualizando biblioteca...</span>
        </div>
      ) : itensBiblioteca.length === 0 ? (
        <div className="library-empty-state">
          <FaBookmark className="empty-lib-icon" />
          <h3>Nenhum jogo encontrado</h3>
          <p>
            {statusFiltro !== null
              ? "Nenhum jogo corresponde a esse filtro de status."
              : buscaBiblioteca
              ? "Nenhum jogo encontrado com esse termo de busca."
              : isOwner
              ? "Sua biblioteca está vazia. Comece a adicionar os jogos que você está jogando ou já zerou!"
              : `${perfil?.nomeUsuario || "Este usuário"} ainda não adicionou jogos a esta categoria.`}
          </p>
          {isOwner && (
            <Link to="/jogos" className="btn-browse-games">
              Explorar Catálogo de Jogos
            </Link>
          )}
        </div>
      ) : (
        <div className="library-cards-grid">
          {itensBiblioteca.map((item) => (
            <BibliotecaCard
              key={item.id || item.jogoId}
              item={item}
              isOwner={isOwner}
              onRemover={onRemoverJogo}
            />
          ))}
        </div>
      )}
    </section>
  );
};

export default PerfilBibliotecaTab;
