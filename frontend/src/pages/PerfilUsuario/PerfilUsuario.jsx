import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { fetchUserProfile, updateUserProfile } from "./actions/PerfilUsuarioActions";
import { FaUser, FaEnvelope, FaLock, FaShieldAlt, FaGamepad, FaSave } from "react-icons/fa";
import "./PerfilUsuario.css";

const PerfilUsuario = () => {
  const { userId } = useParams();
  const { user, loadUserFromToken } = useAuth();
  const toast = useToast();

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
        toast.error(error.message || "Erro ao carregar dados do perfil.");
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

  const validateUpdate = () => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email.trim())) {
      toast.error("Informe um endereço de e-mail válido.");
      return false;
    }

    if (formData.nomeUsuario.trim().length < 3) {
      toast.warning("O nome de usuário deve ter no mínimo 3 caracteres.");
      return false;
    }

    if (formData.novaSenha) {
      if (formData.novaSenha.length < 6) {
        toast.warning("A nova senha deve ter no mínimo 6 caracteres.");
        return false;
      }
      if (!/[A-Z]/.test(formData.novaSenha)) {
        toast.warning("A nova senha deve conter pelo menos uma letra maiúscula (A-Z).");
        return false;
      }
      if (!/[0-9]/.test(formData.novaSenha)) {
        toast.warning("A nova senha deve conter pelo menos um número (0-9).");
        return false;
      }
      if (formData.novaSenha !== formData.confirmarNovaSenha) {
        toast.error("A confirmação da nova senha não confere.");
        return false;
      }
    }

    if (!formData.senhaAtual) {
      toast.warning("Digite sua senha atual para autorizar as alterações.");
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateUpdate()) return;

    setSubmitting(true);
    try {
      const payload = {
        nomeUsuario: formData.nomeUsuario.trim(),
        email: formData.email.trim(),
        senhaAtual: formData.senhaAtual,
        novaSenha: formData.novaSenha ? formData.novaSenha : null,
        fotoDePerfil: formData.fotoDePerfil || ""
      };

      await updateUserProfile(user.id, payload);
      toast.success("Perfil atualizado com sucesso!");
      setFormData(prev => ({ ...prev, senhaAtual: "", novaSenha: "", confirmarNovaSenha: "" }));
      loadUserFromToken();
    } catch (error) {
      toast.error(error.message || "Erro ao atualizar perfil. Verifique sua senha atual.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="perfil-page-container">
      <Navbar />

      <div className="perfil-page-content">
        {/* Banner do Perfil Gamer */}
        <div className="perfil-header-card">
          <div className="perfil-avatar-badge">
            {formData.fotoDePerfil ? (
              <img src={formData.fotoDePerfil} alt="Avatar" className="perfil-avatar-image" />
            ) : (
              <span className="perfil-avatar-initial">
                {formData.nomeUsuario ? formData.nomeUsuario.charAt(0).toUpperCase() : "G"}
              </span>
            )}
          </div>
          <div className="perfil-user-meta">
            <div className="perfil-badge-row">
              <h2>{formData.nomeUsuario || "Gamer"}</h2>
              <span className="gamer-role-chip"><FaGamepad /> Membro GameLog</span>
            </div>
            <p className="perfil-user-email">{formData.email}</p>
          </div>
        </div>

        {/* Formulário de Configurações */}
        {loading ? (
          <div className="loading-message">Carregando dados do perfil...</div>
        ) : (
          <form onSubmit={handleSubmit} className="perfil-settings-grid">
            {/* Card 1: Informações da Conta */}
            <div className="settings-card">
              <div className="settings-card-header">
                <FaUser className="section-icon" />
                <h3>Informações da Conta</h3>
              </div>

              <div className="settings-form-group">
                <label htmlFor="nomeUsuario">Nome de Usuário (Nick)</label>
                <div className="input-with-icon">
                  <FaGamepad />
                  <input
                    type="text"
                    id="nomeUsuario"
                    name="nomeUsuario"
                    value={formData.nomeUsuario}
                    onChange={handleChange}
                    placeholder="Seu nome de usuário"
                    required
                  />
                </div>
              </div>

              <div className="settings-form-group">
                <label htmlFor="email">E-mail</label>
                <div className="input-with-icon">
                  <FaEnvelope />
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    placeholder="seuemail@exemplo.com"
                    required
                  />
                </div>
              </div>
            </div>

            {/* Card 2: Segurança & Senha */}
            <div className="settings-card">
              <div className="settings-card-header">
                <FaShieldAlt className="section-icon" />
                <h3>Segurança & Senha</h3>
              </div>

              <div className="settings-form-group">
                <label htmlFor="novaSenha">Nova Senha (Opcional)</label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="novaSenha"
                    name="novaSenha"
                    value={formData.novaSenha}
                    onChange={handleChange}
                    placeholder="Deixe em branco para manter a atual"
                  />
                </div>
                <span className="input-hint">Requisitos: Mín. 6 dígitos, 1 letra maiúscula e 1 número.</span>
              </div>

              <div className="settings-form-group">
                <label htmlFor="confirmarNovaSenha">Confirmar Nova Senha</label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="confirmarNovaSenha"
                    name="confirmarNovaSenha"
                    value={formData.confirmarNovaSenha}
                    onChange={handleChange}
                    placeholder="Confirme a nova senha"
                  />
                </div>
              </div>

              <div className="divider-line"></div>

              <div className="settings-form-group current-password-group">
                <label htmlFor="senhaAtual">
                  Senha Atual <span>(Obrigatória para salvar qualquer alteração)</span>
                </label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="senhaAtual"
                    name="senhaAtual"
                    value={formData.senhaAtual}
                    onChange={handleChange}
                    placeholder="Digite sua senha atual"
                    required
                  />
                </div>
              </div>
            </div>

            <div className="perfil-submit-bar">
              <button type="submit" className="btn-save-profile" disabled={submitting}>
                <FaSave /> {submitting ? "Salvando Alterações..." : "Salvar Alterações"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default PerfilUsuario;
