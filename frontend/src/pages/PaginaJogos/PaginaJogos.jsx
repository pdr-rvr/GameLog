import React, { useState, useEffect } from "react";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import { buscarJogos } from "./actions/PaginaJogosActions";
import "./PaginaJogos.css";

function PaginaJogos() {
    const [todosJogos, setTodosJogos] = useState([]);
    const [jogosFiltrados, setJogosFiltrados] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [termoPesquisa, setTermoPesquisa] = useState("");
    const [generoSelecionado, setGeneroSelecionado] = useState("");
    const [anoSelecionado, setAnoSelecionado] = useState("");

    const [generosDisponiveis, setGenerosDisponiveis] = useState([]);
    const [anosDisponiveis, setAnosDisponiveis] = useState([]);

    useEffect(() => {
        const carregarJogos = async () => {
            setLoading(true);
            setError("");
            try {
                const dados = await buscarJogos();
                setTodosJogos(dados);
                setJogosFiltrados(dados);

                const generosSet = new Set();
                const anosSet = new Set();

                dados.forEach(jogo => {
                    if (jogo.generos && Array.isArray(jogo.generos)) {
                        jogo.generos.forEach(g => generosSet.add(g));
                    }
                    if (jogo.dataLancamento) {
                        const ano = jogo.dataLancamento.toString().substring(0, 4);
                        if (ano) anosSet.add(ano);
                    }
                });

                setGenerosDisponiveis(Array.from(generosSet).sort());
                setAnosDisponiveis(Array.from(anosSet).sort((a, b) => b - a));
            } catch (err) {
                console.error("Erro ao carregar jogos:", err);
                setError(err.message || "Não foi possível carregar a lista de jogos.");
            } finally {
                setLoading(false);
            }
        };

        carregarJogos();
    }, []);

    useEffect(() => {
        let resultado = todosJogos;

        if (termoPesquisa.trim() !== "") {
            const termoLower = termoPesquisa.toLowerCase();
            resultado = resultado.filter(jogo =>
                jogo.titulo?.toLowerCase().includes(termoLower) ||
                jogo.descricao?.toLowerCase().includes(termoLower) ||
                jogo.nomeEmpresa?.toLowerCase().includes(termoLower)
            );
        }

        if (generoSelecionado !== "") {
            resultado = resultado.filter(jogo =>
                jogo.generos && jogo.generos.includes(generoSelecionado)
            );
        }

        if (anoSelecionado !== "") {
            resultado = resultado.filter(jogo =>
                jogo.dataLancamento && jogo.dataLancamento.toString().startsWith(anoSelecionado)
            );
        }

        setJogosFiltrados(resultado);
    }, [termoPesquisa, generoSelecionado, anoSelecionado, todosJogos]);

    const limparFiltros = () => {
        setTermoPesquisa("");
        setGeneroSelecionado("");
        setAnoSelecionado("");
    };

    return (
        <div className="pagina-jogos-container">
            <Navbar />
            <div className="pagina-jogos-content">
                <h1 className="pagina-jogos-titulo">Catálogo Completo de Jogos</h1>

                <div className="filtros-secao">
                    <input
                        type="text"
                        placeholder="Buscar por título, descrição ou empresa..."
                        value={termoPesquisa}
                        onChange={(e) => setTermoPesquisa(e.target.value)}
                        className="filtro-input filtro-busca"
                    />

                    <select
                        value={generoSelecionado}
                        onChange={(e) => setGeneroSelecionado(e.target.value)}
                        className="filtro-select"
                    >
                        <option value="">Todos os Gêneros</option>
                        {generosDisponiveis.map(genero => (
                            <option key={genero} value={genero}>{genero}</option>
                        ))}
                    </select>

                    <select
                        value={anoSelecionado}
                        onChange={(e) => setAnoSelecionado(e.target.value)}
                        className="filtro-select"
                    >
                        <option value="">Todos os Anos</option>
                        {anosDisponiveis.map(ano => (
                            <option key={ano} value={ano}>{ano}</option>
                        ))}
                    </select>

                    {(termoPesquisa || generoSelecionado || anoSelecionado) && (
                        <button onClick={limparFiltros} className="btn-limpar-filtros">
                            Limpar Filtros
                        </button>
                    )}
                </div>

                {loading && (
                    <div className="loading-message">Carregando catálogo de jogos...</div>
                )}

                {error && (
                    <div className="error-message">{error}</div>
                )}

                {!loading && !error && jogosFiltrados.length === 0 && (
                    <div className="no-results">Nenhum jogo encontrado com os filtros selecionados.</div>
                )}

                {!loading && !error && jogosFiltrados.length > 0 && (
                    <div className="jogos-grid">
                        {jogosFiltrados.map(jogo => (
                            <JogoCard key={jogo.jogoId} jogo={jogo} />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}

export default PaginaJogos;
