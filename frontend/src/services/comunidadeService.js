import api from "./api";

export const ComunidadeService = {
  async obterTendencias() {
    const response = await api.get("/Comunidade/tendencias");
    return response.data;
  }
};
