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
      const [avaliacoesData, jogosData] = await Promise.all([
        buscarAvaliacoes(),
        buscarJogos()
      ]);
      
      console.log('Jogos recebidos:', jogosData);
      
      setAvaliacoes(Array.isArray(avaliacoesData) ? avaliacoesData : []);
      setJogos(Array.isArray(jogosData) ? jogosData : []);
    } catch (err) {
      console.error('Erro ao carregar dados:', err);
      setError('Erro ao carregar dados');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitAvaliacao = async (avaliacaoData) => {
    if (!avaliacaoData.jogoId || !avaliacaoData.textoAvaliacao) {
      setError('Preencha todos os campos');
      return;
    }

    try {
      setLoading(true);
      await criarAvaliacao(avaliacaoData);
      await carregarDados();
      setError('');
    } catch (err) {
      setError(err.message || 'Erro ao criar avaliação');
    } finally {
      setLoading(false);
    }
  };

  const avaliacoesFiltradas = Array.isArray(avaliacoes) 
    ? (activeTab === 'home' 
        ? avaliacoes 
        : avaliacoes.filter(av => av.usuarioId === usuario?.userId))
    : [];

  return (
    <div className="home-container">
      <Navbar 
        activeTab={activeTab} 
        setActiveTab={setActiveTab} 
        usuario={usuario} 
      />

      <div className="home-content">
        <FormAvaliacao 
          jogos={jogos} 
          onSubmit={handleSubmitAvaliacao} 
          loading={loading}
          error={error}
        />

        <div className="avaliacoes-feed">
          {loading && <div className="loading">Carregando...</div>}
          
          {avaliacoesFiltradas.length === 0 && !loading && (
            <div className="sem-avaliacoes">
              {activeTab === 'home' 
                ? 'Nenhuma avaliação encontrada' 
                : 'Você ainda não fez nenhuma avaliação'}
            </div>
          )}

          {avaliacoesFiltradas.map(avaliacao => {
            const jogo = Array.isArray(jogos) 
              ? jogos.find(j => j.jogoId === avaliacao.jogoId)
              : null;
              
            return (
              <AvaliacaoCard 
                key={avaliacao.avaliacaoId || avaliacao.id} 
                avaliacao={avaliacao} 
                jogo={jogo} 
              />
            );
          })}
        </div>
      </div>
    </div>
  );
}

export default TelaHome;