import React, { useRef } from 'react';
import MyAvaliacaoCard from '../MyAvaliacaoCard/MyAvaliacaoCard';
import './MyAvaliacaoCarrossel.css';

const MyAvaliacaoCarrossel = ({ title, avaliacoes, onEditReview, onDeleteReview }) => {
  const carouselRef = useRef(null);

  const scroll = (direction) => {
    if (carouselRef.current) {
      const cardContainerWidth = 330; 
      const scrollAmount = direction === 'left' ? -cardContainerWidth : cardContainerWidth;
      carouselRef.current.scrollBy({ left: scrollAmount, behavior: 'smooth' });
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
              <MyAvaliacaoCard
                avaliacao={avaliacao}
                onEdit={onEditReview}
                onDelete={onDeleteReview}
              />
            </div>
          ))}
        </div>
        <button className="carousel-nav-button right" onClick={() => scroll('right')}>&#8250;</button>
      </div>
    </div>
  );
};

export default MyAvaliacaoCarrossel;