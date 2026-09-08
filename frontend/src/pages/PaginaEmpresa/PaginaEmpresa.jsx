import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import { fetchEmpresa, fetchJogosDaEmpresa } from "./actions/PaginaEmpresaActions";
import { 
  FaBuilding, 
  FaGamepad, 
  FaStar, 
  FaArrowLeft, 
  FaLayerGroup 
} from "react-icons/fa";
import "./PaginaEmpresa.css";

const PaginaEmpresa = () => {
  const { empresaId } = useParams();
  const [empresa, setEmpresa] = useState(null);
  const [jogos, setJogos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const carregarDados = useCallback(async () => {
    if (!empresaId) return;
    setLoading(true);
    setError("");
    try {
      const [dadosEmpresa, dadosJogos] = await Promise.all([
        fetchEmpresa(empresaId),
        fetchJogosDaEmpresa(empresaId)
      ]);
      setEmpresa(dadosEmpresa);
      setJogos(dadosJogos || []);
    } catch (err) {
      console.error("Erro ao carregar empresa:", err);
      setError(err.message || "Não foi possível carregar os dados da empresa.");
    } finally {
      setLoading(false);
    }
  }, [empresaId]);

  useEffect(() => {
    carregarDados();
  }, [carregarDados]);

  const totalJogos = jogos.length;
  const mediaNotas = empresa?.mediaNotasJogos !== null && empresa?.mediaNotasJogos !== undefined
    ? Number(empresa.mediaNotasJogos).toFixed(1)
    : totalJogos > 0
      ? (jogos.reduce((acc, curr) => acc + (Number(curr.mediaAvaliacoes) || 0), 0) / totalJogos).toFixed(1)
      : "0.0";

  return (
    <div className="empresa-page-container">
      <Navbar />

      <main className="empresa-page-content">
        <header className="empresa-top-nav">
          <Link to="/jogos" className="btn-back-catalog">
            <FaArrowLeft /> <span>Voltar ao Catálogo</span>
          </Link>
        </header>

        {loading && (
          <div className="empresa-state loading">
            <div className="empresa-spinner"></div>
            <span>Carregando produções do estúdio...</span>
          </div>
        )}

        {error && !loading && (
          <div className="empresa-state error">
            <span>{error}</span>
          </div>
        )}

        {!loading && !error && empresa && (
          <>
            {/* Hero Banner da Empresa */}
            <div className="empresa-hero-card">
              <div className="empresa-icon-wrapper">
                <FaBuilding className="empresa-hero-icon" />
              </div>

              <div className="empresa-hero-details">
                <div className="empresa-title-row">
                  <h1 className="empresa-title">{empresa.nomeEmpresa}</h1>
                  <span className="empresa-badge">
                    <FaLayerGroup /> Desenvolvedora & Publisher
                  </span>
                </div>

                <div className="empresa-stats-row">
                  <div className="empresa-stat-item">
                    <div className="stat-icon purple">
                      <FaGamepad />
                    </div>
                    <div>
                      <span className="stat-value">{totalJogos}</span>
                      <span className="stat-label">Jogos Catalogados</span>
                    </div>
                  </div>

                  <div className="empresa-stat-item">
                    <div className="stat-icon gold">
                      <FaStar />
                    </div>
                    <div>
                      <span className="stat-value">{mediaNotas}</span>
                      <span className="stat-label">Média das Notas</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Catálogo de Jogos do Estúdio */}
            <section className="empresa-games-section">
              <div className="section-header">
                <h2 className="section-title">
                  Produções de {empresa.nomeEmpresa}
                </h2>
                <span className="section-counter">{totalJogos} {totalJogos === 1 ? "título" : "títulos"}</span>
              </div>

              {jogos.length === 0 ? (
                <div className="empty-empresa-games">
                  <FaGamepad className="empty-icon" />
                  <h3>Nenhum jogo cadastrado no momento</h3>
                  <p>Não encontramos jogos associados a este estúdio no catálogo do GameLog.</p>
                </div>
              ) : (
                <div className="empresa-games-grid">
                  {jogos.map((jogo) => (
                    <JogoCard key={jogo.jogoId || jogo.id} jogo={jogo} />
                  ))}
                </div>
              )}
            </section>
          </>
        )}
      </main>
    </div>
  );
};

export default PaginaEmpresa;
