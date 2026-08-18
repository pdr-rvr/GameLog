import React, { useRef } from 'react';
import JogoCard from '../JogoCard/JogoCard';
import './JogosCarrossel.css';

const JogosCarrossel = ({ title, jogos }) => {
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

    if (!jogos || jogos.length === 0) {
        return null;
    }

    return (
        <div className="jogos-carrossel-container">
            <h2 className="carrossel-title">{title}</h2>
            <div className="carrossel-wrapper">
                <button className="carousel-nav-button left" onClick={() => scroll('left')}>&#8249;</button>
                <div className="jogos-carrossel-track" ref={carouselRef}>
                    {jogos.map(jogo => (
                        <div key={jogo.jogoId} className="jogos-carrossel-item">
                            <JogoCard jogo={jogo} />
                        </div>
                    ))}
                </div>
                <button className="carousel-nav-button right" onClick={() => scroll('right')}>&#8250;</button>
            </div>
        </div>
    );
};

export default JogosCarrossel;