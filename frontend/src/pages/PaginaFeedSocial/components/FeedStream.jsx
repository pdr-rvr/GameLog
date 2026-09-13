import React from "react";
import { Link } from "react-router-dom";
import { FaLayerGroup, FaUserFriends, FaCompass, FaPlus } from "react-icons/fa";
import ReviewCardV2 from "../../../components/ReviewCardV2/ReviewCardV2";
import DiscussionFeedCard from "../../../components/DiscussionFeedCard/DiscussionFeedCard";

const FeedStream = ({
  loadingFeed,
  itensFiltrados,
  handleToggleCurtirReview,
  onOpenPublishModal
}) => {
  if (loadingFeed) {
    return (
      <div className="feed-loading-state">
        <div className="feed-spinner"></div>
        <span>Carregando feed de amigos...</span>
      </div>
    );
  }

  if (!itensFiltrados || itensFiltrados.length === 0) {
    return (
      <div className="feed-empty-state">
        <FaUserFriends className="feed-empty-icon" />
        <h3>Seu feed social está calmo por enquanto</h3>
        <p>
          Siga outros jogadores e amigos para acompanhar suas análises, discussões e coleções em tempo real!
        </p>
        <div className="feed-empty-actions">
          <Link to="/comunidade" className="btn-feed-explore">
            <FaCompass /> <span>Explorar Comunidade</span>
          </Link>
          <button 
            type="button" 
            className="btn-feed-publish-first"
            onClick={onOpenPublishModal}
          >
            <FaPlus /> <span>Publicar sua Análise</span>
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="feed-cards-stream">
      {itensFiltrados.map((item, index) => {
        const tipo = item.tipoAtividade || item.tipo || "Avaliacao";

        // 1. Discussão / Resposta a uma resenha
        if (tipo === "Discussao" || tipo === "Comentario" || item.comentarioTexto) {
          return (
            <DiscussionFeedCard
              key={item.id || index}
              item={item}
              onToggleCurtir={handleToggleCurtirReview}
            />
          );
        }

        // 2. Resenha / Análise
        if (tipo === "Avaliacao" || item.textoAvaliacao || item.nota) {
          return (
            <ReviewCardV2
              key={item.avaliacaoId || item.id || index}
              avaliacao={item}
              onToggleCurtir={handleToggleCurtirReview}
            />
          );
        }

        // 3. Coleção Criada por Amigo
        if (tipo === "CriouLista" || tipo === "Lista" || tipo === "ListaCriada") {
          const autorId = item.autorId || item.usuarioId;
          const autorNome = item.autorNome || item.nomeUsuario || item.usuarioNome || "Gamer";
          const autorFoto = item.autorFoto || item.fotoPerfilUsuario || item.usuarioFoto;
          const listaId = item.listaId || item.id;
          const listaTitulo = item.listaTitulo || item.titulo || "Coleção";
          const listaDescricao = item.listaDescricao || item.descricao;
          const totalJogosLista = item.totalJogosLista || item.totalJogos || 0;
          const capasPreview = item.capasPreviewLista || [];

          return (
            <article key={item.id || index} className="social-collection-created-card">
              <div className="finished-card-header">
                <Link to={`/perfil/${autorId}`} className="finished-author-link">
                  {autorFoto ? (
                    <img 
                      src={autorFoto} 
                      alt={autorNome} 
                      className="finished-author-avatar"
                    />
                  ) : (
                    <div className="finished-avatar-fallback">
                      {autorNome.charAt(0).toUpperCase()}
                    </div>
                  )}
                  <div>
                    <span className="finished-author-name">{autorNome}</span>
                    <span className="finished-action-text">criou uma nova coleção</span>
                  </div>
                </Link>
                <div className="collection-status-badge">
                  <FaLayerGroup /> Coleção ({totalJogosLista} {totalJogosLista === 1 ? "jogo" : "jogos"})
                </div>
              </div>

              <div className="collection-card-details">
                <Link to={`/listas/${listaId}`} className="collection-created-title">
                  {listaTitulo}
                </Link>
                {listaDescricao && <p className="collection-created-desc">{listaDescricao}</p>}

                {capasPreview.length > 0 && (
                  <div className="collection-covers-preview">
                    {capasPreview.map((capa, idx) => (
                      <img 
                        key={idx} 
                        src={capa} 
                        alt="Capa do Jogo" 
                        className="collection-preview-thumb" 
                      />
                    ))}
                  </div>
                )}
              </div>
            </article>
          );
        }

        return null;
      })}
    </div>
  );
};

export default FeedStream;
