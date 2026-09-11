import React from "react";
import { Link } from "react-router-dom";
import { 
  FaCheckCircle, 
  FaGamepad, 
  FaStar, 
  FaLayerGroup,
  FaArrowRight 
} from "react-icons/fa";
import "./ActivityTimelineItem.css";

const ActivityTimelineItem = ({ item }) => {
  if (!item) return null;

  const {
    id,
    tipo = "",
    dataAtividade,
    usuarioId,
    usuarioNome = "Gamer",
    usuarioFoto,
    jogoId,
    jogoTitulo,
    jogoImagem,
    nota,
    textoCurto,
    listaId,
    listaTitulo,
    totalJogos
  } = item;

  const formatarTempoRelativo = (dtStr) => {
    if (!dtStr) return "";
    try {
      const agora = new Date();
      const dt = new Date(dtStr);
      const diffMs = agora - dt;
      const diffMin = Math.floor(diffMs / (1000 * 60));
      const diffHoras = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDias = Math.floor(diffMs / (1000 * 60 * 60 * 24));

      if (diffMin < 1) return "agora mesmo";
      if (diffMin < 60) return `há ${diffMin} min`;
      if (diffHoras === 1) return "há 1 hora";
      if (diffHoras < 24) return `há ${diffHoras} horas`;
      if (diffDias === 1) return "ontem";
      return `há ${diffDias} dias`;
    } catch {
      return String(dtStr).substring(0, 10);
    }
  };

  const renderIconeTipo = () => {
    switch (tipo) {
      case "Zerou":
        return <div className="timeline-icon-badge zerou"><FaCheckCircle /></div>;
      case "Jogando":
        return <div className="timeline-icon-badge jogando"><FaGamepad /></div>;
      case "Avaliou":
        return <div className="timeline-icon-badge avaliou"><FaStar /></div>;
      case "CriouLista":
        return <div className="timeline-icon-badge lista"><FaLayerGroup /></div>;
      default:
        return <div className="timeline-icon-badge padrao"><FaGamepad /></div>;
    }
  };

  return (
    <div className="activity-timeline-row" data-testid={`activity-item-${id}`}>
      <div className="timeline-track">
        {renderIconeTipo()}
        <div className="timeline-line"></div>
      </div>

      <div className="timeline-card-content">
        <div className="timeline-header">
          <Link to={`/perfil/${usuarioId}`} className="timeline-user-link">
            {usuarioFoto ? (
              <img 
                src={usuarioFoto} 
                alt={usuarioNome} 
                className="timeline-user-avatar"
                onError={(e) => {
                  e.currentTarget.onerror = null;
                  e.currentTarget.src = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80";
                }}
              />
            ) : (
              <div className="timeline-avatar-fallback">
                {usuarioNome ? usuarioNome.charAt(0).toUpperCase() : "G"}
              </div>
            )}
            <span className="timeline-user-name">{usuarioNome}</span>
          </Link>

          <span className="timeline-action-label">
            {tipo === "Zerou" && "marcou como zerado"}
            {tipo === "Jogando" && "começou a jogar"}
            {tipo === "Avaliou" && `avaliou (${nota}/5)`}
            {tipo === "CriouLista" && "criou a coleção"}
          </span>

          <span className="timeline-time">{formatarTempoRelativo(dataAtividade)}</span>
        </div>

        {/* Detalhes do Alvo (Jogo ou Coleção) */}
        {(tipo === "Zerou" || tipo === "Jogando" || tipo === "Avaliou") && jogoTitulo && (
          <div className="timeline-target-game">
            {jogoImagem && (
              <Link to={`/jogos/${jogoId}`} className="timeline-game-thumb-link">
                <img 
                  src={jogoImagem} 
                  alt={jogoTitulo} 
                  className="timeline-game-thumb"
                  onError={(e) => {
                    e.currentTarget.onerror = null;
                    e.currentTarget.src = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=150&auto=format&fit=crop&q=80";
                  }}
                />
              </Link>
            )}
            <div className="timeline-target-info">
              <Link to={`/jogos/${jogoId}`} className="timeline-game-title">
                {jogoTitulo}
              </Link>
              {tipo === "Avaliou" && textoCurto && (
                <p className="timeline-mini-review">"{textoCurto}"</p>
              )}
            </div>
          </div>
        )}

        {tipo === "CriouLista" && listaTitulo && (
          <div className="timeline-target-list">
            <div className="timeline-list-details">
              <Link to={`/listas/${listaId}`} className="timeline-list-title">
                {listaTitulo}
              </Link>
              <span className="timeline-list-count">
                {totalJogos || 0} {totalJogos === 1 ? "jogo adicionado" : "jogos adicionados"}
              </span>
            </div>
            <Link to={`/listas/${listaId}`} className="btn-view-timeline-list" aria-label="Ver coleção">
              <FaArrowRight />
            </Link>
          </div>
        )}
      </div>
    </div>
  );
};

export default ActivityTimelineItem;
