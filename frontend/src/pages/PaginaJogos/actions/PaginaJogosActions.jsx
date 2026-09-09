import api from "../../../services/api";

export const obterMetadadosFiltros = async () => {
    try {
        const response = await api.get("/Jogos/metadados-filtros");
        return response.data || { generos: [], empresas: [], anos: [] };
    } catch (error) {
        console.error("Erro ao obter metadados dos filtros:", error);
        return { generos: [], empresas: [], anos: [] };
    }
};

export const buscarJogos = async () => {
    try {
        const response = await api.get("/Jogos");
        const dados = Array.isArray(response.data) ? response.data : (response.data?.$values || []);
        return dados.map(jogo => {
            const jogoId = jogo.jogoId ?? jogo.id;
            return {
                ...jogo,
                id: jogoId,
                jogoId: jogoId,
                imagem: jogo.imagem || "/game-images/default_game_cover.png"
            };
        });
    } catch (error) {
        console.error("Erro ao buscar jogos:", error);
        throw new Error("Não foi possível carregar a lista de jogos");
    }
};

export const buscarJogosPaginados = async ({
    pagina = 1,
    itensPorPagina = 12,
    busca = "",
    genero = "",
    ano = "",
    empresa = "",
    ordenacao = "melhores"
}) => {
    try {
        const params = new URLSearchParams();
        params.append("pagina", pagina);
        params.append("itensPorPagina", itensPorPagina);
        if (busca && busca.trim()) params.append("busca", busca.trim());
        if (genero) params.append("genero", genero);
        if (ano) params.append("ano", ano);
        if (empresa) params.append("empresa", empresa);
        if (ordenacao) params.append("ordenacao", ordenacao);

        const response = await api.get(`/Jogos?${params.toString()}`);
        const data = response.data;

        const itens = (data.itens || []).map(jogo => {
            const jogoId = jogo.jogoId ?? jogo.id;
            return {
                ...jogo,
                id: jogoId,
                jogoId: jogoId,
                imagem: jogo.imagem || "/game-images/default_game_cover.png"
            };
        });

        return {
            itens,
            paginaAtual: data.paginaAtual || 1,
            totalPaginas: data.totalPaginas || 1,
            totalItens: data.totalItens || 0,
            itensPorPagina: data.itensPorPagina || itensPorPagina,
            temAnterior: Boolean(data.temAnterior),
            temProxima: Boolean(data.temProxima)
        };
    } catch (error) {
        console.error("Erro ao buscar jogos paginados:", error);
        throw new Error("Não foi possível carregar a página de jogos.");
    }
};
