import React from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect, vi } from 'vitest';
import AvaliacaoCard from './AvaliacaoCard';
import { ToastProvider } from '../../context/ToastContext';

vi.mock('../../services/authService', () => ({
  AuthService: {
    getCurrentUser: vi.fn(() => ({ usuarioId: '0191e7a4-312c-7b00-8802-b2586a76059d' })),
    isAuthenticated: vi.fn(() => true)
  }
}));

describe('AvaliacaoCard Component', () => {
  const mockAvaliacao = {
    avaliacaoId: '0191e7b1-4567-7b00-8802-998877665544',
    jogoId: '0191e7a4-312c-7b00-8802-b2586a76059d',
    nomeJogo: 'Elden Ring',
    usuarioId: '0191e7a4-312c-7b00-8802-b2586a76059d',
    nomeUsuario: 'TarnishedGamer',
    nota: 5,
    textoAvaliacao: 'Uma obra-prima absoluta do design de mundo aberto.',
    dataPublicacao: '2023-01-15T10:00:00Z',
    totalCurtidas: 12,
    totalRespostas: 3,
    curtidaPorMim: false
  };

  it('deve renderizar o autor, nome do jogo e texto da análise', () => {
    render(
      <ToastProvider>
        <MemoryRouter>
          <AvaliacaoCard avaliacao={mockAvaliacao} />
        </MemoryRouter>
      </ToastProvider>
    );

    expect(screen.getByText('TarnishedGamer')).toBeInTheDocument();
    expect(screen.getByText('Elden Ring')).toBeInTheDocument();
    expect(screen.getByText('Uma obra-prima absoluta do design de mundo aberto.')).toBeInTheDocument();
    expect(screen.getByText('12')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });

  it('deve renderizar o link para o perfil do usuário usando UUID', () => {
    render(
      <ToastProvider>
        <MemoryRouter>
          <AvaliacaoCard avaliacao={mockAvaliacao} />
        </MemoryRouter>
      </ToastProvider>
    );

    const links = screen.getAllByRole('link');
    const userProfileLink = links.find(l => l.getAttribute('href') === '/perfil/0191e7a4-312c-7b00-8802-b2586a76059d');
    expect(userProfileLink).toBeDefined();
  });
});
