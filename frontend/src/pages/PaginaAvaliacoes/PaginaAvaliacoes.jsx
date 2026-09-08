import React, { useState, useEffect } from "react";
import Navbar from "../../components/Navbar/Navbar";
import AvaliacaoCard from "../../components/AvaliacaoCard/AvaliacaoCard";
import { buscarAvaliacoes } from "../TelaHome/actions/TelaHomeActions";
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
        setAvaliacoes(dados);
      } catch (error) {
        console.error("Erro ao carregar avaliações:", error);
      } finally {
        setLoading(false);
      }
    };

    carregarAvaliacoes();
  }, []);

  const avaliacoesFiltradas = avaliacoes.filter((avaliacao) => {
    const termo = searchTerm.toLowerCase();
    const nomeJogo = (avaliacao.nomeJogo || "").toLowerCase();
    const nomeUsuario = (avaliacao.nomeUsuario || "").toLowerCase();
    const comentario = (avaliacao.textoAvaliacao || "").toLowerCase();
    return nomeJogo.includes(termo) || nomeUsuario.includes(termo) || comentario.includes(termo);
  });

  return (
    <div className="pagina-avaliacoes-container">
      <Navbar />
      <div className="pagina-avaliacoes-content">
        <header className="pagina-avaliacoes-header">
          <h1>Todas as Avaliações</h1>
          <p>Veja o que a comunidade do GameLog está achando dos jogos mais recentes e populares.</p>
          <div className="search-bar-avaliacoes">
            <input
              type="text"
              placeholder="Buscar por jogo, usuário ou comentário..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </header>

        {loading ? (
          <div className="loading-message">Carregando avaliações...</div>
        ) : avaliacoesFiltradas.length > 0 ? (
          <div className="avaliacoes-grid">
            {avaliacoesFiltradas.map((avaliacao) => (
              <AvaliacaoCard key={avaliacao.avaliacaoId} avaliacao={avaliacao} />
            ))}
          </div>
        ) : (
          <div className="no-avaliations-message">
            Nenhuma avaliação encontrada{searchTerm ? " para o termo buscado" : ""}.
          </div>
        )}
      </div>
    </div>
  );
};

export default PaginaAvaliacoes;
