import React, { useState, useEffect } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import HeroBanner from "../../components/HeroBanner/HeroBanner";
import CategoryPills from "../../components/CategoryPills/CategoryPills";
import JogosCarrossel from "../../components/JogosCarrossel/JogosCarrossel";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { 
  buscarAvaliacoes, 
  buscarJogos, 
  criarAvaliacao, 
  buscarRecomendacoes,
  buscarDestaques,
  buscarTopAvaliados
} from "./actions/TelaHomeActions";
import { 
  FaGamepad, 
  FaFire, 
  FaTrophy, 
  FaComments,
  FaArrowRight,
  FaCompass
} from "react-icons/fa";
import "./TelaHome.css";

const TelaHome = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const location = useLocation();

  const [destaques, setDestaques] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [topAvaliados, setTopAvaliados] = useState([]);
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [recomendacoes, setRecomendacoes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalAberto, setModalAberto] = useState(false);
  const [salvandoAvaliacao, setSalvandoAvaliacao] = useState(false);

  useEffect(() => {
    const carregarDadosHome = async () => {
      setLoading(true);
      try {
        const [dadosDestaques, dadosJogos, dadosTopAvaliados, dadosAvaliacoes] = await Promise.all([
          buscarDestaques(5),
          buscarJogos(),
          buscarTopAvaliados(),
          buscarAvaliacoes()
        ]);

        setDestaques(dadosDestaques || []);
        setJogos(dadosJogos || []);
        setTopAvaliados(dadosTopAvaliados || []);
        setAvaliacoes(dadosAvaliacoes || []);

        if (user && user.id) {
          const dadosRecomendacoes = await buscarRecomendacoes(user.id);
          setRecomendacoes(dadosRecomendacoes || []);
        }
      } catch (error) {
        console.error("Erro ao carregar dados da Home:", error);
      } finally {
        setLoading(false);
      }
    };

    carregarDadosHome();
  }, [user]);

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
    } catch (error) {
      console.error("Erro ao salvar avaliação:", error);
      toast.error(error.message || "Erro ao publicar avaliação. Tente novamente.");
    } finally {
      setSalvandoAvaliacao(false);
    }
  };

  return (
    <div className="home-container">
      <Navbar onPublicarClick={() => setModalAberto(true)} />

      <main className="home-content">
        {loading ? (
          <div className="home-loading-state">
            <div className="home-spinner"></div>
            <span>Carregando universo GameLog...</span>
          </div>
        ) : (
          <>
            {/* 1. Hero Banner Dinâmico com Destaques Canônicos */}
            <HeroBanner 
              jogos={destaques.length > 0 ? destaques : jogos.slice(0, 5)} 
            />

            {/* 2. Categorias e Gêneros Populares */}
            <CategoryPills />

            {/* 3. Seção Recomendados Para Você (quando logado) */}
            {user && recomendacoes.length > 0 && (
              <section className="home-section-block">
                <JogosCarrossel
                  title="Recomendados Para Você"
                  jogos={recomendacoes}
                />
              </section>
            )}

            {/* 4. Em Alta na Semana */}
            <section className="home-section-block">
              <JogosCarrossel
                title="Em Alta na Semana"
                jogos={jogos.slice(0, 15)}
              />
            </section>

            {/* 5. Hall da Fama */}
            <section className="home-section-block">
              <JogosCarrossel
                title="Hall da Fama"
                jogos={topAvaliados.length > 0 ? topAvaliados : jogos.slice(15, 30)}
              />
            </section>

            {/* 6. Banner de Exploração e Chamada para Ação */}
            <section className="home-discovery-banner">
              <div className="discovery-banner-content">
                <div className="discovery-icon-wrapper">
                  <FaCompass className="discovery-icon" />
                </div>
                <div className="discovery-text">
                  <h3>Descubra Mais de 2.500 Jogos Autênticos</h3>
                  <p>Filtre por estúdios lendários, anos de lançamento e notas consolidadas da comunidade.</p>
                </div>
                <Link to="/jogos" className="btn-discovery-cta">
                  <span>Explorar Catálogo Completo</span> <FaArrowRight />
                </Link>
              </div>
            </section>

            {/* 7. Destaques das Últimas Avaliações da Comunidade */}
            <section className="home-section-block">
              <div className="home-section-header-row">
                <h2 className="home-section-custom-title">
                  <FaComments className="title-icon" />
                  <span>Últimas Avaliações da Comunidade</span>
                </h2>
                <Link to="/comunidade" className="home-view-all-link">
                  <span>Ver todas na Comunidade</span> <FaArrowRight />
                </Link>
              </div>
              <AvaliacaoCarrossel
                title=""
                avaliacoes={avaliacoes}
              />
            </section>
          </>
        )}
      </main>

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
