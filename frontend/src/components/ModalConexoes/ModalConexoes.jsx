import React, { useState, useEffect, useCallback } from "react";
import { Link } from "react-router-dom";
import { FaTimes, FaUserFriends, FaUserPlus, FaUserCheck, FaUserSlash } from "react-icons/fa";
import { SocialService } from "../../services/socialService";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import "./ModalConexoes.css";

const ModalConexoes = ({
  isOpen,
  onClose,
  usuarioId,
  nomeUsuario,
  initialTab = "seguidores",
  onConexaoAlterada
}) => {
  const { user } = useAuth();
  const toast = useToast();

  const [activeTab, setActiveTab] = useState(initialTab);
  const [lista, setLista] = useState([]);
  const [loading, setLoading] = useState(true);
  const [processandoId, setProcessandoId] = useState(null);

  const carregarConexoes = useCallback(async (tab) => {
    if (!usuarioId) return;
    setLoading(true);
    try {
      if (tab === "seguidores") {
        const dados = await SocialService.obterSeguidores(usuarioId);
        setLista(dados || []);
      } else {
        const dados = await SocialService.obterSeguindo(usuarioId);
        setLista(dados || []);
      }
    } catch (err) {
      console.error("Erro ao carregar conexoes:", err);
      toast.error("Nao foi possivel carregar a lista de conexoes.");
    } finally {
      setLoading(false);
    }
  }, [usuarioId, toast]);

  useEffect(() => {
    if (isOpen) {
      setActiveTab(initialTab);
      carregarConexoes(initialTab);
    }
  }, [isOpen, initialTab, carregarConexoes]);

  const handleTabChange = (novaTab) => {
    setActiveTab(novaTab);
    carregarConexoes(novaTab);
  };

  const handleToggleSeguir = async (targetId, nome) => {
    if (!user) {
      toast.warning("Faca login para seguir outros usuarios.");
      return;
    }
    setProcessandoId(targetId);
    try {
      const res = await SocialService.alternarSeguir(targetId);
      setLista((prev) =>
        prev.map((item) =>
          item.usuarioId === targetId
            ? { ...item, seguidoPorMim: res.seguido }
            : item
        )
      );
      toast.success(res.seguido ? `Voce esta seguindo ${nome}.` : `Voce deixou de seguir ${nome}.`);
      if (onConexaoAlterada) onConexaoAlterada();
    } catch (err) {
      console.error("Erro ao alterar relacao de seguir:", err);
      toast.error(err.response?.data?.detail || err.response?.data?.message || "Erro ao atualizar conexao.");
    } finally {
      setProcessandoId(null);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-conexoes-overlay" onClick={onClose}>
      <div className="modal-conexoes-container" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="modal-conexoes-header">
          <div className="modal-conexoes-title-group">
            <FaUserFriends className="modal-conexoes-icon" />
            <div>
              <h3>Conexoes Sociais</h3>
              <p className="modal-conexoes-sub">{nomeUsuario || "Gamer"}</p>
            </div>
          </div>
          <button type="button" className="btn-close-modal" onClick={onClose} aria-label="Fechar modal">
            <FaTimes />
          </button>
        </div>

        {/* Abas */}
        <div className="modal-conexoes-tabs">
          <button
            type="button"
            className={`modal-tab-btn ${activeTab === "seguidores" ? "active" : ""}`}
            onClick={() => handleTabChange("seguidores")}
          >
            <span>Seguidores</span>
          </button>
          <button
            type="button"
            className={`modal-tab-btn ${activeTab === "seguindo" ? "active" : ""}`}
            onClick={() => handleTabChange("seguindo")}
          >
            <span>Seguindo</span>
          </button>
        </div>

        {/* Conteúdo */}
        <div className="modal-conexoes-body">
          {loading ? (
            <div className="modal-conexoes-loading">
              <div className="conexoes-spinner"></div>
              <span>Carregando conexoes...</span>
            </div>
          ) : lista.length === 0 ? (
            <div className="modal-conexoes-empty">
              <FaUserFriends className="empty-icon" />
              <h4>Nenhum usuario encontrado</h4>
              <p>
                {activeTab === "seguidores"
                  ? "Este perfil ainda nao possui seguidores."
                  : "Este perfil ainda nao esta seguindo ninguem."}
              </p>
            </div>
          ) : (
            <div className="conexoes-user-list">
              {lista.map((u) => {
                const isSelf = user && String(user.id).toLowerCase() === String(u.usuarioId).toLowerCase();
                const isFollowed = u.seguidoPorMim;
                const isProcessing = processandoId === u.usuarioId;

                return (
                  <div key={u.usuarioId} className="conexao-user-card">
                    <Link
                      to={`/perfil/${u.usuarioId}`}
                      className="conexao-user-info"
                      onClick={onClose}
                    >
                      {u.fotoPerfil ? (
                        <img
                          src={u.fotoPerfil}
                          alt={u.nomeUsuario}
                          className="conexao-avatar"
                          onError={(e) => {
                            e.currentTarget.onerror = null;
                            e.currentTarget.src =
                              "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&auto=format&fit=crop&q=80";
                          }}
                        />
                      ) : (
                        <div className="conexao-avatar fallback">
                          <span>{u.nomeUsuario ? u.nomeUsuario.charAt(0).toUpperCase() : "G"}</span>
                        </div>
                      )}
                      <div className="conexao-texts">
                        <span className="conexao-nome">{u.nomeUsuario}</span>
                        {u.bio && <span className="conexao-bio">{u.bio}</span>}
                      </div>
                    </Link>

                    {!isSelf && user && (
                      <button
                        type="button"
                        className={`btn-toggle-follow-conexao ${isFollowed ? "following" : ""}`}
                        disabled={isProcessing}
                        onClick={() => handleToggleSeguir(u.usuarioId, u.nomeUsuario)}
                      >
                        {isFollowed ? (
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
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ModalConexoes;
