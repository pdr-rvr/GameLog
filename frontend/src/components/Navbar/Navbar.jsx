import React, { useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { FaUserCircle, FaBars, FaTimes } from "react-icons/fa";
import { useAuth } from "../../context/AuthContext";
import "./Navbar.css";

const Navbar = ({ onPublicarClick }) => {
  const { user, isAuthenticated, logout } = useAuth();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const handleDropdownToggle = () => {
    setDropdownOpen(!dropdownOpen);
  };

  const handleMobileMenuToggle = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  const handleLogout = () => {
    logout();
    setDropdownOpen(false);
    setIsMobileMenuOpen(false);
    navigate("/login");
  };

  const handlePublicar = () => {
    if (onPublicarClick) {
      onPublicarClick();
    } else {
      navigate("/home?publish=true");
    }
  };

  return (
    <nav className="navbar">
      <div className="navbar-logo">
        <Link to="/home">GameLog</Link>
      </div>

      <div className="menu-icon" onClick={handleMobileMenuToggle}>
        {isMobileMenuOpen ? <FaTimes /> : <FaBars />}
      </div>

      <ul className={`navbar-links ${isMobileMenuOpen ? "active" : ""}`}>
        <li>
          <Link to="/home" onClick={() => setIsMobileMenuOpen(false)}>Início</Link>
        </li>
        <li>
          <Link to="/jogos" onClick={() => setIsMobileMenuOpen(false)}>Jogos</Link>
        </li>
        <li>
          <Link to="/avaliacoes" onClick={() => setIsMobileMenuOpen(false)}>Avaliações</Link>
        </li>
      </ul>

      <div className="navbar-actions">
        {isAuthenticated ? (
          <>
            <button className="navbar-publish-button" onClick={handlePublicar}>
              Publicar
            </button>
            <div className="user-profile-menu">
              <button className="user-profile-button" onClick={handleDropdownToggle}>
                {user?.fotoDePerfil ? (
                  <img src={user.fotoDePerfil} alt="Perfil" className="user-avatar" />
                ) : (
                  <FaUserCircle className="user-icon" />
                )}
                <span className="user-name">{user?.nomeUsuario || "Minha Conta"}</span>
              </button>

              {dropdownOpen && (
                <div className="dropdown-menu">
                  <Link to={`/perfil/${user?.id}`} onClick={() => setDropdownOpen(false)}>
                    Meu Perfil
                  </Link>
                  <Link to="/minhas-avaliacoes" onClick={() => setDropdownOpen(false)}>
                    Minhas Avaliações
                  </Link>
                  <button onClick={handleLogout} className="logout-button">
                    Sair
                  </button>
                </div>
              )}
            </div>
          </>
        ) : (
          <div className="auth-buttons">
            <Link to="/login" className="btn-login">Entrar</Link>
            <Link to="/cadastro" className="btn-cadastro">Cadastrar</Link>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar;
