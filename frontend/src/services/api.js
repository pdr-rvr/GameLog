import axios from "axios";

const getBaseUrl = () => {
  if (process.env.REACT_APP_API_BASE_URL) {
    return process.env.REACT_APP_API_BASE_URL.endsWith("/api") 
      ? process.env.REACT_APP_API_BASE_URL 
      : `${process.env.REACT_APP_API_BASE_URL}/api`;
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
