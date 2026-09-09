import api from "./api";

export const SocialService = {
  // Alternar Seguir / Deixar de Seguir
  async alternarSeguir(usuarioId) {
    const response = await api.post(`/usuarios/${usuarioId}/seguir`);
    return response.data;
  },

  // Verificar se o usuário autenticado segue determinado usuário
  async verificarStatusSeguir(usuarioId) {
    const response = await api.get(`/usuarios/${usuarioId}/status-seguir`);
    return response.data;
  },

  // Obter estatísticas sociais (total seguidores e total seguindo)
  async obterEstatisticasSociais(usuarioId) {
    const response = await api.get(`/usuarios/${usuarioId}/estatisticas-sociais`);
    return response.data;
  },

  // Obter lista de seguidores
  async obterSeguidores(usuarioId) {
    const response = await api.get(`/usuarios/${usuarioId}/seguidores`);
    return response.data;
  },

  // Obter lista de quem o usuário segue
  async obterSeguindo(usuarioId) {
    const response = await api.get(`/usuarios/${usuarioId}/seguindo`);
    return response.data;
  },

  // Obter Feed Social de Atividades
  async obterFeedSocial(pagina = 1, itensPorPagina = 20) {
    const response = await api.get("/usuarios/feed", {
      params: { pagina, itensPorPagina },
    });
    return response.data;
  },
};
