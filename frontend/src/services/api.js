import axios from "axios";

const getBaseUrl = () => {
  const envUrl = (typeof import.meta !== "undefined" && import.meta.env?.VITE_API_BASE_URL) 
    || (typeof process !== "undefined" && process.env?.REACT_APP_API_BASE_URL);

  if (envUrl) {
    return envUrl.endsWith("/api") ? envUrl : `${envUrl}/api`;
  }
  return "http://localhost:7096/api";
};

const api = axios.create({
  baseURL: getBaseUrl(),
  headers: {
    "Content-Type": "application/json",
  },
});

// Request Interceptor: Anexa Token JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response Interceptor: Trata 401 e extrai mensagens RFC 7807 (Problem Details)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const token = localStorage.getItem("token");
      if (token) {
        localStorage.removeItem("token");
        localStorage.removeItem("usuario");
        window.dispatchEvent(new Event("gamelog-unauthorized"));
      }
    }

    if (error.response?.data) {
      const data = error.response.data;
      error.userMessage = data.detail || data.title || data.message || "Ocorreu um erro ao processar a requisição.";
    } else {
      error.userMessage = error.message || "Não foi possível conectar ao servidor.";
    }

    return Promise.reject(error);
  }
);

export default api;
