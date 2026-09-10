import React, { useState, useEffect, useRef } from "react";
import { FaBuilding, FaSearch, FaTimes, FaCheck } from "react-icons/fa";
import "./StudioSearchInput.css";

const StudioSearchInput = ({
  empresas = [],
  empresaSelecionada = "",
  onSelectEmpresa,
  placeholder = "Buscar estúdio (ex: Nintendo, Capcom)..."
}) => {
  const [termo, setTermo] = useState(empresaSelecionada || "");
  const [aberto, setAberto] = useState(false);
  const containerRef = useRef(null);

  useEffect(() => {
    setTermo(empresaSelecionada || "");
  }, [empresaSelecionada]);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target)) {
        setAberto(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const sugestoes = termo.trim()
    ? empresas.filter((emp) =>
        emp.toLowerCase().includes(termo.trim().toLowerCase())
      ).slice(0, 8)
    : empresas.slice(0, 8);

  const handleInputChange = (e) => {
    const val = e.target.value;
    setTermo(val);
    setAberto(true);
    if (val === "") {
      onSelectEmpresa("");
    }
  };

  const handleSelect = (empresa) => {
    setTermo(empresa);
    setAberto(false);
    onSelectEmpresa(empresa);
  };

  const handleLimpar = (e) => {
    e.stopPropagation();
    setTermo("");
    setAberto(false);
    onSelectEmpresa("");
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") {
      e.preventDefault();
      if (sugestoes.length > 0) {
        handleSelect(sugestoes[0]);
      } else if (termo.trim()) {
        onSelectEmpresa(termo.trim());
        setAberto(false);
      }
    } else if (e.key === "Escape") {
      setAberto(false);
    }
  };

  return (
    <div className="studio-search-container" ref={containerRef}>
      <label className="studio-search-label">
        <FaBuilding /> Estúdio
      </label>

      <div className="studio-search-input-wrapper">
        <input
          type="text"
          value={termo}
          onChange={handleInputChange}
          onFocus={() => setAberto(true)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="studio-search-input"
        />

        {termo ? (
          <button
            type="button"
            className="btn-limpar-studio"
            onClick={handleLimpar}
            title="Limpar estúdio"
          >
            <FaTimes />
          </button>
        ) : (
          <FaSearch className="studio-search-icon" />
        )}
      </div>

      {aberto && sugestoes.length > 0 && (
        <div className="studio-suggestions-dropdown">
          {sugestoes.map((emp) => {
            const isSelected = emp.toLowerCase() === (empresaSelecionada || "").toLowerCase();
            return (
              <div
                key={emp}
                className={`studio-suggestion-item ${isSelected ? "selected" : ""}`}
                onClick={() => handleSelect(emp)}
              >
                <span className="studio-suggestion-name">{emp}</span>
                {isSelected && <FaCheck className="studio-check-icon" />}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default StudioSearchInput;
