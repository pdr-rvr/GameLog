import api from '../../../services/api'; 

export const loginUsuario = async (dadosLogin) => {
  try {

    const response = await api.post('/Usuarios/login', dadosLogin);
    
    
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('usuario', JSON.stringify(response.data.usuario));
    }
    
    return response.data;
  } catch (error) {
    if (error.response) {
      throw new Error(error.response.data.message || 'Erro ao fazer login');
    } else if (error.request) {
      throw new Error('Sem resposta do servidor');
    } else {
      throw new Error('Erro ao configurar a requisição');
    }
  }
};