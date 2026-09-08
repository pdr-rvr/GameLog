import api from "../../../services/api";

export const fetchUserProfile = async (userId) => {
  try {
    const response = await api.get(`/Usuarios/${userId}`);
    return response.data;
  } catch (error) {
    const errorMessage = error.response?.data?.message || "Erro ao carregar perfil.";
    console.error("Erro ao buscar perfil do usuário:", error);
    throw new Error(errorMessage);
  }
};

export const fetchUserReviews = async (userId) => {
  try {
    const response = await api.get(`/Avaliacoes/usuario/${userId}`);
    return response.data || [];
  } catch (error) {
    console.error("Erro ao buscar avaliações do usuário:", error);
    return [];
  }
};

export const fetchUserTopGenres = async (userId) => {
  try {
    const response = await api.get(`/Usuarios/${userId}/generos-favoritos`);
    return response.data || [];
  } catch (error) {
    console.error("Erro ao buscar gêneros favoritos:", error);
    return [];
  }
};

