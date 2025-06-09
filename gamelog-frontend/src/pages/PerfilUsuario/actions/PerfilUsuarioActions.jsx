const API_BASE_URL = 'https://localhost:7096/api';

export const fetchUserProfile = async (userId, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    const response = await fetch(`${API_BASE_URL}/Usuarios/${userId}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao carregar perfil.');
    }

    return await response.json();
  } catch (error) {
    throw error;
  }
};

export const updateUserProfile = async (userId, userData, token) => {
  if (!token) {
    throw new Error('Token de autenticação ausente.');
  }
  try {
    const response = await fetch(`${API_BASE_URL}/Usuarios/${userId}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao atualizar perfil.');
    }

    return await response.json();
  } catch (error) {
    throw error;
  }
};