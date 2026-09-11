import React, { useState, useEffect, useMemo, useRef } from "react";
import { FaTimes, FaSearch, FaPlus, FaTrash, FaLayerGroup, FaLock, FaGlobe, FaCheck } from "react-icons/fa";
import { buscarJogos } from "../../pages/PaginaJogos/actions/PaginaJogosActions";
import { ListaService } from "../../services/listaService";
import { useToast } from "../../context/ToastContext";
import "./ModalCriarLista.css";

const ModalCriarLista = ({
  isOpen,
  onClose,
  listaParaEditar = null,
  onListaSalva
}) => {
  const toast = useToast();
  const [titulo, setTitulo] = useState("");
  const [descricao, setDescricao] = useState("");
  const [estaPublica, setEstaPublica] = useState(true);
  const [jogosSelecionados, setJogosSelecionados] = useState([]);
  const [catalogo, setCatalogo] = useState([]);
  const [termoBusca, setTermoBusca] = useState("");
  const [salvando, setSalvando] = useState(false);
  const [carregandoCatalogo, setCarregandoCatalogo] = useState(false);
  const searchInputRef = useRef(null);

  useEffect(() => {
    if (isOpen) {
      if (listaParaEditar) {
        setTitulo(listaParaEditar.titulo || "");
        setDescricao(listaParaEditar.descricao || "");
        setEstaPublica(listaParaEditar.estaPublica !== false);
        setJogosSelecionados(
          (listaParaEditar.itens || []).map((i) => ({
            id: i.jogoId || i.id,
            titulo: i.tituloJogo || i.titulo,
            imagem: i.imagemJogo || i.imagem || "/game-images/default_game_cover.png",
            nomeEmpresa: i.nomeEmpresa || ""
          }))
        );
      } else {
        setTitulo("");
        setDescricao("");
        setEstaPublica(true);
        setJogosSelecionados([]);
      }
      setTermoBusca("");

      if (catalogo.length === 0) {
        setCarregandoCatalogo(true);
        buscarJogos()
          .then((dados) => {
            const normalizados = (dados || []).map((j) => ({
              ...j,
              id: j.id || j.jogoId,
              jogoId: j.id || j.jogoId
            }));
            setCatalogo(normalizados);
          })
          .catch((err) => {
            console.error("Erro ao carregar catálogo:", err);
          })
          .finally(() => setCarregandoCatalogo(false));
      }
    }
  }, [isOpen, listaParaEditar]);

  const jogosDisponiveis = useMemo(() => {
    const idsJaSelecionados = new Set(jogosSelecionados.map((j) => String(j.id).toLowerCase()));
    return catalogo.filter((j) => {
      if (idsJaSelecionados.has(String(j.id).toLowerCase())) return false;
      if (!termoBusca.trim()) return true;
      const t = termoBusca.toLowerCase();
      return (
        j.titulo?.toLowerCase().includes(t) ||
        j.nomeEmpresa?.toLowerCase().includes(t)
      );
    });
  }, [catalogo, termoBusca, jogosSelecionados]);

  const handleAdicionarJogo = (jogo) => {
    setJogosSelecionados((prev) => [
      ...prev,
      {
        id: jogo.id || jogo.jogoId,
        titulo: jogo.titulo,
        imagem: jogo.imagem || "/game-images/default_game_cover.png",
        nomeEmpresa: jogo.nomeEmpresa || ""
      }
    ]);
  };

  const handleRemoverJogo = (jogoId) => {
    setJogosSelecionados((prev) => prev.filter((j) => j.id !== jogoId));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!titulo.trim()) {
      toast.warning("Por favor, digite um título para a lista.");
      return;
    }

    setSalvando(true);
    try {
      const payload = {
        titulo: titulo.trim(),
        descricao: descricao.trim() || null,
        estaPublica,
        jogosIds: jogosSelecionados.map((j) => j.id)
      };

      let resultado;
      if (listaParaEditar) {
        resultado = await ListaService.editarLista(listaParaEditar.listaId || listaParaEditar.id, payload);
        toast.success("Coleção atualizada com sucesso!");
      } else {
        resultado = await ListaService.criarLista(payload);
        toast.success("Coleção criada com sucesso!");
      }

      if (onListaSalva) onListaSalva(resultado);
      onClose();
    } catch (err) {
      console.error("Erro ao salvar lista:", err);
      toast.error(err.response?.data?.message || "Erro ao salvar a coleção.");
    } finally {
      setSalvando(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-lista-overlay" onClick={onClose}>
      <div className="modal-lista-dialog" onClick={(e) => e.stopPropagation()}>
        <header className="modal-lista-header">
          <div className="modal-lista-title-wrap">
            <div className="modal-lista-icon">
              <FaLayerGroup />
            </div>
            <div>
              <h2>{listaParaEditar ? "Editar Coleção" : "Criar Nova Coleção"}</h2>
              <p>Monte listas temáticas de jogos e compartilhe com a comunidade</p>
            </div>
          </div>
          <button type="button" className="btn-modal-close" onClick={onClose} aria-label="Fechar">
            <FaTimes />
          </button>
        </header>

        <form onSubmit={handleSubmit} className="modal-lista-body">
          <div className="modal-lista-fields-grid">
            <div className="lista-form-group">
              <label htmlFor="listaTitulo">Título da Coleção *</label>
              <input
                id="listaTitulo"
                type="text"
                maxLength={100}
                placeholder="Ex: Melhores RPGs de Todos os Tempos, Backlog 2026..."
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                required
                className="input-modern"
              />
            </div>

            <div className="lista-form-group">
              <label htmlFor="listaDescricao">Descrição / Apresentação (Opcional)</label>
              <textarea
                id="listaDescricao"
                rows={2}
                maxLength={500}
                placeholder="Conte sobre o tema da sua lista ou compartilhe seus critérios de escolha..."
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
                className="textarea-modern"
              />
            </div>

            <div className="lista-visibilidade-row">
              <span className="visibilidade-label">Visibilidade da Coleção:</span>
              <div className="visibilidade-toggle-group">
                <button
                  type="button"
                  className={`btn-visibilidade-opt ${estaPublica ? "active" : ""}`}
                  onClick={() => setEstaPublica(true)}
                >
                  <FaGlobe /> <span>Pública (Visível a todos)</span>
                </button>
                <button
                  type="button"
                  className={`btn-visibilidade-opt ${!estaPublica ? "active" : ""}`}
                  onClick={() => setEstaPublica(false)}
                >
                  <FaLock /> <span>Privada (Apenas para mim)</span>
                </button>
              </div>
            </div>
          </div>

          <div className="lista-jogos-section">
            <div className="jogos-section-header">
              <div>
                <h3>Jogos na Coleção ({jogosSelecionados.length})</h3>
                <span className="section-hint">Adicione os títulos que compõem sua lista</span>
              </div>
            </div>

            {/* Jogos Adicionados */}
            {jogosSelecionados.length > 0 && (
              <div className="selected-games-mosaic">
                {jogosSelecionados.map((jogo, index) => (
                  <div key={jogo.id} className="selected-game-pill">
                    <span className="game-order-badge">{index + 1}</span>
                    <img src={jogo.imagem} alt={jogo.titulo} className="game-thumb-mini" />
                    <div className="game-meta-mini">
                      <strong title={jogo.titulo}>{jogo.titulo}</strong>
                      <span>{jogo.nomeEmpresa || "Game"}</span>
                    </div>
                    <button
                      type="button"
                      className="btn-remove-selected-game"
                      onClick={() => handleRemoverJogo(jogo.id)}
                      title="Remover da lista"
                    >
                      <FaTimes />
                    </button>
                  </div>
                ))}
              </div>
            )}

            {/* Busca para adicionar mais jogos */}
            <div className="add-games-search-box">
              <div className="search-input-wrap">
                <FaSearch className="search-icon" />
                <input
                  ref={searchInputRef}
                  type="text"
                  placeholder="Pesquise jogos pelo título ou estúdio para adicionar..."
                  value={termoBusca}
                  onChange={(e) => setTermoBusca(e.target.value)}
                  className="input-search-games"
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

              {termoBusca.trim() && (
                <div className="search-results-tray">
                  {carregandoCatalogo ? (
                    <div className="tray-loading">Carregando jogos...</div>
                  ) : jogosDisponiveis.length > 0 ? (
                    <div className="tray-grid">
                      {jogosDisponiveis.slice(0, 8).map((j) => (
                        <div
                          key={j.id}
                          className="tray-item"
                          onClick={() => handleAdicionarJogo(j)}
                        >
                          <img src={j.imagem} alt={j.titulo} className="tray-thumb" />
                          <div className="tray-info">
                            <strong>{j.titulo}</strong>
                            <span>{j.nomeEmpresa || "Game"}</span>
                          </div>
                          <button type="button" className="btn-add-tray">
                            <FaPlus />
                          </button>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="tray-empty">Nenhum jogo disponível encontrado.</div>
                  )}
                </div>
              )}
            </div>
          </div>

          <footer className="modal-lista-footer">
            <button
              type="button"
              className="btn-cancel-lista"
              onClick={onClose}
              disabled={salvando}
            >
              Cancelar
            </button>
            <button
              type="submit"
              className="btn-save-lista"
              disabled={salvando || !titulo.trim()}
            >
              <FaCheck />
              <span>{salvando ? "Salvando..." : listaParaEditar ? "Salvar Alterações" : "Criar Coleção"}</span>
            </button>
          </footer>
        </form>
      </div>
    </div>
  );
};

export default ModalCriarLista;
