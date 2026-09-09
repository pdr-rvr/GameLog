import React, { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { fetchReviewById, updateReview } from "./actions/EditarAvaliacaoActions";
import { useToast } from "../../context/ToastContext";
import { FaStar, FaGamepad, FaArrowLeft, FaSave } from "react-icons/fa";
import "./EditarAvaliacao.css";

const ratingLabels = {
  1: "1 / 5 - Ruim",
  2: "2 / 5 - Regular",
  3: "3 / 5 - Bom",
  4: "4 / 5 - Muito Bom",
  5: "5 / 5 - Excelente"
};

const EditarAvaliacao = () => {
  const params = useParams();
  const reviewId = params.reviewId || params.avaliacaoId || params.id;
  const navigate = useNavigate();
  const location = useLocation();
  const toast = useToast();

  const returnUrl = location.state?.from || (reviewId ? `/avaliacoes/${reviewId}` : "/minhas-avaliacoes");

  const [formData, setFormData] = useState({
    nomeJogo: "",
    nota: 5,
    textoAvaliacao: ""
  });
  const [hoverRating, setHoverRating] = useState(0);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const carregarAvaliacao = async () => {
      if (!reviewId) {
        setError("Identificador de avaliação não fornecido.");
        setLoading(false);
        return;
      }

      setLoading(true);
      setError("");
      try {
        const dados = await fetchReviewById(reviewId);
        setFormData({
          nomeJogo: dados.nomeJogo || dados.tituloJogo || "Jogo",
          nota: dados.nota || 5,
          textoAvaliacao: dados.textoAvaliacao || ""
        });
      } catch (err) {
        console.error("Erro ao carregar avaliação para edição:", err);
        setError(err.message || "Não foi possível carregar a avaliação.");
      } finally {
        setLoading(false);
      }
    };

    carregarAvaliacao();
  }, [reviewId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.textoAvaliacao.trim() || !reviewId) return;

    setSubmitting(true);
    setError("");

    try {
      await updateReview(reviewId, {
        nota: parseInt(formData.nota, 10),
        textoAvaliacao: formData.textoAvaliacao.trim()
      });
      toast.success("Avaliação atualizada com sucesso!");
      navigate(returnUrl);
    } catch (err) {
      setError(err.message || "Erro ao atualizar a avaliação.");
      toast.error(err.message || "Erro ao atualizar a avaliação.");
    } finally {
      setSubmitting(false);
    }
  };

  const currentDisplayRating = hoverRating || formData.nota;

  return (
    <div className="editar-avaliacao-page">
      <Navbar />
      <main className="editar-avaliacao-container">
        <div className="editar-avaliacao-card">
          <header className="editar-header">
            <button
              type="button"
              className="btn-back-link"
              onClick={() => navigate(returnUrl)}
            >
              <FaArrowLeft /> <span>{returnUrl.includes("minhas-avaliacoes") ? "Voltar para minhas avaliações" : "Voltar"}</span>
            </button>
            <h1 className="editar-title">Editar Avaliação</h1>
          </header>

          {loading && (
            <div className="editar-state loading">
              <div className="editar-spinner"></div>
              <span>Carregando dados da avaliação...</span>
            </div>
          )}

          {error && !loading && (
            <div className="editar-state error">
              <span>{error}</span>
            </div>
          )}

          {!loading && (
            <form onSubmit={handleSubmit} className="editar-form">
              {/* Jogo em Edição */}
              <div className="editar-game-banner">
                <div className="game-banner-icon">
                  <FaGamepad />
                </div>
                <div className="game-banner-info">
                  <span className="game-banner-label">Jogo Avaliado</span>
                  <span className="game-banner-title">{formData.nomeJogo}</span>
                </div>
              </div>

              {/* Seletor de Estrelas */}
              <div className="editar-form-group">
                <label className="form-label">Sua Classificação</label>
                <div className="stars-picker-wrapper">
                  <div className="stars-picker">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <button
                        type="button"
                        key={star}
                        className={`star-pick-btn ${currentDisplayRating >= star ? "active" : ""}`}
                        onClick={() => setFormData((prev) => ({ ...prev, nota: star }))}
                        onMouseEnter={() => setHoverRating(star)}
                        onMouseLeave={() => setHoverRating(0)}
                        aria-label={`Nota ${star}`}
                      >
                        <FaStar />
                      </button>
                    ))}
                  </div>
                  <span className="stars-label-text">
                    {ratingLabels[currentDisplayRating] || `${currentDisplayRating} / 5`}
                  </span>
                </div>
              </div>

              {/* Texto da Avaliação */}
              <div className="editar-form-group">
                <label htmlFor="textoAvaliacao" className="form-label">
                  Sua Opinião / Review
                </label>
                <textarea
                  id="textoAvaliacao"
                  rows="6"
                  maxLength={500}
                  placeholder="Compartilhe suas impressões detalhadas sobre o jogo..."
                  value={formData.textoAvaliacao}
                  onChange={(e) =>
                    setFormData((prev) => ({ ...prev, textoAvaliacao: e.target.value }))
                  }
                  required
                />
                <div className="editar-char-counter">
                  <span className={formData.textoAvaliacao.length > 450 ? "near-limit" : ""}>
                    {formData.textoAvaliacao.length}
                  </span>{" "}
                  / 500 caracteres
                </div>
              </div>

              {/* Botões de Ação */}
              <div className="editar-actions">
                <button
                  type="button"
                  className="btn-editar-cancel"
                  onClick={() => navigate(returnUrl)}
                  disabled={submitting}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="btn-editar-save"
                  disabled={submitting || !formData.textoAvaliacao.trim()}
                >
                  <FaSave />
                  <span>{submitting ? "Salvando..." : "Salvar Alterações"}</span>
                </button>
              </div>
            </form>
          )}
        </div>
      </main>
    </div>
  );
};

export default EditarAvaliacao;

