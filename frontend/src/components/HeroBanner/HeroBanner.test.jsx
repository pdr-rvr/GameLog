import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import HeroBanner from "./HeroBanner";

const mockJogos = [
  {
    jogoId: "01a092af-bc46-72bb-922f-bfc76774c385",
    titulo: "Elden Ring",
    descricao: "Nas Terras Intermedias regidas pela Rainha Marika...",
    imagem: "https://example.com/eldenring.jpg",
    dataLancamento: "2022-02-25",
    mediaAvaliacoes: 5.0,
    nomeEmpresa: "FromSoftware",
    generos: ["Acao", "RPG", "Fantasia"]
  },
  {
    jogoId: "01a092af-bc47-79b5-9fca-1da49c7a27d5",
    titulo: "Baldur's Gate III",
    descricao: "Um RPG monumental baseado em D&D...",
    imagem: "https://example.com/bg3.jpg",
    dataLancamento: "2023-08-03",
    mediaAvaliacoes: 4.9,
    nomeEmpresa: "Larian Studios",
    generos: ["RPG", "Estrategia"]
  }
];

describe("HeroBanner Component", () => {
  it("deve renderizar dados do jogo em destaque inicial", () => {
    render(
      <BrowserRouter>
        <HeroBanner jogos={mockJogos} />
      </BrowserRouter>
    );

    expect(screen.getByTestId("hero-title")).toHaveTextContent("Elden Ring");
    expect(screen.getByTestId("hero-rating")).toHaveTextContent("5.0");
    expect(screen.getByTestId("hero-description")).toHaveTextContent("Nas Terras Intermedias");
    expect(screen.getByText("FromSoftware")).toBeInTheDocument();
  });

  it("deve alternar para o próximo jogo ao clicar na seta direita", () => {
    render(
      <BrowserRouter>
        <HeroBanner jogos={mockJogos} />
      </BrowserRouter>
    );

    const btnNext = screen.getByLabelText("Próximo destaque");
    fireEvent.click(btnNext);

    expect(screen.getByTestId("hero-title")).toHaveTextContent("Baldur's Gate III");
  });

  it("não deve quebrar se jogos for vazio", () => {
    const { container } = render(
      <BrowserRouter>
        <HeroBanner jogos={[]} />
      </BrowserRouter>
    );
    expect(container.firstChild).toBeNull();
  });
});
