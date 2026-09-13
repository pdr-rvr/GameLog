import { describe, it, expect, beforeEach, vi } from 'vitest';
import api, { setAccessToken, getAccessToken } from './api';

describe('API Service & Axios Interceptors', () => {
  beforeEach(() => {
    setAccessToken(null);
    vi.restoreAllMocks();
  });

  it('deve gerenciar o token em memória sem interagir com localStorage', () => {
    localStorage.clear();
    setAccessToken('test-in-memory-token');
    expect(getAccessToken()).toBe('test-in-memory-token');
    expect(localStorage.getItem('token')).toBeNull();

    setAccessToken(null);
    expect(getAccessToken()).toBeNull();
  });

  it('deve anexar o header Authorization quando há token em memória', async () => {
    setAccessToken('bearer-12345');

    // Interceptor de requisição
    const config = { headers: {} };
    const interceptedConfig = api.interceptors.request.handlers[0].fulfilled(config);

    expect(interceptedConfig.headers.Authorization).toBe('Bearer bearer-12345');
  });

  it('não deve anexar header Authorization quando não há token em memória', async () => {
    setAccessToken(null);

    const config = { headers: {} };
    const interceptedConfig = api.interceptors.request.handlers[0].fulfilled(config);

    expect(interceptedConfig.headers.Authorization).toBeUndefined();
  });
});
