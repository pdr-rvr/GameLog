import React, { useState, useEffect } from "react";
import Navbar from "../../components/Navbar/Navbar";
import AvaliacaoCard from "../../components/AvaliacaoCard/AvaliacaoCard";
import { buscarAvaliacoes } from "../TelaHome/actions/TelaHomeActions";
import { FaSearch, FaComments } from "react-icons/fa";
import "./PaginaAvaliacoes.css";

const PaginaAvaliacoes = () => {
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    const carregarAvaliacoes = async () => {
      setLoading(true);
      try {
        const dados = await buscarAvaliacoes();
        setAvaliacoes(dados || []);
      } catch (error) {
        console.error("Erro ao carregar avaliações:", error);
      } finally {
        setLoading(false);
      }
    };

    carregarAvaliacoes();
  }, []);

  const avaliacoesFiltradas = avaliacoes.filter((avaliacao) => {
    if (!searchTerm.trim()) return true;
    const termo = searchTerm.toLowerCase().trim();
    const nomeJogo = (avaliacao.nomeJogo || "").toLowerCase();
    const nomeUsuario = (avaliacao.nomeUsuario || "").toLowerCase();
    return nomeJogo.includes(termo) || nomeUsuario.includes(termo);
  });

  return (
    <div className="pagina-avaliacoes-container">
      <Navbar />
      <div className="pagina-avaliacoes-content">
        <header className="pagina-avaliacoes-header">
          <h1>Comunidade GameLog</h1>
          <p>Veja o que os jogadores estão achando dos jogos mais recentes e populares.</p>
          
          <div className="search-bar-avaliacoes">
            <FaSearch className="search-input-icon" />
            <input
              type="text"
              placeholder="Buscar avaliações por jogo ou usuário..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            {searchTerm && (
              <button 
                type="button" 
                className="btn-clear-search"
                onClick={() => setSearchTerm("")}
              >
                ×
              </button>
            )}
          </div>
        </header>

        {loading ? (
          <div className="avaliacoes-loading-state">
            <div className="avaliacoes-spinner"></div>
            <span>Carregando avaliações...</span>
          </div>
        ) : avaliacoesFiltradas.length > 0 ? (
          <>
            <div className="avaliacoes-counter-bar">
              <span>{avaliacoesFiltradas.length} {avaliacoesFiltradas.length === 1 ? "avaliação encontrada" : "avaliações encontradas"}</span>
            </div>
            <div className="avaliacoes-grid">
              {avaliacoesFiltradas.map((avaliacao) => (
                <AvaliacaoCard key={avaliacao.avaliacaoId || avaliacao.id} avaliacao={avaliacao} />
              ))}
            </div>
          </>
        ) : (
          <div className="no-avaliations-message">
            <FaComments className="empty-icon" />
            <h3>Nenhuma avaliação encontrada</h3>
            <p>
              {searchTerm 
                ? `Nenhuma avaliação corresponde ao termo "${searchTerm}".`
                : "Ainda não há avaliações cadastradas."}
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default PaginaAvaliacoes;

