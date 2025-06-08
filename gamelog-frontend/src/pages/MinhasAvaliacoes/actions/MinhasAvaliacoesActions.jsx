import api from '../../../services/api';

const API_BASE_URL = 'http://localhost:7096/api';

export const fetchUserReviews = async (userId, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    const response = await api.get(`/Avaliacoes/usuario/${userId}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || `Erro ao carregar avaliações: ${error.response.statusText}`);
    } else if (error.request) {
      throw new Error('Erro de rede: Nenhuma resposta do servidor.');
    } else {
      throw new Error('Erro ao configurar a requisição para carregar avaliações.');
    }
  }
};

export const deleteReview = async (reviewId, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    const response = await api.delete(`/Avaliacoes/${reviewId}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || `Erro ao excluir avaliação: ${error.response.statusText}`);
    } else if (error.request) {
      throw new Error('Erro de rede: Nenhuma resposta do servidor.');
    } else {
      throw new Error('Erro ao configurar a requisição para excluir avaliação.');
    }
  }
};