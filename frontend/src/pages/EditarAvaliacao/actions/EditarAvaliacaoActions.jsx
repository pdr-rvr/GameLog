import api from '../../../services/api';

const API_BASE_URL = 'http://localhost:7096/api'; 

export const fetchReviewById = async (reviewId, token) => {
    if (!token) {
        throw new Error('Token de autenticação ausente.');
    }
    try {
        const response = await api.get(`/Avaliacoes/${reviewId}`, {
            headers: {
                'Authorization': `Bearer ${token}`,
            },
        });
        return response.data;
    } catch (error) {
        if (error.response) {
            if (error.response.status === 404) {
                throw new Error('Avaliação não encontrada.');
            }
            if (error.response.status === 401 || error.response.status === 403) {
                throw new Error('Você não tem permissão para acessar esta avaliação.');
            }
            throw new Error(error.response.data.message || `Erro ao carregar avaliação: ${error.response.statusText}`);
        } else if (error.request) {
            throw new Error('Erro de rede: Nenhuma resposta do servidor.');
        } else {
            throw new Error('Erro ao configurar a requisição para carregar avaliação.');
        }
    }
};

export const updateReview = async (reviewId, reviewData, token) => {
    if (!token) {
        throw new Error('Token de autenticação ausente.');
    }
    try {
        const response = await api.put(`/Avaliacoes/${reviewId}`, reviewData, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
            },
        });
        return response.data;
    } catch (error) {
        if (error.response) {
            if (error.response.status === 400 && error.response.data.errors) {
                // Se o backend retornar erros de validação
                const validationErrors = Object.values(error.response.data.errors).flat().join('; ');
                throw new Error(`Erro de validação: ${validationErrors}`);
            }
            if (error.response.status === 401 || error.response.status === 403) {
                throw new Error('Você não tem permissão para atualizar esta avaliação.');
            }
            throw new Error(error.response.data.message || `Erro ao atualizar avaliação: ${error.response.statusText}`);
        } else if (error.request) {
            throw new Error('Erro de rede: Nenhuma resposta do servidor.');
        } else {
            throw new Error('Erro ao configurar a requisição para atualizar avaliação.');
        }
    }
};