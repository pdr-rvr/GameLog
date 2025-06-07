import React, { useRef } from 'react';
import AvaliacaoCard from '../AvaliacaoCard/AvaliacaoCard';
import './AvaliacaoCarrossel'; 

const AvaliacoesCarrossel = ({ title, avaliacoes }) => {
  const carouselRef = useRef(null);

  const scroll = (direction) => {
    if (carouselRef.current) {
      const scrollAmount = carouselRef.current.clientWidth / 2;
      if (direction === 'left') {
        carouselRef.current.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
      } else {
        carouselRef.current.scrollBy({ left: scrollAmount, behavior: 'smooth' });
      }
    }
  };

  if (!avaliacoes || avaliacoes.length === 0) {
    return null; 
  }

  return (
    <div className="avaliacoes-carrossel-container">
      <h2 className="carrossel-title">{title}</h2>
      <div className="carrossel-wrapper">
        <button className="carousel-nav-button left" onClick={() => scroll('left')}>&#8249;</button>
        <div className="avaliacoes-carrossel-track" ref={carouselRef}>
          {avaliacoes.map(avaliacao => (
            <div key={avaliacao.avaliacaoId} className="avaliacoes-carrossel-item">
              <AvaliacaoCard avaliacao={avaliacao} />
            </div>
          ))}
        </div>
        <button className="carousel-nav-button right" onClick={() => scroll('right')}>&#8250;</button>
      </div>
    </div>
  );
};

export default AvaliacoesCarrossel;