import React, { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import PodioFavoritos from "../../components/PodioFavoritos/PodioFavoritos";
import ModalEditarFavoritos from "../../components/ModalEditarFavoritos/ModalEditarFavoritos";
import ModalCriarLista from "../../components/ModalCriarLista/ModalCriarLista";
import ModalConexoes from "../../components/ModalConexoes/ModalConexoes";
import ConfirmModal from "../../components/ConfirmModal/ConfirmModal";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { UsuarioService } from "../../services/usuarioService";
import { AvaliacaoService } from "../../services/avaliacaoService";
import { BibliotecaService } from "../../services/bibliotecaService";
import { ListaService } from "../../services/listaService";
import { SocialService } from "../../services/socialService";

import PerfilHeader from "./components/PerfilHeader";
import PerfilStatsBar from "./components/PerfilStatsBar";
import PerfilBibliotecaTab from "./components/PerfilBibliotecaTab";
import PerfilColecoesTab from "./components/PerfilColecoesTab";
import PerfilAvaliacoesTab from "./components/PerfilAvaliacoesTab";

import { FaBookmark, FaLayerGroup, FaComments } from "react-icons/fa";
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

  // Social Stats & Follow State
  const [statsSociais, setStatsSociais] = useState({
    totalSeguidores: 0,
    totalSeguindo: 0,
    seguidoPorMim: false
  });
  const [modalConexoesAberto, setModalConexoesAberto] = useState(false);
  const [modalConexoesTab, setModalConexoesTab] = useState("seguidores");
  const [processandoSeguir, setProcessandoSeguir] = useState(false);

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
  const targetId = userId || user?.id;
  const isOwner = Boolean(user && targetId && String(user.id).toLowerCase() === String(targetId).toLowerCase());

  const carregarDadosPerfil = useCallback(async () => {
    if (!targetId) {
      setError("Identificador de usuário não encontrado.");
      setLoading(false);
      return;
    }

    setLoading(true);
    setError("");
    try {
      const [dadosUsuario, dadosAvaliacoes, dadosGeneros, dadosFavoritos, dadosStats, dadosListas, dadosSociais] = await Promise.all([
        UsuarioService.obterPerfil(targetId),
        AvaliacaoService.listarPorUsuario(targetId).catch(() => []),
        UsuarioService.obterGenerosFavoritos(targetId).catch(() => []),
        BibliotecaService.obterFavoritos(targetId).catch(() => []),
        BibliotecaService.obterEstatisticas(targetId).catch(() => ({
          totalJogos: 0,
          totalQueroJogar: 0,
          totalJogando: 0,
          totalZerados: 0,
          totalPausados: 0,
          totalAbandonados: 0,
        })),
        ListaService.listarListasDoUsuario(targetId).catch(() => []),
        SocialService.obterEstatisticasSociais(targetId).catch(() => ({
          totalSeguidores: 0,
          totalSeguindo: 0,
          seguidoPorMim: false
        }))
      ]);

      setPerfil(dadosUsuario);
      setAvaliacoes(dadosAvaliacoes || []);
      setTopGeneros(dadosGeneros || []);
      setFavoritos(dadosFavoritos || []);
      setStatsBiblioteca(dadosStats);
      setListas(dadosListas || []);
      setStatsSociais(dadosSociais);
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

  const totalAvaliacoes = avaliacoes.length;
  const mediaNotas = totalAvaliacoes > 0
    ? (avaliacoes.reduce((acc, curr) => acc + (Number(curr.nota) || 0), 0) / totalAvaliacoes).toFixed(1)
    : "0.0";

  const handleToggleSeguirPerfil = async () => {
    if (!user) {
      toast.warning("Faça login para seguir outros jogadores.");
      return;
    }
    setProcessandoSeguir(true);
    try {
      const res = await SocialService.alternarSeguir(targetId);
      setStatsSociais((prev) => ({
        ...prev,
        seguidoPorMim: res.seguido,
        totalSeguidores: res.totalSeguidores
      }));
      toast.success(res.seguido ? `Você agora está seguindo ${perfil?.nomeUsuario}.` : `Você deixou de seguir ${perfil?.nomeUsuario}.`);
    } catch (err) {
      console.error("Erro ao alterar seguir:", err);
      toast.error(err.response?.data?.detail || err.response?.data?.message || "Erro ao atualizar relacionamento.");
    } finally {
      setProcessandoSeguir(false);
    }
  };

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
      await AvaliacaoService.excluirAvaliacao(itemParaExcluir);
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
            <PerfilHeader
              perfil={perfil}
              statsSociais={statsSociais}
              isOwner={isOwner}
              user={user}
              processandoSeguir={processandoSeguir}
              onToggleSeguir={handleToggleSeguirPerfil}
              onOpenConexoes={(tab) => {
                setModalConexoesTab(tab);
                setModalConexoesAberto(true);
              }}
            />

            <PodioFavoritos
              favoritos={favoritos}
              isOwner={isOwner}
              onEditar={() => setModalFavoritosAberto(true)}
            />

            <PerfilStatsBar
              statsBiblioteca={statsBiblioteca}
              totalColecoes={listas.length}
              mediaNotas={mediaNotas}
              topGeneros={topGeneros}
            />

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

            {secaoAtiva === "biblioteca" && (
              <PerfilBibliotecaTab
                perfil={perfil}
                isOwner={isOwner}
                statsBiblioteca={statsBiblioteca}
                statusFiltro={statusFiltro}
                setStatusFiltro={setStatusFiltro}
                buscaBiblioteca={buscaBiblioteca}
                setBuscaBiblioteca={setBuscaBiblioteca}
                loadingBiblioteca={loadingBiblioteca}
                itensBiblioteca={itensBiblioteca}
                onRemoverJogo={(id) => setJogoParaRemoverBiblioteca(id)}
              />
            )}

            {secaoAtiva === "listas" && (
              <PerfilColecoesTab
                perfil={perfil}
                isOwner={isOwner}
                listas={listas}
                onCriarLista={() => {
                  setListaEmEdicao(null);
                  setModalCriarListaAberto(true);
                }}
              />
            )}

            {secaoAtiva === "avaliacoes" && (
              <PerfilAvaliacoesTab
                perfil={perfil}
                isOwner={isOwner}
                avaliacoes={avaliacoes}
                totalAvaliacoes={totalAvaliacoes}
                onEditReview={handleEditReview}
                onDeleteReview={handleDeleteRequest}
              />
            )}
          </>
        )}
      </main>

      <ModalEditarFavoritos
        isOpen={modalFavoritosAberto}
        onClose={() => setModalFavoritosAberto(false)}
        favoritosAtuais={favoritos}
        onSalvo={(novos) => setFavoritos(novos || [])}
      />

      <ModalCriarLista
        isOpen={modalCriarListaAberto}
        onClose={() => {
          setModalCriarListaAberto(false);
          setListaEmEdicao(null);
        }}
        listaParaEditar={listaEmEdicao}
        onListaSalva={recarregarListas}
      />

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

      {modalConexoesAberto && (
        <ModalConexoes
          isOpen={modalConexoesAberto}
          onClose={() => setModalConexoesAberto(false)}
          usuarioId={targetId}
          nomeUsuario={perfil?.nomeUsuario}
          initialTab={modalConexoesTab}
          onConexaoAlterada={carregarDadosPerfil}
        />
      )}
    </div>
  );
};

export default PerfilUsuario;
