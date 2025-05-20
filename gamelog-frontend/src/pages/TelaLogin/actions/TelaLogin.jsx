import api from '../../../services/api'; 

export const loginUsuario = async (dadosLogin) => {
  try {
    // O endpoint correto parece ser /Usuarios, mas confirme se é para login ou cadastro
    // Normalmente login tem um endpoint específico como /Usuarios/login
    const response = await api.post('/Usuarios/login', dadosLogin);
    
    // Assumindo que a resposta contém um token
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      // Você pode também armazenar outros dados do usuário se necessário
      localStorage.setItem('usuario', JSON.stringify(response.data.usuario));
    }
    
    return response.data;
  } catch (error) {
    // Tratamento de erro personalizado
    if (error.response) {
      // A requisição foi feita e o servidor respondeu com um status code
      // que está fora do range de 2xx
      throw new Error(error.response.data.message || 'Erro ao fazer login');
    } else if (error.request) {
      // A requisição foi feita mas nenhuma resposta foi recebida
      throw new Error('Sem resposta do servidor');
    } else {
      // Algum erro ocorreu ao configurar a requisição
      throw new Error('Erro ao configurar a requisição');
    }
  }
};