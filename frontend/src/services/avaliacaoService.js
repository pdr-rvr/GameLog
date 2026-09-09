import api from "./api";

export const AvaliacaoService = {
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

  async toggleCurtirResposta(respostaId) {
    const response = await api.post(`/Avaliacoes/respostas/${respostaId}/curtir`);
    return response.data;
  }
};

