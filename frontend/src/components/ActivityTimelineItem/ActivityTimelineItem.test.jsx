import React from "react";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import ActivityTimelineItem from "./ActivityTimelineItem";

const mockItem = {
  id: "act-1",
  tipo: "Zerou",
  dataAtividade: new Date().toISOString(),
  usuarioId: "user-1",
  usuarioNome: "ana_gamer",
  usuarioFoto: "https://example.com/ana.jpg",
  jogoId: "jogo-1",
  jogoTitulo: "Elden Ring",
  jogoImagem: "https://example.com/eldenring.jpg"
};

describe("ActivityTimelineItem Component", () => {
  it("deve renderizar o autor, ação de zerado e o jogo", () => {
    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("ana_gamer")).toBeInTheDocument();
    expect(screen.getByText(/marcou como/)).toBeInTheDocument();
    expect(screen.getByText("zerado")).toBeInTheDocument();
    expect(screen.getByText("Elden Ring")).toBeInTheDocument();
    expect(screen.getByText("agora mesmo")).toBeInTheDocument();
  });

  it("deve renderizar atividade de lista com contagem de jogos", () => {
    const mockListaItem = {
      id: "act-2",
      tipo: "CriouLista",
      dataAtividade: new Date().toISOString(),
      usuarioId: "user-2",
      usuarioNome: "pedro",
      listaId: "lista-1",
      listaTitulo: "RPGs Lendários",
      totalJogos: 5
    };

    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockListaItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("pedro")).toBeInTheDocument();
    expect(screen.getByText(/criou uma nova/)).toBeInTheDocument();
    expect(screen.getByText("RPGs Lendários")).toBeInTheDocument();
    expect(screen.getByText("5 jogos adicionados")).toBeInTheDocument();
  });

  it("deve renderizar atividade de comentário em análise", () => {
    const mockCommentItem = {
      id: "act-3",
      tipo: "Comentou",
      dataAtividade: new Date().toISOString(),
      usuarioId: "user-3",
      usuarioNome: "carlos",
      autorAvaliacaoRespondidaNome: "lucas",
      jogoId: "jogo-2",
      jogoTitulo: "God of War",
      comentarioTexto: "Excelente análise, concordo com os pontos sobre o combate!"
    };

    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockCommentItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("carlos")).toBeInTheDocument();
    expect(screen.getByText("@lucas")).toBeInTheDocument();
    expect(screen.getByText("God of War")).toBeInTheDocument();
    expect(screen.getByText('"Excelente análise, concordo com os pontos sobre o combate!"')).toBeInTheDocument();
  });

  it("deve renderizar atividade de avaliação com nota e minianálise", () => {
    const mockAvaliouItem = {
      id: "act-4",
      tipo: "Avaliou",
      dataAtividade: new Date(Date.now() - 3600000 * 2).toISOString(), // há 2 horas
      usuarioId: "user-4",
      usuarioNome: "maria",
      jogoId: "jogo-3",
      jogoTitulo: "Hollow Knight",
      nota: 5,
      textoCurto: "Obra-prima dos metroidvanias!"
    };

    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockAvaliouItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("maria")).toBeInTheDocument();
    expect(screen.getByText(/avaliou com/)).toBeInTheDocument();
    expect(screen.getByText("5/5")).toBeInTheDocument();
    expect(screen.getByText("Hollow Knight")).toBeInTheDocument();
    expect(screen.getByText('"Obra-prima dos metroidvanias!"')).toBeInTheDocument();
  });

  it("deve renderizar atividade de biblioteca com status Quero Jogar", () => {
    const mockBibItem = {
      id: "act-5",
      tipo: "AdicionouBiblioteca",
      dataAtividade: new Date(Date.now() - 3600000 * 25).toISOString(), // ontem
      usuarioId: "user-5",
      usuarioNome: "lucas",
      statusBiblioteca: "QueroJogar",
      jogoId: "jogo-4",
      jogoTitulo: "Cyberpunk 2077"
    };

    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockBibItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("lucas")).toBeInTheDocument();
    expect(screen.getByText(/adicionou à biblioteca \(Quero Jogar\)/)).toBeInTheDocument();
  });

  it("deve renderizar atividade de adição de jogo à coleção", () => {
    const mockAddListaItem = {
      id: "act-6",
      tipo: "AdicionouJogoLista",
      dataAtividade: new Date(Date.now() - 3600000 * 50).toISOString(), // há 2 dias
      usuarioId: "user-6",
      usuarioNome: "joao",
      listaTitulo: "Meus Favoritos",
      jogoId: "jogo-5",
      jogoTitulo: "Zelda BotW"
    };

    render(
      <BrowserRouter>
        <ActivityTimelineItem item={mockAddListaItem} />
      </BrowserRouter>
    );

    expect(screen.getByText("joao")).toBeInTheDocument();
    expect(screen.getByText(/adicionou à coleção/)).toBeInTheDocument();
    expect(screen.getByText("Meus Favoritos")).toBeInTheDocument();
  });

  it("deve retornar null se item for nulo", () => {
    const { container } = render(
      <BrowserRouter>
        <ActivityTimelineItem item={null} />
      </BrowserRouter>
    );
    expect(container.firstChild).toBeNull();
  });
});

