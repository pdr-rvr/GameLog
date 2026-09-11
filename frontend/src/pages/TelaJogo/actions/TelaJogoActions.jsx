import api from "../../../services/api";

export const buscarJogoPorId = async (jogoId) => {
    // 1. Se for um identificador explícito de jogo externo da RAWG (ex: "rawg-1234")
    if (typeof jogoId === "string" && jogoId.startsWith("rawg-")) {
        const rawgId = parseInt(jogoId.replace("rawg-", ""), 10);
        if (!isNaN(rawgId) && rawgId > 0) {
            const importRes = await api.post(`/Jogos/rawg/importar/${rawgId}`);
            const jogo = importRes.data;
            const idReal = jogo.jogoId ?? jogo.id;
            return {
                ...jogo,
                id: idReal,
                jogoId: idReal,
                imagem: jogo.imagem || "/game-images/default_game_cover.png"
            };
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
        // 2. Se falhou e jogoId for puramente numérico, tentar importar da RAWG como fallback
        if (typeof jogoId === "number" || (/^\d+$/.test(String(jogoId)))) {
            const numId = parseInt(jogoId, 10);
            if (!isNaN(numId) && numId > 0) {
                try {
                    const importRes = await api.post(`/Jogos/rawg/importar/${numId}`);
                    if (importRes.data) {
                        const jogo = importRes.data;
                        const idReal = jogo.jogoId ?? jogo.id;
                        return {
                            ...jogo,
                            id: idReal,
                            jogoId: idReal,
                            imagem: jogo.imagem || "/game-images/default_game_cover.png"
                        };
                    }
                } catch (importErr) {
                    console.warn(`Tentativa de importação RAWG para ID ${numId} falhou:`, importErr);
                }
            }
        }

        console.error(`Erro ao buscar jogo com ID ${jogoId}:`, error);
        throw new Error(error.userMessage || error.response?.data?.detail || error.response?.data?.message || "Não foi possível carregar os detalhes do jogo.");
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
