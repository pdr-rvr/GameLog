import api from "./api";

export const EmpresaService = {
  async listarEmpresas() {
    const response = await api.get("/Empresas");
    return response.data;
  },

  async obterEmpresaPorId(empresaId) {
    const response = await api.get(`/Empresas/${empresaId}`);
    return response.data;
  },

  async listarJogosPorEmpresa(empresaId) {
    const response = await api.get(`/Empresas/${empresaId}/jogos`);
    return response.data;
  }
};

export default EmpresaService;
