import React, { useState, useEffect, useCallback } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogosCarrossel from "../../components/JogosCarrossel/JogosCarrossel";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { buscarAvaliacoes, buscarJogos, criarAvaliacao, buscarRecomendacoes } from "./actions/TelaHomeActions";
import { SocialService } from "../../services/socialService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { 
  FaGlobe, 
  FaUserFriends, 
  FaGamepad, 
  FaStar, 
  FaCheckCircle, 
  FaLayerGroup, 
  FaCalendarAlt,
  FaArrowRight,
  FaHeart,
  FaRegHeart,
  FaCommentDots
} from "react-icons/fa";
import "./TelaHome.css";

const TelaHome = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const location = useLocation();

  const [abaAtiva, setAbaAtiva] = useState("global");
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [recomendacoes, setRecomendacoes] = useState([]);
  const [feedAmigos, setFeedAmigos] = useState([]);
  const [loadingFeed, setLoadingFeed] = useState(false);
  const [modalAberto, setModalAberto] = useState(false);
  const [salvandoAvaliacao, setSalvandoAvaliacao] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const carregarDados = async () => {
      setLoading(true);
      try {
        const [dadosAvaliacoes, dadosJogos] = await Promise.all([
          buscarAvaliacoes(),
          buscarJogos()
        ]);
        setAvaliacoes(dadosAvaliacoes);
        setJogos(dadosJogos);

        if (user && user.id) {
          const dadosRecomendacoes = await buscarRecomendacoes(user.id);
          setRecomendacoes(dadosRecomendacoes);
        }
      } catch (error) {
        console.error("Erro ao carregar dados da home:", error);
      } finally {
        setLoading(false);
      }
    };

    carregarDados();
  }, [user]);

  const carregarFeedAmigos = useCallback(async () => {
    if (!user) return;
    setLoadingFeed(true);
    try {
      const feed = await SocialService.obterFeedSocial(1, 20);
      setFeedAmigos(feed || []);
    } catch (err) {
      console.error("Erro ao carregar feed social:", err);
    } finally {
      setLoadingFeed(false);
    }
  }, [user]);

  const handleToggleCurtirFeed = async (avaliacaoId) => {
    if (!user) {
      toast.warning("Faça login para curtir avaliações.");
      return;
    }
    try {
      const res = await AvaliacaoService.toggleCurtir(avaliacaoId);
      setFeedAmigos((prev) =>
        prev.map((item) =>
          item.avaliacaoId === avaliacaoId
            ? {
                ...item,
                curtidaPorMim: res.curtido,
                totalCurtidas: res.totalCurtidas,
              }
            : item
        )
      );
    } catch (err) {
      console.error("Erro ao curtir avaliação no feed:", err);
      toast.error(err.response?.data?.message || err.message || "Erro ao atualizar curtida.");
    }
  };

  useEffect(() => {
    if (abaAtiva === "amigos" && user) {
      carregarFeedAmigos();
    }
  }, [abaAtiva, user, carregarFeedAmigos]);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    if (params.get("publish") === "true") {
      setModalAberto(true);
    }
  }, [location]);

  const fecharModal = () => {
    setModalAberto(false);
    const params = new URLSearchParams(location.search);
    if (params.get("publish") === "true") {
      navigate("/home", { replace: true });
    }
  };

  const handleSalvarAvaliacao = async (avaliacaoData) => {
    setSalvandoAvaliacao(true);
    try {
      await criarAvaliacao(avaliacaoData);
      fecharModal();
      toast.success("Avaliação publicada com sucesso!");
      const novasAvaliacoes = await buscarAvaliacoes();
      setAvaliacoes(novasAvaliacoes);
      if (abaAtiva === "amigos") {
        carregarFeedAmigos();
      }
    } catch (error) {
      console.error("Erro ao salvar avaliação:", error);
      toast.error(error.message || "Erro ao publicar avaliação. Tente novamente.");
    } finally {
      setSalvandoAvaliacao(false);
    }
  };

  const formatarData = (dataStr) => {
    if (!dataStr) return "";
    try {
      const d = new Date(dataStr);
      return d.toLocaleDateString("pt-BR", {
        day: "2-digit",
        month: "short",
        hour: "2-digit",
        minute: "2-digit"
      });
    } catch {
      return String(dataStr).substring(0, 10);
    }
  };

  return (
    <div className="home-container">
      <Navbar onPublicarClick={() => setModalAberto(true)} />

      <div className="home-content">
        {/* Seletor de Abas da Home (Global vs Feed Amigos) */}
        {user && (
          <div className="home-tabs-container">
            <div className="home-tabs-wrapper">
              <button
                type="button"
                className={`home-tab-btn ${abaAtiva === "global" ? "active" : ""}`}
                onClick={() => setAbaAtiva("global")}
              >
                <FaGlobe className="tab-icon" />
                <span>Destaques da Comunidade</span>
              </button>
              <button
                type="button"
                className={`home-tab-btn ${abaAtiva === "amigos" ? "active" : ""}`}
                onClick={() => setAbaAtiva("amigos")}
              >
                <FaUserFriends className="tab-icon" />
                <span>Feed dos Amigos</span>
              </button>
            </div>
          </div>
        )}

        {/* Visualização: Destaques Globais */}
        {abaAtiva === "global" && (
          <>
            {loading ? (
              <div className="loading-message">
                <div className="home-spinner"></div>
                <span>Carregando catálogo e avaliações...</span>
              </div>
            ) : (
              <>
                {user && recomendacoes.length > 0 && (
                  <JogosCarrossel
                    title="Recomendados Para Você"
                    jogos={recomendacoes}
                  />
                )}

                <JogosCarrossel
                  title="Jogos em Destaque"
                  jogos={jogos.slice(0, 20)}
                />

                <AvaliacaoCarrossel
                  title="Últimas Avaliações da Comunidade"
                  avaliacoes={avaliacoes}
                />
              </>
            )}
          </>
        )}

        {/* Visualização: Feed Social dos Amigos */}
        {abaAtiva === "amigos" && user && (
          <div className="social-feed-section">
            <header className="social-feed-header">
              <h2>Atividades Recentes dos Amigos</h2>
              <p>Acompanhe o que os jogadores que você segue estão avaliando, jogando e colecionando.</p>
            </header>

            {loadingFeed ? (
              <div className="social-feed-loading">
                <div className="home-spinner"></div>
                <span>Carregando feed de atividades...</span>
              </div>
            ) : feedAmigos.length === 0 ? (
              <div className="social-feed-empty">
                <FaUserFriends className="empty-social-icon" />
                <h3>Nenhuma atividade recente</h3>
                <p>
                  Você ainda não está seguindo outros jogadores ou seus amigos não publicaram novas atividades recentemente.
                </p>
                <Link to="/jogos" className="btn-explore-social">
                  <FaGamepad /> <span>Explorar Catálogo e Avaliações</span>
                </Link>
              </div>
            ) : (
              <div className="social-feed-timeline">
                {feedAmigos.map((item) => (
                  <article key={item.id} className="social-feed-card">
                    {/* Header da Atividade */}
                    <div className="social-card-header">
                      <Link to={`/perfil/${item.autorId}`} className="social-author-chip">
                        {item.autorFoto ? (
                          <img
                            src={item.autorFoto}
                            alt={item.autorNome}
                            className="social-author-avatar"
                            onError={(e) => {
                              e.currentTarget.onerror = null;
                              e.currentTarget.src =
                                "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=200&auto=format&fit=crop&q=80";
                            }}
                          />
                        ) : (
                          <div className="social-author-avatar fallback">
                            <span>{item.autorNome ? item.autorNome.charAt(0).toUpperCase() : "G"}</span>
                          </div>
                        )}
                        <div className="social-author-meta">
                          <span className="social-author-name">{item.autorNome}</span>
                          <span className="social-activity-action">
                            {item.tipoAtividade === "Avaliacao" && "publicou uma avaliação"}
                            {item.tipoAtividade === "JogoZerado" && "zerou um jogo"}
                            {item.tipoAtividade === "ListaCriada" && "criou uma nova coleção"}
                          </span>
                        </div>
                      </Link>

                      <span className="social-activity-date">
                        <FaCalendarAlt /> {formatarData(item.dataAtividade)}
                      </span>
                    </div>

                    {/* Conteúdo da Atividade: Avaliação */}
                    {item.tipoAtividade === "Avaliacao" && (
                      <div className="social-activity-body avaliacao">
                        <Link to={`/jogos/${item.jogoId}`} className="social-game-thumb-link">
                          <img
                            src={item.jogoImagem}
                            alt={item.jogoTitulo}
                            className="social-game-thumb"
                            onError={(e) => {
                              e.currentTarget.onerror = null;
                              e.currentTarget.src =
                                "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=300&auto=format&fit=crop&q=80";
                            }}
                          />
                        </Link>
                        <div className="social-activity-details">
                          <Link to={`/jogos/${item.jogoId}`} className="social-game-title">
                            {item.jogoTitulo}
                          </Link>
                          {item.nomeEmpresa && (
                            <span className="social-game-studio">{item.nomeEmpresa}</span>
                          )}

                          <div className="social-stars-row">
                            {[1, 2, 3, 4, 5].map((star) => (
                              <FaStar
                                key={star}
                                className={`star-icon ${star <= (item.nota || 0) ? "filled" : ""}`}
                              />
                            ))}
                            <span className="rating-badge">{item.nota}/5</span>
                          </div>

                          {item.textoAvaliacao && (
                            <Link to={`/avaliacoes/${item.avaliacaoId}`} className="social-review-text-link">
                              <p className="social-review-text">"{item.textoAvaliacao}"</p>
                            </Link>
                          )}

                          {/* Ações Sociais da Avaliação no Feed */}
                          <div className="social-review-actions-bar">
                            <button
                              type="button"
                              className={`btn-feed-like ${item.curtidaPorMim ? "liked" : ""}`}
                              onClick={() => handleToggleCurtirFeed(item.avaliacaoId)}
                              title={item.curtidaPorMim ? "Descurtir" : "Curtir"}
                            >
                              {item.curtidaPorMim ? <FaHeart className="like-icon" /> : <FaRegHeart className="like-icon" />}
                              <span>{item.totalCurtidas || 0}</span>
                            </button>

                            <Link to={`/avaliacoes/${item.avaliacaoId}`} className="btn-feed-comments" title="Ver respostas">
                              <FaCommentDots className="comment-icon" />
                              <span>{item.totalRespostas || 0} {item.totalRespostas === 1 ? "resposta" : "respostas"}</span>
                            </Link>

                            <Link to={`/avaliacoes/${item.avaliacaoId}`} className="btn-view-full-review">
                              <span>Ver Avaliação</span> <FaArrowRight />
                            </Link>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Conteúdo da Atividade: Jogo Zerado */}
                    {item.tipoAtividade === "JogoZerado" && (
                      <div className="social-activity-body zerado">
                        <Link to={`/jogos/${item.jogoId}`} className="social-game-thumb-link">
                          <img
                            src={item.jogoImagem}
                            alt={item.jogoTitulo}
                            className="social-game-thumb"
                            onError={(e) => {
                              e.currentTarget.onerror = null;
                              e.currentTarget.src =
                                "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=300&auto=format&fit=crop&q=80";
                            }}
                          />
                        </Link>
                        <div className="social-activity-details">
                          <div className="zerado-badge">
                            <FaCheckCircle /> <span>Jogo Zerado</span>
                          </div>
                          <Link to={`/jogos/${item.jogoId}`} className="social-game-title">
                            {item.jogoTitulo}
                          </Link>
                          {item.nomeEmpresa && (
                            <span className="social-game-studio">{item.nomeEmpresa}</span>
                          )}
                        </div>
                      </div>
                    )}

                    {/* Conteúdo da Atividade: Lista Criada */}
                    {item.tipoAtividade === "ListaCriada" && (
                      <div className="social-activity-body lista">
                        <div className="social-list-info">
                          <div className="social-list-header">
                            <span className="social-list-badge">
                              <FaLayerGroup /> Coleção ({item.totalJogosLista} títulos)
                            </span>
                          </div>
                          <Link to={`/listas/${item.listaId}`} className="social-list-title">
                            {item.listaTitulo}
                          </Link>
                          {item.listaDescricao && (
                            <p className="social-list-desc">{item.listaDescricao}</p>
                          )}
                        </div>

                        {item.capasPreviewLista && item.capasPreviewLista.length > 0 && (
                          <div className="social-list-mosaic">
                            {item.capasPreviewLista.slice(0, 4).map((capa, idx) => (
                              <img key={idx} src={capa} alt="Capa" className="social-mosaic-thumb" />
                            ))}
                          </div>
                        )}

                        <div className="social-list-action">
                          <Link to={`/listas/${item.listaId}`} className="btn-view-social-list">
                            <span>Ver Coleção</span> <FaArrowRight />
                          </Link>
                        </div>
                      </div>
                    )}
                  </article>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <FormAvaliacao
        isOpen={modalAberto}
        onClose={fecharModal}
        onSubmit={handleSalvarAvaliacao}
        loading={salvandoAvaliacao}
        jogos={jogos}
      />
    </div>
  );
};

export default TelaHome;

