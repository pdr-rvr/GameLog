import { useState, useEffect } from 'react';

/**
 * Hook customizado para debounce de valores (ex: campos de busca de texto).
 * Evita chamadas excessivas a APIs ou computações pesadas enquanto o usuário digita.
 * 
 * @param {*} value Valor a ser observado
 * @param {number} delay Tempo de espera em milissegundos (default: 300ms)
 * @returns {*} Valor com debounce aplicado
 */
export function useDebounce(value, delay = 300) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default useDebounce;
