import axios from "axios";

const getBaseUrl = () => {
  const envUrl = (typeof import.meta !== "undefined" && import.meta.env?.VITE_API_BASE_URL) 
    || (typeof process !== "undefined" && process.env?.REACT_APP_API_BASE_URL);

  if (envUrl) {
    return envUrl.endsWith("/api") ? envUrl : `${envUrl}/api`;
  }
  return "http://localhost:7096/api";
};

let inMemoryToken = null;

export const setAccessToken = (token) => {
  inMemoryToken = token;
};

export const getAccessToken = () => {
  return inMemoryToken;
};

const api = axios.create({
  baseURL: getBaseUrl(),
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request Interceptor: Anexa Token JWT da memória
api.interceptors.request.use((config) => {
  if (inMemoryToken) {
    config.headers.Authorization = `Bearer ${inMemoryToken}`;
  }
  return config;
});

// Fila de requisições pendentes aguardando renovação de token
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: Trata 401 com refresh automático transparente
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Se erro não for 401 ou requisição original não existir, propaga erro
    if (!error.response || error.response.status !== 401 || !originalRequest) {
      if (error.response?.data) {
        const data = error.response.data;
        error.userMessage = data.detail || data.title || data.message || "Ocorreu um erro ao processar a requisição.";
      } else {
        error.userMessage = error.message || "Não foi possível conectar ao servidor.";
      }
      return Promise.reject(error);
    }

    const url = originalRequest.url || "";
    // Não tentar renovar para rotas de autenticação
    const isAuthRoute = url.includes("/Usuarios/login") || 
                        url.includes("/Usuarios/refresh") || 
                        url.includes("/Usuarios/revogar") ||
                        url.includes("/usuarios/login") || 
                        url.includes("/usuarios/refresh") || 
                        url.includes("/usuarios/revogar");

    if (isAuthRoute || originalRequest._retry) {
      inMemoryToken = null;
      window.dispatchEvent(new Event("gamelog-unauthorized"));
      if (error.response?.data) {
        const data = error.response.data;
        error.userMessage = data.detail || data.title || data.message || "Sessão expirada. Faça login novamente.";
      }
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      // Chama o endpoint de refresh enviando o cookie HttpOnly
      const refreshResponse = await api.post("/Usuarios/refresh", {});
      const newToken = refreshResponse.data?.token;

      if (!newToken) {
        throw new Error("Token não retornado na renovação.");
      }

      setAccessToken(newToken);
      processQueue(null, newToken);

      // Atualiza header da requisição original e reexecuta
      originalRequest.headers.Authorization = `Bearer ${newToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      setAccessToken(null);
      window.dispatchEvent(new Event("gamelog-unauthorized"));
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

export default api;
