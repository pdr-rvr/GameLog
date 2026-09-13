import React from "react";
import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi, beforeEach } from "vitest";
import PaginaJogos from "./PaginaJogos";
import { JogoService } from "../../services/jogoService";

vi.mock("../../services/jogoService", () => ({
  JogoService: {
    obterMetadadosFiltros: vi.fn(),
    buscarJogosPaginados: vi.fn()
  }
}));

vi.mock("../../context/ToastContext", () => ({
  useToast: () => ({ showToast: vi.fn() })
}));

vi.mock("../../context/AuthContext", () => ({
  useAuth: () => ({ usuario: { id: "user1", nome: "Tester" } })
}));

describe("PaginaJogos Component", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    JogoService.obterMetadadosFiltros.mockResolvedValue({
      generos: ["Ação", "RPG", "Soulslike"],
      anos: [2024, 2023, 2022]
    });

    JogoService.buscarJogosPaginados.mockResolvedValue({
      itens: [
        {
          id: "1",
          jogoId: "1",
          titulo: "Elden Ring",
          imagem: "er.jpg",
          nomeDesenvolvedora: "FromSoftware",
          genero: "Soulslike",
          dataLancamento: "2022-02-25",
          mediaAvaliacoes: 4.9,
          totalAvaliacoes: 100
        }
      ],
      totalPaginas: 1,
      totalItens: 1,
      paginaAtual: 1
    });
  });

  it("deve renderizar o título e carregar jogos iniciais", async () => {
    render(
      <BrowserRouter>
        <PaginaJogos />
      </BrowserRouter>
    );

    expect(screen.getByText("Catálogo de Jogos")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Elden Ring")).toBeInTheDocument();
    });
  });

  it("deve permitir adicionar múltiplos gêneros e exibir os chips", async () => {
    render(
      <BrowserRouter>
        <PaginaJogos />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(JogoService.obterMetadadosFiltros).toHaveBeenCalled();
    });

    // Selecionar Gênero RPG
    const selects = screen.getAllByRole("combobox");
    const genreSelect = selects[0];
    fireEvent.change(genreSelect, { target: { value: "RPG" } });

    await waitFor(() => {
      expect(screen.getByText("Gêneros selecionados:")).toBeInTheDocument();
      expect(screen.getByText("RPG")).toBeInTheDocument();
    });
  });
});
