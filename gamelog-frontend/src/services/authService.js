import api from './api';

export const AuthService = {
  async login(email, senha) {
    try {
      const response = await api.post('/usuarios/login', {
        email,
        senha
      });
      
      // Armazena o token e dados do usuário
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.usuario));
      
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || 'Erro ao fazer login');
    }
  },

  async register(nick, email, senha) {
    try {
      const response = await api.post('/usuarios', {
        nomeUsuario: nick,
        email,
        senha
      });
      return response.data;
    } catch (error) {
      throw new Error(error.response?.data?.message || 'Erro ao registrar');
    }
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  isAuthenticated() {
    return !!localStorage.getItem('token');
  }
};