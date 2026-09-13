/**
 * GameLog Logger Utility
 * Inibe chamadas de logging ruidosas em ambiente de produção (Vite prod build),
 * preservando visibilidade completa em desenvolvimento (import.meta.env.DEV).
 */
export const logger = {
  error: (...args) => {
    if (import.meta.env.DEV) {
      console.error(...args);
    }
  },
  warn: (...args) => {
    if (import.meta.env.DEV) {
      console.warn(...args);
    }
  },
  info: (...args) => {
    if (import.meta.env.DEV) {
      console.info(...args);
    }
  },
  log: (...args) => {
    if (import.meta.env.DEV) {
      console.log(...args);
    }
  }
};

export default logger;
