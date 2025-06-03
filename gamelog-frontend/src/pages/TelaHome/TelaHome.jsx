import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { buscarAvaliacoes, buscarJogos, criarAvaliacao } from './actions/TelaHomeActions';
import { jwtDecode } from 'jwt-decode';
import Navbar from '../../components/Navbar/Navbar';
import FormAvaliacao from '../../components/FormAvaliacao/FormAvaliacao';
import AvaliacaoCard from '../../components/AvaliacaoCard/AvaliacaoCard';
import './TelaHome.css';
import { useAuth } from '../../context/AuthContext';

function TelaHome() {
    const { user, isAuthenticated, loadingAuth } = useAuth();
    const [activeTab, setActiveTab] = useState('home'); 
    const [avaliacoes, setAvaliacoes] = useState([]);
    const [jogos, setJogos] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [showForm, setShowForm] = useState(false); 

    const navigate = useNavigate();

    useEffect(() => {
        carregarDados();
    }, [activeTab, user, isAuthenticated]);

    const carregarDados = async () => {
        setLoading(true);
        setError('');
        try {
            const [jogosData, avaliacoesData] = await Promise.all([
                buscarJogos(),
                buscarAvaliacoes(activeTab === 'minhas' && isAuthenticated && user ? user.id : null) // Usando a action importada
            ]);

            setJogos(jogosData);
            setAvaliacoes(avaliacoesData);
        } catch (err) {
            console.error('Erro ao carregar dados:', err);
            setError('Erro ao carregar dados. Tente novamente mais tarde.');
        } finally {
            setLoading(false);
        }
    };

    const handleSubmitAvaliacao = async (avaliacaoData) => {
        if (!isAuthenticated) {
            navigate('/login'); 
            return;
        }
        setLoading(true);
        setError('');
        try {
            await criarAvaliacao(avaliacaoData);
            await carregarDados(); 
            setShowForm(false); 
            setError('');
        } catch (err) {
            setError(err.message || 'Erro ao criar avaliação.');
        } finally {
            setLoading(false);
        }
    };

    const handleOpenPublishForm = () => {
        if (!isAuthenticated) {
            navigate('/login'); 
            return;
        }
        setShowForm(true);
    };

    return (
        <div className="home-container">
            <Navbar 
                activeTab={activeTab} 
                setActiveTab={setActiveTab} 
                usuario={user}
                onPublishClick={handleOpenPublishForm} 
            />

            <div className="home-content">
                {isAuthenticated && !showForm && (
                    <button 
                        className="floating-button"
                        onClick={handleOpenPublishForm} 
                        aria-label="Criar nova avaliação"
                    >
                        +
                    </button>
                )}

                {showForm && isAuthenticated && (
                    <div className="form-overlay">
                        <FormAvaliacao 
                            jogos={jogos} 
                            onSubmit={handleSubmitAvaliacao} 
                            loading={loading}
                            error={error}
                            onCancel={() => setShowForm(false)} 
                        />
                    </div>
                )}

                <div className="avaliacoes-feed">
                    {loading && <div className="loading">Carregando avaliações...</div>}
                    
                    {avaliacoes.length === 0 && !loading && (
                        <div className="sem-avaliacoes">
                            Nenhuma avaliação encontrada.
                        </div>
                    )}

                    {avaliacoes.map(avaliacao => (
                        <AvaliacaoCard 
                            key={avaliacao.avaliacaoId} 
                            avaliacao={avaliacao} 
                        />
                    ))}
                </div>
            </div>
        </div>
    );
}

export default TelaHome;