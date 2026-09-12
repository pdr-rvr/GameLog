import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import ReviewCardV2 from "../../components/ReviewCardV2/ReviewCardV2";
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
  FaGamepad, 
  FaUserFriends, 
  FaPlus,
  FaCheckCircle,
  FaLayerGroup,
  FaHeart
} from "react-icons/fa";
import "./PaginaFeedSocial.css";

const PaginaFeedSocial = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [abaAtiva, setAbaAtiva] = useState("feed"); // "feed" ou "atividades"
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
  }, [user]);

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
                {loadingFeed ? (
                  <div className="feed-loading-state">
                    <div className="feed-spinner"></div>
                    <span>Carregando feed de amigos...</span>
                  </div>
                ) : itensFeed.length > 0 ? (
                  <div className="feed-cards-stream">
                    {itensFeed.map((item, index) => {
                      const tipo = item.tipoAtividade || item.tipo || "Avaliacao";

                      // Resenha / Análise
                      if (tipo === "Avaliacao" || item.textoAvaliacao || item.nota) {
                        return (
                          <ReviewCardV2
                            key={item.avaliacaoId || item.id || index}
                            avaliacao={item}
                            onToggleCurtir={handleToggleCurtirReview}
                          />
                        );
                      }

                      // Jogo Zerado por Amigo
                      if (tipo === "Zerou" || tipo === "JogoZerado") {
                        return (
                          <article key={item.id || index} className="social-finished-game-card">
                            <div className="finished-card-header">
                              <Link to={`/perfil/${item.usuarioId}`} className="finished-author-link">
                                {item.fotoPerfilUsuario || item.usuarioFoto ? (
                                  <img 
                                    src={item.fotoPerfilUsuario || item.usuarioFoto} 
                                    alt={item.nomeUsuario || item.usuarioNome} 
                                    className="finished-author-avatar"
                                  />
                                ) : (
                                  <div className="finished-avatar-fallback">
                                    {(item.nomeUsuario || item.usuarioNome || "G").charAt(0).toUpperCase()}
                                  </div>
                                )}
                                <div>
                                  <span className="finished-author-name">{item.nomeUsuario || item.usuarioNome}</span>
                                  <span className="finished-action-text">zerou este jogo</span>
                                </div>
                              </Link>
                              <div className="finished-status-badge">
                                <FaCheckCircle /> Zerado
                              </div>
                            </div>

                            <div className="finished-card-body">
                              {item.imagemJogo || item.jogoImagem ? (
                                <Link to={`/jogos/${item.jogoId}`}>
                                  <img 
                                    src={item.imagemJogo || item.jogoImagem} 
                                    alt={item.nomeJogo || item.jogoTitulo} 
                                    className="finished-game-cover"
                                  />
                                </Link>
                              ) : null}
                              <div className="finished-game-info">
                                <Link to={`/jogos/${item.jogoId}`} className="finished-game-title">
                                  {item.nomeJogo || item.jogoTitulo}
                                </Link>
                                {item.nomeEmpresa && <span className="finished-game-studio">{item.nomeEmpresa}</span>}
                              </div>
                            </div>
                          </article>
                        );
                      }

                      // Coleção Criada por Amigo
                      if (tipo === "CriouLista" || tipo === "Lista") {
                        return (
                          <article key={item.id || index} className="social-collection-created-card">
                            <div className="finished-card-header">
                              <Link to={`/perfil/${item.usuarioId}`} className="finished-author-link">
                                <div className="finished-avatar-fallback">
                                  {(item.nomeUsuario || item.usuarioNome || "G").charAt(0).toUpperCase()}
                                </div>
                                <div>
                                  <span className="finished-author-name">{item.nomeUsuario || item.usuarioNome}</span>
                                  <span className="finished-action-text">criou uma nova coleção</span>
                                </div>
                              </Link>
                              <div className="collection-status-badge">
                                <FaLayerGroup /> Coleção
                              </div>
                            </div>

                            <div className="collection-card-details">
                              <Link to={`/listas/${item.listaId || item.id}`} className="collection-created-title">
                                {item.titulo || item.listaTitulo}
                              </Link>
                              {item.descricao && <p className="collection-created-desc">{item.descricao}</p>}
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
                      Siga outros jogadores e amigos para acompanhar suas análises, conquistas e jogos finalizados em tempo real!
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
