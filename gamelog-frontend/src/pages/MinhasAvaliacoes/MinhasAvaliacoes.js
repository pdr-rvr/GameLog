import React, { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar/Navbar';
import AvaliacaoCard from '../../components/AvaliacaoCard/AvaliacaoCard';
//import { buscarAvaliacoes } from '../../services/AvaliacoesService'; 
import { useAuth } from '../../context/AuthContext'; 
import './MinhasAvaliacoes.css'; 

const MinhasAvaliacoes = () => {
  /*const { user } = useAuth();
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchMinhasAvaliacoes = async () => {
      if (!user?.id) {
        setError("Usuário não logado ou ID não encontrado.");
        setLoading(false);
        return;
      }

      setLoading(true);
      try {
        const data = await buscarAvaliacoes(user.id); 
        setAvaliacoes(data);
      } catch (err) {
        console.error("Erro ao buscar minhas avaliações:", err);
        setError("Não foi possível carregar suas avaliações.");
      } finally {
        setLoading(false);
      }
    };
    fetchMinhasAvaliacoes();
  }, [user]);

  return (
    <div className="page-container">
      <Navbar />
      <div className="content-area" style={{ padding: '20px' }}>
        <h2>Minhas Avaliações</h2>
        {loading && <div className="loading">Carregando suas avaliações...</div>}
        {error && <div className="error-message">{error}</div>}
        {!loading && avaliacoes.length === 0 && (
          <div className="no-data-message">Você ainda não publicou nenhuma avaliação.</div>
        )}
        <div className="avaliacoes-grid">
          {avaliacoes.map(avaliacao => (
            <AvaliacaoCard key={avaliacao.avaliacaoId} avaliacao={avaliacao} />
          ))}
        </div>
      </div>
    </div>
  );*/
};

export default MinhasAvaliacoes;