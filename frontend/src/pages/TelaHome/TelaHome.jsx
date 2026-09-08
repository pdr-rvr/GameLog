import React, { useState, useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogosCarrossel from "../../components/JogosCarrossel/JogosCarrossel";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { buscarAvaliacoes, buscarJogos, criarAvaliacao, buscarRecomendacoes } from "./actions/TelaHomeActions";
import "./TelaHome.css";

const TelaHome = () => {
  const { user } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const location = useLocation();
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [recomendacoes, setRecomendacoes] = useState([]);
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
      toast.success("Avaliação publicada com sucesso no feed!");
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

      <div className="home-content">
        {loading ? (
          <div className="loading-message">Carregando catálogo e avaliações...</div>
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
              jogos={jogos.slice(0, 10)}
            />

            <AvaliacaoCarrossel
              title="Últimas Avaliações da Comunidade"
              avaliacoes={avaliacoes}
            />
          </>
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
