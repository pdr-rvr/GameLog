import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { useAuth } from "../../context/AuthContext";
import { fetchUserProfile, updateUserProfile } from "./actions/PerfilUsuarioActions";
import "./PerfilUsuario.css";

const PerfilUsuario = () => {
  const { userId } = useParams();
  const { user, loadUserFromToken } = useAuth();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    nomeUsuario: "",
    email: "",
    senhaAtual: "",
    novaSenha: "",
    confirmarNovaSenha: "",
    fotoDePerfil: ""
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [mensagem, setMensagem] = useState({ tipo: "", texto: "" });

  useEffect(() => {
    const carregarPerfil = async () => {
      setLoading(true);
      try {
        const idToFetch = userId || user?.id;
        if (!idToFetch) return;

        const dados = await fetchUserProfile(idToFetch);
        setFormData(prev => ({
          ...prev,
          nomeUsuario: dados.nomeUsuario || "",
          email: dados.email || "",
          fotoDePerfil: dados.fotoDePerfil || ""
        }));
      } catch (error) {
        setMensagem({ tipo: "erro", texto: error.message || "Erro ao carregar perfil." });
      } finally {
        setLoading(false);
      }
    };

    carregarPerfil();
  }, [userId, user]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMensagem({ tipo: "", texto: "" });

    if (!formData.senhaAtual) {
      setMensagem({ tipo: "erro", texto: "Digite sua senha atual para confirmar as alterações." });
      return;
    }

    if (formData.novaSenha && formData.novaSenha !== formData.confirmarNovaSenha) {
      setMensagem({ tipo: "erro", texto: "A confirmação da nova senha não confere." });
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        nomeUsuario: formData.nomeUsuario,
        email: formData.email,
        senhaAtual: formData.senhaAtual,
        novaSenha: formData.novaSenha || null,
        fotoDePerfil: formData.fotoDePerfil || ""
      };

      await updateUserProfile(user.id, payload);
      setMensagem({ tipo: "sucesso", texto: "Perfil atualizado com sucesso!" });
      setFormData(prev => ({ ...prev, senhaAtual: "", novaSenha: "", confirmarNovaSenha: "" }));
      loadUserFromToken();
    } catch (error) {
      setMensagem({ tipo: "erro", texto: error.message || "Erro ao atualizar perfil." });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="perfil-usuario-container">
      <Navbar />
      <div className="perfil-usuario-content">
        <h1>Meu Perfil</h1>

        {mensagem.texto && (
          <div className={`mensagem-alerta ${mensagem.tipo}`}>
            {mensagem.texto}
          </div>
        )}

        {loading ? (
          <div className="loading-message">Carregando dados do perfil...</div>
        ) : (
          <form onSubmit={handleSubmit} className="perfil-form">
            <div className="form-group">
              <label>Nome de Usuário</label>
              <input
                type="text"
                name="nomeUsuario"
                value={formData.nomeUsuario}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label>E-mail</label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                required
              />
            </div>

            <hr className="divider" />
            <h3>Alterar Senha (Opcional)</h3>

            <div className="form-group">
              <label>Nova Senha</label>
              <input
                type="password"
                name="novaSenha"
                value={formData.novaSenha}
                onChange={handleChange}
                placeholder="Deixe em branco para manter a mesma"
              />
            </div>

            <div className="form-group">
              <label>Confirmar Nova Senha</label>
              <input
                type="password"
                name="confirmarNovaSenha"
                value={formData.confirmarNovaSenha}
                onChange={handleChange}
                placeholder="Confirme a nova senha"
              />
            </div>

            <hr className="divider" />

            <div className="form-group">
              <label>Senha Atual (Obrigatória para salvar)</label>
              <input
                type="password"
                name="senhaAtual"
                value={formData.senhaAtual}
                onChange={handleChange}
                required
                placeholder="Digite sua senha atual"
              />
            </div>

            <button type="submit" className="btn-salvar-perfil" disabled={submitting}>
              {submitting ? "Salvando..." : "Salvar Alterações"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};

export default PerfilUsuario;
