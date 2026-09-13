import api from "./api";

export const JogoService = {
  async listarJogos(params = {}) {
    const response = await api.get("/Jogos", { params });
    const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    return dados.map(jogo => {
      const jogoId = jogo.jogoId ?? jogo.id;
      return {
        ...jogo,
        id: jogoId,
        jogoId: jogoId,
        imagem: jogo.imagem || "/game-images/default_game_cover.png"
      };
    });
  },

  async obterMetadadosFiltros() {
    try {
      const response = await api.get("/Jogos/metadados-filtros");
      return response.data || { generos: [], empresas: [], anos: [] };
    } catch {
      return { generos: [], empresas: [], anos: [] };
    }
  },

  async buscarJogosPaginados({
    pagina = 1,
    itensPorPagina = 12,
    busca = "",
    genero = "",
    ano = "",
    empresa = "",
    nota = "",
    ordenacao = "melhores"
  } = {}) {
    const params = new URLSearchParams();
    params.append("pagina", pagina);
    params.append("itensPorPagina", itensPorPagina);
    if (busca && busca.trim()) params.append("busca", busca.trim());
    if (Array.isArray(genero)) {
      genero.forEach(g => {
        if (g && g.trim()) params.append("genero", g.trim());
      });
    } else if (genero && genero.trim()) {
      params.append("genero", genero.trim());
    }
    if (ano) params.append("ano", ano);
    if (empresa) params.append("empresa", empresa);
    if (nota) params.append("notaMinima", nota);
    if (ordenacao) params.append("ordenacao", ordenacao);

    const response = await api.get(`/Jogos?${params.toString()}`);
    const data = response.data;

    const itens = (data.itens || []).map(jogo => {
      const jogoId = jogo.jogoId ?? jogo.id;
      return {
        ...jogo,
        id: jogoId,
        jogoId: jogoId,
        imagem: jogo.imagem || "/game-images/default_game_cover.png"
      };
    });

    return {
      itens,
      paginaAtual: data.paginaAtual || 1,
      totalPaginas: data.totalPaginas || 1,
      totalItens: data.totalItens || 0,
      itensPorPagina: data.itensPorPagina || itensPorPagina,
      temAnterior: Boolean(data.temAnterior),
      temProxima: Boolean(data.temProxima)
    };
  },

  async obterDestaques(limite = 5) {
    try {
      const response = await api.get("/Jogos/destaques", { params: { limite } });
      return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    } catch {
      return [];
    }
  },

  async obterTopAvaliados() {
    try {
      const response = await api.get("/Jogos/top-avaliados");
      return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    } catch {
      return [];
    }
  },

  async obterRecomendacoes(usuarioId) {
    try {
      const response = await api.get(`/Usuarios/${usuarioId}/recomendacoes`);
      return Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    } catch {
      return [];
    }
  },

  async importarJogoRawg(rawgId) {
    const response = await api.post(`/Jogos/rawg/importar/${rawgId}`);
    const jogo = response.data;
    const idReal = jogo.jogoId ?? jogo.id;
    return {
      ...jogo,
      id: idReal,
      jogoId: idReal,
      imagem: jogo.imagem || "/game-images/default_game_cover.png"
    };
  },

  async obterJogoPorId(jogoId) {
    if (typeof jogoId === "string" && jogoId.startsWith("rawg-")) {
      const rawgId = parseInt(jogoId.replace("rawg-", ""), 10);
      if (!isNaN(rawgId) && rawgId > 0) {
        return await JogoService.importarJogoRawg(rawgId);
      }
    }

    try {
      const response = await api.get(`/Jogos/${jogoId}`);
      const jogo = response.data;
      if (!jogo) {
        throw new Error("Jogo não encontrado.");
      }
      const idReal = jogo.jogoId ?? jogo.id ?? jogoId;
      return {
        ...jogo,
        id: idReal,
        jogoId: idReal,
        imagem: jogo.imagem || "/game-images/default_game_cover.png"
      };
    } catch (error) {
      if (typeof jogoId === "number" || (/^\d+$/.test(String(jogoId)))) {
        const numId = parseInt(jogoId, 10);
        if (!isNaN(numId) && numId > 0) {
          try {
            return await JogoService.importarJogoRawg(numId);
          } catch {
            // fallback
          }
        }
      }
      throw new Error(error.userMessage || error.response?.data?.detail || error.response?.data?.message || "Não foi possível carregar os detalhes do jogo.");
    }
  }
};

export default JogoService;
