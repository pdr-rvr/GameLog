import { JogoService } from "../../../services/jogoService";
import { AvaliacaoService } from "../../../services/avaliacaoService";
import api from "../../../services/api";

export const buscarAvaliacoes = async (usuarioId = null) => {
  const params = usuarioId ? { usuarioId } : {};
  const data = await AvaliacaoService.listarAvaliacoes(params);
  const dados = Array.isArray(data) ? data : (data?.$values || []);
  return dados.map(avaliacao => ({
    ...avaliacao,
    avaliacaoId: avaliacao.avaliacaoId || avaliacao.id,
    jogoId: avaliacao.jogoId,
    usuarioId: avaliacao.usuarioId,
    nomeUsuario: avaliacao.nomeUsuario || "Usuário Anônimo"
  }));
};

export const buscarJogos = () => JogoService.listarJogos();

export const criarAvaliacao = async (avaliacaoData) => {
  const response = await api.post("/Avaliacoes", avaliacaoData);
  return response.data;
};

export const buscarRecomendacoes = (userId) => JogoService.obterRecomendacoes(userId);
export const buscarDestaques = (limite = 5) => JogoService.obterDestaques(limite);
export const buscarTopAvaliados = () => JogoService.obterTopAvaliados();
