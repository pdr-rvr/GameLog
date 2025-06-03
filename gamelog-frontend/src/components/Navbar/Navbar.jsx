import React, { useState, useRef, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import Dropdown from '../Dropdown/Dropdown';
import './Navbar.css';

const Navbar = ({ onPublishClick }) => { 
    const { isAuthenticated, user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login'); 
    };

    const profileContent = user?.profilePictureUrl ? (
        <img src={user.profilePictureUrl} alt="Perfil" className="profile-avatar-img" />
    ) : (
        <span className="profile-avatar-text">{user?.nomeUsuario ? user.nomeUsuario.charAt(0).toUpperCase() : '?'}</span>
    );

    return (
        <nav className="navbar">
            <div className="navbar-container-inner">
                <div className="navbar-brand">
                    <Link to="/home">GameLog</Link> 
                </div>
                <ul className="navbar-nav">
                    <li className="nav-item">
                        <Link to="/home" className="nav-link">Home</Link>
                    </li>
                    <li className="nav-item">
                        <Link to="/jogos" className="nav-link">Jogos</Link>
                    </li>
                    <li className="nav-item separator">
                        <Link to="/avaliacoes" className="nav-link">Avaliações</Link>
                    </li>

                    {isAuthenticated ? (
                        <>
                            <li className="nav-item">
                                <button onClick={onPublishClick} className="nav-link publish-button">Publicar</button>
                            </li>
                            <li className="nav-item profile-dropdown-container">
                                <Dropdown 
                                    trigger={<div className="profile-avatar">{profileContent}</div>}
                                    className="user-dropdown"
                                >
                                    <Link to={`/perfil/${user?.id}`} className="dropdown-item">Meu Perfil</Link>
                                    <Link to="/minhas-avaliacoes" className="dropdown-item">Minhas Avaliações</Link>
                                    <button onClick={handleLogout} className="dropdown-item">Desconectar</button>
                                </Dropdown>
                            </li>
                        </>
                    ) : (
                        <li className="nav-item auth-buttons">
                            <Link to="/login" className="nav-link">Entrar</Link>

                            <Link to="/cadastro" className="nav-link register-button">Cadastrar</Link>
                        </li>
                    )}
                </ul>
            </div>
        </nav>
    );
};

export default Navbar;