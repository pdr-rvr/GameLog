import api from "../../../services/api";

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
