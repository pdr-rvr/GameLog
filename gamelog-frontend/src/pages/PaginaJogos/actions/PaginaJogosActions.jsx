import api from '../../../services/api';

export const buscarJogos = async () => {
    try {
        const response = await api.get('/Jogos');
        const dados = response.data.$values || response.data;
        const jogosFormatados = Array.isArray(dados) 
            ? dados.map(jogo => ({
                ...jogo,
                imagem: jogo.imagem || '/images/default_game_cover.png'
            }))
            : [];
        return jogosFormatados;
    } catch (error) {
        console.error('Erro ao buscar jogos:', error);
        throw new Error('Não foi possível carregar a lista de jogos');
    }
};

// Futuramente, adicionaremos aqui:
// export const buscarJogosFiltrados = async (filtros, termoPesquisa) => { ... }
// export const buscarJogoPorId = async (jogoId) => { ... }