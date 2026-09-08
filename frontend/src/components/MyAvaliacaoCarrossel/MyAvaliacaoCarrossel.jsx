import React, { useState, useEffect, useRef, useMemo } from 'react';
import { FaChevronLeft, FaChevronRight } from 'react-icons/fa';
import MyAvaliacaoCard from '../MyAvaliacaoCard/MyAvaliacaoCard';
import './MyAvaliacaoCarrossel.css';

const CLONES = 4;
const CARD_WIDTH_DESKTOP = 340;
const CARD_WIDTH_MOBILE = 290;
const GAP_DESKTOP = 16;
const GAP_MOBILE = 12;

const MyAvaliacaoCarrossel = ({ title, avaliacoes, onEditReview, onDeleteReview }) => {
  const totalOriginal = avaliacoes ? avaliacoes.length : 0;
  const isCircular = totalOriginal >= CLONES;

  const items = useMemo(() => {
    if (!avaliacoes || avaliacoes.length === 0) return [];
    if (!isCircular) return avaliacoes;
    const head = avaliacoes.slice(-CLONES).map((a, i) => ({ ...a, _uid: `head-${a.avaliacaoId || a.id || i}-${i}` }));
    const middle = avaliacoes.map((a, i) => ({ ...a, _uid: `mid-${a.avaliacaoId || a.id || i}-${i}` }));
    const tail = avaliacoes.slice(0, CLONES).map((a, i) => ({ ...a, _uid: `tail-${a.avaliacaoId || a.id || i}-${i}` }));
    return [...head, ...middle, ...tail];
  }, [avaliacoes, isCircular]);

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

  useEffect(() => {
    if (isCircular) {
      setIsTransitioning(false);
      setCurrentIndex(CLONES);
    } else {
      setCurrentIndex(0);
    }
  }, [totalOriginal, isCircular]);

  const handleTransitionEnd = () => {
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
  const gap = isMobile ? GAP_MOBILE : GAP_DESKTOP;
  const stride = cardWidth + gap;

  const slide = (direction) => {
    if (!isCircular) {
      if (direction === 'next') {
        setCurrentIndex(prev => Math.min(prev + 1, totalOriginal - 1));
      } else {
        setCurrentIndex(prev => Math.max(prev - 1, 0));
      }
      return;
    }

    setIsTransitioning(true);
    if (direction === 'next') {
      setCurrentIndex(prev => prev + 1);
    } else {
      setCurrentIndex(prev => prev - 1);
    }
  };

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

  if (!avaliacoes || avaliacoes.length === 0) {
    return null;
  }

  const translateStyle = {
    transform: `translate3d(-${currentIndex * stride}px, 0, 0)`,
    transition: isTransitioning ? 'transform 0.4s cubic-bezier(0.16, 1, 0.3, 1)' : 'none',
  };

  const showControls = totalOriginal > 1;

  return (
    <section className="my-avaliacoes-carrossel-section">
      <div className="my-avaliacoes-carrossel-header">
        <h2 className="my-avaliacoes-carrossel-title">{title}</h2>

        {showControls && (
          <div className="my-avaliacoes-header-controls">
            <button
              type="button"
              className="my-avaliacoes-header-btn"
              onClick={() => slide('prev')}
              disabled={!isCircular && currentIndex === 0}
              aria-label="Avaliação anterior"
            >
              <FaChevronLeft />
            </button>
            <button
              type="button"
              className="my-avaliacoes-header-btn"
              onClick={() => slide('next')}
              disabled={!isCircular && currentIndex >= totalOriginal - 1}
              aria-label="Próxima avaliação"
            >
              <FaChevronRight />
            </button>
          </div>
        )}
      </div>

      <div 
        className="my-avaliacoes-viewport"
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <div 
          className="my-avaliacoes-slider" 
          style={translateStyle}
          onTransitionEnd={handleTransitionEnd}
        >
          {items.map((avaliacao, idx) => (
            <div 
              key={avaliacao._uid || avaliacao.avaliacaoId || avaliacao.id || idx} 
              className="my-avaliacoes-item"
            >
              <MyAvaliacaoCard
                avaliacao={avaliacao}
                onEdit={onEditReview}
                onDelete={onDeleteReview}
              />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default MyAvaliacaoCarrossel;