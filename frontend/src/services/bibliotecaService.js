import api from "./api";

export const STATUS_JOGO = {
  QUERO_JOGAR: 1,
  JOGANDO: 2,
  ZERADO: 3,
  PAUSADO: 4,
  ABANDONADO: 5
};

export const STATUS_LABELS = {
  1: "Quero Jogar",
  2: "Jogando",
  3: "Zerado",
  4: "Pausado",
  5: "Abandonado"
};

export const BibliotecaService = {
  async salvarItem(jogoId, status) {
    const id = Number(jogoId);
    const response = await api.post("/Biblioteca", {
      jogoId: id,
      status: Number(status)
    });
    return response.data;
  },

  async obterStatusJogo(jogoId) {
    const id = Number(jogoId);
    const response = await api.get(`/Biblioteca/jogo/${id}`);
    return response.data;
  },

  async removerItem(jogoId) {
    const id = Number(jogoId);
    const response = await api.delete(`/Biblioteca/jogo/${id}`);
    return response.data;
  },

  async listarBibliotecaUsuario(usuarioId, status = null, busca = null) {
    const params = new URLSearchParams();
    if (status !== null && status !== undefined && status !== "") {
      params.append("status", status);
    }
    if (busca && busca.trim()) {
      params.append("busca", busca.trim());
    }

    const queryString = params.toString();
    const url = `/Biblioteca/usuario/${usuarioId}${queryString ? `?${queryString}` : ""}`;
    const response = await api.get(url);
    return response.data;
  },

  async obterEstatisticas(usuarioId) {
    const response = await api.get(`/Biblioteca/usuario/${usuarioId}/estatisticas`);
    return response.data;
  },

  async obterFavoritos(usuarioId) {
    const response = await api.get(`/Biblioteca/usuario/${usuarioId}/favoritos`);
    return response.data;
  },

  async salvarFavoritos(favoritos) {
    // favoritos: Array of { posicao: 1..5, jogoId: number }
    const response = await api.put("/Biblioteca/meus-favoritos", {
      favoritos
    });
    return response.data;
  }
};
