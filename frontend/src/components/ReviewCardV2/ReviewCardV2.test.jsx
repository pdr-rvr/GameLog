import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import ReviewCardV2 from "./ReviewCardV2";

const mockAvaliacao = {
  avaliacaoId: "eval-123",
  nota: 5,
  textoAvaliacao: "Experiência absolutamente fantástica!",
  nomeJogo: "The Witcher 3: Wild Hunt",
  imagemJogo: "https://example.com/witcher.jpg",
  jogoId: "jogo-123",
  nomeEmpresa: "CD Projekt Red",
  nomeUsuario: "pedro_gamer",
  fotoPerfilUsuario: "https://example.com/pedro.jpg",
  usuarioId: "user-123",
  dataPublicacao: "2026-09-10T12:00:00Z",
  totalCurtidas: 4,
  curtidaPorMim: false,
  totalRespostas: 2
};

describe("ReviewCardV2 Component", () => {
  it("deve renderizar dados da avaliação, autor e jogo", () => {
    render(
      <BrowserRouter>
        <ReviewCardV2 avaliacao={mockAvaliacao} />
      </BrowserRouter>
    );

    expect(screen.getByText("pedro_gamer")).toBeInTheDocument();
    expect(screen.getByText("The Witcher 3: Wild Hunt")).toBeInTheDocument();
    expect(screen.getByText('"Experiência absolutamente fantástica!"')).toBeInTheDocument();
    expect(screen.getByText("5/5")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("2 respostas")).toBeInTheDocument();
  });

  it("deve disparar onToggleCurtir ao clicar no botão de curtir", () => {
    const handleLike = vi.fn();
    render(
      <BrowserRouter>
        <ReviewCardV2 avaliacao={mockAvaliacao} onToggleCurtir={handleLike} />
      </BrowserRouter>
    );

    const btnLike = screen.getByTestId("btn-like-review");
    fireEvent.click(btnLike);

    expect(handleLike).toHaveBeenCalledWith("eval-123");
  });
});
