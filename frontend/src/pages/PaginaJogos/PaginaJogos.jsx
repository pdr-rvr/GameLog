import React, { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar/Navbar';
import JogosCarrossel from '../../components/JogosCarrossel/JogosCarrossel';
import FiltroDropdown from '../../components/FiltroDropdown/FiltroDropdown';
import SearchBar from '../../components/SearchBar/SearchBar';
import JogoCard from '../../components/JogoCard/JogoCard';
import { buscarJogos } from './actions/PaginaJogosActions';
import './PaginaJogos.css';

function PaginaJogos() {
    const [todosJogos, setTodosJogos] = useState([]);
    const [jogosFiltrados, setJogosFiltrados] = useState([]);
    const [jogosCarrossel, setJogosCarrossel] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const [filtroAno, setFiltroAno] = useState('');
    const [filtroRating, setFiltroRating] = useState('');
    const [filtroGenero, setFiltroGenero] = useState('');
    const [termoPesquisa, setTermoPesquisa] = useState('');
    const [sugestoesPesquisa, setSugestoesPesquisa] = useState([]);

    const generosOptions = [
        { label: 'Ação', value: 'acao' },
        { label: 'Aventura', value: 'aventura' },
        { label: 'RPG', value: 'rpg' },
        { label: 'Esportes', value: 'esportes' },
        { label: 'Estratégia', value: 'estrategia' },
        { label: 'Simulação', value: 'simulacao' },
        { label: 'Terror', value: 'terror' },
        { label: 'Puzzle', value: 'puzzle' },
        { label: 'Corrida', value: 'corrida' },
        { label: 'Luta', value: 'luta' },
    ];

    useEffect(() => {
        const carregarTodosOsJogos = async () => {
            setLoading(true);
            setError('');
            try {
                const data = await buscarJogos(); 
                setTodosJogos(data);
                setJogosFiltrados(data); 

                const shuffled = [...data].sort(() => 0.5 - Math.random());
                setJogosCarrossel(shuffled.slice(0, 10));

            } catch (err) {
                setError(err.message);
                console.error("Erro ao carregar todos os jogos:", err);
            } finally {
                setLoading(false);
            }
        };

        carregarTodosOsJogos();
    }, []);

    useEffect(() => {
        let jogosAtuais = [...todosJogos];

        // Aplicar filtro de ano
        if (filtroAno) {
            jogosAtuais = jogosAtuais.filter(jogo => {
                const anoLancamento = new Date(jogo.dataLancamento).getFullYear();
                if (filtroAno.includes('s (')) { 
                    const startYear = parseInt(filtroAno.substring(0, 4));
                    const endYear = startYear + 9;
                    return anoLancamento >= startYear && anoLancamento <= endYear;
                } else if (filtroAno === 'Pre-1970') {
                    return anoLancamento < 1970;
                } else {
                    return anoLancamento === parseInt(filtroAno);
                }
            });
        }

        // Aplicar filtro de rating (logica a ser implementada com dados reais)
        if (filtroRating) {
            // Por enquanto, apenas um placeholder para rating
            // if (filtroRating === 'popular') { ... }
        }

        // Aplicar filtro de gênero (logica a ser implementada com dados reais)
        if (filtroGenero) {
             // if (jogo.generos.includes(filtroGenero)) { ... }
        }

        if (termoPesquisa) {
            const termoLowerCase = termoPesquisa.toLowerCase();
            jogosAtuais = jogosAtuais.filter(jogo => 
                jogo.titulo.toLowerCase().includes(termoLowerCase)
            );
        }

        setJogosFiltrados(jogosAtuais);
    }, [filtroAno, filtroRating, filtroGenero, termoPesquisa, todosJogos]);

    const handleSearch = (query) => {
        setTermoPesquisa(query);
        if (query.length > 0) {
            const filteredSuggestions = todosJogos.filter(jogo => 
                jogo.titulo.toLowerCase().includes(query.toLowerCase())
            );
            setSugestoesPesquisa(filteredSuggestions.slice(0, 5)); 
        } else {
            setSugestoesPesquisa([]);
        }
    };

    const handleSelectSuggestion = (jogo) => {
        setTermoPesquisa(jogo.titulo);
        setJogosFiltrados([jogo]); 
        setSugestoesPesquisa([]); 
        setFiltroAno('');
        setFiltroRating('');
        setFiltroGenero('');
    };

    return (
        <div className="pagina-jogos-container">
            <Navbar />
            <div className="pagina-jogos-content">
                <div className="filtro-pesquisa-bar">
                    {/* <FiltroDropdown 
                        title="Ano" 
                        type="year-range"
                        currentSelection={filtroAno}
                        onSelect={setFiltroAno} 
                    />
                    <FiltroDropdown 
                        title="Rating" 
                        type="rating"
                        currentSelection={filtroRating}
                        onSelect={setFiltroRating} 
                    />
                    <FiltroDropdown 
                        title="Gêneros" 
                        type="list" 
                        options={generosOptions} 
                        currentSelection={generosOptions.find(opt => opt.value === filtroGenero)?.label || ''}
                        onSelect={setFiltroGenero} 
                    /> */}
                    <SearchBar 
                        onSearch={handleSearch} 
                        suggestions={sugestoesPesquisa} 
                        onSelectSuggestion={handleSelectSuggestion}
                    />
                </div>

                {loading && <div className="loading-message">Carregando jogos...</div>}
                {error && <div className="error-message">{error}</div>}

                {!loading && !error && jogosCarrossel.length > 0 && termoPesquisa === '' && (
                    <JogosCarrossel 
                        title="Destaques da Semana" 
                        jogos={jogosCarrossel} 
                    />
                )}

                {!loading && !error && (
                    <div className="jogos-grid">
                        {jogosFiltrados.length === 0 && termoPesquisa !== '' ? (
                            <p className="no-results">Nenhum jogo encontrado para "{termoPesquisa}".</p>
                        ) : jogosFiltrados.length === 0 && termoPesquisa === '' ? (
                             <p className="no-results">Nenhum jogo disponível.</p>
                        ) : (
                            jogosFiltrados.map(jogo => (
                                <JogoCard key={jogo.jogoId} jogo={jogo} />
                            ))
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}

export default PaginaJogos;