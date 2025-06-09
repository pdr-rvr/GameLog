import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { fetchReviewById, updateReview } from './actions/EditarAvaliacaoActions'; 
import FormAvaliacao from '../../components/FormAvaliacao/FormAvaliacao'; 
import Navbar from '../../components/Navbar/Navbar'; 
import './EditarAvaliacao.css'; 
import { buscarJogos } from '../TelaHome/actions/TelaHomeActions';

const EditarAvaliacao = () => {
    const { reviewId } = useParams();
    const navigate = useNavigate();
    const { user, loadingAuth, isAuthenticated } = useAuth();

    const [avaliacaoOriginal, setAvaliacaoOriginal] = useState(null);
    const [jogos, setJogos] = useState([]);
    const [loadingPage, setLoadingPage] = useState(true);
    const [loadingSubmit, setLoadingSubmit] = useState(false);
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState(null);
    const [activeTab, setActiveTab] = useState('minhas');

    useEffect(() => {
        const loadPageData = async () => {
            setLoadingPage(true);
            setError(null);

            if (loadingAuth) {
                return;
            }

            const token = localStorage.getItem('token');

            if (!isAuthenticated || !user?.id || !token) {
                setError('Você precisa estar logado para editar avaliações.');
                setLoadingPage(false);
                navigate('/login');
                return;
            }

            try {
                const fetchedAvaliacao = await fetchReviewById(reviewId, token);
                
                if (fetchedAvaliacao) {
                    setAvaliacaoOriginal(fetchedAvaliacao);
                } else {
                    setError('Avaliação não encontrada ou você não tem permissão para editá-la.');
                    setLoadingPage(false);
                    return;
                }

                const fetchedJogos = await buscarJogos();
                setJogos(fetchedJogos);

            } catch (err) {
                setError(err.message || 'Ocorreu um erro ao carregar a página de edição.');
                console.error("Erro ao carregar dados da página de edição:", err);
            } finally {
                setLoadingPage(false);
            }
        };

        loadPageData();
    }, [reviewId, loadingAuth, user, isAuthenticated, navigate]);

    const handleSubmitAvaliacao = async (updatedAvaliacaoData) => {
        setLoadingSubmit(true);
        setError(null);
        setSuccessMessage(null);

        const token = localStorage.getItem('token');
        if (!token) {
            setError('Sessão expirada. Por favor, faça login novamente.');
            setLoadingSubmit(false);
            navigate('/login');
            return;
        }

        try {
            const dataToSend = {
                nota: parseInt(updatedAvaliacaoData.nota),
                textoAvaliacao: updatedAvaliacaoData.textoAvaliacao
            };

            await updateReview(reviewId, dataToSend, token);
            setSuccessMessage('Avaliação atualizada com sucesso!');
            setTimeout(() => {
                navigate('/minhas-avaliacoes'); 
            }, 1500); 
        } catch (err) {
            setError(err.message || 'Erro ao atualizar avaliação.');
            console.error("Erro ao atualizar avaliação:", err);
        } finally {
            setLoadingSubmit(false);
        }
    };

    const handleCancel = () => {
        navigate('/minhas-avaliacoes');
    };

    const handleOpenPublishForm = () => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
        navigate('/avaliacoes/criar');
    };

    if (loadingPage || loadingAuth) {
        return <div className="loading-message">Carregando avaliação para edição...</div>;
    }

    if (!isAuthenticated || !user?.id) {
        return <div className="error-message">{error || 'Você não está logado para ver esta página.'}</div>;
    }

    if (error && !avaliacaoOriginal) { 
        return <div className="error-message">{error}</div>;
    }

    if (!avaliacaoOriginal) {
        return <div className="no-data-message">Avaliação não encontrada ou você não tem permissão para acessá-la.</div>;
    }

    const jogoSelecionado = jogos.find(jogo => jogo.jogoId === avaliacaoOriginal.jogoId);

    const initialAvaliacaoData = {
        jogoId: avaliacaoOriginal.jogoId,
        nota: avaliacaoOriginal.nota,
        textoAvaliacao: avaliacaoOriginal.textoAvaliacao,
        tituloJogo: avaliacaoOriginal.nomeJogo || (jogoSelecionado ? jogoSelecionado.titulo : '')
    };

    return (
        <div className="minhas-avaliacoes-page-container">
            <Navbar
                activeTab={activeTab}
                setActiveTab={setActiveTab}
                usuario={user}
                onPublishClick={handleOpenPublishForm}
            />
            <div className="editar-avaliacao-content">
                <div className="form-overlay">
                    <FormAvaliacao
                        jogos={jogos}
                        onSubmit={handleSubmitAvaliacao}
                        loading={loadingSubmit}
                        error={null}
                        onCancel={handleCancel} 
                        initialData={initialAvaliacaoData}
                        isEditing={true}
                    />
                </div>

                {successMessage && <div className="success-message">{successMessage}</div>}
                {error && <div className="error-message">{error}</div>}
            </div>
        </div>
    );
};

export default EditarAvaliacao;