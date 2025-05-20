import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7096/api', // Porta do seu backend
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar o token às requisições após login
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;