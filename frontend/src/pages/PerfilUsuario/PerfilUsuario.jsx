import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import AvaliacaoCard from "../../components/AvaliacaoCard/AvaliacaoCard";
import ConfirmModal from "../../components/ConfirmModal/ConfirmModal";
import PodioFavoritos from "../../components/PodioFavoritos/PodioFavoritos";
import ModalEditarFavoritos from "../../components/ModalEditarFavoritos/ModalEditarFavoritos";
import ModalCriarLista from "../../components/ModalCriarLista/ModalCriarLista";
import BibliotecaCard from "../../components/BibliotecaCard/BibliotecaCard";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { fetchUserProfile, fetchUserReviews, fetchUserTopGenres } from "./actions/PerfilUsuarioActions";
import { deleteReview } from "../../pages/MinhasAvaliacoes/actions/MinhasAvaliacoesActions";
import { BibliotecaService, STATUS_JOGO } from "../../services/bibliotecaService";
import { ListaService } from "../../services/listaService";
import { 
  FaGamepad, 
  FaStar, 
  FaCog, 
  FaComments, 
  FaLayerGroup, 
  FaCalendarAlt,
  FaAward,
  FaBookmark,
  FaSearch,
  FaTimes,
  FaPlus,
  FaGlobe,
  FaLock
} from "react-icons/fa";
import "./PerfilUsuario.css";

const PerfilUsuario = () => {
  const { userId } = useParams();
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [perfil, setPerfil] = useState(null);
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [topGeneros, setTopGeneros] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [itemParaExcluir, setItemParaExcluir] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Top 5 Favorites State
  const [favoritos, setFavoritos] = useState([]);
  const [modalFavoritosAberto, setModalFavoritosAberto] = useState(false);

  // Collections / Lists State
  const [listas, setListas] = useState([]);
  const [modalCriarListaAberto, setModalCriarListaAberto] = useState(false);
  const [listaEmEdicao, setListaEmEdicao] = useState(null);

  // Active Main Section (biblioteca | listas | avaliacoes)
  const [secaoAtiva, setSecaoAtiva] = useState("biblioteca");

  // Library State
  const [itensBiblioteca, setItensBiblioteca] = useState([]);
  const [statsBiblioteca, setStatsBiblioteca] = useState({
    totalJogos: 0,
    totalQueroJogar: 0,
    totalJogando: 0,
    totalZerados: 0,
    totalPausados: 0,
    totalAbandonados: 0,
  });
  const [statusFiltro, setStatusFiltro] = useState(null);
  const [buscaBiblioteca, setBuscaBiblioteca] = useState("");
  const [loadingBiblioteca, setLoadingBiblioteca] = useState(false);
  const [jogoParaRemoverBiblioteca, setJogoParaRemoverBiblioteca] = useState(null);

  // Target User ID to load
  const targetId = userId ? parseInt(userId, 10) : user?.id;
  const isOwner = user && targetId && Number(user.id) === Number(targetId);

  const carregarDadosPerfil = useCallback(async () => {
    if (!targetId) {
      setError("Identificador de usuário não encontrado.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");
    try {
      const [dadosUsuario, dadosAvaliacoes, dadosGeneros, dadosFavoritos, dadosStats, dadosListas] = await Promise.all([
        fetchUserProfile(targetId),
        fetchUserReviews(targetId),
        fetchUserTopGenres(targetId),
        BibliotecaService.obterFavoritos(targetId).catch(() => []),
        BibliotecaService.obterEstatisticas(targetId).catch(() => ({
          totalJogos: 0,
          totalQueroJogar: 0,
          totalJogando: 0,
          totalZerados: 0,
          totalPausados: 0,
          totalAbandonados: 0,
        })),
        ListaService.listarListasDoUsuario(targetId).catch(() => [])
      ]);

      setPerfil(dadosUsuario);
      setAvaliacoes(dadosAvaliacoes || []);
      setTopGeneros(dadosGeneros || []);
      setFavoritos(dadosFavoritos || []);
      setStatsBiblioteca(dadosStats);
      setListas(dadosListas || []);
    } catch (err) {
      console.error("Erro ao carregar perfil:", err);
      setError(err.message || "Não foi possível carregar o perfil do jogador.");
    } finally {
      setLoading(false);
    }
  }, [targetId]);

  useEffect(() => {
    carregarDadosPerfil();
  }, [carregarDadosPerfil]);

  // Load Library Items with filters
  const carregarItensBiblioteca = useCallback(async () => {
    if (!targetId) return;
    setLoadingBiblioteca(true);
    try {
      const itens = await BibliotecaService.listarBibliotecaUsuario(
        targetId,
        statusFiltro,
        buscaBiblioteca
      );
      setItensBiblioteca(itens || []);
    } catch (err) {
      console.error("Erro ao carregar biblioteca:", err);
    } finally {
      setLoadingBiblioteca(false);
    }
  }, [targetId, statusFiltro, buscaBiblioteca]);

  useEffect(() => {
    carregarItensBiblioteca();
  }, [carregarItensBiblioteca]);

  // Refresh Stats and Library
  const recarregarBibliotecaCompleta = async () => {
    if (!targetId) return;
    try {
      const [itens, stats] = await Promise.all([
        BibliotecaService.listarBibliotecaUsuario(targetId, statusFiltro, buscaBiblioteca),
        BibliotecaService.obterEstatisticas(targetId)
      ]);
      setItensBiblioteca(itens || []);
      setStatsBiblioteca(stats);
    } catch (err) {
      console.error("Erro ao recarregar estatísticas da biblioteca:", err);
    }
  };

  // Statistics calculation
  const totalAvaliacoes = avaliacoes.length;
  const mediaNotas = totalAvaliacoes > 0
    ? (avaliacoes.reduce((acc, curr) => acc + (Number(curr.nota) || 0), 0) / totalAvaliacoes).toFixed(1)
    : "0.0";

  const handleEditReview = (avaliacao) => {
    const id = avaliacao?.avaliacaoId || avaliacao?.id || avaliacao;
    navigate(`/avaliacoes/editar/${id}`, { state: { from: `/perfil/${targetId}` } });
  };

  const handleDeleteRequest = (avaliacaoId) => {
    setItemParaExcluir(avaliacaoId);
  };

  const handleConfirmDelete = async () => {
    if (!itemParaExcluir) return;
    setIsDeleting(true);
    try {
      await deleteReview(itemParaExcluir);
      setAvaliacoes((prev) => prev.filter((a) => (a.avaliacaoId || a.id) !== itemParaExcluir));
      toast.success("Avaliação excluída com sucesso!");
    } catch (err) {
      toast.error(err.message || "Erro ao excluir avaliação.");
    } finally {
      setIsDeleting(false);
      setItemParaExcluir(null);
    }
  };

  const handleConfirmRemoverBiblioteca = async () => {
    if (!jogoParaRemoverBiblioteca) return;
    try {
      await BibliotecaService.removerItem(jogoParaRemoverBiblioteca);
      toast.success("Jogo removido da biblioteca.");
      setJogoParaRemoverBiblioteca(null);
      await recarregarBibliotecaCompleta();
    } catch (err) {
      console.error("Erro ao remover da biblioteca:", err);
      toast.error(err.response?.data?.message || "Erro ao remover jogo da biblioteca.");
    }
  };

  const recarregarListas = async () => {
    if (!targetId) return;
    try {
      const dados = await ListaService.listarListasDoUsuario(targetId);
      setListas(dados || []);
    } catch (err) {
      console.error("Erro ao recarregar coleções:", err);
    }
  };

  const tabsFiltroBiblioteca = [
    { status: null, label: "Todos", count: statsBiblioteca.totalJogos },
    { status: STATUS_JOGO.QUERO_JOGAR, label: "Quero Jogar", count: statsBiblioteca.totalQueroJogar },
    { status: STATUS_JOGO.JOGANDO, label: "Jogando", count: statsBiblioteca.totalJogando },
    { status: STATUS_JOGO.ZERADO, label: "Zerado", count: statsBiblioteca.totalZerados },
    { status: STATUS_JOGO.PAUSADO, label: "Pausado", count: statsBiblioteca.totalPausados },
    { status: STATUS_JOGO.ABANDONADO, label: "Abandonado", count: statsBiblioteca.totalAbandonados },
  ];

  return (
    <div className="perfil-social-page">
      <Navbar />

      <main className="perfil-social-container">
        {loading && (
          <div className="perfil-state loading">
            <div className="perfil-spinner"></div>
            <span>Carregando perfil gamer...</span>
          </div>
        )}

        {error && !loading && (
          <div className="perfil-state error">
            <span>{error}</span>
          </div>
        )}

        {!loading && !error && perfil && (
          <>
            {/* Hero Header do Gamer */}
            <header className="gamer-hero-card">
              <div className="gamer-avatar-wrapper">
                {perfil.fotoDePerfil ? (
                  <img src={perfil.fotoDePerfil} alt={perfil.nomeUsuario} className="gamer-avatar-img" />
                ) : (
                  <div className="gamer-avatar-fallback">
                    <span>{perfil.nomeUsuario ? perfil.nomeUsuario.charAt(0).toUpperCase() : "G"}</span>
                  </div>
                )}
              </div>

              <div className="gamer-hero-info">
                <div className="gamer-hero-top">
                  <div className="gamer-title-group">
                    <h1 className="gamer-username">{perfil.nomeUsuario || "Gamer"}</h1>
                    <span className="gamer-badge">
                      <FaGamepad /> Membro GameLog
                    </span>
                  </div>

                  {/* Botão de Configurações - Somente visível para o dono */}
                  {isOwner && (
                    <Link to="/configuracoes" className="btn-edit-settings">
                      <FaCog /> <span>Configurações de Conta</span>
                    </Link>
                  )}
                </div>

                {/* Bio Gamer */}
                {perfil.bio ? (
                  <p className="gamer-bio-text">{perfil.bio}</p>
                ) : isOwner ? (
                  <p className="gamer-bio-text empty">
                    Você ainda não adicionou uma bio. <Link to="/configuracoes">Adicione uma apresentação</Link>
                  </p>
                ) : (
                  <p className="gamer-bio-text empty">Este jogador ainda não adicionou uma bio.</p>
                )}
              </div>
            </header>

            {/* Pódio Top 5 Jogos Favoritos (Estilo Letterboxd) */}
            <PodioFavoritos
              favoritos={favoritos}
              isOwner={isOwner}
              onEditar={() => setModalFavoritosAberto(true)}
            />

            {/* Barra de Estatísticas Rápidas do Gamer */}
            <section className="gamer-stats-bar">
              <div className="stat-card">
                <div className="stat-icon-wrapper purple">
                  <FaBookmark />
                </div>
                <div className="stat-data">
                  <span className="stat-value">{statsBiblioteca.totalJogos}</span>
                  <span className="stat-label">Jogos na Biblioteca</span>
                </div>
              </div>

              <div className="stat-card">
                <div className="stat-icon-wrapper cyan">
                  <FaGamepad />
                </div>
                <div className="stat-data">
                  <span className="stat-value">{statsBiblioteca.totalZerados}</span>
                  <span className="stat-label">Jogos Zerados</span>
                </div>
              </div>

              <div className="stat-card">
                <div className="stat-icon-wrapper green">
                  <FaLayerGroup />
                </div>
                <div className="stat-data">
                  <span className="stat-value">{listas.length}</span>
                  <span className="stat-label">Coleções Criadas</span>
                </div>
              </div>

              <div className="stat-card">
                <div className="stat-icon-wrapper gold">
                  <FaStar />
                </div>
                <div className="stat-data">
                  <span className="stat-value">{mediaNotas}</span>
                  <span className="stat-label">Média das Avaliações</span>
                </div>
              </div>

              <div className="stat-card full-span">
                <div className="stat-icon-wrapper pink">
                  <FaAward />
                </div>
                <div className="stat-data">
                  <span className="stat-label">Gêneros Favoritos</span>
                  <div className="genre-chips-wrapper">
                    {topGeneros.length > 0 ? (
                      topGeneros.map((g, idx) => (
                        <span key={idx} className="genre-chip">
                          {g.genero || g.tituloGenero || g}
                        </span>
                      ))
                    ) : (
                      <span className="genre-chip empty">Gamer Eclético</span>
                    )}
                  </div>
                </div>
              </div>
            </section>

            {/* Seletor de Seções Principais do Perfil */}
            <nav className="perfil-nav-sections">
              <button
                type="button"
                className={`btn-section-tab ${secaoAtiva === "biblioteca" ? "active" : ""}`}
                onClick={() => setSecaoAtiva("biblioteca")}
              >
                <FaBookmark />
                <span>Biblioteca ({statsBiblioteca.totalJogos})</span>
              </button>

              <button
                type="button"
                className={`btn-section-tab ${secaoAtiva === "listas" ? "active" : ""}`}
                onClick={() => setSecaoAtiva("listas")}
              >
                <FaLayerGroup />
                <span>Coleções & Listas ({listas.length})</span>
              </button>

              <button
                type="button"
                className={`btn-section-tab ${secaoAtiva === "avaliacoes" ? "active" : ""}`}
                onClick={() => setSecaoAtiva("avaliacoes")}
              >
                <FaComments />
                <span>Avaliações ({totalAvaliacoes})</span>
              </button>
            </nav>

            {/* SEÇÃO 1: Biblioteca Gamer */}
            {secaoAtiva === "biblioteca" && (
              <section className="gamer-library-section">
                <div className="library-section-header">
                  <div>
                    <h2 className="section-title">
                      Biblioteca de {isOwner ? "Você" : perfil.nomeUsuario}
                    </h2>
                    <span className="section-subtitle">Coleção e status de progresso dos jogos</span>
                  </div>
                </div>

                {/* Filter Tabs and Search Bar */}
                <div className="library-toolbar">
                  <div className="library-tabs-row">
                    {tabsFiltroBiblioteca.map((tab, idx) => {
                      const isAtivo = statusFiltro === tab.status;
                      return (
                        <button
                          key={idx}
                          type="button"
                          className={`library-tab-btn ${isAtivo ? "ativo" : ""}`}
                          onClick={() => setStatusFiltro(tab.status)}
                        >
                          <span className="tab-label">{tab.label}</span>
                          <span className="tab-count">{tab.count}</span>
                        </button>
                      );
                    })}
                  </div>

                  <div className="library-search-input-wrap">
                    <FaSearch className="search-icon" />
                    <input
                      type="text"
                      placeholder="Buscar na biblioteca..."
                      value={buscaBiblioteca}
                      onChange={(e) => setBuscaBiblioteca(e.target.value)}
                    />
                    {buscaBiblioteca && (
                      <button
                        type="button"
                        className="btn-clear-library-search"
                        onClick={() => setBuscaBiblioteca("")}
                      >
                        <FaTimes />
                      </button>
                    )}
                  </div>
                </div>

                {/* Library Cards Grid */}
                {loadingBiblioteca ? (
                  <div className="library-loading-state">
                    <div className="perfil-spinner small"></div>
                    <span>Atualizando biblioteca...</span>
                  </div>
                ) : itensBiblioteca.length === 0 ? (
                  <div className="library-empty-state">
                    <FaBookmark className="empty-lib-icon" />
                    <h3>Nenhum jogo encontrado</h3>
                    <p>
                      {statusFiltro !== null
                        ? "Nenhum jogo corresponde a esse filtro de status."
                        : buscaBiblioteca
                        ? "Nenhum jogo encontrado com esse termo de busca."
                        : isOwner
                        ? "Sua biblioteca está vazia. Comece a adicionar os jogos que você está jogando ou já zerou!"
                        : `${perfil.nomeUsuario} ainda não adicionou jogos a esta categoria.`}
                    </p>
                    {isOwner && (
                      <Link to="/jogos" className="btn-browse-games">
                        Explorar Catálogo de Jogos
                      </Link>
                    )}
                  </div>
                ) : (
                  <div className="library-cards-grid">
                    {itensBiblioteca.map((item) => (
                      <BibliotecaCard
                        key={item.id || item.jogoId}
                        item={item}
                        isOwner={isOwner}
                        onRemover={(id) => setJogoParaRemoverBiblioteca(id)}
                      />
                    ))}
                  </div>
                )}
              </section>
            )}

            {/* SEÇÃO 2: Coleções & Listas */}
            {secaoAtiva === "listas" && (
              <section className="gamer-collections-section">
                <div className="collections-section-header">
                  <div>
                    <h2 className="section-title">
                      Coleções de {isOwner ? "Você" : perfil.nomeUsuario}
                    </h2>
                    <span className="section-subtitle">Listas temáticas e seleções especiais</span>
                  </div>

                  {isOwner && (
                    <button
                      type="button"
                      className="btn-create-collection"
                      onClick={() => {
                        setListaEmEdicao(null);
                        setModalCriarListaAberto(true);
                      }}
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
                        : `${perfil.nomeUsuario} ainda não criou coleções públicas.`}
                    </p>
                    {isOwner && (
                      <button
                        type="button"
                        className="btn-create-collection-cta"
                        onClick={() => {
                          setListaEmEdicao(null);
                          setModalCriarListaAberto(true);
                        }}
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
                                <img key={idx} src={capa} alt="Capa" className="mosaic-thumb" />
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
            )}

            {/* SEÇÃO 3: Feed de Avaliações do Usuário */}
            {secaoAtiva === "avaliacoes" && (
              <section className="gamer-reviews-section">
                <div className="section-header">
                  <div>
                    <h2 className="section-title">
                      Avaliações de {isOwner ? "Você" : perfil.nomeUsuario}
                    </h2>
                    <span className="section-subtitle">Críticas e impressões detalhadas</span>
                  </div>
                  <span className="section-counter">{totalAvaliacoes} {totalAvaliacoes === 1 ? "publicação" : "publicações"}</span>
                </div>

                {avaliacoes.length === 0 ? (
                  <div className="empty-gamer-reviews">
                    <FaGamepad className="empty-icon" />
                    <h3>Nenhuma avaliação publicada ainda</h3>
                    <p>
                      {isOwner
                        ? "Você ainda não avaliou nenhum jogo. Explore nosso catálogo e compartilhe suas opiniões!"
                        : `${perfil.nomeUsuario} ainda não publicou nenhuma avaliação.`}
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
                          onEdit={isOwner ? () => handleEditReview(avaliacao) : null}
                          onDelete={isOwner ? () => handleDeleteRequest(avaliacao.avaliacaoId || avaliacao.id) : null}
                        />
                      </div>
                    ))}
                  </div>
                )}
              </section>
            )}
          </>
        )}
      </main>

      {/* Modal de Personalização dos 5 Favoritos (Pódio) */}
      <ModalEditarFavoritos
        isOpen={modalFavoritosAberto}
        onClose={() => setModalFavoritosAberto(false)}
        favoritosAtuais={favoritos}
        onSalvo={(novos) => setFavoritos(novos || [])}
      />

      {/* Modal para Criar / Editar Lista */}
      <ModalCriarLista
        isOpen={modalCriarListaAberto}
        onClose={() => {
          setModalCriarListaAberto(false);
          setListaEmEdicao(null);
        }}
        listaParaEditar={listaEmEdicao}
        onListaSalva={recarregarListas}
      />

      {/* Modal de Confirmação para exclusão de Avaliação */}
      <ConfirmModal
        isOpen={Boolean(itemParaExcluir)}
        title="Excluir Avaliação"
        message="Tem certeza que deseja excluir esta avaliação do seu perfil público?"
        confirmText="Excluir Definitivamente"
        cancelText="Cancelar"
        confirmVariant="danger"
        loading={isDeleting}
        onConfirm={handleConfirmDelete}
        onCancel={() => !isDeleting && setItemParaExcluir(null)}
      />

      {/* Modal de Confirmação para remoção de Jogo da Biblioteca */}
      <ConfirmModal
        isOpen={Boolean(jogoParaRemoverBiblioteca)}
        title="Remover da Biblioteca"
        message="Deseja realmente remover este jogo da sua biblioteca de jogos?"
        confirmText="Remover"
        cancelText="Cancelar"
        confirmVariant="danger"
        onConfirm={handleConfirmRemoverBiblioteca}
        onCancel={() => setJogoParaRemoverBiblioteca(null)}
      />
    </div>
  );
};

export default PerfilUsuario;
