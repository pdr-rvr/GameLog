import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import ReviewCardV2 from "../../components/ReviewCardV2/ReviewCardV2";
import DiscussionFeedCard from "../../components/DiscussionFeedCard/DiscussionFeedCard";
import ActivityTimelineItem from "../../components/ActivityTimelineItem/ActivityTimelineItem";
import SocialSidebar from "../../components/SocialSidebar/SocialSidebar";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { SocialService } from "../../services/socialService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { buscarJogos, criarAvaliacao } from "../TelaHome/actions/TelaHomeActions";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { 
  FaRss, 
  FaHistory, 
  FaCompass, 
  FaStar,
  FaCommentDots,
  FaLayerGroup,
  FaPlus,
  FaUserFriends
} from "react-icons/fa";
import "./PaginaFeedSocial.css";

const PaginaFeedSocial = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [abaAtiva, setAbaAtiva] = useState("feed"); // "feed" ou "atividades"
  const [filtroConteudo, setFiltroConteudo] = useState("todos"); // "todos", "avaliacoes", "discussoes", "colecoes"
  const [itensFeed, setItensFeed] = useState([]);
  const [atividadesTimeline, setAtividadesTimeline] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [amigosJogando, setAmigosJogando] = useState([]);
  const [gamersSugeridos, setGamersSugeridos] = useState([]);
  const [jogosPodio, setJogosPodio] = useState([]);
  const [loadingFeed, setLoadingFeed] = useState(true);
  const [loadingTimeline, setLoadingTimeline] = useState(false);
  const [modalAberto, setModalAberto] = useState(false);
  const [salvandoAvaliacao, setSalvandoAvaliacao] = useState(false);

  // Carrega feed social principal e dados da sidebar
  useEffect(() => {
    const carregarFeedSocial = async () => {
      setLoadingFeed(true);
      try {
        const [feedData, dadosJogos] = await Promise.all([
          SocialService.obterFeedSocial(1, 30).catch(() => ({
            itens: [],
            amigosJogandoAgora: [],
            gamersSugeridos: []
          })),
          buscarJogos().catch(() => [])
        ]);

        const itens = feedData?.itens || (Array.isArray(feedData) ? feedData : []);
        setItensFeed(itens);
        setJogos(dadosJogos || []);

        // Sidebar data
        if (feedData?.amigosJogandoAgora) {
          setAmigosJogando(feedData.amigosJogandoAgora);
        }
        if (feedData?.gamersSugeridos) {
          setGamersSugeridos(feedData.gamersSugeridos);
        }
      } catch (error) {
        console.error("Erro ao carregar feed social:", error);
      } finally {
        setLoadingFeed(false);
      }
    };

    carregarFeedSocial();
  }, [user?.id]);

  // Carrega timeline de atividades ao trocar para a aba "atividades"
  useEffect(() => {
    if (abaAtiva === "atividades" && atividadesTimeline.length === 0) {
      const carregarTimeline = async () => {
        setLoadingTimeline(true);
        try {
          const dados = await SocialService.obterAtividadesTimeline(1, 40);
          const lista = Array.isArray(dados) ? dados : dados?.itens || [];
          setAtividadesTimeline(lista);
        } catch (error) {
          console.error("Erro ao carregar timeline de atividades:", error);
        } finally {
          setLoadingTimeline(false);
        }
      };

      carregarTimeline();
    }
  }, [abaAtiva, atividadesTimeline.length]);

  const handleToggleCurtirReview = async (avaliacaoId) => {
    try {
      await AvaliacaoService.toggleCurtir(avaliacaoId);
      // Atualiza estado local de curtidas
      setItensFeed((prev) =>
        prev.map((item) => {
          if (item.avaliacaoId === avaliacaoId || item.id === avaliacaoId) {
            const jaCurtido = item.curtidaPorMim;
            return {
              ...item,
              curtidaPorMim: !jaCurtido,
              totalCurtidas: jaCurtido ? Math.max(0, (item.totalCurtidas || 1) - 1) : (item.totalCurtidas || 0) + 1
            };
          }
          return item;
        })
      );
    } catch (error) {
      console.error("Erro ao curtir avaliação:", error);
      toast.error("Não foi possível registrar a curtida.");
    }
  };

  const handleAlternarSeguir = async (usuarioId) => {
    try {
      const res = await SocialService.alternarSeguir(usuarioId);
      toast.success(res?.mensagem || "Ação de seguir atualizada!");
      
      // Atualiza lista de gamers sugeridos
      setGamersSugeridos((prev) =>
        prev.map((g) =>
          g.usuarioId === usuarioId
            ? { ...g, seguidoPorMim: !g.seguidoPorMim }
            : g
        )
      );

      // Recarrega feed e atividades
      const feedData = await SocialService.obterFeedSocial(1, 30);
      setItensFeed(feedData?.itens || (Array.isArray(feedData) ? feedData : []));
    } catch (error) {
      console.error("Erro ao alternar seguir:", error);
      toast.error("Não foi possível atualizar a ação de seguir.");
    }
  };

  const handleSalvarAvaliacao = async (avaliacaoData) => {
    setSalvandoAvaliacao(true);
    try {
      await criarAvaliacao(avaliacaoData);
      setModalAberto(false);
      toast.success("Avaliação publicada com sucesso!");
      
      // Recarrega feed
      const feedData = await SocialService.obterFeedSocial(1, 30);
      setItensFeed(feedData?.itens || (Array.isArray(feedData) ? feedData : []));
    } catch (error) {
      console.error("Erro ao salvar avaliação:", error);
      toast.error(error.message || "Erro ao publicar avaliação.");
    } finally {
      setSalvandoAvaliacao(false);
    }
  };

  const itensFiltrados = itensFeed.filter((item) => {
    const tipo = item.tipoAtividade || item.tipo || "Avaliacao";
    if (filtroConteudo === "avaliacoes") {
      return tipo === "Avaliacao" || (item.nota && item.textoAvaliacao && !item.comentarioTexto);
    }
    if (filtroConteudo === "discussoes") {
      return tipo === "Discussao" || tipo === "Comentario" || Boolean(item.comentarioTexto);
    }
    if (filtroConteudo === "colecoes") {
      return tipo === "ListaCriada" || tipo === "CriouLista" || tipo === "Lista";
    }
    return true; // "todos"
  });

  return (
    <div className="pagina-feed-container">
      <Navbar onPublicarClick={() => setModalAberto(true)} />

      <main className="feed-main-content">
        {/* Cabeçalho e Seleção de Abas */}
        <header className="feed-header">
          <div className="feed-tabs-selector">
            <button
              type="button"
              className={`feed-tab-button ${abaAtiva === "feed" ? "active" : ""}`}
              onClick={() => setAbaAtiva("feed")}
              data-testid="tab-feed-social"
            >
              <FaRss className="feed-tab-icon" />
              <span>Feed Social</span>
            </button>

            <button
              type="button"
              className={`feed-tab-button ${abaAtiva === "atividades" ? "active" : ""}`}
              onClick={() => setAbaAtiva("atividades")}
              data-testid="tab-atividades"
            >
              <FaHistory className="feed-tab-icon" />
              <span>Atividades</span>
            </button>
          </div>
        </header>

        {/* Layout de Duas Colunas (Feed Principal + Sidebar) */}
        <div className="feed-layout-grid">
          {/* Coluna Principal */}
          <div className="feed-primary-column">
            {/* ABA 1: FEED SOCIAL RICO */}
            {abaAtiva === "feed" && (
              <>
                {/* Barra de Filtros de Conteúdo */}
                <div className="feed-content-filter-bar">
                  <button
                    type="button"
                    className={`feed-filter-pill ${filtroConteudo === "todos" ? "active" : ""}`}
                    onClick={() => setFiltroConteudo("todos")}
                    data-testid="filter-todos"
                  >
                    <FaRss className="pill-icon" />
                    <span>Tudo</span>
                  </button>
                  <button
                    type="button"
                    className={`feed-filter-pill ${filtroConteudo === "avaliacoes" ? "active" : ""}`}
                    onClick={() => setFiltroConteudo("avaliacoes")}
                    data-testid="filter-avaliacoes"
                  >
                    <FaStar className="pill-icon" />
                    <span>Resenhas</span>
                  </button>
                  <button
                    type="button"
                    className={`feed-filter-pill ${filtroConteudo === "discussoes" ? "active" : ""}`}
                    onClick={() => setFiltroConteudo("discussoes")}
                    data-testid="filter-discussoes"
                  >
                    <FaCommentDots className="pill-icon" />
                    <span>Discussões</span>
                  </button>
                  <button
                    type="button"
                    className={`feed-filter-pill ${filtroConteudo === "colecoes" ? "active" : ""}`}
                    onClick={() => setFiltroConteudo("colecoes")}
                    data-testid="filter-colecoes"
                  >
                    <FaLayerGroup className="pill-icon" />
                    <span>Coleções</span>
                  </button>
                </div>

                {loadingFeed ? (
                  <div className="feed-loading-state">
                    <div className="feed-spinner"></div>
                    <span>Carregando feed de amigos...</span>
                  </div>
                ) : itensFiltrados.length > 0 ? (
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
                ) : (
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
                        onClick={() => setModalAberto(true)}
                      >
                        <FaPlus /> <span>Publicar sua Análise</span>
                      </button>
                    </div>
                  </div>
                )}
              </>
            )}

            {/* ABA 2: ATIVIDADES (TIMELINE VERTICAL DIRETA) */}
            {abaAtiva === "atividades" && (
              <>
                {loadingTimeline ? (
                  <div className="feed-loading-state">
                    <div className="feed-spinner"></div>
                    <span>Carregando linha do tempo...</span>
                  </div>
                ) : atividadesTimeline.length > 0 ? (
                  <div className="activities-timeline-stream">
                    {atividadesTimeline.map((item, index) => (
                      <ActivityTimelineItem
                        key={item.id || index}
                        item={item}
                      />
                    ))}
                  </div>
                ) : (
                  <div className="feed-empty-state">
                    <FaHistory className="feed-empty-icon" />
                    <h3>Nenhuma atividade recente encontrada</h3>
                    <p>
                      Quando os gamers que você segue jogarem, avaliarem ou zerarem títulos, as atividades aparecerão cronologicamente aqui.
                    </p>
                    <Link to="/comunidade" className="btn-feed-explore">
                      <FaCompass /> <span>Descobrir Jogadores na Comunidade</span>
                    </Link>
                  </div>
                )}
              </>
            )}
          </div>

          {/* Coluna Lateral: Social Sidebar */}
          <div className="feed-secondary-column">
            <SocialSidebar
              amigosJogando={amigosJogando}
              gamersSugeridos={gamersSugeridos}
              jogosPodio={jogosPodio}
              onSeguirClick={handleAlternarSeguir}
            />
          </div>
        </div>
      </main>

      <FormAvaliacao
        isOpen={modalAberto}
        onClose={() => setModalAberto(false)}
        onSubmit={handleSalvarAvaliacao}
        loading={salvandoAvaliacao}
        jogos={jogos}
      />
    </div>
  );
};

export default PaginaFeedSocial;
