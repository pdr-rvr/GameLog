import React, { useState, useEffect, useCallback } from "react";
import { useNavigate, Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import MyAvaliacaoCard from "../../components/MyAvaliacaoCard/MyAvaliacaoCard";
import ConfirmModal from "../../components/ConfirmModal/ConfirmModal";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { fetchUserReviews, deleteReview } from "./actions/MinhasAvaliacoesActions";
import { FaGamepad } from "react-icons/fa";
import "./MinhasAvaliacoes.css";

const MinhasAvaliacoes = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [avaliacoes, setAvaliacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [itemParaExcluir, setItemParaExcluir] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const carregarMinhasAvaliacoes = useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    setError("");
    try {
      const dados = await fetchUserReviews(user.id);
      setAvaliacoes(dados || []);
    } catch (err) {
      console.error("Erro ao carregar minhas avaliações:", err);
      setError(err.message || "Não foi possível carregar suas avaliações.");
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    carregarMinhasAvaliacoes();
  }, [carregarMinhasAvaliacoes]);

  const handleEdit = (avaliacaoId) => {
    navigate(`/avaliacoes/editar/${avaliacaoId}`, { state: { from: "/minhas-avaliacoes" } });
  };

  const handleDeleteRequest = (avaliacaoId) => {
    setItemParaExcluir(avaliacaoId);
  };

  const handleConfirmDelete = async () => {
    if (!itemParaExcluir) return;
    setIsDeleting(true);
    try {
      await deleteReview(itemParaExcluir);
      setAvaliacoes((prev) => prev.filter((a) => (a.avaliacaoId || a.id) !== itemParaExcluir));
      toast.success("Avaliação excluída com sucesso!");
    } catch (err) {
      toast.error(err.message || "Erro ao excluir avaliação.");
    } finally {
      setIsDeleting(false);
      setItemParaExcluir(null);
    }
  };

  return (
    <div className="minhas-avaliacoes-page-container">
      <Navbar />
      <main className="minhas-avaliacoes-content">
        <header className="minhas-avaliacoes-header">
          <h1 className="page-title">Minhas Avaliações</h1>
          <p className="page-subtitle">Gerencie suas análises e opiniões sobre os jogos que jogou</p>
        </header>

        {loading && (
          <div className="state-message loading-state">
            <div className="loading-spinner"></div>
            <span>Carregando suas avaliações...</span>
          </div>
        )}

        {error && !loading && (
          <div className="state-message error-state">
            <span>{error}</span>
          </div>
        )}

        {!loading && !error && avaliacoes.length === 0 && (
          <div className="empty-reviews-card">
            <div className="empty-icon-badge">
              <FaGamepad />
            </div>
            <h3>Nenhuma avaliação encontrada</h3>
            <p>Você ainda não avaliou nenhum jogo na plataforma.</p>
            <Link to="/jogos" className="btn-explore-games">
              Explorar Catálogo de Jogos
            </Link>
          </div>
        )}

        {!loading && !error && avaliacoes.length > 0 && (
          <div className="minhas-avaliacoes-grid">
            {avaliacoes.map((avaliacao) => (
              <div key={avaliacao.avaliacaoId || avaliacao.id} className="minhas-avaliacoes-grid-item">
                <MyAvaliacaoCard
                  avaliacao={avaliacao}
                  onEdit={() => handleEdit(avaliacao.avaliacaoId || avaliacao.id)}
                  onDelete={() => handleDeleteRequest(avaliacao.avaliacaoId || avaliacao.id)}
                />
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Modal de Confirmação Customizado */}
      <ConfirmModal
        isOpen={Boolean(itemParaExcluir)}
        title="Excluir Avaliação"
        message="Tem certeza que deseja excluir esta avaliação? Esta ação removerá sua opinião do catálogo e não poderá ser desfeita."
        confirmText="Excluir Definitivamente"
        cancelText="Cancelar"
        confirmVariant="danger"
        loading={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={() => !isDeleting && setItemParaExcluir(null)}
      />
    </div>
  );
};

export default MinhasAvaliacoes;
