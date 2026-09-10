import React, { useState, useEffect, useCallback, useRef } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged, switchMap } from "rxjs/operators";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import StudioSearchInput from "../../components/StudioSearchInput/StudioSearchInput";
import { buscarJogosPaginados, obterMetadadosFiltros } from "./actions/PaginaJogosActions";
import rawgService from "../../services/rawgService";
import { useToast } from "../../context/ToastContext";
import { 
  FaGamepad, 
  FaSearch, 
  FaCalendarAlt, 
  FaSortAmountDown, 
  FaTimes, 
  FaChevronLeft, 
  FaChevronRight, 
  FaSpinner 
} from "react-icons/fa";
import "./PaginaJogos.css";

const ITENS_POR_PAGINA = 15;

function PaginaJogos() {
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();
    const toast = useToast();

    const [jogos, setJogos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const pageFromUrl = parseInt(searchParams.get("page") || "1", 10);
    const [paginaAtual, setPaginaAtual] = useState(pageFromUrl > 0 ? pageFromUrl : 1);
    const [totalPaginas, setTotalPaginas] = useState(1);
    const [totalItens, setTotalItens] = useState(0);

    const [termoPesquisa, setTermoPesquisa] = useState(searchParams.get("q") || "");
    const [buscaAtiva, setBuscaAtiva] = useState(searchParams.get("q") || "");
    const [generoSelecionado, setGeneroSelecionado] = useState(searchParams.get("genero") || "");
    const [anoSelecionado, setAnoSelecionado] = useState(searchParams.get("ano") || "");
    const [anoInput, setAnoInput] = useState(searchParams.get("ano") || "");
    const [empresaSelecionada, setEmpresaSelecionada] = useState(searchParams.get("empresa") || "");
    const [ordenacao, setOrdenacao] = useState(searchParams.get("ordem") || "melhores");

    // Dynamic filter options
    const [generosDisponiveis, setGenerosDisponiveis] = useState([]);
    const [anosDisponiveis, setAnosDisponiveis] = useState([]);
    const [empresasDisponiveis, setEmpresasDisponiveis] = useState([]);

    const [importandoJogoId, setImportandoJogoId] = useState(null);

    // RxJS Subject for reactive query stream & cancellation
    const filterSubject$ = useRef(null);

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

    // Load filter options once
    useEffect(() => {
        obterMetadadosFiltros()
            .then(meta => {
                setGenerosDisponiveis(meta.generos || []);
                setAnosDisponiveis(meta.anos || []);
                setEmpresasDisponiveis(meta.empresas || []);
            })
            .catch(err => console.error("Erro ao carregar metadados dos filtros:", err));
    }, []);

    // Setup RxJS Reactive Pipeline for continuous debounced request streaming
    useEffect(() => {
        filterSubject$.current = new Subject();

        const subscription = filterSubject$.current.pipe(
            debounceTime(250),
            distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
            switchMap(async (params) => {
                setLoading(true);
                setError("");
                try {
                    const res = await buscarJogosPaginados(params);
                    return { res, error: null };
                } catch (err) {
                    return { res: null, error: err.message || "Não foi possível carregar os jogos." };
                }
            })
        ).subscribe(({ res, error: err }) => {
            if (err) {
                setError(err);
            } else if (res) {
                setJogos(res.itens || []);
                setTotalPaginas(res.totalPaginas || 1);
                setTotalItens(res.totalItens || 0);
            }
            setLoading(false);
        });

        return () => {
            subscription.unsubscribe();
        };
    }, []);

    // Push latest filter state to RxJS stream
    useEffect(() => {
        if (filterSubject$.current) {
            filterSubject$.current.next({
                pagina: paginaAtual,
                itensPorPagina: ITENS_POR_PAGINA,
                busca: buscaAtiva,
                genero: generoSelecionado,
                ano: anoSelecionado ? parseInt(anoSelecionado, 10) : null,
                empresa: empresaSelecionada,
                ordenacao
            });
        }
    }, [paginaAtual, buscaAtiva, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao]);

    const handleSearchChange = (e) => {
        const val = e.target.value;
        setTermoPesquisa(val);
        setBuscaAtiva(val);
        setPaginaAtual(1);
        atualizarUrl(1, val, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao);
    };

    const handleLimparBusca = () => {
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

    const handleAnoChange = (e) => {
        const val = e.target.value.replace(/\D/g, "").slice(0, 4);
        setAnoInput(val);
        if (val === "" || val.length === 4) {
            setAnoSelecionado(val);
            setPaginaAtual(1);
            atualizarUrl(1, buscaAtiva, generoSelecionado, val, empresaSelecionada, ordenacao);
        }
    };

    const handleLimparAno = () => {
        setAnoInput("");
        setAnoSelecionado("");
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generoSelecionado, "", empresaSelecionada, ordenacao);
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

    const limparFiltros = () => {
        setTermoPesquisa("");
        setBuscaAtiva("");
        setGeneroSelecionado("");
        setAnoInput("");
        setAnoSelecionado("");
        setEmpresaSelecionada("");
        setOrdenacao("melhores");
        setPaginaAtual(1);
        atualizarUrl(1, "", "", "", "", "melhores");
    };

    const handleMudarPagina = (novaPagina) => {
        if (novaPagina < 1 || novaPagina > totalPaginas || novaPagina === paginaAtual) return;
        setPaginaAtual(novaPagina);
        atualizarUrl(novaPagina, buscaAtiva, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao);
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    const handleGameClick = async (jogo) => {
        if (jogo.ehExterno && jogo.rawgId) {
            setImportandoJogoId(jogo.rawgId);
            try {
                const imported = await rawgService.importarJogoRawg(jogo.rawgId);
                const targetId = imported.jogoId || imported.id;
                navigate(`/jogos/${targetId}`);
            } catch (err) {
                console.error("Erro ao importar jogo:", err);
                navigate(`/jogos/${jogo.jogoId || jogo.id}`);
            } finally {
                setImportandoJogoId(null);
            }
        } else {
            navigate(`/jogos/${jogo.jogoId || jogo.id}`);
        }
    };

    const temFiltrosAtivos = Boolean(
        buscaAtiva || 
        generoSelecionado || 
        anoSelecionado || 
        empresaSelecionada || 
        (ordenacao && ordenacao !== "melhores")
    );

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
                <button
                    key={1}
                    className={`btn-page-number ${paginaAtual === 1 ? "active" : ""}`}
                    onClick={() => handleMudarPagina(1)}
                >
                    1
                </button>
            );
            if (start > 2) {
                pages.push(<span key="dots-start" className="page-dots">...</span>);
            }
        }

        for (let i = start; i <= end; i++) {
            pages.push(
                <button
                    key={i}
                    className={`btn-page-number ${paginaAtual === i ? "active" : ""}`}
                    onClick={() => handleMudarPagina(i)}
                >
                    {i}
                </button>
            );
        }

        if (end < totalPaginas) {
            if (end < totalPaginas - 1) {
                pages.push(<span key="dots-end" className="page-dots">...</span>);
            }
            pages.push(
                <button
                    key={totalPaginas}
                    className={`btn-page-number ${paginaAtual === totalPaginas ? "active" : ""}`}
                    onClick={() => handleMudarPagina(totalPaginas)}
                >
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
                {/* Header da Página */}
                <div className="pagina-jogos-header">
                    <h1 className="pagina-jogos-titulo">Catálogo de Jogos</h1>
                    <p className="pagina-jogos-subtitulo">
                        Explore milhares de títulos populares, avalie suas experiências e gerencie sua biblioteca pessoal.
                    </p>
                </div>

                {/* Caixa de Filtros e Busca */}
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
                            >
                                <FaTimes />
                            </button>
                        )}
                    </div>

                    {/* Grid de Seletores e Filtros */}
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
                                    >
                                        <FaTimes />
                                    </button>
                                )}
                            </div>
                        </div>

                        {/* Busca Escrita Inteligente de Estúdio com Autocomplete */}
                        <div className="filtro-studio-wrapper">
                            <StudioSearchInput
                                empresas={empresasDisponiveis}
                                empresaSelecionada={empresaSelecionada}
                                onSelectEmpresa={handleEmpresaChange}
                            />
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

                {/* Status Bar com Contagem */}
                {!loading && !error && (
                    <div className="catalogo-status-bar">
                        <span className="resultados-contagem">
                            {temFiltrosAtivos ? (
                                <>Mostrando <strong>{jogos.length}</strong> de <strong>{totalItens.toLocaleString('pt-BR')}</strong> {totalItens === 1 ? "jogo encontrado" : "jogos encontrados"}</>
                            ) : (
                                <>Catálogo com <strong>{totalItens.toLocaleString('pt-BR')} Jogos</strong> disponíveis para você avaliar e colecionar</>
                            )}
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

                {/* Grid de 15 Jogos */}
                {!loading && !error && jogos.length > 0 && (
                    <>
                        <div className="jogos-grid-modern">
                            {jogos.map(jogo => (
                                <div 
                                    key={jogo.jogoId || jogo.id || jogo.rawgId} 
                                    className="jogo-card-wrapper"
                                >
                                    <JogoCard jogo={jogo} onClick={handleGameClick} />
                                    {Boolean(importandoJogoId && jogo.rawgId && importandoJogoId === jogo.rawgId) && (
                                        <div className="importing-overlay">
                                            <FaSpinner className="spin" />
                                            <span>Abrindo jogo...</span>
                                        </div>
                                    )}
                                </div>
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
