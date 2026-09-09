import React, { useState, useEffect, useCallback, useRef } from "react";
import { useSearchParams } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import { buscarJogosPaginados, buscarJogos } from "./actions/PaginaJogosActions";
import { 
  FaGamepad, 
  FaSearch, 
  FaCalendarAlt, 
  FaBuilding, 
  FaSortAmountDown, 
  FaTimes, 
  FaChevronLeft,
  FaChevronRight
} from "react-icons/fa";
import "./PaginaJogos.css";

const ITENS_POR_PAGINA = 12;

function PaginaJogos() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [jogos, setJogos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Pagination state
    const pageFromUrl = parseInt(searchParams.get("page") || "1", 10);
    const [paginaAtual, setPaginaAtual] = useState(pageFromUrl > 0 ? pageFromUrl : 1);
    const [totalPaginas, setTotalPaginas] = useState(1);
    const [totalItens, setTotalItens] = useState(0);

    // Filter states
    const [termoPesquisa, setTermoPesquisa] = useState(searchParams.get("q") || "");
    const [buscaAtiva, setBuscaAtiva] = useState(searchParams.get("q") || "");
    const [generoSelecionado, setGeneroSelecionado] = useState(searchParams.get("genero") || "");
    const [anoSelecionado, setAnoSelecionado] = useState(searchParams.get("ano") || "");
    const [empresaSelecionada, setEmpresaSelecionada] = useState(searchParams.get("empresa") || "");
    const [ordenacao, setOrdenacao] = useState(searchParams.get("ordem") || "melhores");

    const debounceTimerRef = useRef(null);

    useEffect(() => {
        return () => {
            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }
        };
    }, []);

    // Dynamic filter options
    const [generosDisponiveis, setGenerosDisponiveis] = useState([]);
    const [anosDisponiveis, setAnosDisponiveis] = useState([]);
    const [empresasDisponiveis, setEmpresasDisponiveis] = useState([]);

    // Load filter options once
    useEffect(() => {
        buscarJogos()
            .then(todos => {
                const generosSet = new Set();
                const anosSet = new Set();
                const empresasSet = new Set();

                (todos || []).forEach(jogo => {
                    if (jogo.generos && Array.isArray(jogo.generos)) {
                        jogo.generos.forEach(g => generosSet.add(g));
                    }
                    if (jogo.dataLancamento) {
                        const ano = jogo.dataLancamento.toString().substring(0, 4);
                        if (ano) anosSet.add(ano);
                    }
                    if (jogo.nomeEmpresa) {
                        empresasSet.add(jogo.nomeEmpresa);
                    }
                });

                setGenerosDisponiveis(Array.from(generosSet).sort());
                setAnosDisponiveis(Array.from(anosSet).sort((a, b) => b - a));
                setEmpresasDisponiveis(Array.from(empresasSet).sort());
            })
            .catch(err => console.error("Erro ao carregar metadados dos filtros:", err));
    }, []);

    // Sync state with URL params
    const atualizarUrl = useCallback((novaPagina, busca, genero, ano, empresa, ordem) => {
        const params = {};
        if (novaPagina > 1) params.page = novaPagina;
        if (busca && busca.trim()) params.q = busca.trim();
        if (genero) params.genero = genero;
        if (ano) params.ano = ano;
        if (empresa) params.empresa = empresa;
        if (ordem && ordem !== "melhores") params.ordem = ordem;
        setSearchParams(params);
    }, [setSearchParams]);

    // Fetch Paginated Games
    const carregarJogos = useCallback(async () => {
        setLoading(true);
        setError("");
        try {
            const res = await buscarJogosPaginados({
                pagina: paginaAtual,
                itensPorPagina: ITENS_POR_PAGINA,
                busca: buscaAtiva,
                genero: generoSelecionado,
                ano: anoSelecionado ? parseInt(anoSelecionado, 10) : null,
                empresa: empresaSelecionada,
                ordenacao
            });

            setJogos(res.itens || []);
            setTotalPaginas(res.totalPaginas || 1);
            setTotalItens(res.totalItens || 0);
        } catch (err) {
            console.error("Erro ao carregar jogos paginados:", err);
            setError(err.message || "Não foi possível carregar os jogos.");
        } finally {
            setLoading(false);
        }
    }, [paginaAtual, buscaAtiva, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao]);

    useEffect(() => {
        carregarJogos();
    }, [carregarJogos]);

    const handleSearchChange = (e) => {
        const val = e.target.value;
        setTermoPesquisa(val);
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }
        debounceTimerRef.current = setTimeout(() => {
            setPaginaAtual(1);
            setBuscaAtiva(val);
            atualizarUrl(1, val, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao);
        }, 300);
    };

    const handleLimparBusca = () => {
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }
        setTermoPesquisa("");
        setBuscaAtiva("");
        setPaginaAtual(1);
        atualizarUrl(1, "", generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao);
    };

    const handleGeneroChange = (val) => {
        setGeneroSelecionado(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, val, anoSelecionado, empresaSelecionada, ordenacao);
    };

    const handleAnoChange = (val) => {
        setAnoSelecionado(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generoSelecionado, val, empresaSelecionada, ordenacao);
    };

    const handleEmpresaChange = (val) => {
        setEmpresaSelecionada(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generoSelecionado, anoSelecionado, val, ordenacao);
    };

    const handleOrdenacaoChange = (val) => {
        setOrdenacao(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generoSelecionado, anoSelecionado, empresaSelecionada, val);
    };

    const handleMudarPagina = (novaPagina) => {
        if (novaPagina < 1 || novaPagina > totalPaginas || novaPagina === paginaAtual) return;
        setPaginaAtual(novaPagina);
        atualizarUrl(novaPagina, buscaAtiva, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao);
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    const limparFiltros = () => {
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }
        setTermoPesquisa("");
        setBuscaAtiva("");
        setGeneroSelecionado("");
        setAnoSelecionado("");
        setEmpresaSelecionada("");
        setOrdenacao("melhores");
        setPaginaAtual(1);
        setSearchParams({});
    };

    const temFiltrosAtivos = termoPesquisa !== "" || generoSelecionado !== "" || anoSelecionado !== "" || empresaSelecionada !== "" || ordenacao !== "melhores";

    // Build page buttons array
    const renderPageButtons = () => {
        const pages = [];
        const maxVisible = 5;
        let start = Math.max(1, paginaAtual - Math.floor(maxVisible / 2));
        let end = Math.min(totalPaginas, start + maxVisible - 1);

        if (end - start + 1 < maxVisible) {
            start = Math.max(1, end - maxVisible + 1);
        }

        if (start > 1) {
            pages.push(
                <button key={1} type="button" className={`btn-page-num ${paginaAtual === 1 ? "active" : ""}`} onClick={() => handleMudarPagina(1)}>
                    1
                </button>
            );
            if (start > 2) {
                pages.push(<span key="dots-start" className="page-dots">...</span>);
            }
        }

        for (let p = start; p <= end; p++) {
            pages.push(
                <button key={p} type="button" className={`btn-page-num ${paginaAtual === p ? "active" : ""}`} onClick={() => handleMudarPagina(p)}>
                    {p}
                </button>
            );
        }

        if (end < totalPaginas) {
            if (end < totalPaginas - 1) {
                pages.push(<span key="dots-end" className="page-dots">...</span>);
            }
            pages.push(
                <button key={totalPaginas} type="button" className={`btn-page-num ${paginaAtual === totalPaginas ? "active" : ""}`} onClick={() => handleMudarPagina(totalPaginas)}>
                    {totalPaginas}
                </button>
            );
        }

        return pages;
    };

    return (
        <div className="pagina-jogos-container">
            <Navbar />
            <div className="pagina-jogos-content">
                {/* Header Banner */}
                <header className="pagina-jogos-header">
                    <h1 className="pagina-jogos-titulo">Explorar Catálogo</h1>
                    <p className="pagina-jogos-subtitulo">
                        Descubra títulos aclamados, lançamentos e clássicos com avaliações da nossa comunidade.
                    </p>
                </header>

                {/* Caixa de Filtros e Busca */}
                <div className="filtros-card">
                    {/* Barra de Busca */}
                    <div className="filtro-busca-wrapper">
                        <FaSearch className="filtro-busca-icon" />
                        <input
                            type="text"
                            placeholder="Buscar por título, estúdio ou descrição..."
                            value={termoPesquisa}
                            onChange={handleSearchChange}
                            className="filtro-input-busca"
                        />
                        {termoPesquisa && (
                            <button
                                type="button"
                                className="btn-limpar-busca"
                                onClick={handleLimparBusca}
                            >
                                <FaTimes />
                            </button>
                        )}
                    </div>

                    {/* Seletores */}
                    <div className="filtros-seletores-grid">
                        <div className="filtro-select-group">
                            <label><FaGamepad /> Gênero</label>
                            <select
                                value={generoSelecionado}
                                onChange={(e) => handleGeneroChange(e.target.value)}
                                className="filtro-select-modern"
                            >
                                <option value="">Todos os Gêneros</option>
                                {generosDisponiveis.map(genero => (
                                    <option key={genero} value={genero}>{genero}</option>
                                ))}
                            </select>
                        </div>

                        <div className="filtro-select-group">
                            <label><FaCalendarAlt /> Ano</label>
                            <select
                                value={anoSelecionado}
                                onChange={(e) => handleAnoChange(e.target.value)}
                                className="filtro-select-modern"
                            >
                                <option value="">Todos os Anos</option>
                                {anosDisponiveis.map(ano => (
                                    <option key={ano} value={ano}>{ano}</option>
                                ))}
                            </select>
                        </div>

                        <div className="filtro-select-group">
                            <label><FaBuilding /> Estúdio</label>
                            <select
                                value={empresaSelecionada}
                                onChange={(e) => handleEmpresaChange(e.target.value)}
                                className="filtro-select-modern"
                            >
                                <option value="">Todos os Estúdios</option>
                                {empresasDisponiveis.map(empresa => (
                                    <option key={empresa} value={empresa}>{empresa}</option>
                                ))}
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

                    {/* Botão de Limpar Filtros */}
                    {temFiltrosAtivos && (
                        <div className="filtros-actions-bar">
                            <button onClick={limparFiltros} className="btn-limpar-filtros-modern">
                                <FaTimes /> <span>Limpar Todos os Filtros</span>
                            </button>
                        </div>
                    )}
                </div>

                {/* Contador de Resultados */}
                {!loading && !error && (
                    <div className="catalogo-status-bar">
                        <span className="resultados-contagem">
                            Mostrando <strong>{jogos.length}</strong> de <strong>{totalItens}</strong> {totalItens === 1 ? "jogo encontrado" : "jogos encontrados"}
                            {totalPaginas > 1 && ` • Página ${paginaAtual} de ${totalPaginas}`}
                        </span>
                    </div>
                )}

                {/* Estados de Carregamento e Erro */}
                {loading && (
                    <div className="catalogo-state loading">
                        <div className="catalogo-spinner"></div>
                        <span>Carregando catálogo de jogos...</span>
                    </div>
                )}

                {error && (
                    <div className="catalogo-state error">
                        <span>{error}</span>
                    </div>
                )}

                {!loading && !error && jogos.length === 0 && (
                    <div className="no-games-found">
                        <FaGamepad className="empty-icon" />
                        <h3>Nenhum jogo encontrado</h3>
                        <p>Tente ajustar os termos de pesquisa ou remover alguns filtros aplicados.</p>
                        {temFiltrosAtivos && (
                            <button onClick={limparFiltros} className="btn-reset-filters">
                                Redefinir Filtros
                            </button>
                        )}
                    </div>
                )}

                {/* Grid de Jogos */}
                {!loading && !error && jogos.length > 0 && (
                    <>
                        <div className="jogos-grid-modern">
                            {jogos.map(jogo => (
                                <JogoCard key={jogo.jogoId || jogo.id} jogo={jogo} />
                            ))}
                        </div>

                        {/* Paginação */}
                        {totalPaginas > 1 && (
                            <div className="catalogo-paginacao-bar">
                                <button
                                    type="button"
                                    className="btn-page-nav"
                                    onClick={() => handleMudarPagina(paginaAtual - 1)}
                                    disabled={paginaAtual <= 1}
                                    aria-label="Página anterior"
                                >
                                    <FaChevronLeft /> <span>Anterior</span>
                                </button>

                                <div className="page-numbers-list">
                                    {renderPageButtons()}
                                </div>

                                <button
                                    type="button"
                                    className="btn-page-nav"
                                    onClick={() => handleMudarPagina(paginaAtual + 1)}
                                    disabled={paginaAtual >= totalPaginas}
                                    aria-label="Próxima página"
                                >
                                    <span>Próxima</span> <FaChevronRight />
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}

export default PaginaJogos;
