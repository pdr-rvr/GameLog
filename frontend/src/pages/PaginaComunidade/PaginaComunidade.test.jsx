import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import PaginaComunidade from "./PaginaComunidade";

vi.mock("../../services/comunidadeService", () => ({
  ComunidadeService: {
    obterTendencias: vi.fn(() => Promise.resolve({
      jogosEmAlta: [
        { jogoId: "j1", nome: "Elden Ring", genero: "RPG", notaMedia: 4.9, imagemCapa: "elden.jpg" }
      ],
      avaliacoesPopulares: [],
      colecoesDestaque: [
        { listaId: "l1", nome: "Melhores Soulslike", nomeCriador: "MiyazakiFan", totalJogos: 5 }
      ]
    }))
  }
}));

vi.mock("../../services/avaliacaoService", () => ({
  AvaliacaoService: {
    listarAvaliacoes: vi.fn(() => Promise.resolve([
      {
        avaliacaoId: "a1",
        nota: 5,
        textoAvaliacao: "Simplesmente épico!",
        nomeJogo: "The Witcher 3",
        nomeUsuario: "Pedro",
        dataPublicacao: "2026-09-10T12:00:00Z"
      }
    ])),
    toggleCurtir: vi.fn()
  }
}));

vi.mock("../TelaHome/actions/TelaHomeActions", () => ({
  buscarJogos: vi.fn(() => Promise.resolve([])),
  criarAvaliacao: vi.fn(() => Promise.resolve({}))
}));

vi.mock("../../context/AuthContext", () => ({
  useAuth: () => ({ user: { id: "u1", nome: "Tester" }, token: "token" })
}));

vi.mock("../../context/ToastContext", () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn() })
}));

describe("PaginaComunidade Component", () => {
  it("deve renderizar o cabeçalho da comunidade e seções de tendências", async () => {
    render(
      <BrowserRouter>
        <PaginaComunidade />
      </BrowserRouter>
    );

    expect(screen.getByText("Tendências & Resenhas dos Jogadores")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Elden Ring")).toBeInTheDocument();
      expect(screen.getByText("The Witcher 3")).toBeInTheDocument();
      expect(screen.getByText("Melhores Soulslike")).toBeInTheDocument();
      expect(screen.getByText('"Simplesmente épico!"')).toBeInTheDocument();
    });
  });

  it("deve renderizar os botões de filtro de estrelas exatas", async () => {
    render(
      <BrowserRouter>
        <PaginaComunidade />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText("Todas")).toBeInTheDocument();
      expect(screen.getByText("5 Estrelas")).toBeInTheDocument();
      expect(screen.getByText("4 Estrelas")).toBeInTheDocument();
      expect(screen.getByText("3 Estrelas")).toBeInTheDocument();
      expect(screen.getByText("2 Estrelas")).toBeInTheDocument();
      expect(screen.getByText("1 Estrela")).toBeInTheDocument();
    });
  });
});
