import React, { useState, useEffect } from 'react';
import Navbar from '../../components/Navbar/Navbar';
import AvaliacaoCard from '../../components/AvaliacaoCard/AvaliacaoCard';
//import { buscarAvaliacoes } from '../../services/AvaliacoesService';
import './PaginaAvaliacoes.css';

const PaginaAvaliacoes = () => {
  /*const [avaliacoes, setAvaliacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchAvaliacoes = async () => {
      setLoading(true);
      try {
        const data = await buscarAvaliacoes();
        setAvaliacoes(data);
      } catch (err) {
        console.error("Erro ao buscar avaliações:", err);
        setError("Não foi possível carregar as avaliações.");
      } finally {
        setLoading(false);
      }
    };
    fetchAvaliacoes();
  }, []);

  return (
    <div className="page-container">
      <Navbar />
      <div className="content-area" style={{ padding: '20px' }}>
        <h2>Todas as Avaliações</h2>
        {loading && <div className="loading">Carregando avaliações...</div>}
        {error && <div className="error-message">{error}</div>}
        {!loading && avaliacoes.length === 0 && (
          <div className="no-data-message">Nenhuma avaliação encontrada.</div>
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

export default PaginaAvaliacoes;