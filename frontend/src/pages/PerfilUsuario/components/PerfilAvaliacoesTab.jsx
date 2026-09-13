import React from "react";
import { Link } from "react-router-dom";
import { FaGamepad } from "react-icons/fa";
import AvaliacaoCard from "../../../components/AvaliacaoCard/AvaliacaoCard";

const PerfilAvaliacoesTab = ({
  perfil,
  isOwner,
  avaliacoes,
  totalAvaliacoes,
  onEditReview,
  onDeleteReview
}) => {
  return (
    <section className="gamer-reviews-section">
      <div className="section-header">
        <div>
          <h2 className="section-title">
            Avaliações de {isOwner ? "Você" : perfil?.nomeUsuario}
          </h2>
          <span className="section-subtitle">Críticas e impressões detalhadas</span>
        </div>
        <span className="section-counter">
          {totalAvaliacoes} {totalAvaliacoes === 1 ? "publicação" : "publicações"}
        </span>
      </div>

      {avaliacoes.length === 0 ? (
        <div className="empty-gamer-reviews">
          <FaGamepad className="empty-icon" />
          <h3>Nenhuma avaliação publicada ainda</h3>
          <p>
            {isOwner
              ? "Você ainda não avaliou nenhum jogo. Explore nosso catálogo e compartilhe suas opiniões!"
              : `${perfil?.nomeUsuario || "Este usuário"} ainda não publicou nenhuma avaliação.`}
          </p>
          {isOwner && (
            <Link to="/jogos" className="btn-browse-games">
              Explorar Catálogo de Jogos
            </Link>
          )}
        </div>
      ) : (
        <div className="gamer-reviews-grid">
          {avaliacoes.map((avaliacao) => (
            <div key={avaliacao.avaliacaoId || avaliacao.id} className="gamer-review-grid-item">
              <AvaliacaoCard
                avaliacao={avaliacao}
                onEdit={isOwner ? () => onEditReview(avaliacao) : null}
                onDelete={isOwner ? () => onDeleteReview(avaliacao.avaliacaoId || avaliacao.id) : null}
              />
            </div>
          ))}
        </div>
      )}
    </section>
  );
};

export default PerfilAvaliacoesTab;
