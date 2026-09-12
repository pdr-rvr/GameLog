import React from "react";
import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect, vi, beforeEach } from "vitest";
import PaginaFeedSocial from "./PaginaFeedSocial";
import { SocialService } from "../../services/socialService";
import { AvaliacaoService } from "../../services/avaliacaoService";

vi.mock("../../services/socialService", () => ({
  SocialService: {
    obterFeedSocial: vi.fn(() => Promise.resolve({
      itens: [
        {
          avaliacaoId: "eval-1",
          tipoAtividade: "Avaliacao",
          nota: 5,
          textoAvaliacao: "Incrível demais!",
          nomeJogo: "Zelda: Tears of the Kingdom",
          nomeUsuario: "LinkHero",
          totalCurtidas: 3,
          curtidaPorMim: false,
          totalRespostas: 1
        },
        {
          id: "comm-1",
          tipoAtividade: "Discussao",
          dataAtividade: "2026-09-10T12:00:00Z",
          autorId: "u4",
          autorNome: "ZeldaPrincess",
          autorAvaliacaoRespondidaNome: "LinkHero",
          avaliacaoId: "eval-1",
          comentarioTexto: "Excelente análise, concordo com os pontos sobre a história!",
          jogoId: "j1",
          jogoTitulo: "Zelda: Tears of the Kingdom",
          totalCurtidas: 2
        }
      ],
      amigosJogandoAgora: [
        { usuarioId: "u2", usuarioNome: "Mario", jogoId: "j1", jogoTitulo: "Super Mario Wonder" }
      ],
      gamersSugeridos: [
        { usuarioId: "u3", nomeUsuario: "Samus", bio: "Bounty Hunter", seguidoPorMim: false }
      ]
    })),
    obterAtividadesTimeline: vi.fn(() => Promise.resolve([
      {
        id: "act-1",
        tipo: "Zerou",
        dataAtividade: "2026-09-10T12:00:00Z",
        usuarioId: "u2",
        usuarioNome: "Mario",
        jogoId: "j2",
        jogoTitulo: "Metroid Dread"
      }
    ])),
    alternarSeguir: vi.fn(() => Promise.resolve({ mensagem: "Seguindo com sucesso!" }))
  }
}));

vi.mock("../../services/avaliacaoService", () => ({
  AvaliacaoService: {
    toggleCurtir: vi.fn(() => Promise.resolve({}))
  }
}));

vi.mock("../TelaHome/actions/TelaHomeActions", () => ({
  buscarJogos: vi.fn(() => Promise.resolve([])),
  criarAvaliacao: vi.fn(() => Promise.resolve({}))
}));

const mockUser = { id: "u1", nome: "Tester" };

vi.mock("../../context/AuthContext", () => ({
  useAuth: () => ({ user: mockUser, isAuthenticated: true, token: "token" })
}));

vi.mock("../../context/ToastContext", () => ({
  useToast: () => ({ success: vi.fn(), error: vi.fn(), info: vi.fn() })
}));

describe("PaginaFeedSocial Component", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("deve renderizar o Feed Social com reviews, discussões e sidebar", async () => {
    render(
      <BrowserRouter>
        <PaginaFeedSocial />
      </BrowserRouter>
    );

    expect(screen.getByTestId("tab-feed-social")).toBeInTheDocument();
    expect(screen.getByTestId("tab-atividades")).toBeInTheDocument();
    expect(screen.getByTestId("filter-todos")).toBeInTheDocument();
    expect(screen.getByTestId("filter-avaliacoes")).toBeInTheDocument();
    expect(screen.getByTestId("filter-discussoes")).toBeInTheDocument();
    expect(screen.getByTestId("filter-colecoes")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("LinkHero")).toBeInTheDocument();
      expect(screen.getByText('"Incrível demais!"')).toBeInTheDocument();
      expect(screen.getByText("ZeldaPrincess")).toBeInTheDocument();
      expect(screen.getByText('"Excelente análise, concordo com os pontos sobre a história!"')).toBeInTheDocument();
      expect(screen.getByText("Amigos Jogando Agora")).toBeInTheDocument();
      expect(screen.getByText("Gamers Sugeridos")).toBeInTheDocument();
    });
  });

  it("deve filtrar por Discussões ao clicar no filtro", async () => {
    render(
      <BrowserRouter>
        <PaginaFeedSocial />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText("ZeldaPrincess")).toBeInTheDocument();
    });

    const btnFilterDiscussoes = screen.getByTestId("filter-discussoes");
    fireEvent.click(btnFilterDiscussoes);

    // Deve exibir o card de discussão
    expect(screen.getByText(/Excelente análise, concordo com os pontos/)).toBeInTheDocument();
    // E não deve exibir a resenha pura
    expect(screen.queryByText(/Incrível demais!/)).not.toBeInTheDocument();
  });

  it("deve alternar para a aba de Atividades e renderizar a timeline", async () => {
    render(
      <BrowserRouter>
        <PaginaFeedSocial />
      </BrowserRouter>
    );

    const btnTabAtividades = screen.getByTestId("tab-atividades");
    fireEvent.click(btnTabAtividades);

    await waitFor(() => {
      expect(screen.getByText(/marcou como/)).toBeInTheDocument();
      expect(screen.getByText("zerado")).toBeInTheDocument();
      expect(screen.getByText("Metroid Dread")).toBeInTheDocument();
    });
  });
});
