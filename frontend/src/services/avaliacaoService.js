import api from "./api";

export const AvaliacaoService = {
  async listarAvaliacoes(params = {}) {
    const response = await api.get("/Avaliacoes", { params });
    return response.data;
  },

  async obterAvaliacaoPorId(id) {
    const response = await api.get(`/Avaliacoes/${id}`);
    return response.data;
  },

  async toggleCurtir(avaliacaoId) {
    const response = await api.post(`/Avaliacoes/${avaliacaoId}/curtir`);
    return response.data;
  },

  async removerCurtida(avaliacaoId) {
    const response = await api.delete(`/Avaliacoes/${avaliacaoId}/curtir`);
    return response.data;
  },

  async listarRespostas(avaliacaoId) {
    const response = await api.get(`/Avaliacoes/${avaliacaoId}/respostas`);
    return response.data;
  },

  async adicionarResposta(avaliacaoId, comentario) {
    const response = await api.post(`/Avaliacoes/${avaliacaoId}/respostas`, {
      comentario
    });
    return response.data;
  },

  async deletarResposta(respostaId) {
    const response = await api.delete(`/Avaliacoes/respostas/${respostaId}`);
    return response.data;
  },

  async criarAvaliacao(dados) {
    const response = await api.post("/Avaliacoes", dados);
    return response.data;
  },

  async atualizarAvaliacao(id, dados) {
    const response = await api.put(`/Avaliacoes/${id}`, dados);
    return response.data;
  },

  async excluirAvaliacao(id) {
    const response = await api.delete(`/Avaliacoes/${id}`);
    return response.data;
  },

  async listarPorUsuario(usuarioId) {
    const response = await api.get(`/Avaliacoes/usuario/${usuarioId}`);
    return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
  },

  async listarPorJogo(jogoId) {
    const response = await api.get(`/Avaliacoes/jogo/${jogoId}`);
    return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
  },

  async toggleCurtirResposta(respostaId) {
    const response = await api.post(`/Avaliacoes/respostas/${respostaId}/curtir`);
    return response.data;
  }
};

export default AvaliacaoService;
