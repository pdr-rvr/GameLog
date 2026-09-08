import api from "../../../services/api";

export const fetchUserProfile = async (userId) => {
  try {
    const response = await api.get(`/Usuarios/${userId}`);
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || "Erro ao carregar dados do usuário.");
    }
    throw new Error("Erro de conexão com o servidor.");
  }
};

export const updateUserProfile = async (userId, data) => {
  try {
    const response = await api.put(`/Usuarios/${userId}`, data);
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || "Erro ao atualizar perfil.");
    }
    throw new Error("Erro de conexão com o servidor.");
  }
};
