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