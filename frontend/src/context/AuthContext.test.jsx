import React from 'react';
import { render, screen, act, waitFor } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AuthProvider, useAuth } from './AuthContext';
import { setAccessToken, getAccessToken } from '../services/api';
import { AuthService } from '../services/authService';

const TestConsumer = () => {
  const { user, isAuthenticated, loadingAuth, login, logout } = useAuth();
  if (loadingAuth) return <div>Carregando...</div>;
  return (
    <div>
      <div data-testid="auth-status">{isAuthenticated ? 'Autenticado' : 'Não autenticado'}</div>
      {user && <div data-testid="user-id">{user.id}</div>}
      {user && <div data-testid="user-name">{user.nomeUsuario}</div>}
      <button onClick={() => login('gamer@test.com', 'Password123!')} data-testid="login-btn">Login</button>
      <button onClick={() => logout()} data-testid="logout-btn">Logout</button>
    </div>
  );
};

describe('AuthContext Component & Hook', () => {
  beforeEach(() => {
    localStorage.clear();
    setAccessToken(null);
    vi.restoreAllMocks();
  });

  it('deve inicializar como não autenticado quando não há sessão nem cookie ativo', async () => {
    vi.spyOn(AuthService, 'refreshToken').mockRejectedValue(new Error('No token'));

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('Não autenticado');
    });

    expect(localStorage.getItem('token')).toBeNull();
  });

  it('deve autenticar quando o silent refresh obtém um novo token', async () => {
    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(
      JSON.stringify({
        sub: '0191e7a4-312c-7b00-8802-b2586a76059d',
        nomeUsuario: 'GamerMaster',
        email: 'gamer@test.com',
        exp: futureExp
      })
    );
    const token = `${header}.${payload}.signature`;

    vi.spyOn(AuthService, 'refreshToken').mockResolvedValue({
      token,
      usuario: {
        id: '0191e7a4-312c-7b00-8802-b2586a76059d',
        nomeUsuario: 'GamerMaster',
        email: 'gamer@test.com'
      }
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('Autenticado');
      expect(screen.getByTestId('user-id')).toHaveTextContent('0191e7a4-312c-7b00-8802-b2586a76059d');
      expect(screen.getByTestId('user-name')).toHaveTextContent('GamerMaster');
    });

    // Token deve estar exclusivamente na memória e NÃO no localStorage
    expect(getAccessToken()).toBe(token);
    expect(localStorage.getItem('token')).toBeNull();
  });

  it('deve autenticar após login mantendo token exclusivamente em memória', async () => {
    vi.spyOn(AuthService, 'refreshToken').mockRejectedValue(new Error('No cookie'));

    const futureExp = Math.floor(Date.now() / 1000) + 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(
      JSON.stringify({
        sub: '0191e7a4-312c-7b00-8802-b2586a76059d',
        nomeUsuario: 'GamerLogin',
        email: 'gamer@login.com',
        exp: futureExp
      })
    );
    const token = `${header}.${payload}.signature`;

    vi.spyOn(AuthService, 'login').mockImplementation(async () => {
      setAccessToken(token);
      return {
        token,
        usuario: {
          id: '0191e7a4-312c-7b00-8802-b2586a76059d',
          nomeUsuario: 'GamerLogin',
          email: 'gamer@login.com'
        }
      };
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('Não autenticado');
    });

    const loginBtn = screen.getByTestId('login-btn');
    await act(async () => {
      loginBtn.click();
    });

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('Autenticado');
      expect(screen.getByTestId('user-name')).toHaveTextContent('GamerLogin');
    });

    expect(getAccessToken()).toBe(token);
    expect(localStorage.getItem('token')).toBeNull();
  });
});
