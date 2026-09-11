import api from "./api";

export const ListaService = {
  async criarLista(dados) {
    // dados: { titulo: string, descricao?: string, estaPublica?: boolean, jogosIds?: number[] }
    const response = await api.post("/Listas", dados);
    return response.data;
  },

  async obterListaPorId(id) {
    const response = await api.get(`/Listas/${id}`);
    return response.data;
  },

  async listarListasDoUsuario(usuarioId) {
    const response = await api.get(`/Listas/usuario/${usuarioId}`);
    return response.data;
  },

  async editarLista(id, dados) {
    const response = await api.put(`/Listas/${id}`, dados);
    return response.data;
  },

  async deletarLista(id) {
    const response = await api.delete(`/Listas/${id}`);
    return response.data;
  },

  async adicionarJogo(listaId, jogoId) {
    const response = await api.post(`/Listas/${listaId}/jogos`, {
      jogoId: jogoId
    });
    return response.data;
  },

  async removerJogo(listaId, jogoId) {
    const response = await api.delete(`/Listas/${listaId}/jogos/${jogoId}`);
    return response.data;
  }
};
