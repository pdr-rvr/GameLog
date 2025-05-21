import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { buscarAvaliacoes, buscarJogos, criarAvaliacao } from './actions/TelaHomeActions';
import { jwtDecode } from 'jwt-decode';
import Navbar from '../../components/Navbar/Navbar';
import FormAvaliacao from '../../components/FormAvaliacao/FormAvaliacao';
import AvaliacaoCard from '../../components/AvaliacaoCard/AvaliacaoCard';
import './TelaHome.css';

function TelaHome() {
  const [activeTab, setActiveTab] = useState('home');
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [usuario, setUsuario] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }

    try {
      const decoded = jwtDecode(token);
      setUsuario(decoded);
    } catch (err) {
      console.error('Erro ao decodificar token:', err);
      navigate('/login');
    }

    carregarDados();
  }, [navigate]);

  const carregarDados = async () => {
  setLoading(true);
  try {
    const [jogosData, avaliacoesData] = await Promise.all([
      buscarJogos(),
      buscarAvaliacoes(activeTab === 'minhas' ? usuario?.userId : null)
    ]);
    
    console.log('Dados recebidos - Jogos:', jogosData);
    console.log('Dados recebidos - Avaliações:', avaliacoesData);
    
    setJogos(jogosData);
    setAvaliacoes(avaliacoesData);
  } catch (err) {
    console.error('Erro ao carregar dados:', err);
    setError('Erro ao carregar dados');
  } finally {
    setLoading(false);
  }
};

  useEffect(() => {
    if (usuario) {
      carregarDados();
    }
  }, [activeTab]);

  const handleSubmitAvaliacao = async (avaliacaoData) => {
    try {
      setLoading(true);
      await criarAvaliacao(avaliacaoData);
      await carregarDados(); 
      setShowForm(false);
      setError('');
    } catch (err) {
      setError(err.message || 'Erro ao criar avaliação');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="home-container">
      <Navbar 
        activeTab={activeTab} 
        setActiveTab={setActiveTab} 
        usuario={usuario} 
      />

      <div className="home-content">
        {/* Botão flutuante para nova avaliação */}
        {!showForm && (
          <button 
            className="floating-button"
            onClick={() => setShowForm(true)}
            aria-label="Criar nova avaliação"
          >
            +
          </button>
        )}

        {/* Formulário sobreposto quando aberto */}
        {showForm && (
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

        {/* Feed de avaliações - sempre visível */}
        <div className="avaliacoes-feed">
          {loading && <div className="loading">Carregando...</div>}
          
          {avaliacoes.length === 0 && !loading && (
            <div className="sem-avaliacoes">
              {activeTab === 'home' 
                ? 'Nenhuma avaliação encontrada' 
                : 'Você ainda não fez nenhuma avaliação'}
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