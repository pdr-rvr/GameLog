import { EmpresaService } from "../../../services/empresaService";

export const fetchEmpresa = (empresaId) => EmpresaService.obterEmpresaPorId(empresaId);
export const fetchJogosDaEmpresa = (empresaId) => EmpresaService.listarJogosPorEmpresa(empresaId);
