import React, { useRef, useState, useEffect } from "react";
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
  FaCrosshairs,
  FaBolt,
  FaDiceD20,
  FaLayerGroup,
  FaHeart,
  FaChevronLeft,
  FaChevronRight,
  FaCar
} from "react-icons/fa";
import "./CategoryPills.css";

const CATEGORIAS_PADRAO = [
  { nome: "RPG", icone: FaDragon, cor: "#8b5cf6" },
  { nome: "Soulslike", icone: FaGhost, cor: "#6366f1" },
  { nome: "Ação", icone: FaFistRaised, cor: "#ef4444" },
  { nome: "Aventura", icone: FaCompass, cor: "#3b82f6" },
  { nome: "Mundo Aberto", icone: FaGlobeAmericas, cor: "#10b981" },
  { nome: "Metroidvania", icone: FaLayerGroup, cor: "#06b6d4" },
  { nome: "Roguelike / Roguelite", icone: FaDiceD20, cor: "#d946ef" },
  { nome: "Terror & Sobrevivência", icone: FaSkullCrossbones, cor: "#ec4899" },
  { nome: "Survival Horror", icone: FaGhost, cor: "#e11d48" },
  { nome: "Hack and Slash", icone: FaBolt, cor: "#f97316" },
  { nome: "Tiro", icone: FaCrosshairs, cor: "#eab308" },
  { nome: "Estratégia", icone: FaChess, cor: "#f59e0b" },
  { nome: "RTS", icone: FaChess, cor: "#38bdf8" },
  { nome: "Ficção Científica & Cyberpunk", icone: FaRocket, cor: "#14b8a6" },
  { nome: "Corrida", icone: FaCar, cor: "#fb923c" },
  { nome: "Luta", icone: FaFistRaised, cor: "#f43f5e" },
  { nome: "Indie", icone: FaHeart, cor: "#ec4899" }
];

const CategoryPills = ({ categorias = CATEGORIAS_PADRAO, titulo = "Explorar por Gêneros e Categorias" }) => {
  const trackRef = useRef(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);

  const checkScroll = () => {
    if (trackRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = trackRef.current;
      setCanScrollLeft(scrollLeft > 10);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  useEffect(() => {
    checkScroll();
    const el = trackRef.current;
    if (el) {
      el.addEventListener("scroll", checkScroll, { passive: true });
      window.addEventListener("resize", checkScroll);
      return () => {
        el.removeEventListener("scroll", checkScroll);
        window.removeEventListener("resize", checkScroll);
      };
    }
  }, [categorias]);

  const handleScroll = (direction) => {
    if (trackRef.current) {
      const scrollAmount = 320;
      trackRef.current.scrollBy({
        left: direction === "left" ? -scrollAmount : scrollAmount,
        behavior: "smooth"
      });
    }
  };

  return (
    <section className="category-pills-section" data-testid="category-pills-section">
      <div className="category-pills-header">
        {titulo && <h2 className="category-pills-title">{titulo}</h2>}
        
        <div className="category-pills-nav-controls">
          <button 
            type="button"
            className={`category-pills-nav-btn prev ${!canScrollLeft ? "disabled" : ""}`}
            onClick={() => handleScroll("left")}
            aria-label="Rolar categorias para esquerda"
            data-testid="category-pills-prev"
            disabled={!canScrollLeft}
          >
            <FaChevronLeft />
          </button>
          <button 
            type="button"
            className={`category-pills-nav-btn next ${!canScrollRight ? "disabled" : ""}`}
            onClick={() => handleScroll("right")}
            aria-label="Rolar categorias para direita"
            data-testid="category-pills-next"
            disabled={!canScrollRight}
          >
            <FaChevronRight />
          </button>
        </div>
      </div>
      
      <div className="category-pills-carousel-wrapper">
        <div 
          className={`category-pills-track ${canScrollLeft ? "has-scroll-left" : ""} ${canScrollRight ? "has-scroll-right" : ""}`}
          ref={trackRef}
        >
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
      </div>
    </section>
  );
};

export default CategoryPills;
