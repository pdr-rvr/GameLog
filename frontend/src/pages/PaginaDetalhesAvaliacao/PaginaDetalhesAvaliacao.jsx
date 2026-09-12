import React, { useState, useEffect, useCallback } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { 
  FaStar, 
  FaGamepad, 
  FaHeart, 
  FaRegHeart, 
  FaComments, 
  FaPaperPlane, 
  FaTrash, 
  FaEdit, 
  FaArrowLeft, 
  FaSpinner, 
  FaBuilding, 
  FaCalendarAlt, 
  FaUserShield
} from "react-icons/fa";
import { AuthService } from "../../services/authService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { useToast } from "../../context/ToastContext";
import "./PaginaDetalhesAvaliacao.css";

const PaginaDetalhesAvaliacao = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { warning, success, error } = useToast();

  const currentUser = AuthService.getCurrentUser();
  const isAuth = AuthService.isAuthenticated();

  const [avaliacao, setAvaliacao] = useState(null);
  const [respostas, setRespostas] = useState([]);
  const [loading, setLoading] = useState(true);

  // Review Like State
  const [curtido, setCurtido] = useState(false);
  const [totalCurtidas, setTotalCurtidas] = useState(0);
  const [isLiking, setIsLiking] = useState(false);

  // New Comment Form State
  const [novoComentario, setNovoComentario] = useState("");
  const [enviandoComentario, setEnviandoComentario] = useState(false);
  const [deletandoRespostaId, setDeletandoRespostaId] = useState(null);

  const carregarDados = useCallback(async () => {
    setLoading(true);
    try {
      const [dadosAvaliacao, dadosRespostas] = await Promise.all([
        AvaliacaoService.obterAvaliacaoPorId(id),
        AvaliacaoService.listarRespostas(id)
      ]);

      if (!dadosAvaliacao) {
        setAvaliacao(null);
        return;
      }

      setAvaliacao(dadosAvaliacao);
      setCurtido(Boolean(dadosAvaliacao.curtidaPorMim));
      setTotalCurtidas(Number(dadosAvaliacao.totalCurtidas) || 0);
      setRespostas(dadosRespostas || []);
    } catch (err) {
      console.error("Erro ao carregar detalhes da avaliação:", err);
      error("Não foi possível carregar a avaliação.");
    } finally {
      setLoading(false);
    }
  }, [id, error]);

  useEffect(() => {
    if (id) {
      carregarDados();
    }
  }, [id, carregarDados]);

  const isOwnReview = currentUser && avaliacao && currentUser.usuarioId === avaliacao.usuarioId;

  const handleToggleCurtirReview = async () => {
    if (!isAuth) {
      navigate("/login");
      return;
    }

    if (isOwnReview) {
      warning("Você não pode curtir sua própria avaliação.");
      return;
    }

    if (isLiking || !avaliacao) return;

    setIsLiking(true);
    const anteriorCurtido = curtido;
    const anteriorTotal = totalCurtidas;

    const novoCurtido = !anteriorCurtido;
    setCurtido(novoCurtido);
    setTotalCurtidas(novoCurtido ? anteriorTotal + 1 : Math.max(0, anteriorTotal - 1));

    try {
      const res = await AvaliacaoService.toggleCurtir(avaliacao.avaliacaoId);
      if (res && typeof res.curtido === "boolean") {
        setCurtido(res.curtido);
        setTotalCurtidas(res.totalCurtidas);
      }
    } catch (err) {
      console.error("Erro ao alternar curtida da avaliação:", err);
      setCurtido(anteriorCurtido);
      setTotalCurtidas(anteriorTotal);
      error(err.response?.data?.message || "Erro ao curtir avaliação.");
    } finally {
      setIsLiking(false);
    }
  };

  const handleToggleCurtirResposta = async (resposta) => {
    if (!isAuth) {
      navigate("/login");
      return;
    }

    const isOwnComment = currentUser && currentUser.usuarioId === resposta.usuarioId;
    if (isOwnComment) {
      warning("Você não pode curtir seu próprio comentário.");
      return;
    }

    // Optimistic UI update for the comment
    setRespostas((prev) =>
      prev.map((r) => {
        if (r.respostaId === resposta.respostaId) {
          const novoCurtido = !r.curtidaPorMim;
          return {
            ...r,
            curtidaPorMim: novoCurtido,
            totalCurtidas: novoCurtido ? (r.totalCurtidas || 0) + 1 : Math.max(0, (r.totalCurtidas || 0) - 1)
          };
        }
        return r;
      })
    );

    try {
      const res = await AvaliacaoService.toggleCurtirResposta(resposta.respostaId);
      if (res && typeof res.curtido === "boolean") {
        setRespostas((prev) =>
          prev.map((r) =>
            r.respostaId === resposta.respostaId
              ? { ...r, curtidaPorMim: res.curtido, totalCurtidas: res.totalCurtidas }
              : r
          )
        );
      }
    } catch (err) {
      console.error("Erro ao curtir resposta:", err);
      error(err.response?.data?.message || "Erro ao curtir resposta.");
      // Rollback
      carregarDados();
    }
  };

  const handleEnviarComentario = async (e) => {
    e.preventDefault();
    if (!isAuth) {
      navigate("/login");
      return;
    }

    if (!novoComentario.trim() || enviandoComentario || !avaliacao) return;

    setEnviandoComentario(true);
    try {
      const nova = await AvaliacaoService.adicionarResposta(avaliacao.avaliacaoId, novoComentario.trim());
      setRespostas((prev) => [...prev, nova]);
      setNovoComentario("");
      success("Comentário publicado com sucesso!");
    } catch (err) {
      console.error("Erro ao enviar comentário:", err);
      error(err.response?.data?.message || "Não foi possível enviar o comentário.");
    } finally {
      setEnviandoComentario(false);
    }
  };

  const handleDeletarResposta = async (respostaId) => {
    if (!respostaId) return;
    setDeletandoRespostaId(respostaId);
    try {
      await AvaliacaoService.deletarResposta(respostaId);
      setRespostas((prev) => prev.filter((r) => r.respostaId !== respostaId));
      success("Comentário excluído.");
    } catch (err) {
      console.error("Erro ao excluir resposta:", err);
      error("Não foi possível excluir o comentário.");
    } finally {
      setDeletandoRespostaId(null);
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

  const formatarDataHora = (dataStr) => {
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
      return "";
    }
  };

  if (loading) {
    return (
      <div className="pagina-detalhes-avaliacao-root">
        <Navbar />
        <div className="detalhes-avaliacao-loading">
          <FaSpinner className="spinner-icon spin" />
          <span>Carregando discussão da avaliação...</span>
        </div>
      </div>
    );
  }

  if (!avaliacao) {
    return (
      <div className="pagina-detalhes-avaliacao-root">
        <Navbar />
        <div className="detalhes-avaliacao-empty">
          <FaComments className="empty-icon" />
          <h2>Avaliação não encontrada</h2>
          <p>Esta avaliação pode ter sido removida ou não está disponível.</p>
          <button type="button" className="btn-voltar-link" onClick={() => navigate("/comunidade")}>
            <FaArrowLeft /> Voltar para Comunidade
          </button>
        </div>
      </div>
    );
  }

  const userInitial = (avaliacao.nomeUsuario || "G").charAt(0).toUpperCase();
  const nota = Math.min(5, Math.max(1, Number(avaliacao.nota) || 5));

  return (
    <div className="pagina-detalhes-avaliacao-root">
      <Navbar />

      <main className="detalhes-avaliacao-container">
        {/* Navigation Breadcrumb */}
        <div className="detalhes-avaliacao-nav">
          <button type="button" className="btn-voltar" onClick={() => navigate(-1)}>
            <FaArrowLeft /> Voltar
          </button>
          <span className="nav-separator">/</span>
          <Link to="/comunidade" className="nav-link">Comunidade</Link>
          <span className="nav-separator">/</span>
          <span className="nav-current">{avaliacao.nomeJogo}</span>
        </div>

        <div className="detalhes-avaliacao-grid">
          {/* Left Column: Game Context Card */}
          <aside className="detalhes-game-aside">
            <div className="game-context-card">
              <div className="game-context-cover-wrapper">
                {avaliacao.imagemJogo ? (
                  <img src={avaliacao.imagemJogo} alt={avaliacao.nomeJogo} className="game-context-cover" />
                ) : (
                  <div className="game-context-placeholder">
                    <FaGamepad />
                  </div>
                )}
              </div>

              <div className="game-context-info">
                <Link to={`/jogos/${avaliacao.jogoId}`} className="game-context-title">
                  {avaliacao.nomeJogo}
                </Link>

                {avaliacao.nomeEmpresa && (
                  <div className="game-context-meta-item">
                    <FaBuilding className="meta-icon" />
                    {avaliacao.empresaId ? (
                      <Link to={`/empresas/${avaliacao.empresaId}`} className="game-context-studio">
                        {avaliacao.nomeEmpresa}
                      </Link>
                    ) : (
                      <span>{avaliacao.nomeEmpresa}</span>
                    )}
                  </div>
                )}

                {avaliacao.dataLancamentoJogo && (
                  <div className="game-context-meta-item">
                    <FaCalendarAlt className="meta-icon" />
                    <span>{String(avaliacao.dataLancamentoJogo).substring(0, 4)}</span>
                  </div>
                )}

                <Link to={`/jogos/${avaliacao.jogoId}`} className="btn-ver-jogo">
                  <FaGamepad /> Ver Página do Jogo
                </Link>
              </div>
            </div>
          </aside>

          {/* Right Column: Main Review & Discussion Feed */}
          <section className="detalhes-main-section">
            {/* Main Review Card */}
            <article className="detalhes-review-card">
              <div className="review-card-header">
                <div className="review-author-box">
                  <div className="review-author-avatar">
                    {avaliacao.fotoPerfilUsuario ? (
                      <img src={avaliacao.fotoPerfilUsuario} alt={avaliacao.nomeUsuario} />
                    ) : (
                      <span>{userInitial}</span>
                    )}
                  </div>
                  <div className="review-author-meta">
                    <div className="author-name-row">
                      <Link to={`/perfil/${avaliacao.usuarioId}`} className="review-author-name">
                        {avaliacao.nomeUsuario}
                      </Link>
                      {isOwnReview && (
                        <span className="badge-own-review" title="Esta é a sua avaliação">
                          <FaUserShield /> Sua Review
                        </span>
                      )}
                    </div>
                    <span className="review-date-full">
                      Publicado em {formatarData(avaliacao.dataPublicacao)}
                    </span>
                  </div>
                </div>

                <div className="review-stars-box" aria-label={`Nota ${nota} de 5`}>
                  {[1, 2, 3, 4, 5].map((s) => (
                    <FaStar key={s} className={`review-star-lg ${s <= nota ? "filled" : "empty"}`} />
                  ))}
                  <span className="review-rating-number">{nota}/5</span>
                </div>
              </div>

              {/* Review Text Body */}
              <div className="review-content-body">
                <p className="review-full-text">
                  {avaliacao.textoAvaliacao || "Sem comentários detalhados."}
                </p>
              </div>

              {/* Review Social & Action Toolbar */}
              <div className="review-toolbar">
                <div className="toolbar-stats-group">
                  <button
                    type="button"
                    className={`toolbar-btn like-btn ${curtido ? "liked" : ""} ${isOwnReview ? "disabled-self" : ""}`}
                    onClick={handleToggleCurtirReview}
                    disabled={isLiking}
                    title={isOwnReview ? "Você não pode curtir sua própria avaliação" : curtido ? "Descurtir" : "Curtir"}
                  >
                    {curtido ? <FaHeart className="heart-icon active" /> : <FaRegHeart className="heart-icon" />}
                    <span className="btn-count">{totalCurtidas}</span>
                    <span className="btn-label">{totalCurtidas === 1 ? "Curtida" : "Curtidas"}</span>
                  </button>

                  <div className="toolbar-badge comments-badge">
                    <FaComments className="badge-icon" />
                    <span className="btn-count">{respostas.length}</span>
                    <span className="btn-label">{respostas.length === 1 ? "Comentário" : "Comentários"}</span>
                  </div>
                </div>

                {isOwnReview && (
                  <div className="toolbar-actions-owner">
                    <button
                      type="button"
                      className="btn-owner edit"
                      onClick={() => navigate(`/avaliacoes/editar/${avaliacao.avaliacaoId}`, { state: { from: `/avaliacoes/${avaliacao.avaliacaoId}` } })}
                      title="Editar Avaliação"
                    >
                      <FaEdit /> Editar
                    </button>
                  </div>
                )}
              </div>
            </article>

            {/* Community Discussion & Comments Section */}
            <div className="discussion-section">
              <div className="discussion-header">
                <h3>
                  <FaComments className="discussion-icon" /> Discussão da Comunidade ({respostas.length})
                </h3>
              </div>

              {/* Comment Input Box */}
              {isAuth ? (
                <form className="discussion-form" onSubmit={handleEnviarComentario}>
                  <div className="discussion-form-avatar">
                    {currentUser?.fotoDePerfil ? (
                      <img src={currentUser.fotoDePerfil} alt={currentUser.nomeUsuario} />
                    ) : (
                      <span>{(currentUser?.nomeUsuario || "U").charAt(0).toUpperCase()}</span>
                    )}
                  </div>
                  <div className="discussion-form-input-box">
                    {isOwnReview && (
                      <div className="author-reply-hint">
                        <FaUserShield /> Respondendo como autor desta avaliação
                      </div>
                    )}
                    <textarea
                      placeholder={
                        isOwnReview
                          ? "Participe da conversa e responda aos comentários da comunidade..."
                          : "Deixe seu comentário sobre esta avaliação..."
                      }
                      value={novoComentario}
                      onChange={(e) => setNovoComentario(e.target.value)}
                      maxLength={500}
                      rows={3}
                      disabled={enviandoComentario}
                    />
                    <div className="discussion-form-footer">
                      <span className="char-count">{novoComentario.length}/500</span>
                      <button
                        type="submit"
                        className="btn-send-comment"
                        disabled={!novoComentario.trim() || enviandoComentario}
                      >
                        {enviandoComentario ? (
                          <>
                            <FaSpinner className="spinner-icon spin" /> Enviando...
                          </>
                        ) : (
                          <>
                            <FaPaperPlane /> {isOwnReview ? "Responder" : "Comentar"}
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                </form>
              ) : (
                <div className="discussion-auth-prompt">
                  <p>
                    <Link to="/login" className="prompt-link">Faça login</Link> para participar da discussão e comentar nesta avaliação.
                  </p>
                </div>
              )}

              {/* Comments List */}
              <div className="discussion-list">
                {respostas.length > 0 ? (
                  respostas.map((resp) => {
                    const isAuthorOfReview = resp.usuarioId === avaliacao.usuarioId;
                    const isMyComment = currentUser && currentUser.usuarioId === resp.usuarioId;
                    const respInitial = (resp.nomeUsuario || "G").charAt(0).toUpperCase();

                    return (
                      <div
                        key={resp.respostaId}
                        className={`comment-card ${isAuthorOfReview ? "highlight-author-review" : ""}`}
                      >
                        <div className="comment-card-avatar">
                          {resp.fotoPerfilUsuario ? (
                            <img src={resp.fotoPerfilUsuario} alt={resp.nomeUsuario} />
                          ) : (
                            <span>{respInitial}</span>
                          )}
                        </div>

                        <div className="comment-card-content">
                          <div className="comment-header-row">
                            <div className="comment-author-info">
                              <Link to={`/perfil/${resp.usuarioId}`} className="comment-author-name">
                                {resp.nomeUsuario}
                              </Link>
                              {isAuthorOfReview && (
                                <span className="badge-author-tag" title="Autor da avaliação original">
                                  <FaUserShield /> Autor da Review
                                </span>
                              )}
                              <span className="comment-timestamp">
                                {formatarDataHora(resp.dataCriacao)}
                              </span>
                            </div>

                            <div className="comment-actions-row">
                              {/* Like button for comment */}
                              <button
                                type="button"
                                className={`btn-comment-like ${resp.curtidaPorMim ? "liked" : ""} ${isMyComment ? "disabled-self" : ""}`}
                                onClick={() => handleToggleCurtirResposta(resp)}
                                title={isMyComment ? "Você não pode curtir seu próprio comentário" : resp.curtidaPorMim ? "Descurtir" : "Curtir"}
                              >
                                {resp.curtidaPorMim ? (
                                  <FaHeart className="heart-icon active" />
                                ) : (
                                  <FaRegHeart className="heart-icon" />
                                )}
                                <span className="comment-like-count">{resp.totalCurtidas || 0}</span>
                              </button>

                              {/* Delete button if owner of comment or review author */}
                              {(isMyComment || isOwnReview) && (
                                <button
                                  type="button"
                                  className="btn-comment-delete"
                                  onClick={() => handleDeletarResposta(resp.respostaId)}
                                  disabled={deletandoRespostaId === resp.respostaId}
                                  title="Excluir comentário"
                                >
                                  <FaTrash />
                                </button>
                              )}
                            </div>
                          </div>

                          <p className="comment-body-text">{resp.comentario}</p>
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <div className="discussion-empty-box">
                    <FaComments className="empty-chat-icon" />
                    <h4>Ainda não há comentários nesta avaliação.</h4>
                    <p>Seja o primeiro a compartilhar sua opinião sobre este review!</p>
                  </div>
                )}
              </div>
            </div>
          </section>
        </div>
      </main>
    </div>
  );
};

export default PaginaDetalhesAvaliacao;
