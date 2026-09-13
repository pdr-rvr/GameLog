import React from "react";
import { Link } from "react-router-dom";
import { FaLayerGroup, FaPlus, FaGamepad, FaLock } from "react-icons/fa";

const PerfilColecoesTab = ({ perfil, isOwner, listas, onCriarLista }) => {
  return (
    <section className="gamer-collections-section">
      <div className="collections-section-header">
        <div>
          <h2 className="section-title">
            Coleções de {isOwner ? "Você" : perfil?.nomeUsuario}
          </h2>
          <span className="section-subtitle">Listas temáticas e seleções especiais</span>
        </div>

        {isOwner && (
          <button
            type="button"
            className="btn-create-collection"
            onClick={onCriarLista}
          >
            <FaPlus /> <span>Criar Nova Coleção</span>
          </button>
        )}
      </div>

      {listas.length === 0 ? (
        <div className="empty-collections-state">
          <FaLayerGroup className="empty-icon" />
          <h3>Nenhuma coleção criada</h3>
          <p>
            {isOwner
              ? "Crie listas temáticas personalizadas (ex: Melhores RPGs, Jogos para Zerar) e compartilhe seu gosto com a comunidade!"
              : `${perfil?.nomeUsuario || "Este usuário"} ainda não criou coleções públicas.`}
          </p>
          {isOwner && (
            <button
              type="button"
              className="btn-create-collection-cta"
              onClick={onCriarLista}
            >
              <FaPlus /> <span>Criar Minha Primeira Coleção</span>
            </button>
          )}
        </div>
      ) : (
        <div className="collections-grid">
          {listas.map((lista) => (
            <div key={lista.listaId || lista.id} className="collection-card">
              <Link to={`/listas/${lista.listaId || lista.id}`} className="collection-mosaic-wrap">
                {lista.capasPreview && lista.capasPreview.length > 0 ? (
                  <div className={`collection-mosaic count-${Math.min(lista.capasPreview.length, 4)}`}>
                    {lista.capasPreview.slice(0, 4).map((capa, idx) => (
                      <img key={idx} src={capa} alt={`Capa da coleção ${lista.titulo}`} className="mosaic-thumb" />
                    ))}
                  </div>
                ) : (
                  <div className="collection-mosaic-placeholder">
                    <FaGamepad className="placeholder-icon" />
                    <span>Coleção Vazia</span>
                  </div>
                )}
              </Link>

              <div className="collection-card-body">
                <div className="collection-card-header-row">
                  <Link to={`/listas/${lista.listaId || lista.id}`} className="collection-card-title">
                    {lista.titulo}
                  </Link>
                  {!lista.estaPublica && (
                    <span className="collection-private-badge" title="Lista Privada">
                      <FaLock />
                    </span>
                  )}
                </div>

                {lista.descricao && (
                  <p className="collection-card-desc">{lista.descricao}</p>
                )}

                <div className="collection-card-footer">
                  <span className="collection-game-count">
                    <strong>{lista.totalJogos}</strong> {lista.totalJogos === 1 ? "jogo" : "jogos"}
                  </span>
                  <Link to={`/listas/${lista.listaId || lista.id}`} className="btn-view-collection">
                    Ver Coleção →
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
};

export default PerfilColecoesTab;
