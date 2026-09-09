import api from "../../../services/api";

export const buscarJogoPorId = async (jogoId) => {
    try {
        const response = await api.get(`/Jogos/${jogoId}`);
        const jogo = response.data;
        if (!jogo) {
            throw new Error("Jogo não encontrado.");
        }
        const idReal = Number(jogo.jogoId ?? jogo.id ?? jogoId);
        return {
            ...jogo,
            id: idReal,
            jogoId: idReal,
            imagem: jogo.imagem || "/game-images/default_game_cover.png"
        };
    } catch (error) {
        console.error(`Erro ao buscar jogo com ID ${jogoId}:`, error);
        throw new Error(error.response?.data?.message || "Não foi possível carregar os detalhes do jogo.");
    }
};

export const buscarAvaliacoesPorJogoId = async (jogoId) => {
    try {
        const response = await api.get(`/Avaliacoes/jogo/${jogoId}`);
        const avaliacoes = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return avaliacoes;
    } catch (error) {
        console.error(`Erro ao buscar avaliações para o jogo com ID ${jogoId}:`, error);
        return [];
    }
};
