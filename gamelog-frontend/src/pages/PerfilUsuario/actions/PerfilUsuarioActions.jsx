import api from '../../../services/api';

export const fetchUserProfile = async (userId, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    // Usando a instância 'api' do axios
    const response = await api.get(`/Usuarios/${userId}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    return response.data;
  } catch (error) {
    const errorMessage = error.response?.data?.message || 'Erro ao carregar perfil.';
    console.error('Erro ao buscar perfil do usuário:', error);
    throw new Error(errorMessage);
  }
};

export const updateUserProfile = async (userId, userData, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    const response = await api.put(`/Usuarios/${userId}`, userData, { 
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    return response.data;
  } catch (error) {
    const errorMessage = error.response?.data?.message || 'Erro ao atualizar perfil.';
    console.error('Erro ao atualizar perfil do usuário:', error);
    throw new Error(errorMessage);
  }
};

