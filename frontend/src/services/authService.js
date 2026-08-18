import api from './api';
import { jwtDecode } from 'jwt-decode';

export const AuthService = {
  async login(email, senha) {
    try {
      const response = await api.post('/Usuarios/login', {
        email,
        senha
      });

      if (response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.usuario)); 
      }

      return response.data;
    } catch (error) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Credenciais inválidas!');
      } else if (error.request) {
        throw new Error('Sem resposta do servidor. Verifique sua conexão.');
      } else {
        throw new Error('Erro ao configurar a requisição de login.');
      }
    }
  },

  async register(nick, email, senha) {
    try {
      const response = await api.post('/Usuarios', {
        nomeUsuario: nick,
        email,
        senha,
        fotoDePerfil: '' 
      });
      return response.data;
    } catch (error) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Erro ao registrar usuário.');
      } else if (error.request) {
        throw new Error('Sem resposta do servidor. Verifique sua conexão.');
      } else {
        throw new Error('Erro ao configurar a requisição de registro.');
      }
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
    const token = localStorage.getItem('token');
    if (!token) return false;
    try {
      const decoded = jwtDecode(token);
      return decoded.exp * 1000 > Date.now();
    } catch (error) {
      console.error("Token inválido ou expirado na checagem de autenticação:", error);
      AuthService.logout(); 
      return false;
    }
  }
};