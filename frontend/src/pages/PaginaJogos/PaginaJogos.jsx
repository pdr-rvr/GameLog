import React, { useState, useEffect, useCallback, useRef } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { Subject } from "rxjs";
import { debounceTime, distinctUntilChanged, switchMap } from "rxjs/operators";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import { buscarJogosPaginados, obterMetadadosFiltros } from "./actions/PaginaJogosActions";
import rawgService from "../../services/rawgService";
import { useToast } from "../../context/ToastContext";
import { 
  FaGamepad, 
  FaSearch, 
  FaCalendarAlt, 
  FaStar,
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

    const initialQuery = searchParams.get("busca") || searchParams.get("q") || "";
    const pageFromUrl = parseInt(searchParams.get("page") || "1", 10);
    const [paginaAtual, setPaginaAtual] = useState(pageFromUrl > 0 ? pageFromUrl : 1);
    const [totalPaginas, setTotalPaginas] = useState(1);
    const [totalItens, setTotalItens] = useState(0);

    const parseGenerosFromUrl = () => {
        const todos = searchParams.getAll("genero");
        const generosComma = searchParams.get("generos");
        const res = new Set();
        todos.forEach(t => {
            if (t) t.split(",").forEach(g => { if (g.trim()) res.add(g.trim()); });
        });
        if (generosComma) {
            generosComma.split(",").forEach(g => { if (g.trim()) res.add(g.trim()); });
        }
        return Array.from(res);
    };

    const [termoPesquisa, setTermoPesquisa] = useState(initialQuery);
    const [buscaAtiva, setBuscaAtiva] = useState(initialQuery);
    const [generosSelecionados, setGenerosSelecionados] = useState(parseGenerosFromUrl());
    const [anoSelecionado, setAnoSelecionado] = useState(searchParams.get("ano") || "");
    const [anoInput, setAnoInput] = useState(searchParams.get("ano") || "");
    const [notaSelecionada, setNotaSelecionada] = useState(searchParams.get("nota") || "");
    const [ordenacao, setOrdenacao] = useState(searchParams.get("ordem") || "melhores");

    // Dynamic filter options
    const [generosDisponiveis, setGenerosDisponiveis] = useState([]);
    const [anosDisponiveis, setAnosDisponiveis] = useState([]);

    const [importandoJogoId, setImportandoJogoId] = useState(null);

    // RxJS Subject for reactive query stream & cancellation
    const filterSubject$ = useRef(null);

    // Sync state with URL params
    const atualizarUrl = useCallback((novaPagina, busca, generos, ano, nota, ordem) => {
        const params = new URLSearchParams();
        if (novaPagina > 1) params.set("page", novaPagina);
        if (busca && busca.trim()) params.set("busca", busca.trim());
        if (generos && generos.length > 0) {
            generos.forEach(g => params.append("genero", g));
        }
        if (ano) params.set("ano", ano);
        if (nota) params.set("nota", nota);
        if (ordem && ordem !== "melhores") params.set("ordem", ordem);
        setSearchParams(params);
    }, [setSearchParams]);

    // Load filter options once
    useEffect(() => {
        obterMetadadosFiltros()
            .then(meta => {
                setGenerosDisponiveis(meta.generos || []);
                setAnosDisponiveis(meta.anos || []);
            })
            .catch(err => console.error("Erro ao carregar metadados dos filtros:", err));
    }, []);

    // Sincronizar se a URL mudar externamente (ex: busca na navbar ou navegação por gênero)
    useEffect(() => {
        const qUrl = searchParams.get("busca") || searchParams.get("q") || "";
        const gensUrl = parseGenerosFromUrl();
        if (qUrl !== buscaAtiva) {
            setTermoPesquisa(qUrl);
            setBuscaAtiva(qUrl);
            setPaginaAtual(1);
        }
        if (JSON.stringify(gensUrl) !== JSON.stringify(generosSelecionados)) {
            setGenerosSelecionados(gensUrl);
            setPaginaAtual(1);
        }
    }, [searchParams]);

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
                genero: generosSelecionados,
                ano: anoSelecionado ? parseInt(anoSelecionado, 10) : null,
                nota: notaSelecionada ? parseFloat(notaSelecionada) : null,
                ordenacao
            });
        }
    }, [paginaAtual, buscaAtiva, generosSelecionados, anoSelecionado, notaSelecionada, ordenacao]);

    const handleSearchChange = (e) => {
        const val = e.target.value;
        setTermoPesquisa(val);
        setBuscaAtiva(val);
        setPaginaAtual(1);
        atualizarUrl(1, val, generosSelecionados, anoSelecionado, notaSelecionada, ordenacao);
    };

    const handleLimparBusca = () => {
        setTermoPesquisa("");
        setBuscaAtiva("");
        setPaginaAtual(1);
        atualizarUrl(1, "", generosSelecionados, anoSelecionado, notaSelecionada, ordenacao);
    };

    const handleToggleGenero = (gen) => {
        if (!gen) return;
        const novo = generosSelecionados.includes(gen)
            ? generosSelecionados.filter(g => g !== gen)
            : [...generosSelecionados, gen];
        setGenerosSelecionados(novo);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, novo, anoSelecionado, notaSelecionada, ordenacao);
    };

    const handleRemoverGenero = (gen) => {
        const novo = generosSelecionados.filter(g => g !== gen);
        setGenerosSelecionados(novo);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, novo, anoSelecionado, notaSelecionada, ordenacao);
    };

    const handleAnoChange = (e) => {
        const val = e.target.value.replace(/\D/g, "").slice(0, 4);
        setAnoInput(val);
        if (val === "" || val.length === 4) {
            setAnoSelecionado(val);
            setPaginaAtual(1);
            atualizarUrl(1, buscaAtiva, generosSelecionados, val, notaSelecionada, ordenacao);
        }
    };

    const handleLimparAno = () => {
        setAnoInput("");
        setAnoSelecionado("");
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generosSelecionados, "", notaSelecionada, ordenacao);
    };

    const handleNotaChange = (val) => {
        setNotaSelecionada(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generosSelecionados, anoSelecionado, val, ordenacao);
    };

    const handleOrdenacaoChange = (val) => {
        setOrdenacao(val);
        setPaginaAtual(1);
        atualizarUrl(1, buscaAtiva, generosSelecionados, anoSelecionado, notaSelecionada, val);
    };

    const limparFiltros = () => {
        setTermoPesquisa("");
        setBuscaAtiva("");
        setGenerosSelecionados([]);
        setAnoInput("");
        setAnoSelecionado("");
        setNotaSelecionada("");
        setOrdenacao("melhores");
        setPaginaAtual(1);
        atualizarUrl(1, "", [], "", "", "melhores");
    };

    const handleMudarPagina = (novaPagina) => {
        if (novaPagina < 1 || novaPagina > totalPaginas || novaPagina === paginaAtual) return;
        setPaginaAtual(novaPagina);
        atualizarUrl(novaPagina, buscaAtiva, generosSelecionados, anoSelecionado, notaSelecionada, ordenacao);
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
                const fallbackId = jogo.jogoId || jogo.id;
                if (fallbackId) navigate(`/jogos/${fallbackId}`);
            } finally {
                setImportandoJogoId(null);
            }
        } else {
            const targetId = jogo.jogoId || jogo.id;
            if (targetId) navigate(`/jogos/${targetId}`);
        }
    };

    const temFiltrosAtivos = Boolean(
        buscaAtiva || 
        generosSelecionados.length > 0 || 
        anoSelecionado || 
        notaSelecionada || 
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
                                    onClick={() => {
                                        setGenerosSelecionados([]);
                                        setPaginaAtual(1);
                                        atualizarUrl(1, buscaAtiva, [], anoSelecionado, notaSelecionada, ordenacao);
                                    }}
                                >
                                    Limpar Gêneros
                                </button>
                            </div>
                        </div>
                    )}

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
