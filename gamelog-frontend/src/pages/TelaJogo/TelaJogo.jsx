import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Navbar from '../../components/Navbar/Navbar';
import { buscarJogoPorId } from './actions/TelaJogoActions'; 
import './TelaJogo.css';

function TelaJogo() {
    const { jogoId } = useParams();
    const [jogo, setJogo] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const carregarJogo = async () => {
            setLoading(true);
            setError('');
            try {
                const data = await buscarJogoPorId(jogoId);
                setJogo(data);
            } catch (err) {
                setError(err.message);
                console.error("Erro ao carregar detalhes do jogo:", err);
            } finally {
                setLoading(false);
            }
        };

        if (jogoId) {
            carregarJogo();
        }
    }, [jogoId]);

    if (loading) {
        return (
            <div className="tela-jogo-container">
                <Navbar />
                <div className="tela-jogo-content loading-message">Carregando detalhes do jogo...</div>
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

    const dataLancamentoFormatada = jogo.dataLancamento 
        ? new Date(jogo.dataLancamento).toLocaleDateString('pt-BR', { year: 'numeric', month: 'long', day: 'numeric' }) 
        : 'Data Desconhecida';

    return (
        <div className="tela-jogo-container">
            <Navbar />
            <div className="tela-jogo-content">
                <div className="jogo-detalhes-header">
                    <img src={jogo.imagem} alt={jogo.titulo} className="jogo-detalhes-imagem" />
                    <div className="jogo-info-principal">
                        <h1 className="jogo-detalhes-titulo">{jogo.titulo}</h1>
                        <p className="jogo-detalhes-descricao">{jogo.descricao}</p>
                        <p className="jogo-detalhes-info">
                            **Lançamento:** {dataLancamentoFormatada} <br/>
                            **Classificação Indicativa:** {jogo.classificacaoIndicativa !== null ? jogo.classificacaoIndicativa : 'N/A'} <br/>
                            {jogo.mediaAvaliacoes !== null && (
                                <span>**Média de Avaliações:** ⭐ {jogo.mediaAvaliacoes.toFixed(1)}</span>
                            )}
                        </p>
                        {/* Outras informações como empresa, gêneros (futuramente) */}
                    </div>
                </div>

                {/* Seções adicionais como "Avaliações" (futuramente) */}
                <div className="jogo-avaliacoes-secao">
                    <h2>Avaliações do Jogo</h2>
                    <p>Funcionalidade de avaliações a ser implementada.</p>
                </div>
            </div>
        </div>
    );
}

export default TelaJogo;