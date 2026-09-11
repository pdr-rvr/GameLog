import React from "react";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import CategoryPills from "./CategoryPills";

describe("CategoryPills Component", () => {
  it("deve renderizar o título e categorias padrão", () => {
    render(
      <BrowserRouter>
        <CategoryPills />
      </BrowserRouter>
    );

    expect(screen.getByText("Explorar por Gêneros e Categorias")).toBeInTheDocument();
    expect(screen.getByText("RPG")).toBeInTheDocument();
    expect(screen.getByText("Ação")).toBeInTheDocument();
    expect(screen.getByText("Mundo Aberto")).toBeInTheDocument();
  });

  it("deve criar links com os parâmetros de gênero corretos", () => {
    render(
      <BrowserRouter>
        <CategoryPills />
      </BrowserRouter>
    );

    const rpgLink = screen.getByTestId("category-pill-RPG");
    expect(rpgLink).toHaveAttribute("href", "/jogos?genero=RPG");
  });
});
