import React, { useState, useEffect, useRef, useMemo } from 'react';
import { FaChevronLeft, FaChevronRight } from 'react-icons/fa';
import JogoCard from '../JogoCard/JogoCard';
import './JogosCarrossel.css';

const CLONES = 6;
const CARD_WIDTH_DESKTOP = 175;
const CARD_WIDTH_MOBILE = 145;
const GAP_PX = 16;

const JogosCarrossel = ({ title, jogos }) => {
  const totalOriginal = jogos ? jogos.length : 0;
  const isCircular = totalOriginal >= CLONES;

  // Lightweight buffer: only 6 clones at head and 6 clones at tail
  const items = useMemo(() => {
    if (!jogos || jogos.length === 0) return [];
    if (!isCircular) return jogos;
    const head = jogos.slice(-CLONES).map((j, i) => ({ ...j, _uid: `head-${j.jogoId || j.id || i}-${i}` }));
    const middle = jogos.map((j, i) => ({ ...j, _uid: `mid-${j.jogoId || j.id || i}-${i}` }));
    const tail = jogos.slice(0, CLONES).map((j, i) => ({ ...j, _uid: `tail-${j.jogoId || j.id || i}-${i}` }));
    return [...head, ...middle, ...tail];
  }, [jogos, isCircular]);

  // Initial index starts directly at CLONES (pointing to first real item)
  const [currentIndex, setCurrentIndex] = useState(isCircular ? CLONES : 0);
  const [isTransitioning, setIsTransitioning] = useState(true);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);
  const touchStartX = useRef(null);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth < 768);
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const isAnimatingRef = useRef(false);

  useEffect(() => {
    if (isCircular) {
      setIsTransitioning(false);
      setCurrentIndex(CLONES);
    }
  }, [totalOriginal, isCircular]);

  const handleTransitionEnd = () => {
    isAnimatingRef.current = false;
    if (!isCircular) return;

    if (currentIndex >= totalOriginal + CLONES) {
      setIsTransitioning(false);
      setCurrentIndex(currentIndex - totalOriginal);
    } else if (currentIndex < CLONES) {
      setIsTransitioning(false);
      setCurrentIndex(currentIndex + totalOriginal);
    }
  };

  useEffect(() => {
    if (!isTransitioning) {
      const frame = requestAnimationFrame(() => {
        setIsTransitioning(true);
      });
      return () => cancelAnimationFrame(frame);
    }
  }, [isTransitioning]);

  const cardWidth = isMobile ? CARD_WIDTH_MOBILE : CARD_WIDTH_DESKTOP;
  const stride = cardWidth + GAP_PX;

  const slide = (direction) => {
    if (isAnimatingRef.current) return;
    isAnimatingRef.current = true;

    const step = isMobile ? 1 : 2;
    setIsTransitioning(true);
    if (direction === 'next') {
      setCurrentIndex(prev => prev + step);
    } else {
      setCurrentIndex(prev => prev - step);
    }

    setTimeout(() => {
      isAnimatingRef.current = false;
    }, 420);
  };

  // Touch swipe
  const handleTouchStart = (e) => {
    touchStartX.current = e.touches[0].clientX;
  };

  const handleTouchEnd = (e) => {
    if (touchStartX.current === null) return;
    const diff = touchStartX.current - e.changedTouches[0].clientX;
    if (Math.abs(diff) > 35) {
      if (diff > 0) {
        slide('next');
      } else {
        slide('prev');
      }
    }
    touchStartX.current = null;
  };

  if (!jogos || jogos.length === 0) {
    return null;
  }

  const translateStyle = {
    transform: `translate3d(-${currentIndex * stride}px, 0, 0)`,
    transition: isTransitioning ? 'transform 0.4s cubic-bezier(0.16, 1, 0.3, 1)' : 'none',
  };

  return (
    <section className="jogos-carrossel-section">
      <div className="carrossel-header">
        <h2 className="carrossel-title">{title}</h2>

        <div className="carrossel-header-controls">
          <button
            type="button"
            className="carousel-header-btn"
            onClick={() => slide('prev')}
            aria-label="Anterior"
          >
            <FaChevronLeft />
          </button>
          <button
            type="button"
            className="carousel-header-btn"
            onClick={() => slide('next')}
            aria-label="Próximo"
          >
            <FaChevronRight />
          </button>
        </div>
      </div>

      <div 
        className="carrossel-viewport"
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <div 
          className="jogos-carrossel-slider" 
          style={translateStyle}
          onTransitionEnd={handleTransitionEnd}
        >
          {items.map((jogo, idx) => (
            <div 
              key={jogo._uid || jogo.jogoId || jogo.id || idx} 
              className="jogos-carrossel-item"
            >
              <JogoCard jogo={jogo} />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default JogosCarrossel;