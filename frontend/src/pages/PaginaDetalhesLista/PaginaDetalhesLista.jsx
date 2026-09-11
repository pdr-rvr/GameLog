import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogoCard from "../../components/JogoCard/JogoCard";
import ConfirmModal from "../../components/ConfirmModal/ConfirmModal";
import ModalCriarLista from "../../components/ModalCriarLista/ModalCriarLista";
import { ListaService } from "../../services/listaService";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import {
  FaLayerGroup,
  FaUser,
  FaCalendarAlt,
  FaGlobe,
  FaLock,
  FaEdit,
  FaTrash,
  FaArrowLeft,
  FaGamepad,
  FaStar
} from "react-icons/fa";
import "./PaginaDetalhesLista.css";

const PaginaDetalhesLista = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const toast = useToast();

  const [lista, setLista] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [modalEditarAberto, setModalEditarAberto] = useState(false);
  const [confirmarExclusaoAberto, setConfirmarExclusaoAberto] = useState(false);
  const [excluindo, setExcluindo] = useState(false);

  const carregarLista = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError("");
    try {
      const dados = await ListaService.obterListaPorId(id);
      setLista(dados);
    } catch (err) {
      console.error("Erro ao carregar detalhes da lista:", err);
      setError(err.response?.data?.message || "Lista não encontrada ou privada.");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    carregarLista();
  }, [carregarLista]);

  const isOwner = user && lista && String(user.id).toLowerCase() === String(lista.usuarioId).toLowerCase();

  const handleConfirmarExclusao = async () => {
    setExcluindo(true);
    try {
      await ListaService.deletarLista(id);
      toast.success("Coleção excluída com sucesso.");
      navigate(`/perfil/${user.id}`);
    } catch (err) {
      console.error("Erro ao excluir lista:", err);
      toast.error(err.response?.data?.message || "Erro ao excluir coleção.");
    } finally {
      setExcluindo(false);
      setConfirmarExclusaoAberto(false);
    }
  };

  const formatarData = (dataStr) => {
    if (!dataStr) return "";
    try {
      const d = new Date(dataStr);
      return d.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "long",
        year: "numeric"
      });
    } catch {
      return String(dataStr).substring(0, 10);
    }
  };

  return (
    <div className="detalhes-lista-page">
      <Navbar />

      <main className="detalhes-lista-container">
        <div className="detalhes-lista-nav-top">
          <button
            type="button"
            className="btn-back-nav"
            onClick={() => {
              if (lista?.usuarioId) {
                navigate(`/perfil/${lista.usuarioId}`);
              } else {
                navigate("/jogos");
              }
            }}
          >
            <FaArrowLeft /> <span>{lista?.usuarioId ? "Voltar ao Perfil" : "Voltar"}</span>
          </button>
        </div>

        {loading && (
          <div className="detalhes-lista-state loading">
            <div className="detalhes-spinner"></div>
            <span>Carregando coleção de jogos...</span>
          </div>
        )}

        {error && !loading && (
          <div className="detalhes-lista-state error">
            <FaLayerGroup className="empty-icon" />
            <h2>Coleção indisponível</h2>
            <p>{error}</p>
            <Link to="/jogos" className="btn-explore-link">
              Explorar Catálogo de Jogos
            </Link>
          </div>
        )}

        {!loading && !error && lista && (
          <>
            {/* Header Hero da Lista */}
            <header className="detalhes-lista-hero">
              <div className="hero-badge-row">
                <span className="hero-category-badge">
                  <FaLayerGroup /> Coleção de Jogos
                </span>
                <span className={`hero-visibility-badge ${lista.estaPublica ? "publica" : "privada"}`}>
                  {lista.estaPublica ? <><FaGlobe /> Pública</> : <><FaLock /> Privada</>}
                </span>
              </div>

              <div className="hero-main-row">
                <div className="hero-title-group">
                  <h1 className="hero-lista-titulo">{lista.titulo}</h1>
                  {lista.descricao && (
                    <p className="hero-lista-descricao">{lista.descricao}</p>
                  )}
                </div>

                {isOwner && (
                  <div className="hero-owner-actions">
                    <button
                      type="button"
                      className="btn-edit-lista"
                      onClick={() => setModalEditarAberto(true)}
                    >
                      <FaEdit /> <span>Editar Coleção</span>
                    </button>
                    <button
                      type="button"
                      className="btn-delete-lista"
                      onClick={() => setConfirmarExclusaoAberto(true)}
                    >
                      <FaTrash />
                    </button>
                  </div>
                )}
              </div>

              <div className="hero-meta-footer">
                <Link to={`/perfil/${lista.usuarioId}`} className="author-chip">
                  {lista.fotoPerfilUsuario ? (
                    <img
                      src={lista.fotoPerfilUsuario}
                      alt={lista.nomeUsuario}
                      className="author-avatar"
                      onError={(e) => {
                        e.currentTarget.onerror = null;
                        e.currentTarget.src = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&auto=format&fit=crop&q=80";
                      }}
                    />
                  ) : (
                    <div className="author-avatar fallback">
                      <span>{lista.nomeUsuario ? lista.nomeUsuario.charAt(0).toUpperCase() : "G"}</span>
                    </div>
                  )}
                  <span className="author-name">Curadoria por <strong>{lista.nomeUsuario}</strong></span>
                </Link>

                <div className="meta-info-divider"></div>

                <div className="meta-stats-chip">
                  <FaCalendarAlt className="meta-icon" />
                  <span>Criada em {formatarData(lista.dataCriacao)}</span>
                </div>

                <div className="meta-stats-chip highlight">
                  <FaGamepad className="meta-icon" />
                  <span><strong>{lista.totalJogos}</strong> {lista.totalJogos === 1 ? "título" : "títulos"}</span>
                </div>
              </div>
            </header>

            {/* Grid dos Jogos na Lista */}
            <section className="lista-games-section">
              {(!lista.itens || lista.itens.length === 0) ? (
                <div className="empty-lista-games">
                  <FaGamepad className="empty-icon" />
                  <h3>Esta coleção ainda não possui jogos</h3>
                  <p>Adicione títulos a esta lista para exibi-los aqui.</p>
                  {isOwner && (
                    <button
                      type="button"
                      className="btn-add-games-cta"
                      onClick={() => setModalEditarAberto(true)}
                    >
                      Adicionar Jogos Agora
                    </button>
                  )}
                </div>
              ) : (
                <div className="lista-games-grid">
                  {lista.itens.map((item, index) => (
                    <div key={item.itemId || item.jogoId} className="lista-game-card-wrapper">
                      <div className="game-card-rank-badge">
                        <span>#{index + 1}</span>
                      </div>
                      <JogoCard
                        jogo={{
                          id: item.jogoId,
                          jogoId: item.jogoId,
                          titulo: item.tituloJogo,
                          imagem: item.imagemJogo,
                          nomeEmpresa: item.nomeEmpresa,
                          empresaId: item.empresaId,
                          dataLancamento: item.dataLancamento,
                          mediaAvaliacoes: item.mediaAvaliacoes
                        }}
                      />
                    </div>
                  ))}
                </div>
              )}
            </section>
          </>
        )}
      </main>

      {/* Modal para Editar Lista */}
      {modalEditarAberto && (
        <ModalCriarLista
          isOpen={modalEditarAberto}
          onClose={() => setModalEditarAberto(false)}
          listaParaEditar={lista}
          onListaSalva={(atualizada) => {
            if (atualizada && atualizada.itens) {
              setLista(atualizada);
            } else {
              carregarLista();
            }
          }}
        />
      )}

      {/* Modal de Confirmação para Exclusão */}
      <ConfirmModal
        isOpen={confirmarExclusaoAberto}
        title="Excluir Coleção"
        message="Tem certeza que deseja excluir esta coleção de jogos? Esta ação não pode ser desfeita."
        confirmText="Excluir Coleção"
        cancelText="Cancelar"
        confirmVariant="danger"
        loading={excluindo}
        onConfirm={handleConfirmarExclusao}
        onCancel={() => !excluindo && setConfirmarExclusaoAberto(false)}
      />
    </div>
  );
};

export default PaginaDetalhesLista;
