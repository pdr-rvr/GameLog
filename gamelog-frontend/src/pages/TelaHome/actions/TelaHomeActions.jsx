import api from '../../../services/api';

export const buscarAvaliacoes = async () => {
  try {
    const response = await api.get('/Avaliacoes');
    return response.data.$values || Array.isArray(response.data) ? response.data : [];
  } catch (error) {
    console.error('Erro ao buscar avaliações:', error);
    throw new Error('Não foi possível carregar as avaliações');
  }
};

export const buscarJogos = async () => {
  try {
    const response = await api.get('/Jogos');
    const dados = response.data.$values || response.data;
    return Array.isArray(dados) 
      ? dados.map(jogo => ({
          jogoId: jogo.jogoId,
          titulo: jogo.titulo,
          descricao: jogo.descricao,
        }))
      : [];
  } catch (error) {
    console.error('Erro ao buscar jogos:', error);
    throw new Error('Não foi possível carregar a lista de jogos');
  }
};

export const criarAvaliacao = async (avaliacaoData) => {
  try {
    const token = localStorage.getItem('token');
    if (!token) throw new Error('Usuário não autenticado');

    const response = await api.post('/Avaliacoes', avaliacaoData, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    return response.data;
  } catch (error) {
    console.error('Erro ao criar avaliação:', error);
    throw new Error(error.response?.data?.message || 'Erro ao criar avaliação');
  }
};