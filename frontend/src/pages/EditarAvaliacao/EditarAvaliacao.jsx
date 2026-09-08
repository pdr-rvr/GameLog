import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { fetchReviewById, updateReview } from "./actions/EditarAvaliacaoActions";
import { FaStar } from "react-icons/fa";
import "./EditarAvaliacao.css";

const EditarAvaliacao = () => {
  const { avaliacaoId } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    nomeJogo: "",
    nota: 5,
    textoAvaliacao: ""
  });
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const carregarAvaliacao = async () => {
      setLoading(true);
      setError("");
      try {
        const dados = await fetchReviewById(avaliacaoId);
        setFormData({
          nomeJogo: dados.nomeJogo || "",
          nota: dados.nota || 5,
          textoAvaliacao: dados.textoAvaliacao || ""
        });
      } catch (err) {
        setError(err.message || "Não foi possível carregar a avaliação.");
      } finally {
        setLoading(false);
      }
    };

    if (avaliacaoId) {
      carregarAvaliacao();
    }
  }, [avaliacaoId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      await updateReview(avaliacaoId, {
        nota: parseInt(formData.nota, 10),
        textoAvaliacao: formData.textoAvaliacao
      });
      navigate("/minhas-avaliacoes");
    } catch (err) {
      setError(err.message || "Erro ao atualizar a avaliação.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="editar-avaliacao-container">
      <Navbar />
      <div className="editar-avaliacao-content">
        <h1>Editar Avaliação</h1>
        {formData.nomeJogo && <h2>{formData.nomeJogo}</h2>}

        {error && <div className="error-message">{error}</div>}

        {loading ? (
          <div className="loading-message">Carregando avaliação...</div>
        ) : (
          <form onSubmit={handleSubmit} className="editar-form">
            <div className="form-group rating-selection">
              <label>Nota:</label>
              <div className="stars-input">
                {[1, 2, 3, 4, 5].map((star) => (
                  <FaStar
                    key={star}
                    className={`star-icon ${star <= formData.nota ? "active" : ""}`}
                    onClick={() => setFormData(prev => ({ ...prev, nota: star }))}
                  />
                ))}
                <span className="nota-display">{formData.nota} / 5</span>
              </div>
            </div>

            <div className="form-group">
              <label>Comentário / Opinião:</label>
              <textarea
                rows="5"
                value={formData.textoAvaliacao}
                onChange={(e) => setFormData(prev => ({ ...prev, textoAvaliacao: e.target.value }))}
                required
              />
            </div>

            <div className="botoes-form">
              <button type="submit" className="btn-salvar" disabled={submitting}>
                {submitting ? "Salvando..." : "Salvar Alterações"}
              </button>
              <button type="button" className="btn-cancelar" onClick={() => navigate("/minhas-avaliacoes")}>
                Cancelar
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default EditarAvaliacao;
