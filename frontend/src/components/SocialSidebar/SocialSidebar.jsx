import React from "react";
import { Link } from "react-router-dom";
import { FaUserPlus, FaUserCheck, FaTrophy, FaGamepad, FaUsers } from "react-icons/fa";
import "./SocialSidebar.css";

const SocialSidebar = ({
  amigosJogando = [],
  gamersSugeridos = [],
  jogosPodio = [],
  onSeguirClick
}) => {
  return (
    <aside className="social-sidebar-container" data-testid="social-sidebar">
      {/* 1. Amigos Jogando Agora */}
      {amigosJogando && amigosJogando.length > 0 && (
        <section className="social-sidebar-block">
          <div className="sidebar-block-header">
            <FaGamepad className="block-icon playing" />
            <h3 className="sidebar-block-title">Amigos Jogando Agora</h3>
          </div>
          <div className="playing-now-list">
            {amigosJogando.slice(0, 4).map((item, idx) => (
              <div key={idx} className="playing-now-card">
                <Link to={`/perfil/${item.usuarioId}`} className="playing-user-avatar-link">
                  {item.usuarioFoto ? (
                    <img src={item.usuarioFoto} alt={item.usuarioNome} className="playing-user-avatar" />
                  ) : (
                    <div className="playing-avatar-fallback">
                      {item.usuarioNome ? item.usuarioNome.charAt(0).toUpperCase() : "G"}
                    </div>
                  )}
                </Link>
                <div className="playing-now-info">
                  <Link to={`/perfil/${item.usuarioId}`} className="playing-user-name">
                    {item.usuarioNome}
                  </Link>
                  <Link to={`/jogos/${item.jogoId}`} className="playing-game-name">
                    {item.jogoTitulo}
                  </Link>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* 2. Gamers Sugeridos para Seguir */}
      {gamersSugeridos && gamersSugeridos.length > 0 && (
        <section className="social-sidebar-block">
          <div className="sidebar-block-header">
            <FaUsers className="block-icon suggestions" />
            <h3 className="sidebar-block-title">Gamers Sugeridos</h3>
          </div>
          <div className="suggested-gamers-list">
            {gamersSugeridos.slice(0, 4).map((gamer) => (
              <div key={gamer.usuarioId} className="suggested-gamer-card">
                <Link to={`/perfil/${gamer.usuarioId}`} className="suggested-avatar-link">
                  {gamer.fotoPerfil ? (
                    <img src={gamer.fotoPerfil} alt={gamer.nomeUsuario} className="suggested-avatar" />
                  ) : (
                    <div className="suggested-avatar-fallback">
                      {gamer.nomeUsuario ? gamer.nomeUsuario.charAt(0).toUpperCase() : "G"}
                    </div>
                  )}
                </Link>
                <div className="suggested-gamer-info">
                  <Link to={`/perfil/${gamer.usuarioId}`} className="suggested-name">
                    {gamer.nomeUsuario}
                  </Link>
                  {gamer.bio && <p className="suggested-bio">{gamer.bio}</p>}
                </div>
                <button
                  type="button"
                  className={`btn-suggested-follow ${gamer.seguidoPorMim ? "following" : ""}`}
                  onClick={() => onSeguirClick && onSeguirClick(gamer.usuarioId)}
                  title={gamer.seguidoPorMim ? "Deixar de seguir" : "Seguir"}
                  aria-label={gamer.seguidoPorMim ? "Seguindo" : "Seguir"}
                >
                  {gamer.seguidoPorMim ? <FaUserCheck /> : <FaUserPlus />}
                </button>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* 3. Meu Pódio de Favoritos (Acesso Rápido) */}
      {jogosPodio && jogosPodio.length > 0 && (
        <section className="social-sidebar-block podium-block">
          <div className="sidebar-block-header">
            <FaTrophy className="block-icon trophy" />
            <h3 className="sidebar-block-title">Meu Pódio Top 5</h3>
          </div>
          <div className="podium-mini-mosaic">
            {jogosPodio.slice(0, 5).map((fav, idx) => (
              <Link
                key={idx}
                to={`/jogos/${fav.jogoId || fav.id}`}
                className="podium-mini-slot"
                title={`${idx + 1}º lugar: ${fav.titulo || fav.jogoTitulo}`}
              >
                <span className="podium-slot-pos">{idx + 1}</span>
                <img
                  src={fav.imagem || fav.jogoImagem || "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=200&auto=format&fit=crop&q=80"}
                  alt={fav.titulo || fav.jogoTitulo}
                  className="podium-mini-thumb"
                />
              </Link>
            ))}
          </div>
        </section>
      )}
    </aside>
  );
};

export default SocialSidebar;
