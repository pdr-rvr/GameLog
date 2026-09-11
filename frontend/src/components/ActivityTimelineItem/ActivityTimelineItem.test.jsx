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
    expect(screen.getByText("marcou como zerado")).toBeInTheDocument();
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
    expect(screen.getByText("criou a coleção")).toBeInTheDocument();
    expect(screen.getByText("RPGs Lendários")).toBeInTheDocument();
    expect(screen.getByText("5 jogos adicionados")).toBeInTheDocument();
  });
});
