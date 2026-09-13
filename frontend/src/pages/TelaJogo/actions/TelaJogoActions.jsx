import { JogoService } from "../../../services/jogoService";
import api from "../../../services/api";

export const buscarJogoPorId = (jogoId) => JogoService.obterJogoPorId(jogoId);

export const buscarAvaliacoesPorJogoId = async (jogoId) => {
    try {
        const response = await api.get(`/Avaliacoes/jogo/${jogoId}`);
        const avaliacoes = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return avaliacoes;
    } catch {
        return [];
    }
};
