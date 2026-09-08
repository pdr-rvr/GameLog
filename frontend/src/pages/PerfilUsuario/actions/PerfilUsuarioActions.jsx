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

export const updateUserProfile = async (userId, userData) => {
  try {
    const response = await api.put(`/Usuarios/${userId}`, userData);
    return response.data;
  } catch (error) {
    const errorMessage = error.response?.data?.message || "Erro ao atualizar perfil.";
    console.error("Erro ao atualizar perfil do usuário:", error);
    throw new Error(errorMessage);
  }
};
