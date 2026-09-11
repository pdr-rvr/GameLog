import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import SocialSidebar from "./SocialSidebar";

const mockAmigosJogando = [
  {
    usuarioId: "u1",
    usuarioNome: "ana_gamer",
    usuarioFoto: "https://example.com/ana.jpg",
    jogoId: "j1",
    jogoTitulo: "Cyberpunk 2077"
  }
];

const mockGamersSugeridos = [
  {
    usuarioId: "u2",
    nomeUsuario: "lucas_retro",
    bio: "Amante de retro",
    seguidoPorMim: false
  }
];

const mockPodio = [
  { jogoId: "j1", titulo: "The Witcher 3", imagem: "w3.jpg" },
  { jogoId: "j2", titulo: "Elden Ring", imagem: "er.jpg" }
];

describe("SocialSidebar Component", () => {
  it("deve renderizar amigos jogando, gamers sugeridos e pódio", () => {
    render(
      <BrowserRouter>
        <SocialSidebar
          amigosJogando={mockAmigosJogando}
          gamersSugeridos={mockGamersSugeridos}
          jogosPodio={mockPodio}
        />
      </BrowserRouter>
    );

    expect(screen.getByText("Amigos Jogando Agora")).toBeInTheDocument();
    expect(screen.getByText("ana_gamer")).toBeInTheDocument();
    expect(screen.getByText("Cyberpunk 2077")).toBeInTheDocument();

    expect(screen.getByText("Gamers Sugeridos")).toBeInTheDocument();
    expect(screen.getByText("lucas_retro")).toBeInTheDocument();

    expect(screen.getByText("Meu Pódio Top 5")).toBeInTheDocument();
  });

  it("deve disparar onSeguirClick ao clicar no botão seguir", () => {
    const handleFollow = vi.fn();
    render(
      <BrowserRouter>
        <SocialSidebar
          gamersSugeridos={mockGamersSugeridos}
          onSeguirClick={handleFollow}
        />
      </BrowserRouter>
    );

    const btnFollow = screen.getByLabelText("Seguir");
    fireEvent.click(btnFollow);

    expect(handleFollow).toHaveBeenCalledWith("u2");
  });
});
