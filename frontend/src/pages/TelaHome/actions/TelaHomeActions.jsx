import api from "../../../services/api";

export const buscarAvaliacoes = async (usuarioId = null) => {
  try {
    const endpoint = usuarioId 
      ? `/Avaliacoes/usuario/${usuarioId}`
      : "/Avaliacoes";
    
    const response = await api.get(endpoint);
    const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
    
    return dados.map(avaliacao => ({
      ...avaliacao,
      avaliacaoId: avaliacao.avaliacaoId || avaliacao.id,
      jogoId: avaliacao.jogoId,
      usuarioId: avaliacao.usuarioId,
      nomeUsuario: avaliacao.nomeUsuario || "Usuário Anônimo"
    }));
  } catch (error) {
    console.error("Erro ao buscar avaliações:", error);
    throw new Error("Não foi possível carregar as avaliações");
  }
};

export const buscarJogos = async () => {
    try {
        const response = await api.get("/Jogos");
        const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return dados.map(jogo => ({
            ...jogo,
            imagem: jogo.imagem || "/game-images/default_game_cover.png"
        }));
    } catch (error) {
        console.error("Erro ao buscar jogos:", error);
        throw new Error("Não foi possível carregar a lista de jogos");
    }
};

export const criarAvaliacao = async (avaliacaoData) => {
  try {
    const response = await api.post("/Avaliacoes", avaliacaoData);
    return response.data;
  } catch (error) {
    console.error("Erro ao criar avaliação:", error);
    throw new Error(error.response?.data?.message || "Erro ao criar avaliação");
  }
};

export const buscarRecomendacoes = async (userId) => {
    try {
        const response = await api.get(`/Usuarios/${userId}/recomendacoes`);
        const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return dados;
    } catch (error) {
        console.error(`Erro ao buscar recomendações para o usuário ${userId}:`, error);
        return []; 
    }
};

export const buscarDestaques = async (limite = 5) => {
    try {
        const response = await api.get("/Jogos/destaques", { params: { limite } });
        const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return dados;
    } catch (error) {
        console.error("Erro ao buscar destaques do hero banner:", error);
        return [];
    }
};

export const buscarTopAvaliados = async () => {
    try {
        const response = await api.get("/Jogos/top-avaliados");
        const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return dados;
    } catch (error) {
        console.error("Erro ao buscar jogos top avaliados:", error);
        return [];
    }
};
