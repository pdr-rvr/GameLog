import React from 'react';
import { useNavigate } from 'react-router-dom';
import './Navbar.css';

const Navbar = ({ activeTab, setActiveTab, usuario }) => {
  const navigate = useNavigate();

  return (
    <div className="navbar">
      <div className="navbar-logo">GameLog</div>
      <div className="navbar-tabs">
        <button 
          className={`tab-button ${activeTab === 'home' ? 'active' : ''}`}
          onClick={() => setActiveTab('home')}
        >
          Home
        </button>
        <button 
          className={`tab-button ${activeTab === 'minhas' ? 'active' : ''}`}
          onClick={() => setActiveTab('minhas')}
        >
          Minhas Avaliações
        </button>
        <button 
          className={`tab-button ${activeTab === 'perfil' ? 'active' : ''}`}
          onClick={() => navigate('/perfil')}
        >
          Perfil
        </button>
      </div>
      <div className="navbar-user">
        {usuario?.nomeUsuario || 'Usuário'}
      </div>
    </div>
  );
};

export default Navbar;