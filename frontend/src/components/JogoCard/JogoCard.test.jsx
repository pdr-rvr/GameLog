import React from 'react';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import JogoCard from './JogoCard';

describe('JogoCard Component', () => {
  const mockJogo = {
    jogoId: '0191e7a4-312c-7b00-8802-b2586a76059d',
    titulo: 'The Legend of Zelda: Tears of the Kingdom',
    imagem: 'https://media.rawg.io/zelda.jpg',
    nomeDesenvolvedora: 'Nintendo',
    genero: 'Aventura',
    dataLancamento: '2023-05-12',
    mediaAvaliacoes: 4.9,
    totalAvaliacoes: 42,
    classificacaoIndicativa: 10
  };

  it('deve renderizar título, empresa, gênero e ano corretamente', () => {
    render(
      <MemoryRouter>
        <JogoCard jogo={mockJogo} />
      </MemoryRouter>
    );

    expect(screen.getByText('The Legend of Zelda: Tears of the Kingdom')).toBeInTheDocument();
    expect(screen.getByText('Nintendo')).toBeInTheDocument();
    expect(screen.getByText('Aventura')).toBeInTheDocument();
    expect(screen.getByText('2023')).toBeInTheDocument();
    expect(screen.getByText('4.9')).toBeInTheDocument();
  });

  it('deve criar o link com o UUID correto do jogo', () => {
    render(
      <MemoryRouter>
        <JogoCard jogo={mockJogo} />
      </MemoryRouter>
    );

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/jogos/0191e7a4-312c-7b00-8802-b2586a76059d');
  });

  it('não deve renderizar nada se jogo for null ou undefined', () => {
    const { container } = render(
      <MemoryRouter>
        <JogoCard jogo={null} />
      </MemoryRouter>
    );

    expect(container).toBeEmptyDOMElement();
  });
});
