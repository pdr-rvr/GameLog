import React from "react";
import { Link } from "react-router-dom";
import { 
  FaDragon, 
  FaFistRaised, 
  FaCompass, 
  FaGhost, 
  FaGlobeAmericas, 
  FaChess, 
  FaRocket, 
  FaGamepad, 
  FaSkullCrossbones,
  FaHatWizard
} from "react-icons/fa";
import "./CategoryPills.css";

const CATEGORIAS_PADRAO = [
  { nome: "RPG", icone: FaDragon, cor: "#8b5cf6" },
  { nome: "Ação", icone: FaFistRaised, cor: "#ef4444" },
  { nome: "Aventura", icone: FaCompass, cor: "#3b82f6" },
  { nome: "Fantasia Sombria", icone: FaGhost, cor: "#6366f1" },
  { nome: "Mundo Aberto", icone: FaGlobeAmericas, cor: "#10b981" },
  { nome: "Estratégia", icone: FaChess, cor: "#f59e0b" },
  { nome: "Sci-Fi", icone: FaRocket, cor: "#06b6d4" },
  { nome: "Terror", icone: FaSkullCrossbones, cor: "#ec4899" },
  { nome: "Fantasia", icone: FaHatWizard, cor: "#a855f7" },
  { nome: "Indie", icone: FaGamepad, cor: "#14b8a6" }
];

const CategoryPills = ({ categorias = CATEGORIAS_PADRAO, titulo = "Explorar por Gêneros e Categorias" }) => {
  return (
    <section className="category-pills-section" data-testid="category-pills-section">
      {titulo && <h2 className="category-pills-title">{titulo}</h2>}
      
      <div className="category-pills-grid">
        {categorias.map((cat, idx) => {
          const IconComponent = cat.icone || FaGamepad;
          return (
            <Link
              key={idx}
              to={`/jogos?genero=${encodeURIComponent(cat.nome)}`}
              className="category-pill-card"
              style={{ "--pill-accent": cat.cor || "#3b82f6" }}
              data-testid={`category-pill-${cat.nome}`}
            >
              <div className="category-pill-icon-wrapper">
                <IconComponent className="category-pill-icon" />
              </div>
              <span className="category-pill-name">{cat.nome}</span>
            </Link>
          );
        })}
      </div>
    </section>
  );
};

export default CategoryPills;
