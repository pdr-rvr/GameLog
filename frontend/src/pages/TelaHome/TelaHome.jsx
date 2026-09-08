import React, { useState, useEffect } from "react";
import { useLocation } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import JogoCarrossel from "../../components/JogoCarrossel/JogoCarrossel";
import AvaliacaoCarrossel from "../../components/AvaliacaoCarrossel/AvaliacaoCarrossel";
import FormAvaliacao from "../../components/FormAvaliacao/FormAvaliacao";
import { useAuth } from "../../context/AuthContext";
import { buscarAvaliacoes, buscarJogos, criarAvaliacao, buscarRecomendacoes } from "./actions/TelaHomeActions";
import "./TelaHome.css";

const TelaHome = () => {
  const { user } = useAuth();
  const location = useLocation();
  const [avaliacoes, setAvaliacoes] = useState([]);
  const [jogos, setJogos] = useState([]);
  const [recomendacoes, setRecomendacoes] = useState([]);
  const [modalAberto, setModalAberto] = useState(false);
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

  const handleSalvarAvaliacao = async (avaliacaoData) => {
    try {
      await criarAvaliacao(avaliacaoData);
      setModalAberto(false);
      const novasAvaliacoes = await buscarAvaliacoes();
      setAvaliacoes(novasAvaliacoes);
    } catch (error) {
      console.error("Erro ao salvar avaliação:", error);
      alert("Erro ao publicar avaliação. Tente novamente.");
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
              <JogoCarrossel
                title="Recomendados Para Você"
                jogos={recomendacoes}
              />
            )}

            <JogoCarrossel
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
        onClose={() => setModalAberto(false)}
        onSubmit={handleSalvarAvaliacao}
        jogos={jogos}
      />
    </div>
  );
};

export default TelaHome;
