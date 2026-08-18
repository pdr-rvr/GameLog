import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { buscarAvaliacoes, criarAvaliacao, buscarRecomendacoes } from './actions/TelaHomeActions';
import { buscarJogos as buscarTodosOsJogos } from '../PaginaJogos/actions/PaginaJogosActions'; // Importa do local correto e renomeia

import Navbar from '../../components/Navbar/Navbar';
import FormAvaliacao from '../../components/FormAvaliacao/FormAvaliacao';
import JogosCarrossel from '../../components/JogosCarrossel/JogosCarrossel';
import AvaliacoesCarrossel from '../../components/AvaliacaoCarrossel/AvaliacaoCarrossel';
import RecomendacoesCarrossel from '../../components/RecomendacoesCarrossel/RecomendacoesCarrossel';
import './TelaHome.css';
import { useAuth } from '../../context/AuthContext';

function TelaHome() {
  const { user, isAuthenticated, loadingAuth } = useAuth();
  const [activeTab, setActiveTab] = useState('home');
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [recomendacoes, setRecomendacoes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    const loadAllGames = async () => {
      try {
        const data = await buscarTodosOsJogos();
        setJogos(data);
      } catch (err) {
        console.error('Erro ao carregar todos os jogos:', err);
      }
    };
    loadAllGames();
  }, []);

  useEffect(() => {
    const carregarConteudoAutenticado = async () => {
      setLoading(true);
      setError('');
      try {
        const promises = [
          buscarAvaliacoes(activeTab === 'minhas' && isAuthenticated && user ? user.id : null)
        ];

        if (isAuthenticated && user?.id) {
          promises.push(buscarRecomendacoes(user.id));
        }

        const results = await Promise.all(promises);

        setAvaliacoes(results[0]);

        if (isAuthenticated && user?.id && results.length > 1) { 
          setRecomendacoes(results[1]);
        } else {
          setRecomendacoes([]);
        }

      } catch (err) {
        console.error('Erro ao carregar dados de usuário/avaliações:', err);
        setError('Erro ao carregar dados. Tente novamente mais tarde.');
      } finally {
        setLoading(false);
      }
    };

    if (!loadingAuth) {
        carregarConteudoAutenticado();
    }
  }, [activeTab, user, isAuthenticated, loadingAuth]); 

  const handleSubmitAvaliacao = async (avaliacaoData) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    setLoading(true);
    setError('');
    try {
      await criarAvaliacao(avaliacaoData);
      const updatedAvaliacoes = await buscarAvaliacoes(activeTab === 'minhas' && isAuthenticated && user ? user.id : null);
      setAvaliacoes(updatedAvaliacoes);

      if (isAuthenticated && user?.id) {
        const updatedRecomendacoes = await buscarRecomendacoes(user.id);
        setRecomendacoes(updatedRecomendacoes);
      }

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

  const newInitialData = useMemo(() => ({}), []);

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
              initialData={newInitialData}
              isEditing={false}
            />
          </div>
        )}

        {jogos.length > 0 && (
          <JogosCarrossel title="Jogos em Destaque" jogos={jogos} />
        )}
        {!loading && jogos.length === 0 && (
            <div className="sem-jogos">Nenhum jogo em destaque disponível.</div>
        )}


        {isAuthenticated && user?.id && loadingAuth === false && !loading && recomendacoes.length > 0 && (
          <RecomendacoesCarrossel title="Recomendações para você" recomendacoes={recomendacoes} />
        )}
        {isAuthenticated && user?.id && loadingAuth === false && !loading && recomendacoes.length === 0 && (
            <div className="sem-recomendacoes">
                Não há recomendações disponíveis no momento. Jogue mais e avalie para receber recomendações!
            </div>
        )}

        {loading && <div className="loading">Carregando avaliações...</div>}
        {!loading && avaliacoes.length === 0 && (
          <div className="sem-avaliacoes">
            Nenhuma avaliação encontrada.
          </div>
        )}
        {!loading && avaliacoes.length > 0 && (
          <AvaliacoesCarrossel title="Últimas Avaliações" avaliacoes={avaliacoes} />
        )}

      </div>
    </div>
  );
}

export default TelaHome;