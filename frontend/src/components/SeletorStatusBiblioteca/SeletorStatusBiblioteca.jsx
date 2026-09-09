import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { FaBookmark, FaCheck, FaTrash, FaChevronDown } from "react-icons/fa";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import {
  BibliotecaService,
  STATUS_JOGO,
  STATUS_LABELS,
} from "../../services/bibliotecaService";
import "./SeletorStatusBiblioteca.css";

const STATUS_OPCOES = [
  { id: STATUS_JOGO.QUERO_JOGAR, label: STATUS_LABELS[STATUS_JOGO.QUERO_JOGAR], colorClass: "opt-quero-jogar" },
  { id: STATUS_JOGO.JOGANDO, label: STATUS_LABELS[STATUS_JOGO.JOGANDO], colorClass: "opt-jogando" },
  { id: STATUS_JOGO.ZERADO, label: STATUS_LABELS[STATUS_JOGO.ZERADO], colorClass: "opt-zerado" },
  { id: STATUS_JOGO.PAUSADO, label: STATUS_LABELS[STATUS_JOGO.PAUSADO], colorClass: "opt-pausado" },
  { id: STATUS_JOGO.ABANDONADO, label: STATUS_LABELS[STATUS_JOGO.ABANDONADO], colorClass: "opt-abandonado" },
];

const SeletorStatusBiblioteca = ({ jogoId, onAtualizado }) => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [naBiblioteca, setNaBiblioteca] = useState(false);
  const [statusAtual, setStatusAtual] = useState(null);
  const [painelAberto, setPainelAberto] = useState(false);

  // Load current library status for this game
  useEffect(() => {
    let cancelado = false;
    const carregarStatus = async () => {
      if (!user || !jogoId) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const res = await BibliotecaService.obterStatusJogo(jogoId);
        if (!cancelado) {
          if (res.naBiblioteca && res.item) {
            setNaBiblioteca(true);
            setStatusAtual(res.item.status);
          } else {
            setNaBiblioteca(false);
            setStatusAtual(null);
          }
        }
      } catch (err) {
        console.error("Erro ao obter status do jogo na biblioteca:", err);
      } finally {
        if (!cancelado) setLoading(false);
      }
    };

    carregarStatus();
    return () => {
      cancelado = true;
    };
  }, [user, jogoId]);

  const handleSalvarStatus = async (novoStatus) => {
    if (!user) {
      toast.info("Faça login para adicionar jogos à sua biblioteca.");
      navigate("/login");
      return;
    }

    setSalvando(true);
    try {
      const res = await BibliotecaService.salvarItem(jogoId, novoStatus);
      setNaBiblioteca(true);
      setStatusAtual(res.status);
      toast.success(`Status atualizado para "${res.statusNome}" na sua biblioteca!`);
      if (onAtualizado) onAtualizado(res);
      setPainelAberto(false);
    } catch (err) {
      console.error("Erro ao salvar status:", err);
      toast.error(err.response?.data?.message || "Erro ao atualizar biblioteca.");
    } finally {
      setSalvando(false);
    }
  };

  const handleRemoverDaBiblioteca = async () => {
    if (!user || !jogoId) return;

    setSalvando(true);
    try {
      await BibliotecaService.removerItem(jogoId);
      setNaBiblioteca(false);
      setStatusAtual(null);
      toast.success("Jogo removido da sua biblioteca.");
      if (onAtualizado) onAtualizado(null);
      setPainelAberto(false);
    } catch (err) {
      console.error("Erro ao remover da biblioteca:", err);
      toast.error(err.response?.data?.message || "Erro ao remover da biblioteca.");
    } finally {
      setSalvando(false);
    }
  };

  if (loading) {
    return <div className="seletor-status-loading">Carregando status...</div>;
  }

  return (
    <div className="seletor-status-biblioteca">
      <div className="seletor-bar-main">
        {naBiblioteca ? (
          <div className="status-ativo-container">
            <div className="status-ativo-pill">
              <span className="status-indicator"></span>
              <span className="status-texto">
                Status: <strong>{STATUS_LABELS[statusAtual] || "Registrado"}</strong>
              </span>
            </div>

            <button
              type="button"
              className="btn-abrir-seletor"
              onClick={() => setPainelAberto(!painelAberto)}
              title="Alterar status do jogo"
            >
              <span>Alterar</span>
              <FaChevronDown className={`chevron-icon ${painelAberto ? "aberto" : ""}`} />
            </button>
          </div>
        ) : (
          <button
            type="button"
            className="btn-add-biblioteca-principal"
            onClick={() => {
              if (!user) {
                toast.info("Faça login para adicionar jogos à sua biblioteca.");
                navigate("/login");
                return;
              }
              setPainelAberto(!painelAberto);
            }}
          >
            <FaBookmark />
            <span>Adicionar à Biblioteca</span>
            <FaChevronDown className={`chevron-icon ${painelAberto ? "aberto" : ""}`} />
          </button>
        )}
      </div>

      {painelAberto && (
        <div className="seletor-painel-dropdown">
          <div className="painel-section">
            <label className="painel-label">Escolha o Status:</label>
            <div className="painel-status-grid">
              {STATUS_OPCOES.map((opt) => {
                const isSelected = statusAtual === opt.id;
                return (
                  <button
                    key={opt.id}
                    type="button"
                    className={`btn-status-choice ${opt.colorClass} ${
                      isSelected ? "selecionado" : ""
                    }`}
                    onClick={() => handleSalvarStatus(opt.id)}
                    disabled={salvando}
                  >
                    {isSelected && <FaCheck className="check-icon" />}
                    <span>{opt.label}</span>
                  </button>
                );
              })}
            </div>
          </div>

          {naBiblioteca && (
            <div className="painel-footer-remover">
              <button
                type="button"
                className="btn-remover-biblioteca"
                onClick={handleRemoverDaBiblioteca}
                disabled={salvando}
              >
                <FaTrash /> <span>Remover da Biblioteca</span>
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default SeletorStatusBiblioteca;
