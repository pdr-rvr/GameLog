import React, { useState, useRef, useEffect } from 'react';
import './SearchBar.css';

const SearchBar = ({ onSearch, suggestions = [], onSelectSuggestion }) => {
    const [query, setQuery] = useState('');
    const [showSuggestions, setShowSuggestions] = useState(false);
    const searchBarRef = useRef(null);

    const handleChange = (e) => {
        const value = e.target.value;
        setQuery(value);
        if (value.length > 0) {
            onSearch(value);
            setShowSuggestions(true);
        } else {
            setShowSuggestions(false);
        }
    };

    const handleSelect = (suggestion) => {
        setQuery(suggestion.titulo);
        onSelectSuggestion(suggestion);
        setShowSuggestions(false);
    };

    const handleClickOutside = (event) => {
        if (searchBarRef.current && !searchBarRef.current.contains(event.target)) {
            setShowSuggestions(false);
        }
    };

    useEffect(() => {
        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    return (
        <div className="search-bar-container" ref={searchBarRef}>
            <input
                type="text"
                placeholder="Pesquisar jogos..."
                value={query}
                onChange={handleChange}
                className="search-input"
            />
            {showSuggestions && suggestions.length > 0 && (
                <div className="suggestions-dropdown">
                    {suggestions.map(jogo => (
                        <div 
                            key={jogo.jogoId} 
                            className="suggestion-item" 
                            onClick={() => handleSelect(jogo)}
                        >
                            {jogo.titulo} ({new Date(jogo.dataLancamento).getFullYear()})
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default SearchBar;