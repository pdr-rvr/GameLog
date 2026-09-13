import { logger } from "../utils/logger";
import api, { setAccessToken, getAccessToken } from './api';
import { jwtDecode } from 'jwt-decode';

export const AuthService = {
  async login(email, senha) {
    try {
      const response = await api.post('/Usuarios/login', {
        email,
        senha
      });

      if (response.data?.token) {
        setAccessToken(response.data.token);
        if (response.data.usuario) {
          localStorage.setItem('user', JSON.stringify(response.data.usuario));
        }
      }

      return response.data;
    } catch (error) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
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
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      } else if (error.request) {
        throw new Error('Sem resposta do servidor. Verifique sua conexão.');
      } else {
        throw new Error('Erro ao configurar a requisição de registro.');
      }
    }
  },

  async refreshToken() {
    try {
      const response = await api.post('/Usuarios/refresh', {});
      if (response.data?.token) {
        setAccessToken(response.data.token);
        if (response.data.usuario) {
          localStorage.setItem('user', JSON.stringify(response.data.usuario));
        }
      }
      return response.data;
    } catch (error) {
      setAccessToken(null);
      throw error;
    }
  },

  async logout() {
    try {
      await api.post('/Usuarios/revogar', {});
    } catch (error) {
      logger.error("Erro ao revogar token no logout:", error);
    } finally {
      setAccessToken(null);
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  },

  getCurrentUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  isAuthenticated() {
    const token = getAccessToken();
    if (!token) return false;
    try {
      const decoded = jwtDecode(token);
      return decoded.exp * 1000 > Date.now();
    } catch (error) {
      logger.error("Token inválido ou expirado na checagem de autenticação:", error);
      setAccessToken(null);
      return false;
    }
  }
};
