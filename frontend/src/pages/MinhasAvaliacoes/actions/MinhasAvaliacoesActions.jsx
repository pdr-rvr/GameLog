import api from "../../../services/api";

export const fetchUserReviews = async (userId) => {
  try {
    const response = await api.get(`/Avaliacoes/usuario/${userId}`);
    const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    return dados;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || `Erro ao carregar avaliações: ${error.response.statusText}`);
    } else if (error.request) {
      throw new Error("Erro de rede: Nenhuma resposta do servidor.");
    } else {
      throw new Error("Erro ao configurar a requisição para carregar avaliações.");
    }
  }
};

export const deleteReview = async (reviewId) => {
  try {
    const response = await api.delete(`/Avaliacoes/${reviewId}`);
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || `Erro ao excluir avaliação: ${error.response.statusText}`);
    } else if (error.request) {
      throw new Error("Erro de rede: Nenhuma resposta do servidor.");
    } else {
      throw new Error("Erro ao configurar a requisição para excluir avaliação.");
    }
  }
};
