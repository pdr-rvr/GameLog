import api from "./api";

export const JogoService = {
  async listarJogos(params = {}) {
    const response = await api.get("/Jogos", { params });
    return response.data;
  },

  async obterDestaques(limite = 5) {
    const response = await api.get("/Jogos/destaques", {
      params: { limite }
    });
    return response.data;
  },

  async obterJogoPorId(id) {
    const response = await api.get(`/Jogos/${id}`);
    return response.data;
  },

  async obterTopAvaliados() {
    const response = await api.get("/Jogos/top-avaliados");
    return response.data;
  }
};
