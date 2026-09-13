import api from "./api";

export const UsuarioService = {
  async obterPerfil(userId) {
    const response = await api.get(`/Usuarios/${userId}`);
    return response.data;
  },

  async atualizarPerfil(userId, data) {
    const response = await api.put(`/Usuarios/${userId}`, data);
    return response.data;
  },

  async obterGenerosFavoritos(userId) {
    try {
      const response = await api.get(`/Usuarios/${userId}/generos-favoritos`);
      return response.data || [];
    } catch {
      return [];
    }
  },

  async obterRecomendacoes(userId) {
    try {
      const response = await api.get(`/Usuarios/${userId}/recomendacoes`);
      return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    } catch {
      return [];
    }
  }
};

export default UsuarioService;
