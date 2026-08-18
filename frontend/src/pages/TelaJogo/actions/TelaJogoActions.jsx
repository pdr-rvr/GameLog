import api from '../../../services/api';

export const buscarJogoPorId = async (jogoId) => {
    try {
        const response = await api.get(`/Jogos/${jogoId}`);
        const jogo = response.data.$values ? response.data.$values[0] : response.data;

        if (!jogo) {
            throw new Error('Jogo não encontrado.');
        }

        jogo.imagem = jogo.imagem || '/images/default_game_cover.png';

        return jogo;
    } catch (error) {
        console.error(`Erro ao buscar jogo com ID ${jogoId}:`, error);
        throw new Error(error.response?.data?.message || 'Não foi possível carregar os detalhes do jogo.');
    }
};

export const buscarAvaliacoesPorJogoId = async (jogoId) => {
    try {
        const response = await api.get(`/Avaliacoes/jogo/${jogoId}`);

        const avaliacoes = response.data.$values || response.data;

        if (!Array.isArray(avaliacoes)) {

            console.warn('Formato inesperado da resposta da API de avaliações:', response.data);
            return []; 
        }

        return avaliacoes;
    } catch (error) {
        console.error(`Erro ao buscar avaliações para o jogo com ID ${jogoId}:`, error);
        return [];
    }
};