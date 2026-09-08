import React, { useState, useRef, useEffect } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { 
  FaGamepad, 
  FaUser, 
  FaStar, 
  FaSignOutAlt, 
  FaBars, 
  FaTimes, 
  FaPlus,
  FaHome,
  FaThLarge,
  FaComments
} from "react-icons/fa";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import "./Navbar.css";

const Navbar = ({ onPublicarClick }) => {
  const { user, isAuthenticated, logout } = useAuth();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const handleLogout = () => {
    logout();
    setDropdownOpen(false);
    setIsMobileMenuOpen(false);
    toast.info("Você foi desconectado com sucesso.");
    navigate("/login");
  };

  const handlePublicar = () => {
    if (onPublicarClick) {
      onPublicarClick();
    } else {
      navigate("/home?publish=true");
    }
  };

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const isActive = (path) => {
    if (path === "/home" && (location.pathname === "/" || location.pathname === "/home")) {
      return true;
    }
    return location.pathname === path;
  };

  return (
    <header className="navbar-wrapper">
      <div className="navbar-container">
        {/* Logo da Marca */}
        <Link to="/home" className="navbar-brand">
          <div className="navbar-brand-icon">
            <FaGamepad />
          </div>
          <span className="navbar-brand-text">
            Game<span>Log</span>
          </span>
        </Link>

        {/* Links de Navegação Desktop */}
        <nav className="navbar-links-desktop">
          <Link to="/home" className={`nav-item ${isActive("/home") ? "active" : ""}`}>
            <FaHome className="nav-icon" /> Início
          </Link>
          <Link to="/jogos" className={`nav-item ${isActive("/jogos") ? "active" : ""}`}>
            <FaThLarge className="nav-icon" /> Catálogo
          </Link>
          <Link to="/avaliacoes" className={`nav-item ${isActive("/avaliacoes") ? "active" : ""}`}>
            <FaComments className="nav-icon" /> Avaliações
          </Link>
        </nav>

        {/* Ações da Direita */}
        <div className="navbar-actions">
          {isAuthenticated ? (
            <>
              <button className="btn-publish-cta" onClick={handlePublicar}>
                <FaPlus /> <span>Publicar</span>
              </button>

              {/* Menu do Usuário */}
              <div className="user-menu-container" ref={dropdownRef}>
                <button 
                  className="user-avatar-button" 
                  onClick={() => setDropdownOpen(!dropdownOpen)}
                  aria-label="Abrir menu do usuário"
                >
                  {user?.fotoDePerfil ? (
                    <img src={user.fotoDePerfil} alt={user.nomeUsuario} className="user-avatar-img" />
                  ) : (
                    <span className="user-avatar-fallback">
                      {user?.nomeUsuario ? user.nomeUsuario.charAt(0).toUpperCase() : "G"}
                    </span>
                  )}
                </button>

                {dropdownOpen && (
                  <div className="user-dropdown-menu">
                    <div className="dropdown-user-info">
                      <p className="dropdown-user-name">{user?.nomeUsuario || "Gamer"}</p>
                      <p className="dropdown-user-email">{user?.email || ""}</p>
                    </div>

                    <div className="dropdown-divider"></div>

                    <Link 
                      to={`/perfil/${user?.id}`} 
                      className="dropdown-link" 
                      onClick={() => setDropdownOpen(false)}
                    >
                      <FaUser /> Meu Perfil
                    </Link>

                    <Link 
                      to="/minhas-avaliacoes" 
                      className="dropdown-link" 
                      onClick={() => setDropdownOpen(false)}
                    >
                      <FaStar /> Minhas Avaliações
                    </Link>

                    <div className="dropdown-divider"></div>

                    <button className="dropdown-link btn-dropdown-logout" onClick={handleLogout}>
                      <FaSignOutAlt /> Desconectar
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="auth-nav-buttons">
              <Link to="/login" className="btn-nav-login">Entrar</Link>
              <Link to="/cadastro" className="btn-nav-register">Cadastrar</Link>
            </div>
          )}

          {/* Botão Mobile Hamburger */}
          <button 
            className="mobile-menu-toggle" 
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            aria-label="Toggle menu"
          >
            {isMobileMenuOpen ? <FaTimes /> : <FaBars />}
          </button>
        </div>
      </div>

      {/* Menu Mobile Retrátil */}
      {isMobileMenuOpen && (
        <div className="navbar-mobile-drawer">
          <Link 
            to="/home" 
            className={`mobile-nav-link ${isActive("/home") ? "active" : ""}`}
            onClick={() => setIsMobileMenuOpen(false)}
          >
            <FaHome /> Início
          </Link>
          <Link 
            to="/jogos" 
            className={`mobile-nav-link ${isActive("/jogos") ? "active" : ""}`}
            onClick={() => setIsMobileMenuOpen(false)}
          >
            <FaThLarge /> Catálogo
          </Link>
          <Link 
            to="/avaliacoes" 
            className={`mobile-nav-link ${isActive("/avaliacoes") ? "active" : ""}`}
            onClick={() => setIsMobileMenuOpen(false)}
          >
            <FaComments /> Avaliações
          </Link>

          {isAuthenticated ? (
            <>
              <div className="mobile-drawer-divider"></div>
              <Link 
                to={`/perfil/${user?.id}`} 
                className="mobile-nav-link"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                <FaUser /> Meu Perfil
              </Link>
              <Link 
                to="/minhas-avaliacoes" 
                className="mobile-nav-link"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                <FaStar /> Minhas Avaliações
              </Link>
              <button className="mobile-nav-link mobile-logout" onClick={handleLogout}>
                <FaSignOutAlt /> Desconectar
              </button>
            </>
          ) : (
            <div className="mobile-auth-actions">
              <Link to="/login" className="btn-nav-login" onClick={() => setIsMobileMenuOpen(false)}>Entrar</Link>
              <Link to="/cadastro" className="btn-nav-register" onClick={() => setIsMobileMenuOpen(false)}>Cadastrar</Link>
            </div>
          )}
        </div>
      )}
    </header>
  );
};

export default Navbar;
