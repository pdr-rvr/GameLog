import React, { useState, useEffect, useMemo } from "react";
import { FaTimes, FaSearch, FaTrash, FaCheck, FaTrophy } from "react-icons/fa";
import { buscarJogos } from "../../pages/PaginaJogos/actions/PaginaJogosActions";
import { BibliotecaService } from "../../services/bibliotecaService";
import { useToast } from "../../context/ToastContext";
import "./ModalEditarFavoritos.css";

const POSICOES = [
  { id: 1, label: "1º Lugar", destaque: "Ouro" },
  { id: 2, label: "2º Lugar", destaque: "Prata" },
  { id: 3, label: "3º Lugar", destaque: "Bronze" },
  { id: 4, label: "4º Lugar", destaque: "Top 4" },
  { id: 5, label: "5º Lugar", destaque: "Top 5" },
];

const ModalEditarFavoritos = ({ isOpen, onClose, favoritosAtuais = [], onSalvo }) => {
  const toast = useToast();
  const [catalogo, setCatalogo] = useState([]);
  const [carregandoCatalogo, setCarregandoCatalogo] = useState(false);
  const [salvando, setSalvando] = useState(false);

  // Map of position -> game object
  const [selecoes, setSelecoes] = useState({
    1: null,
    2: null,
    3: null,
    4: null,
    5: null,
  });

  const [posicaoAtiva, setPosicaoAtiva] = useState(null);
  const [termoBusca, setTermoBusca] = useState("");

  // Initialize selected games from props
  useEffect(() => {
    if (isOpen) {
      const mapa = { 1: null, 2: null, 3: null, 4: null, 5: null };
      if (Array.isArray(favoritosAtuais)) {
        favoritosAtuais.forEach((f) => {
          if (f.posicao >= 1 && f.posicao <= 5) {
            const jId = Number(f.jogoId || f.id);
            if (!isNaN(jId) && jId > 0) {
              mapa[f.posicao] = {
                id: jId,
                jogoId: jId,
                titulo: f.tituloJogo || f.titulo || "Jogo",
                imagem: f.imagemJogo || f.imagem || "/game-images/default_game_cover.png",
                nomeEmpresa: f.nomeEmpresa || "Game",
              };
            }
          }
        });
      }
      setSelecoes(mapa);
      setPosicaoAtiva(null);
      setTermoBusca("");

      // Fetch all games for selection if not fetched
      if (catalogo.length === 0) {
        setCarregandoCatalogo(true);
        buscarJogos()
          .then((dados) => {
            const normalizados = (dados || []).map((j) => ({
              ...j,
              id: Number(j.id || j.jogoId),
              jogoId: Number(j.id || j.jogoId),
            }));
            setCatalogo(normalizados);
          })
          .catch((err) => {
            console.error("Erro ao carregar catálogo:", err);
            toast.error("Não foi possível carregar o catálogo de jogos.");
          })
          .finally(() => setCarregandoCatalogo(false));
      }
    }
  }, [isOpen, favoritosAtuais]);

  // Filter games based on search and exclude already selected in OTHER positions
  const jogosFiltrados = useMemo(() => {
    const idsJaSelecionados = Object.entries(selecoes)
      .filter(([pos, j]) => Number(pos) !== Number(posicaoAtiva) && j !== null && j !== undefined)
      .map(([_, j]) => Number(j.id || j.jogoId))
      .filter((id) => !isNaN(id) && id > 0);

    return catalogo.filter((j) => {
      const jId = Number(j.id || j.jogoId);
      if (idsJaSelecionados.includes(jId)) return false;
      if (!termoBusca.trim()) return true;
      const t = termoBusca.toLowerCase();
      const tituloMatch = j.titulo?.toLowerCase().includes(t);
      const empresaMatch = j.nomeEmpresa?.toLowerCase().includes(t);
      return tituloMatch || empresaMatch;
    });
  }, [catalogo, termoBusca, selecoes, posicaoAtiva]);

  const handleSelecionarJogo = (jogo) => {
    if (!posicaoAtiva) return;
    const jId = Number(jogo.id || jogo.jogoId);
    setSelecoes((prev) => ({
      ...prev,
      [posicaoAtiva]: {
        id: jId,
        jogoId: jId,
        titulo: jogo.titulo,
        imagem: jogo.imagem || "/game-images/default_game_cover.png",
        nomeEmpresa: jogo.nomeEmpresa || "Game",
      },
    }));
    setPosicaoAtiva(null);
    setTermoBusca("");
  };

  const handleRemoverPosicao = (pos) => {
    setSelecoes((prev) => ({
      ...prev,
      [pos]: null,
    }));
    if (posicaoAtiva === pos) {
      setPosicaoAtiva(null);
    }
  };

  const handleSalvar = async () => {
    setSalvando(true);
    try {
      const listaParaSalvar = [];
      Object.entries(selecoes).forEach(([pos, jogo]) => {
        if (jogo) {
          const jId = Number(jogo.id || jogo.jogoId);
          if (!isNaN(jId) && jId > 0) {
            listaParaSalvar.push({
              posicao: Number(pos),
              jogoId: jId,
            });
          }
        }
      });

      const resultado = await BibliotecaService.salvarFavoritos(listaParaSalvar);
      toast.success("Top 5 jogos favoritos atualizado com sucesso!");
      if (onSalvo) {
        onSalvo(resultado);
      }
      onClose();
    } catch (err) {
      console.error("Erro ao salvar favoritos:", err);
      toast.error(err.response?.data?.message || "Erro ao salvar jogos favoritos.");
    } finally {
      setSalvando(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-favoritos-overlay" onClick={onClose}>
      <div className="modal-favoritos-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-favoritos-header">
          <div className="modal-header-title">
            <FaTrophy className="header-trophy-icon" />
            <div>
              <h2>Personalizar Top 5 Favoritos</h2>
              <p>Escolha os 5 jogos mais marcantes do seu perfil gamer</p>
            </div>
          </div>
          <button className="btn-modal-close" onClick={onClose} aria-label="Fechar">
            <FaTimes />
          </button>
        </div>

        <div className="modal-favoritos-body">
          {/* 5 Slots Selector */}
          <div className="slots-favoritos-grid">
            {POSICOES.map((pos) => {
              const jogo = selecoes[pos.id];
              const isAtivo = posicaoAtiva === pos.id;

              return (
                <div
                  key={pos.id}
                  className={`slot-favorito-item ${jogo ? "preenchido" : "vazio"} ${
                    isAtivo ? "em-edicao" : ""
                  }`}
                >
                  <div className="slot-posicao-tag">
                    <span className={`pos-number pos-${pos.id}`}>{pos.id}º</span>
                    <span className="pos-label">{pos.destaque}</span>
                  </div>

                  {jogo ? (
                    <div className="slot-jogo-card">
                      <img src={jogo.imagem} alt={jogo.titulo} className="slot-jogo-thumb" />
                      <div className="slot-jogo-info">
                        <strong className="slot-jogo-titulo" title={jogo.titulo}>
                          {jogo.titulo}
                        </strong>
                        <span className="slot-jogo-empresa">{jogo.nomeEmpresa || "Game"}</span>
                      </div>
                      <div className="slot-actions">
                        <button
                          type="button"
                          className="btn-slot-trocar"
                          onClick={() => setPosicaoAtiva(pos.id)}
                          title="Trocar jogo"
                        >
                          Trocar
                        </button>
                        <button
                          type="button"
                          className="btn-slot-remover"
                          onClick={() => handleRemoverPosicao(pos.id)}
                          title="Remover jogo"
                        >
                          <FaTrash />
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button
                      type="button"
                      className="btn-slot-adicionar"
                      onClick={() => setPosicaoAtiva(pos.id)}
                    >
                      <span className="btn-add-plus">+</span>
                      <span>Selecionar Jogo ({pos.label})</span>
                    </button>
                  )}
                </div>
              );
            })}
          </div>

          {/* Search & Picker Drawer when a slot is clicked */}
          {posicaoAtiva && (
            <div className="picker-drawer">
              <div className="picker-drawer-header">
                <h3>
                  Escolha o jogo para a <strong>{posicaoAtiva}ª posição</strong>
                </h3>
                <button
                  type="button"
                  className="btn-picker-cancel"
                  onClick={() => setPosicaoAtiva(null)}
                >
                  Cancelar seleção
                </button>
              </div>

              <div className="picker-search-bar">
                <FaSearch className="search-icon" />
                <input
                  type="text"
                  placeholder="Buscar pelo título ou produtora..."
                  value={termoBusca}
                  onChange={(e) => setTermoBusca(e.target.value)}
                  autoFocus
                />
                {termoBusca && (
                  <button
                    type="button"
                    className="btn-clear-search"
                    onClick={() => setTermoBusca("")}
                  >
                    <FaTimes />
                  </button>
                )}
              </div>

              <div className="picker-games-list">
                {carregandoCatalogo ? (
                  <div className="picker-loading">Carregando catálogo...</div>
                ) : jogosFiltrados.length === 0 ? (
                  <div className="picker-empty">
                    Nenhum jogo encontrado com esse termo.
                  </div>
                ) : (
                  jogosFiltrados.map((jogo) => {
                    const jId = Number(jogo.id || jogo.jogoId);
                    return (
                      <div
                        key={jId}
                        className="picker-game-item"
                        onClick={() => handleSelecionarJogo(jogo)}
                      >
                        <img src={jogo.imagem} alt={jogo.titulo} className="picker-game-thumb" />
                        <div className="picker-game-details">
                          <strong className="picker-game-title">{jogo.titulo}</strong>
                          <span className="picker-game-sub">
                            {jogo.nomeEmpresa || "Game"} {jogo.generos?.length ? `• ${jogo.generos.join(", ")}` : ""}
                          </span>
                        </div>
                        <button type="button" className="btn-picker-select">
                          <FaCheck /> Escolher
                        </button>
                      </div>
                    );
                  })
                )}
              </div>
            </div>
          )}
        </div>

        <div className="modal-favoritos-footer">
          <button
            type="button"
            className="btn-footer-cancelar"
            onClick={onClose}
            disabled={salvando}
          >
            Cancelar
          </button>
          <button
            type="button"
            className="btn-footer-salvar"
            onClick={handleSalvar}
            disabled={salvando}
          >
            {salvando ? "Salvando..." : "Salvar Pódio Favoritos"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ModalEditarFavoritos;
