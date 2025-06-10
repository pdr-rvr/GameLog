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
  const [jogos, setJogos] = useState([]); // Este é o estado que queremos garantir que seja populado
  const [recomendacoes, setRecomendacoes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);

  const navigate = useNavigate();

  // NOVO useEffect para carregar TODOS os jogos, independente do login
  useEffect(() => {
    const loadAllGames = async () => {
      try {
        const data = await buscarTodosOsJogos();
        setJogos(data); // Popula o estado de jogos aqui
      } catch (err) {
        console.error('Erro ao carregar todos os jogos:', err);
        // Pode definir um erro específico para jogos se quiser
      }
    };
    loadAllGames();
  }, []); // Array de dependências vazio para carregar apenas uma vez ao montar

  // useEffect para carregar avaliações e recomendações (dependente do login e tab)
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

        if (isAuthenticated && user?.id && results.length > 1) { // Mudou para results.length > 1, pois jogos não está mais aqui
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

    // Só carrega conteúdo autenticado se não estiver carregando a autenticação e houver uma mudança relevante
    if (!loadingAuth) {
        carregarConteudoAutenticado();
    }
  }, [activeTab, user, isAuthenticated, loadingAuth]); // Mantenha as dependências para o conteúdo do usuário

  const handleSubmitAvaliacao = async (avaliacaoData) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    setLoading(true);
    setError('');
    try {
      await criarAvaliacao(avaliacaoData);
      // Após criar avaliação, recarregue apenas as avaliações (e talvez recomendações se for o caso)
      // Não precisa recarregar todos os jogos novamente
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
              jogos={jogos} // 'jogos' virá agora do useEffect que carrega apenas jogos
              onSubmit={handleSubmitAvaliacao}
              loading={loading}
              error={error}
              onCancel={() => setShowForm(false)}
              initialData={newInitialData}
              isEditing={false}
            />
          </div>
        )}

        {/* Renderiza o JogosCarrossel se houver jogos */}
        {jogos.length > 0 && (
          <JogosCarrossel title="Jogos em Destaque" jogos={jogos} />
        )}
        {/* Adicione uma mensagem se não houver jogos e não estiver carregando */}
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
          <AvaliacoesCarrossel
            title="Últimas Avaliações"
            avaliacoes={avaliacoes}
            currentUserId={user?.id}
            onEditReview={() => { /* Implementar edição */ }} // Placeholder para edição
            onDeleteReview={() => { /* Implementar exclusão */ }} // Placeholder para exclusão
          />
        )}

      </div>
    </div>
  );
}

export default TelaHome;