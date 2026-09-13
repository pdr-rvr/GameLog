import { JogoService } from "../../../services/jogoService";

export const obterMetadadosFiltros = () => JogoService.obterMetadadosFiltros();
export const buscarJogos = () => JogoService.listarJogos();
export const buscarJogosPaginados = (params) => JogoService.buscarJogosPaginados(params);
