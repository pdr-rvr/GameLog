import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import MyAvaliacaoCard from "../../components/MyAvaliacaoCard/MyAvaliacaoCard";
import { useAuth } from "../../context/AuthContext";
import { fetchUserReviews, deleteReview } from "./actions/MinhasAvaliacoesActions";
import "./MinhasAvaliacoes.css";

const MinhasAvaliacoes = () => {
  const { user } = useAuth();
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const carregarMinhasAvaliacoes = async () => {
    if (!user?.id) return;
    setLoading(true);
    setError("");
    try {
      const dados = await fetchUserReviews(user.id);
      setAvaliacoes(dados);
    } catch (err) {
      console.error("Erro ao carregar minhas avaliações:", err);
      setError(err.message || "Não foi possível carregar suas avaliações.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarMinhasAvaliacoes();
  }, [user]);

  const handleEdit = (avaliacaoId) => {
    navigate(`/avaliacoes/editar/${avaliacaoId}`);
  };

  const handleDelete = async (avaliacaoId) => {
    if (window.confirm("Tem certeza que deseja excluir esta avaliação?")) {
      try {
        await deleteReview(avaliacaoId);
        setAvaliacoes(prev => prev.filter(a => a.avaliacaoId !== avaliacaoId));
      } catch (err) {
        alert(err.message || "Erro ao excluir avaliação.");
      }
    }
  };

  return (
    <div className="minhas-avaliacoes-container">
      <Navbar />
      <div className="minhas-avaliacoes-content">
        <h1>Minhas Avaliações</h1>

        {loading && <div className="loading-message">Carregando suas avaliações...</div>}
        {error && <div className="error-message">{error}</div>}

        {!loading && !error && avaliacoes.length === 0 && (
          <div className="no-avaliations-message">
            Você ainda não publicou nenhuma avaliação.
          </div>
        )}

        {!loading && !error && avaliacoes.length > 0 && (
          <div className="avaliacoes-grid">
            {avaliacoes.map(avaliacao => (
              <MyAvaliacaoCard
                key={avaliacao.avaliacaoId}
                avaliacao={avaliacao}
                onEdit={handleEdit}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default MinhasAvaliacoes;
