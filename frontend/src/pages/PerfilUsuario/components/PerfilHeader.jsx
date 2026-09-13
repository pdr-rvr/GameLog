import React from "react";
import { Link } from "react-router-dom";
import { FaGamepad, FaCog, FaUserPlus, FaUserCheck } from "react-icons/fa";

const PerfilHeader = ({
  perfil,
  statsSociais,
  isOwner,
  user,
  processandoSeguir,
  onToggleSeguir,
  onOpenConexoes
}) => {
  return (
    <header className="gamer-hero-card">
      <div className="gamer-avatar-wrapper">
        {perfil.fotoDePerfil ? (
          <img src={perfil.fotoDePerfil} alt={perfil.nomeUsuario} className="gamer-avatar-img" />
        ) : (
          <div className="gamer-avatar-fallback">
            <span>{perfil.nomeUsuario ? perfil.nomeUsuario.charAt(0).toUpperCase() : "G"}</span>
          </div>
        )}
      </div>

      <div className="gamer-hero-info">
        <div className="gamer-hero-top">
          <div className="gamer-title-group">
            <h1 className="gamer-username">{perfil.nomeUsuario || "Gamer"}</h1>
            <span className="gamer-badge">
              <FaGamepad /> Membro GameLog
            </span>
          </div>

          <div className="gamer-hero-actions">
            {!isOwner && user && (
              <button
                type="button"
                className={`btn-follow-profile ${statsSociais.seguidoPorMim ? "following" : ""}`}
                disabled={processandoSeguir}
                onClick={onToggleSeguir}
              >
                {statsSociais.seguidoPorMim ? (
                  <>
                    <FaUserCheck /> <span>Seguindo</span>
                  </>
                ) : (
                  <>
                    <FaUserPlus /> <span>Seguir</span>
                  </>
                )}
              </button>
            )}

            {isOwner && (
              <Link to="/configuracoes" className="btn-edit-settings">
                <FaCog /> <span>Configurações de Conta</span>
              </Link>
            )}
          </div>
        </div>

        <div className="gamer-social-counters">
          <button
            type="button"
            className="social-counter-btn"
            onClick={() => onOpenConexoes("seguidores")}
          >
            <strong>{statsSociais.totalSeguidores}</strong>{" "}
            <span>{statsSociais.totalSeguidores === 1 ? "Seguidor" : "Seguidores"}</span>
          </button>
          <span className="counter-dot">•</span>
          <button
            type="button"
            className="social-counter-btn"
            onClick={() => onOpenConexoes("seguindo")}
          >
            <strong>{statsSociais.totalSeguindo}</strong> <span>Seguindo</span>
          </button>
        </div>

        {perfil.bio ? (
          <p className="gamer-bio-text">{perfil.bio}</p>
        ) : isOwner ? (
          <p className="gamer-bio-text empty">
            Você ainda não adicionou uma bio. <Link to="/configuracoes">Adicione uma apresentação</Link>
          </p>
        ) : (
          <p className="gamer-bio-text empty">Este jogador ainda não adicionou uma bio.</p>
        )}
      </div>
    </header>
  );
};

export default PerfilHeader;
