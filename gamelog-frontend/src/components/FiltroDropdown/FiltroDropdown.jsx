import React, { useState, useRef, useEffect } from 'react';
import './FiltroDropdown.css';

const FiltroDropdown = ({ title, options, onSelect, type = 'list', currentSelection }) => {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef(null);
    const inputRef = useRef(null);

    const toggleDropdown = () => setIsOpen(!isOpen);

    const handleSelect = (value) => {
        onSelect(value);
        setIsOpen(false);
    };

    const handleClickOutside = (event) => {
        if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
            setIsOpen(false);
        }
    };

    useEffect(() => {
        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    const renderOptions = () => {
        if (type === 'year-range') {
            const years = [];
            for (let i = new Date().getFullYear(); i >= 1980; i--) {
                years.push(i);
            }
            const ranges = [
                '2020s (2020-2029)', '2010s (2010-2019)', '2000s (2000-2009)',
                '1990s (1990-1999)', '1980s (1980-1989)', '1970s (1970-1979)',
                'Pre-1970'
            ];

            return (
                <>
                    <input
                        ref={inputRef}
                        type="number"
                        placeholder="Digite o ano"
                        className="dropdown-year-input"
                        onChange={(e) => handleSelect(e.target.value)}
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') {
                                handleSelect(e.target.value);
                                setIsOpen(false);
                            }
                        }}
                    />
                    <div className="dropdown-options-group-title">Intervalos de tempo:</div>
                    {ranges.map((range, index) => (
                        <div key={index} className="dropdown-option" onClick={() => handleSelect(range)}>
                            {range}
                        </div>
                    ))}
                     <div className="dropdown-options-group-title">Anos específicos:</div>
                    {years.map(year => (
                        <div key={year} className="dropdown-option" onClick={() => handleSelect(year.toString())}>
                            {year}
                        </div>
                    ))}
                </>
            );
        } else if (type === 'rating') {
             return (
                <>
                    <div className="dropdown-option" onClick={() => handleSelect('popular')}>Mais Populares</div>
                    <div className="dropdown-option" onClick={() => handleSelect('less-popular')}>Menos Populares</div>
                    <div className="dropdown-option" onClick={() => handleSelect('top-25-popular')}>Top 25 Mais Populares</div>
                    <div className="dropdown-option" onClick={() => handleSelect('top-25-less-popular')}>Top 25 Menos Populares</div>
                </>
            );
        } else {
            return options.map((option, index) => (
                <div key={index} className="dropdown-option" onClick={() => handleSelect(option.value)}>
                    {option.label}
                </div>
            ));
        }
    };

    return (
        <div className="filtro-dropdown" ref={dropdownRef}>
            <button className="dropdown-toggle" onClick={toggleDropdown}>
                {currentSelection || title} <span className="arrow">&#9662;</span>
            </button>
            {isOpen && (
                <div className="dropdown-menu">
                    {renderOptions()}
                </div>
            )}
        </div>
    );
};

export default FiltroDropdown;