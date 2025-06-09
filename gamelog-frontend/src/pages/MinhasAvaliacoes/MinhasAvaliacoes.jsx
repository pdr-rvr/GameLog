import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { fetchUserReviews, deleteReview } from './actions/MinhasAvaliacoesActions';
import AvaliacaoCarrossel from '../../components/AvaliacaoCarrossel/AvaliacaoCarrossel';
import Navbar from '../../components/Navbar/Navbar';
import './MinhasAvaliacoes.css';

const MinhasAvaliacoes = () => {
  const navigate = useNavigate();
  const { user, loadingAuth, isAuthenticated } = useAuth();
  const [reviews, setReviews] = useState([]);
  const [pageLoading, setPageLoading] = useState(true);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [activeTab, setActiveTab] = useState('minhas'); // Mantido 'minhas' ou o valor padrão que você usa para MinhasAvaliacoes

  const loadReviews = async () => {
    setError(null);
    setSuccessMessage(null);
    setPageLoading(true);

    if (loadingAuth) {
      return; 
    }

    const token = localStorage.getItem('token');

    if (!isAuthenticated || !user?.id || !token) {
      setError('Você precisa estar logado para ver suas avaliações.');
      setPageLoading(false);
      navigate('/login');
      return;
    }
    
    try {
      const fetchedReviewsData = await fetchUserReviews(user.id, token);
      
      if (fetchedReviewsData && Array.isArray(fetchedReviewsData.$values)) {
        setReviews(fetchedReviewsData.$values);
      } else {
        console.warn("Resposta da API de avaliações não está no formato esperado ($values não é um array):", fetchedReviewsData);
        setReviews([]);
        setError("Formato de resposta inesperado da API de avaliações.");
      }
      setError(null);
    } catch (err) {
      setError(err.message || 'Ocorreu um erro ao carregar suas avaliações.');
      setReviews([]);
    } finally {
      setPageLoading(false);
    }
  };

  useEffect(() => {
    loadReviews();
  }, [loadingAuth, user, isAuthenticated, navigate]);

  // Modificação aqui: A rota agora é '/avaliacoes/editar/:reviewId'
  const handleEditReview = (reviewId) => {
    navigate(`/avaliacoes/editar/${reviewId}`); 
  };

  const handleDeleteReview = async (reviewId) => {
    if (!window.confirm('Tem certeza que deseja excluir esta avaliação?')) {
      return;
    }

    setPageLoading(true);
    setError(null);
    setSuccessMessage(null);
    const token = localStorage.getItem('token');

    try {
      await deleteReview(reviewId, token);
      setSuccessMessage('Avaliação excluída com sucesso!');
      // Usando avaliacaoId para filtrar, como no seu modelo de dados
      setReviews(prevReviews => prevReviews.filter(review => review.avaliacaoId !== reviewId)); 
    } catch (err) {
      setError(err.message || 'Erro ao excluir avaliação.');
    } finally {
      setPageLoading(false);
    }
  };

  const handleOpenPublishForm = () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    navigate('/avaliacoes/criar'); // Mantido como você já tem
  };

  if (loadingAuth || pageLoading) {
    return <div className="loading-message">Carregando suas avaliações...</div>;
  }

  if (!isAuthenticated || !user?.id) {
    return <div className="error-message">{error || 'Você não está logado para ver esta página.'}</div>;
  }

  return (
    <div className="minhas-avaliacoes-page-container">
      {/* Navbar será a mesma que você já usa */}
      <Navbar
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        usuario={user}
        onPublishClick={handleOpenPublishForm}
      />
      <div className="minhas-avaliacoes-content">
        
        {error && <div className="error-message">{error}</div>}
        {successMessage && <div className="success-message">{successMessage}</div>}

        {reviews.length === 0 ? (
          <div className="no-reviews-message">
            Você ainda não fez nenhuma avaliação. Que tal explorar alguns jogos?
            <button className="explore-games-button" onClick={() => navigate('/jogos')}>Explorar Jogos</button>
          </div>
        ) : (
          // AvaliacaoCarrossel já tem props onEditReview e onDeleteReview
          <AvaliacaoCarrossel 
            title="Suas Avaliações Recentes" 
            avaliacoes={reviews} 
            onEditReview={handleEditReview} 
            onDeleteReview={handleDeleteReview} 
          />
        )}
      </div>
    </div>
  );
};

export default MinhasAvaliacoes;