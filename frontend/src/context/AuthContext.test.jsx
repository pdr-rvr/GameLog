import React from 'react';
import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { AuthProvider, useAuth } from './AuthContext';

const TestConsumer = () => {
  const { user, isAuthenticated, loadingAuth } = useAuth();
  if (loadingAuth) return <div>Carregando...</div>;
  return (
    <div>
      <div data-testid="auth-status">{isAuthenticated ? 'Autenticado' : 'Não autenticado'}</div>
      {user && <div data-testid="user-id">{user.id}</div>}
      {user && <div data-testid="user-name">{user.nomeUsuario}</div>}
    </div>
  );
};

describe('AuthContext Component & Hook', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('deve inicializar como não autenticado quando não há token no storage', () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    expect(screen.getByTestId('auth-status')).toHaveTextContent('Não autenticado');
  });

  it('deve carregar os dados de usuário preservando o UUID como string quando token é válido', () => {
    // Fake JWT payload com expiração futura
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

    localStorage.setItem('token', token);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    expect(screen.getByTestId('auth-status')).toHaveTextContent('Autenticado');
    expect(screen.getByTestId('user-id')).toHaveTextContent('0191e7a4-312c-7b00-8802-b2586a76059d');
    expect(screen.getByTestId('user-name')).toHaveTextContent('GamerMaster');
  });
});
