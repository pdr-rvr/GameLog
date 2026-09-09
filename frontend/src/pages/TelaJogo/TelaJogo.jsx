import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import SeletorStatusBiblioteca from "../../components/SeletorStatusBiblioteca/SeletorStatusBiblioteca";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import api from "../../services/api";
import { buscarJogoPorId, buscarAvaliacoesPorJogoId } from "./actions/TelaJogoActions";
import { FaStar, FaEdit, FaPlus, FaGamepad } from "react-icons/fa";
import "./TelaJogo.css";

function TelaJogo() {
    const { jogoId } = useParams();
    const { user } = useAuth();
    const toast = useToast();
    const navigate = useNavigate();

    const [jogo, setJogo] = useState(null);
    const [avaliacoes, setAvaliacoes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Modal de Avaliação
    const [modalAvaliacaoAberto, setModalAvaliacaoAberto] = useState(false);
    const [salvandoAvaliacao, setSalvandoAvaliacao] = useState(false);

    const carregarDadosDoJogo = useCallback(async () => {
        if (!jogoId) return;
        setLoading(true);
        setError("");
        try {
            const [dadosJogo, dadosAvaliacoes] = await Promise.all([
                buscarJogoPorId(jogoId),
                buscarAvaliacoesPorJogoId(jogoId)
            ]);

            setJogo(dadosJogo);
            setAvaliacoes(dadosAvaliacoes || []);
        } catch (err) {
            console.error("Erro ao carregar dados do jogo:", err);
            setError(err.message || "Não foi possível carregar os detalhes do jogo.");
        } finally {
            setLoading(false);
        }
    }, [jogoId]);

    useEffect(() => {
        carregarDadosDoJogo();
    }, [carregarDadosDoJogo]);

    // Verificar se o usuário logado já avaliou este jogo
    const minhaAvaliacao = user && avaliacoes.length > 0
        ? avaliacoes.find(a => Number(a.usuarioId || a.idUsuario) === Number(user.id))
        : null;

    const handleSalvarAvaliacao = async (dados) => {
        setSalvandoAvaliacao(true);
        try {
            if (minhaAvaliacao) {
                const id = minhaAvaliacao.avaliacaoId || minhaAvaliacao.id;
                await api.put(`/Avaliacoes/${id}`, {
                    nota: dados.nota,
                    textoAvaliacao: dados.textoAvaliacao
                });
                toast.success("Avaliação atualizada com sucesso!");
            } else {
                const idJogo = Number(jogo.jogoId ?? jogo.id ?? jogoId);
                await api.post("/Avaliacoes", {
                    jogoId: idJogo,
                    nota: dados.nota,
                    textoAvaliacao: dados.textoAvaliacao
                });
                toast.success("Avaliação publicada com sucesso!");
            }
            setModalAvaliacaoAberto(false);
            
            // Recarregar dados atualizados do jogo e lista de avaliações
            const [novosDadosJogo, novasAvaliacoes] = await Promise.all([
                buscarJogoPorId(jogoId),
                buscarAvaliacoesPorJogoId(jogoId)
            ]);
            setJogo(novosDadosJogo);
            setAvaliacoes(novasAvaliacoes || []);
        } catch (err) {
            console.error("Erro ao salvar avaliação:", err);
            toast.error(err.userMessage || err.response?.data?.message || "Erro ao salvar avaliação.");
        } finally {
            setSalvandoAvaliacao(false);
        }
    };

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
            const date = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
            return date.toLocaleDateString("pt-BR", { year: "numeric", month: "long", day: "numeric" });
        }
        return dataStr;
    };

    const idJogoReal = Number(jogo.jogoId ?? jogo.id ?? jogoId);

    return (
        <div className="tela-jogo-container">
            <Navbar />
            <div className="tela-jogo-content">
                <div className="jogo-detalhes-header">
                    <img 
                        src={jogo.imagem || "/game-images/default_game_cover.png"} 
                        alt={jogo.titulo} 
                        className="jogo-detalhes-imagem" 
                        onError={(e) => {
                            e.target.onerror = null;
                            e.target.src = "/game-images/default_game_cover.png";
                        }}
                    />
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
                                    <strong>Média da Comunidade:</strong> <FaStar className="star-inline" /> {Number(jogo.mediaAvaliacoes).toFixed(1)} ({jogo.totalAvaliacoes || 0} avaliações)
                                </p>
                            )}
                        </div>

                        {/* Barra de Ações Integrada: Biblioteca + Avaliação */}
                        <div className="jogo-acoes-container">
                            {/* 1. Adicionar / Status na Biblioteca */}
                            <div className="jogo-biblioteca-seletor-wrapper">
                                <SeletorStatusBiblioteca jogoId={idJogoReal} />
                            </div>

                            {/* 2. Botão de Avaliar Jogo */}
                            <div className="jogo-avaliar-btn-wrapper">
                                {minhaAvaliacao ? (
                                    <div className="minha-avaliacao-status-card">
                                        <div className="minha-nota-tag">
                                            <FaStar className="star-inline" />
                                            <span>Sua Nota: <strong>{minhaAvaliacao.nota}/5</strong></span>
                                        </div>
                                        <button
                                            type="button"
                                            className="btn-editar-minha-avaliacao"
                                            onClick={() => setModalAvaliacaoAberto(true)}
                                            title="Editar sua análise deste jogo"
                                        >
                                            <FaEdit /> <span>Editar Análise</span>
                                        </button>
                                    </div>
                                ) : (
                                    <button
                                        type="button"
                                        className="btn-avaliar-jogo-principal"
                                        onClick={() => {
                                            if (!user) {
                                                toast.info("Faça login para avaliar este jogo.");
                                                navigate("/login");
                                                return;
                                            }
                                            setModalAvaliacaoAberto(true);
                                        }}
                                        title="Escrever uma avaliação para este jogo"
                                    >
                                        <FaStar className="star-btn-icon" />
                                        <span>Avaliar este Jogo</span>
                                    </button>
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                <div className="jogo-avaliacoes-secao">
                    <AvaliacaoCarrossel
                        title="Avaliações dos Usuários"
                        avaliacoes={avaliacoes}
                    />
                    {avaliacoes.length === 0 && (
                        <div className="no-avaliations-wrapper">
                            <p className="no-avaliations-message">Nenhuma avaliação encontrada para este jogo ainda.</p>
                            <button
                                type="button"
                                className="btn-seja-o-primeiro-avaliar"
                                onClick={() => {
                                    if (!user) {
                                        toast.info("Faça login para avaliar este jogo.");
                                        navigate("/login");
                                        return;
                                    }
                                    setModalAvaliacaoAberto(true);
                                }}
                            >
                                <FaPlus /> Seja o primeiro a avaliar!
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal de Criar / Editar Avaliação */}
            <FormAvaliacao
                isOpen={modalAvaliacaoAberto}
                onClose={() => setModalAvaliacaoAberto(false)}
                onSubmit={handleSalvarAvaliacao}
                loading={salvandoAvaliacao}
                jogos={[jogo]}
                lockGame={true}
                initialData={
                    minhaAvaliacao
                        ? {
                            avaliacaoId: minhaAvaliacao.avaliacaoId || minhaAvaliacao.id,
                            jogoId: idJogoReal,
                            tituloJogo: jogo.titulo,
                            nota: minhaAvaliacao.nota,
                            textoAvaliacao: minhaAvaliacao.textoAvaliacao
                        }
                        : {
                            jogoId: idJogoReal,
                            tituloJogo: jogo.titulo,
                            nota: 0,
                            textoAvaliacao: ""
                        }
                }
                isEditing={Boolean(minhaAvaliacao)}
            />
        </div>
    );
}

export default TelaJogo;
