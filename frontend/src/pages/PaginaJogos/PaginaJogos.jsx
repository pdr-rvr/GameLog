import React, { useState, useEffect, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import { buscarJogos } from "./actions/PaginaJogosActions";
import { 
  FaGamepad, 
  FaSearch, 
  FaCalendarAlt, 
  FaBuilding, 
  FaSortAmountDown, 
  FaTimes, 
  FaLayerGroup 
} from "react-icons/fa";
import "./PaginaJogos.css";

function PaginaJogos() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [todosJogos, setTodosJogos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Filter states
    const [termoPesquisa, setTermoPesquisa] = useState(searchParams.get("q") || "");
    const [generoSelecionado, setGeneroSelecionado] = useState("");
    const [anoSelecionado, setAnoSelecionado] = useState("");
    const [empresaSelecionada, setEmpresaSelecionada] = useState("");
    const [ordenacao, setOrdenacao] = useState("melhores");

    // Dynamic filter options
    const [generosDisponiveis, setGenerosDisponiveis] = useState([]);
    const [anosDisponiveis, setAnosDisponiveis] = useState([]);
    const [empresasDisponiveis, setEmpresasDisponiveis] = useState([]);

    useEffect(() => {
        const queryFromUrl = searchParams.get("q");
        if (queryFromUrl !== null && queryFromUrl !== termoPesquisa) {
            setTermoPesquisa(queryFromUrl);
        }
    }, [searchParams]);

    useEffect(() => {
        const carregarJogos = async () => {
            setLoading(true);
            setError("");
            try {
                const dados = await buscarJogos();
                setTodosJogos(dados || []);

                const generosSet = new Set();
                const anosSet = new Set();
                const empresasSet = new Set();

                (dados || []).forEach(jogo => {
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
            } catch (err) {
                console.error("Erro ao carregar jogos:", err);
                setError(err.message || "Não foi possível carregar a lista de jogos.");
            } finally {
                setLoading(false);
            }
        };

        carregarJogos();
    }, []);

    // Filter and Sort memoized calculation
    const jogosFiltrados = useMemo(() => {
        let resultado = [...todosJogos];

        // 1. Text Search Filter
        if (termoPesquisa.trim() !== "") {
            const termoLower = termoPesquisa.toLowerCase().trim();
            resultado = resultado.filter(jogo =>
                (jogo.titulo || "").toLowerCase().includes(termoLower) ||
                (jogo.descricao || "").toLowerCase().includes(termoLower) ||
                (jogo.nomeEmpresa || "").toLowerCase().includes(termoLower)
            );
        }

        // 2. Genre Filter
        if (generoSelecionado !== "") {
            resultado = resultado.filter(jogo =>
                jogo.generos && Array.isArray(jogo.generos) && jogo.generos.includes(generoSelecionado)
            );
        }

        // 3. Year Filter
        if (anoSelecionado !== "") {
            resultado = resultado.filter(jogo =>
                jogo.dataLancamento && jogo.dataLancamento.toString().startsWith(anoSelecionado)
            );
        }

        // 4. Company Filter
        if (empresaSelecionada !== "") {
            resultado = resultado.filter(jogo =>
                jogo.nomeEmpresa === empresaSelecionada
            );
        }

        // 5. Sorting
        resultado.sort((a, b) => {
            if (ordenacao === "melhores") {
                const notaA = Number(a.mediaAvaliacoes) || 0;
                const notaB = Number(b.mediaAvaliacoes) || 0;
                return notaB - notaA;
            }
            if (ordenacao === "recentes") {
                const dataA = a.dataLancamento ? new Date(a.dataLancamento) : new Date(0);
                const dataB = b.dataLancamento ? new Date(b.dataLancamento) : new Date(0);
                return dataB - dataA;
            }
            if (ordenacao === "antigos") {
                const dataA = a.dataLancamento ? new Date(a.dataLancamento) : new Date(0);
                const dataB = b.dataLancamento ? new Date(b.dataLancamento) : new Date(0);
                return dataA - dataB;
            }
            if (ordenacao === "az") {
                return (a.titulo || "").localeCompare(b.titulo || "");
            }
            if (ordenacao === "za") {
                return (b.titulo || "").localeCompare(a.titulo || "");
            }
            return 0;
        });

        return resultado;
    }, [todosJogos, termoPesquisa, generoSelecionado, anoSelecionado, empresaSelecionada, ordenacao]);

    const handleSearchChange = (e) => {
        const val = e.target.value;
        setTermoPesquisa(val);
        if (val) {
            setSearchParams({ q: val });
        } else {
            setSearchParams({});
        }
    };

    const limparFiltros = () => {
        setTermoPesquisa("");
        setGeneroSelecionado("");
        setAnoSelecionado("");
        setEmpresaSelecionada("");
        setOrdenacao("melhores");
        setSearchParams({});
    };

    const temFiltrosAtivos = termoPesquisa !== "" || generoSelecionado !== "" || anoSelecionado !== "" || empresaSelecionada !== "" || ordenacao !== "melhores";

    return (
        <div className="pagina-jogos-container">
            <Navbar />
            <div className="pagina-jogos-content">
                {/* Header Banner */}
                <header className="pagina-jogos-header">
                    <div className="catalogo-badge">
                        <FaLayerGroup /> Catálogo GameLog
                    </div>
                    <h1 className="pagina-jogos-titulo">Explorar Jogos</h1>
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
                                onClick={() => {
                                    setTermoPesquisa("");
                                    setSearchParams({});
                                }}
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
                                onChange={(e) => setGeneroSelecionado(e.target.value)}
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
                                onChange={(e) => setAnoSelecionado(e.target.value)}
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
                                onChange={(e) => setEmpresaSelecionada(e.target.value)}
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
                                onChange={(e) => setOrdenacao(e.target.value)}
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
                            <strong>{jogosFiltrados.length}</strong> {jogosFiltrados.length === 1 ? "jogo encontrado" : "jogos encontrados"}
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

                {!loading && !error && jogosFiltrados.length === 0 && (
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
                {!loading && !error && jogosFiltrados.length > 0 && (
                    <div className="jogos-grid-modern">
                        {jogosFiltrados.map(jogo => (
                            <JogoCard key={jogo.jogoId || jogo.id} jogo={jogo} />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}

export default PaginaJogos;
