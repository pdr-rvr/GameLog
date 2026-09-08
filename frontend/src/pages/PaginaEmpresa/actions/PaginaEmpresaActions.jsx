import api from "../../../services/api";

export const fetchEmpresa = async (empresaId) => {
  try {
    const response = await api.get(`/Empresas/${empresaId}`);
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || "Erro ao carregar detalhes da empresa.");
    }
    throw new Error("Erro de conexão com o servidor.");
  }
};

export const fetchJogosDaEmpresa = async (empresaId) => {
  try {
    const response = await api.get(`/Empresas/${empresaId}/jogos`);
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || "Erro ao carregar jogos da empresa.");
    }
    throw new Error("Erro de conexão com o servidor.");
  }
};
