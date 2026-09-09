import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import { FaStar } from "react-icons/fa";
import SeletorStatusBiblioteca from "../../components/SeletorStatusBiblioteca/SeletorStatusBiblioteca";
import { buscarJogoPorId, buscarAvaliacoesPorJogoId } from "./actions/TelaJogoActions";
import "./TelaJogo.css";

function TelaJogo() {
    const { jogoId } = useParams();
    const [jogo, setJogo] = useState(null);
    const [avaliacoes, setAvaliacoes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const carregarDadosDoJogo = async () => {
            setLoading(true);
            setError("");
            try {
                const [dadosJogo, dadosAvaliacoes] = await Promise.all([
                    buscarJogoPorId(jogoId),
                    buscarAvaliacoesPorJogoId(jogoId)
                ]);

                setJogo(dadosJogo);
                setAvaliacoes(dadosAvaliacoes);
            } catch (err) {
                console.error("Erro ao carregar dados do jogo:", err);
                setError(err.message || "Não foi possível carregar os detalhes do jogo.");
            } finally {
                setLoading(false);
            }
        };

        if (jogoId) {
            carregarDadosDoJogo();
        }
    }, [jogoId]);

    if (loading) {
        return (
            <div className="tela-jogo-container">
                <Navbar />
                <div className="tela-jogo-content loading-message">Carregando detalhes do jogo e avaliações...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="tela-jogo-container">
                <Navbar />
                <div className="tela-jogo-content error-message">{error}</div>
            </div>
        );
    }

    if (!jogo) {
        return (
            <div className="tela-jogo-container">
                <Navbar />
                <div className="tela-jogo-content no-results">Jogo não encontrado.</div>
            </div>
        );
    }

    const formatarData = (dataStr) => {
        if (!dataStr) return "Data Desconhecida";
        const parts = dataStr.toString().split("-");
        if (parts.length >= 3) {
            const date = new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
            return date.toLocaleDateString("pt-BR", { year: "numeric", month: "long", day: "numeric" });
        }
        return dataStr;
    };

    return (
        <div className="tela-jogo-container">
            <Navbar />
            <div className="tela-jogo-content">
                <div className="jogo-detalhes-header">
                    <img src={jogo.imagem} alt={jogo.titulo} className="jogo-detalhes-imagem" />
                    <div className="jogo-info-principal">
                        <h1 className="jogo-detalhes-titulo">{jogo.titulo}</h1>
                        <p className="jogo-detalhes-descricao">{jogo.descricao}</p>
                        
                        <div className="jogo-detalhes-info">
                            <p><strong>Lançamento:</strong> {formatarData(jogo.dataLancamento)}</p>
                            <p><strong>Classificação Indicativa:</strong> {jogo.classificacaoIndicativa !== null ? `${jogo.classificacaoIndicativa} anos` : "Livre"}</p>
                            <p>
                                <strong>Empresa:</strong>{" "}
                                {jogo.empresaId ? (
                                    <Link to={`/empresas/${jogo.empresaId}`} className="jogo-empresa-link">
                                        {jogo.nomeEmpresa}
                                    </Link>
                                ) : (
                                    jogo.nomeEmpresa || "N/A"
                                )}
                            </p>
                            {jogo.generos && jogo.generos.length > 0 && (
                                <p><strong>Gêneros:</strong> {jogo.generos.join(", ")}</p>
                            )}
                            {jogo.mediaAvaliacoes !== null && jogo.mediaAvaliacoes !== undefined && (
                                <p className="media-avaliacoes-tag">
                                    <strong>Média da Comunidade:</strong> <FaStar className="star-inline" /> {jogo.mediaAvaliacoes.toFixed(1)} ({jogo.totalAvaliacoes || 0} avaliações)
                                </p>
                            )}
                        </div>

                        {/* Seletor de Status na Biblioteca */}
                        <div className="jogo-biblioteca-seletor-wrapper">
                            <SeletorStatusBiblioteca jogoId={Number(jogo.jogoId ?? jogo.id ?? jogoId)} />
                        </div>
                    </div>
                </div>

                <div className="jogo-avaliacoes-secao">
                    <AvaliacaoCarrossel
                        title="Avaliações dos Usuários"
                        avaliacoes={avaliacoes}
                    />
                    {avaliacoes.length === 0 && (
                        <p className="no-avaliations-message">Nenhuma avaliação encontrada para este jogo ainda.</p>
                    )}
                </div>
            </div>
        </div>
    );
}

export default TelaJogo;
