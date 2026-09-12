import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import DiscussionFeedCard from "./DiscussionFeedCard";

describe("DiscussionFeedCard", () => {
  const mockItem = {
    id: "comm-123",
    tipoAtividade: "Discussao",
    dataAtividade: "2026-03-01T12:00:00Z",
    autorId: "user-1",
    autorNome: "LucasGamer",
    autorFoto: "https://example.com/lucas.jpg",
    autorAvaliacaoRespondidaId: "user-2",
    autorAvaliacaoRespondidaNome: "PedroDev",
    avaliacaoId: "eval-999",
    avaliacaoOriginalTexto: "Achei a história sensacional!",
    comentarioTexto: "Concordo 100%, principalmente a reviravolta no final!",
    jogoId: "game-10",
    jogoTitulo: "Elden Ring",
    jogoImagem: "https://example.com/eldenring.jpg",
    nomeEmpresa: "FromSoftware",
    totalCurtidas: 4,
    curtidaPorMim: false
  };

  it("renderiza corretamente autor, comentário, contexto do jogo e resenha original", () => {
    render(
      <BrowserRouter>
        <DiscussionFeedCard item={mockItem} onToggleCurtir={vi.fn()} />
      </BrowserRouter>
    );

    expect(screen.getByText("LucasGamer")).toBeInTheDocument();
    expect(screen.getByText("@PedroDev")).toBeInTheDocument();
    expect(screen.getByText("Elden Ring")).toBeInTheDocument();
    expect(screen.getByText("FromSoftware")).toBeInTheDocument();
    expect(screen.getByText('"Concordo 100%, principalmente a reviravolta no final!"')).toBeInTheDocument();
    expect(screen.getByText(/Achei a história sensacional!/)).toBeInTheDocument();
    expect(screen.getByText("Discussão")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
  });

  it("dispara callback ao curtir", () => {
    const handleCurtir = vi.fn();
    render(
      <BrowserRouter>
        <DiscussionFeedCard item={mockItem} onToggleCurtir={handleCurtir} />
      </BrowserRouter>
    );

    const btnLike = screen.getByTestId("btn-like-discussion");
    fireEvent.click(btnLike);
    expect(handleCurtir).toHaveBeenCalledWith("eval-999");
  });
});
