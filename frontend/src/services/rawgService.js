import api from './api';

export const rawgService = {
  /**
   * Busca jogos em tempo real na base de dados global da RAWG.
   * @param {string} termo - Termo de busca (mínimo 2 caracteres)
   * @param {number} pagina - Número da página (padrão 1)
   * @param {number} itensPorPagina - Quantidade por página (padrão 20)
   */
  async buscarJogosRawg(termo, pagina = 1, itensPorPagina = 20) {
    if (!termo || termo.trim().length < 2) {
      return { totalResultados: 0, paginaAtual: 1, jogos: [] };
    }
    const response = await api.get(`/Jogos/rawg/buscar`, {
      params: { termo: termo.trim(), pagina, itensPorPagina }
    });
    return response.data;
  },

  /**
   * Importa sob demanda um jogo da RAWG para a base de dados local do GameLog.
   * @param {number} rawgId - ID do jogo na RAWG
   */
  async importarJogoRawg(rawgId) {
    const response = await api.post(`/Jogos/rawg/importar/${rawgId}`);
    return response.data;
  }
};

export default rawgService;
