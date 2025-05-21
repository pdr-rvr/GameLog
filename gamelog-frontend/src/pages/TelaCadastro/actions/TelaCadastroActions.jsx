import api from '../../../services/api';

export const cadastrarUsuario = async (dadosCadastro) => {
  try {
    const response = await api.post('/Usuarios', {
      nomeUsuario: dadosCadastro.nick,
      email: dadosCadastro.email,
      senha: dadosCadastro.senha,
      fotoDePerfil: ''
    });
    
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || 'Erro ao cadastrar usuário');
    } else if (error.request) {
      throw new Error('Sem resposta do servidor');
    } else {
      throw new Error('Erro ao configurar a requisição');
    }
  }
};