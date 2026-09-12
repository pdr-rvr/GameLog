import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import ReviewCardV2 from "../../components/ReviewCardV2/ReviewCardV2";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { ComunidadeService } from "../../services/comunidadeService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { buscarJogos, criarAvaliacao } from "../TelaHome/actions/TelaHomeActions";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { 
  FaSearch, 
  FaFire, 
  FaLayerGroup, 
  FaComments, 
  FaStar, 
  FaPlus,
  FaFilter,
  FaGamepad,
  FaUsers
} from "react-icons/fa";
import "./PaginaComunidade.css";

const PaginaComunidade = () => {
  const { user } = useAuth();
  const toast = useToast();

  const [tendencias, setTendencias] = useState({
    jogosEmAlta: [],
    avaliacoesPopulares: [],
    colecoesDestaque: []
  });
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingAvaliacoes, setLoadingAvaliacoes] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [filtroNota, setFiltroNota] = useState(null); // null (todas) ou 1, 2, 3, 4, 5
  const [ordenacao, setOrdenacao] = useState("recentes"); // "recentes", "curtidas"
  const [modalAberto, setModalAberto] = useState(false);
  const [salvandoAvaliacao, setSalvandoAvaliacao] = useState(false);

  // Carrega tendências e lista inicial
  useEffect(() => {
    const carregarDadosIniciais = async () => {
      setLoading(true);
      try {
        const [dadosTendencias, dadosJogos] = await Promise.all([
          Promise.resolve(ComunidadeService.obterTendencias()).catch(() => ({
            jogosEmAlta: [],
            avaliacoesPopulares: [],
            colecoesDestaque: []
          })),
          Promise.resolve(buscarJogos()).catch(() => [])
        ]);

        setTendencias(dadosTendencias || {});
        setJogos(dadosJogos || []);
      } catch (error) {
        console.error("Erro ao carregar dados da comunidade:", error);
      } finally {
        setLoading(false);
      }
    };

    carregarDadosIniciais();
  }, []);

  // Carrega avaliações com base no filtro de nota selecionado
  useEffect(() => {
    const carregarAvaliacoesFiltradas = async () => {
      setLoadingAvaliacoes(true);
      try {
        const params = {};
        if (filtroNota !== null) {
          params.nota = filtroNota;
        }
        const data = await AvaliacaoService.listarAvaliacoes(params);
        setAvaliacoes(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error("Erro ao carregar avaliações:", error);
      } finally {
        setLoadingAvaliacoes(false);
      }
    };

    carregarAvaliacoesFiltradas();
  }, [filtroNota]);

  const handleToggleCurtir = async (avaliacaoId) => {
    if (!user) {
      toast.info("Faça login para curtir avaliações da comunidade!");
      return;
    }

    try {
      await AvaliacaoService.toggleCurtir(avaliacaoId);
      setAvaliacoes((prev) =>
        prev.map((av) => {
          const avId = av.avaliacaoId || av.id;
          if (avId === avaliacaoId) {
            const jaCurtido = av.curtidaPorMim;
            return {
              ...av,
              curtidaPorMim: !jaCurtido,
              totalCurtidas: jaCurtido
                ? Math.max(0, (av.totalCurtidas || 1) - 1)
                : (av.totalCurtidas || 0) + 1
            };
          }
          return av;
        })
      );
    } catch (error) {
      console.error("Erro ao curtir avaliação:", error);
      toast.error("Não foi possível registrar a curtida.");
    }
  };

  const handleSalvarAvaliacao = async (avaliacaoData) => {
    setSalvandoAvaliacao(true);
    try {
      await criarAvaliacao(avaliacaoData);
      setModalAberto(false);
      toast.success("Avaliação publicada na comunidade com sucesso!");
      
      // Recarrega avaliações
      const params = filtroNota !== null ? { nota: filtroNota } : {};
      const novasAvaliacoes = await AvaliacaoService.listarAvaliacoes(params);
      setAvaliacoes(novasAvaliacoes || []);
    } catch (error) {
      console.error("Erro ao publicar avaliação:", error);
      toast.error(error.message || "Erro ao publicar avaliação.");
    } finally {
      setSalvandoAvaliacao(false);
    }
  };

  // Filtragem local por termo de busca (nome do jogo ou usuário)
  const avaliacoesFiltradas = avaliacoes.filter((avaliacao) => {
    if (!searchTerm.trim()) return true;
    const termo = searchTerm.toLowerCase().trim();
    const nomeJogo = (avaliacao.nomeJogo || "").toLowerCase();
    const nomeUsuario = (avaliacao.nomeUsuario || "").toLowerCase();
    const comentario = (avaliacao.comentario || "").toLowerCase();
    return nomeJogo.includes(termo) || nomeUsuario.includes(termo) || comentario.includes(termo);
  });

  // Ordenação
  const avaliacoesExibidas = [...avaliacoesFiltradas].sort((a, b) => {
    if (ordenacao === "curtidas") {
      return (b.totalCurtidas || 0) - (a.totalCurtidas || 0);
    }
    // Default: recentes
    return new Date(b.dataCriacao || 0) - new Date(a.dataCriacao || 0);
  });

  const notasDisponiveis = [
    { label: "Todas", valor: null },
    { label: "5 Estrelas", valor: 5 },
    { label: "4 Estrelas", valor: 4 },
    { label: "3 Estrelas", valor: 3 },
    { label: "2 Estrelas", valor: 2 },
    { label: "1 Estrela", valor: 1 }
  ];

  return (
    <div className="pagina-comunidade-container">
      <Navbar onPublicarClick={() => setModalAberto(true)} />

      <main className="comunidade-main-content">
        {/* Header da Comunidade */}
        <header className="comunidade-hero-header">
          <h1>Tendências & Resenhas dos Jogadores</h1>
          <p>
            Explore o que milhares de apaixonados por games estão jogando, analisando e colecionando.
          </p>

          <div className="comunidade-search-wrapper">
            <FaSearch className="comunidade-search-icon" />
            <input
              type="text"
              placeholder="Buscar por jogo, jogador ou palavra-chave..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="comunidade-search-input"
            />
            {searchTerm && (
              <button 
                type="button" 
                className="comunidade-btn-clear"
                onClick={() => setSearchTerm("")}
              >
                ×
              </button>
            )}
          </div>
        </header>

        {loading ? (
          <div className="comunidade-loading-state">
            <div className="comunidade-spinner"></div>
            <span>Carregando pulso da comunidade...</span>
          </div>
        ) : (
          <>
            {/* Seção 1: Em Alta na Comunidade */}
            {(tendencias.jogosMaisDiscutidos || tendencias.jogosEmAlta)?.length > 0 && (
              <section className="comunidade-section-block">
                <div className="comunidade-section-title">
                  <FaFire className="section-title-icon fire-glow" />
                  <h2>Jogos Mais Falados no Momento</h2>
                </div>
                <div className="comunidade-trending-grid">
                  {(tendencias.jogosMaisDiscutidos || tendencias.jogosEmAlta).slice(0, 5).map((jogo) => {
                    const jId = jogo.jogoId || jogo.id;
                    const titulo = jogo.titulo || jogo.nome || "Jogo";
                    const capa = jogo.imagem || jogo.imagemCapa || "https://placehold.co/300x400/1e293b/ffffff?text=Game";
                    const gen = (Array.isArray(jogo.generos) && jogo.generos[0]) || jogo.genero || "";
                    const media = Number(jogo.mediaAvaliacoes || jogo.notaMedia) || 0;

                    return (
                      <Link 
                        key={jId} 
                        to={`/jogos/${jId}`}
                        className="trending-game-card"
                      >
                        <img 
                          src={capa} 
                          alt={titulo}
                          className="trending-game-img"
                        />
                        <div className="trending-game-info">
                          <h4>{titulo}</h4>
                          <div className="trending-game-meta">
                            {gen && <span className="trending-tag">{gen}</span>}
                            {media > 0 && (
                              <span className="trending-rating">
                                <FaStar /> {media.toFixed(1)}
                              </span>
                            )}
                          </div>
                        </div>
                      </Link>
                    );
                  })}
                </div>
              </section>
            )}

            {/* Seção 2: Coleções Populares em Destaque */}
            {(tendencias.listasEmDestaque || tendencias.colecoesDestaque)?.length > 0 && (
              <section className="comunidade-section-block">
                <div className="comunidade-section-title">
                  <FaLayerGroup className="section-title-icon collection-glow" />
                  <h2>Coleções Criadas pela Comunidade</h2>
                </div>
                <div className="comunidade-collections-grid">
                  {(tendencias.listasEmDestaque || tendencias.colecoesDestaque).map((col) => {
                    const lId = col.listaId || col.id;
                    const lTitulo = col.titulo || col.nome || "Coleção";
                    const lCriador = col.nomeUsuario || col.nomeCriador || "Jogador";
                    const lTotal = col.totalJogos || 0;
                    const lDesc = col.descricao || "";

                    return (
                      <Link 
                        key={lId} 
                        to={`/listas/${lId}`}
                        className="community-collection-card"
                      >
                        <div className="collection-card-header">
                          <span className="collection-creator">Por @{lCriador}</span>
                          <span className="collection-games-count">{lTotal} {lTotal === 1 ? "jogo" : "jogos"}</span>
                        </div>
                        <h4 className="collection-card-title">{lTitulo}</h4>
                        {lDesc && <p className="collection-card-desc">{lDesc}</p>}
                      </Link>
                    );
                  })}
                </div>
              </section>
            )}

            {/* Seção 3: Feed Principal de Resenhas & Discussões */}
            <section className="comunidade-section-block">
              <div className="comunidade-reviews-header">
                <div className="comunidade-section-title">
                  <FaComments className="section-title-icon comments-glow" />
                  <h2>Resenhas & Críticas da Comunidade</h2>
                </div>

                <div className="comunidade-controls-bar">
                  {/* Filtro Exato por Nota */}
                  <div className="star-filters-pills">
                    {notasDisponiveis.map((filtro) => (
                      <button
                        key={filtro.label}
                        type="button"
                        className={`star-pill-btn ${filtroNota === filtro.valor ? "active" : ""}`}
                        onClick={() => setFiltroNota(filtro.valor)}
                      >
                        {filtro.valor !== null && <FaStar className="star-pill-icon" />}
                        {filtro.label}
                      </button>
                    ))}
                  </div>

                  {/* Ordenação */}
                  <div className="comunidade-sort-select-wrapper">
                    <select
                      value={ordenacao}
                      onChange={(e) => setOrdenacao(e.target.value)}
                      className="comunidade-sort-select"
                    >
                      <option value="recentes">Mais Recentes</option>
                      <option value="curtidas">Mais Curtidas</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Lista de Avaliações */}
              {loadingAvaliacoes ? (
                <div className="comunidade-loading-mini">
                  <div className="comunidade-spinner"></div>
                  <span>Atualizando resenhas...</span>
                </div>
              ) : avaliacoesExibidas.length > 0 ? (
                <div className="comunidade-reviews-grid">
                  {avaliacoesExibidas.map((avaliacao) => (
                    <ReviewCardV2 
                      key={avaliacao.avaliacaoId || avaliacao.id} 
                      avaliacao={avaliacao} 
                      onToggleCurtir={handleToggleCurtir}
                    />
                  ))}
                </div>
              ) : (
                <div className="comunidade-empty-state">
                  <FaComments className="empty-state-icon" />
                  <h3>Nenhuma resenha encontrada</h3>
                  <p>
                    {filtroNota !== null
                      ? `Nenhuma avaliação com exatamente ${filtroNota} ${filtroNota === 1 ? "estrela" : "estrelas"} foi encontrada.`
                      : searchTerm
                      ? `Nenhuma avaliação corresponde ao termo "${searchTerm}".`
                      : "Ainda não há avaliações publicadas."}
                  </p>
                </div>
              )}
            </section>
          </>
        )}
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

export default PaginaComunidade;
