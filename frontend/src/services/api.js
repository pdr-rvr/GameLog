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

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
