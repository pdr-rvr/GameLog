import React, { useState } from "react";
import { Link } from "react-router-dom";
import { 
  FaCheckCircle, 
  FaGamepad, 
  FaStar, 
  FaLayerGroup,
  FaBookmark,
  FaCommentDots,
  FaPlusCircle,
  FaArrowRight 
} from "react-icons/fa";
import "./ActivityTimelineItem.css";

const ActivityTimelineItem = ({ item }) => {
  const [avatarError, setAvatarError] = useState(false);
  const [thumbError, setThumbError] = useState(false);

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
    statusBiblioteca,
    nota,
    textoCurto,
    comentarioTexto,
    autorAvaliacaoRespondidaNome,
    avaliacaoId,
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

  const formatarStatusBiblioteca = (status) => {
    switch (status) {
      case "QueroJogar":
        return "Quero Jogar";
      case "Pausado":
        return "Pausado";
      case "Abandonado":
        return "Abandonado";
      case "Jogando":
        return "Jogando";
      case "Zerado":
        return "Zerado";
      default:
        return "Biblioteca";
    }
  };

  const renderIconeTipo = () => {
    switch (tipo) {
      case "Zerou":
        return <div className="timeline-icon-badge zerou"><FaCheckCircle /></div>;
      case "Jogando":
        return <div className="timeline-icon-badge jogando"><FaGamepad /></div>;
      case "AdicionouBiblioteca":
        return <div className="timeline-icon-badge biblioteca"><FaBookmark /></div>;
      case "Avaliou":
        return <div className="timeline-icon-badge avaliou"><FaStar /></div>;
      case "Comentou":
        return <div className="timeline-icon-badge comentou"><FaCommentDots /></div>;
      case "CriouLista":
        return <div className="timeline-icon-badge lista"><FaLayerGroup /></div>;
      case "AdicionouJogoLista":
        return <div className="timeline-icon-badge addlista"><FaPlusCircle /></div>;
      default:
        return <div className="timeline-icon-badge padrao"><FaGamepad /></div>;
    }
  };

  const renderActionLabel = () => {
    switch (tipo) {
      case "Zerou":
        return <span className="timeline-action-label">marcou como <strong>zerado</strong></span>;
      case "Jogando":
        return <span className="timeline-action-label">começou a <strong>jogar</strong></span>;
      case "AdicionouBiblioteca":
        return (
          <span className="timeline-action-label">
            adicionou à biblioteca ({formatarStatusBiblioteca(statusBiblioteca)})
          </span>
        );
      case "Avaliou":
        return <span className="timeline-action-label">avaliou com <strong>{nota}/5</strong></span>;
      case "Comentou":
        return (
          <span className="timeline-action-label">
            comentou na análise de {autorAvaliacaoRespondidaNome ? <strong>@{autorAvaliacaoRespondidaNome}</strong> : "um gamer"}
          </span>
        );
      case "CriouLista":
        return <span className="timeline-action-label">criou uma nova <strong>coleção</strong></span>;
      case "AdicionouJogoLista":
        return (
          <span className="timeline-action-label">
            adicionou à coleção {listaTitulo ? <strong>{listaTitulo}</strong> : ""}
          </span>
        );
      default:
        return <span className="timeline-action-label">realizou uma atividade</span>;
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
            {usuarioFoto && !avatarError ? (
              <img 
                src={usuarioFoto} 
                alt={usuarioNome} 
                className="timeline-user-avatar"
                onError={() => setAvatarError(true)}
              />
            ) : (
              <div className="timeline-avatar-fallback">
                {usuarioNome ? usuarioNome.charAt(0).toUpperCase() : "G"}
              </div>
            )}
            <span className="timeline-user-name">{usuarioNome}</span>
          </Link>

          {renderActionLabel()}

          <span className="timeline-time">{formatarTempoRelativo(dataAtividade)}</span>
        </div>

        {/* Detalhes do Alvo: Jogo */}
        {(tipo === "Zerou" || tipo === "Jogando" || tipo === "AdicionouBiblioteca" || tipo === "Avaliou" || tipo === "Comentou" || tipo === "AdicionouJogoLista") && (
          <div className="timeline-target-game">
            {jogoImagem && !thumbError ? (
              <Link to={`/jogos/${jogoId}`} className="timeline-game-thumb-link">
                <img 
                  src={jogoImagem} 
                  alt={jogoTitulo || "Jogo"} 
                  className="timeline-game-thumb"
                  onError={() => setThumbError(true)}
                />
              </Link>
            ) : (
              <div className="timeline-game-thumb-fallback">
                <FaGamepad />
              </div>
            )}
            <div className="timeline-target-info">
              {jogoTitulo && (
                <Link to={`/jogos/${jogoId}`} className="timeline-game-title">
                  {jogoTitulo}
                </Link>
              )}
              {tipo === "Avaliou" && textoCurto && (
                <p className="timeline-mini-review">"{textoCurto}"</p>
              )}
              {tipo === "Comentou" && (comentarioTexto || textoCurto) && (
                <p className="timeline-mini-review">"{comentarioTexto || textoCurto}"</p>
              )}
            </div>
            {tipo === "Comentou" && avaliacaoId && (
              <Link to={`/avaliacoes/${avaliacaoId}`} className="btn-view-timeline-list" aria-label="Ver análise">
                <FaArrowRight />
              </Link>
            )}
          </div>
        )}

        {/* Detalhes do Alvo: Coleção Criada */}
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

