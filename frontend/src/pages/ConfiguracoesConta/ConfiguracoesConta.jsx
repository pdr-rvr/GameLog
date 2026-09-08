import React, { useState, useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import Navbar from "../../components/Navbar/Navbar";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { fetchUserProfile, updateUserProfile } from "./actions/ConfiguracoesContaActions";
import { 
  FaUser, 
  FaEnvelope, 
  FaLock, 
  FaShieldAlt, 
  FaGamepad, 
  FaSave, 
  FaArrowLeft, 
  FaImage,
  FaUpload,
  FaTrashAlt,
  FaAlignLeft
} from "react-icons/fa";
import "./ConfiguracoesConta.css";

const ConfiguracoesConta = () => {
  const { user, loadUserFromToken } = useAuth();
  const toast = useToast();
  const fileInputRef = useRef(null);

  const [formData, setFormData] = useState({
    nomeUsuario: "",
    email: "",
    bio: "",
    senhaAtual: "",
    novaSenha: "",
    confirmarNovaSenha: "",
    fotoDePerfil: ""
  });

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const carregarDados = async () => {
      if (!user?.id) return;
      setLoading(true);
      try {
        const dados = await fetchUserProfile(user.id);
        setFormData(prev => ({
          ...prev,
          nomeUsuario: dados.nomeUsuario || "",
          email: dados.email || "",
          bio: dados.bio || "",
          fotoDePerfil: dados.fotoDePerfil || ""
        }));
      } catch (error) {
        toast.error(error.message || "Erro ao carregar dados da conta.");
      } finally {
        setLoading(false);
      }
    };

    carregarDados();
  }, [user]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleFileUpload = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith("image/")) {
      toast.error("Por favor, selecione um arquivo de imagem válido (PNG, JPG, WEBP).");
      return;
    }

    if (file.size > 3 * 1024 * 1024) {
      toast.warning("A imagem deve ter no máximo 3MB.");
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      setFormData(prev => ({ ...prev, fotoDePerfil: reader.result }));
      toast.info("Imagem carregada! Não esqueça de salvar as alterações.");
    };
    reader.onerror = () => {
      toast.error("Erro ao ler o arquivo de imagem.");
    };
    reader.readAsDataURL(file);
  };

  const handleRemovePhoto = () => {
    setFormData(prev => ({ ...prev, fotoDePerfil: "" }));
    if (fileInputRef.current) fileInputRef.current.value = "";
    toast.info("Foto removida. O perfil usará sua inicial padrão.");
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

    if (formData.bio && formData.bio.length > 300) {
      toast.warning("A bio não pode ultrapassar 300 caracteres.");
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
        bio: formData.bio ? formData.bio.trim() : "",
        senhaAtual: formData.senhaAtual,
        novaSenha: formData.novaSenha ? formData.novaSenha : null,
        fotoDePerfil: formData.fotoDePerfil || ""
      };

      await updateUserProfile(user.id, payload);
      toast.success("Configurações atualizadas com sucesso!");
      setFormData(prev => ({ ...prev, senhaAtual: "", novaSenha: "", confirmarNovaSenha: "" }));
      if (loadUserFromToken) loadUserFromToken();
    } catch (error) {
      toast.error(error.message || "Erro ao atualizar conta. Verifique sua senha atual.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="config-page-container">
      <Navbar />

      <main className="config-page-content">
        <header className="config-header">
          <Link to={`/perfil/${user?.id}`} className="btn-back-to-profile">
            <FaArrowLeft /> <span>Ver Meu Perfil Público</span>
          </Link>
          <h1 className="config-title">Configurações de Conta</h1>
          <p className="config-subtitle">Gerencie suas informações de acesso, credenciais e privacidade</p>
        </header>

        {loading ? (
          <div className="config-state loading">
            <div className="config-spinner"></div>
            <span>Carregando dados da sua conta...</span>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="config-form-grid">
            {/* Card 1: Avatar e Foto de Perfil */}
            <div className="config-card">
              <div className="config-card-header">
                <FaImage className="section-icon" />
                <div>
                  <h3>Foto de Perfil</h3>
                  <p>Faça upload de uma imagem ou informe uma URL direta</p>
                </div>
              </div>

              <div className="avatar-upload-container">
                <div className="avatar-preview-box">
                  {formData.fotoDePerfil ? (
                    <img src={formData.fotoDePerfil} alt="Preview" className="preview-avatar-img" />
                  ) : (
                    <div className="preview-avatar-fallback">
                      <span>{formData.nomeUsuario ? formData.nomeUsuario.charAt(0).toUpperCase() : "G"}</span>
                    </div>
                  )}
                </div>

                <div className="avatar-controls-col">
                  <div className="avatar-actions-buttons">
                    <input
                      type="file"
                      ref={fileInputRef}
                      onChange={handleFileUpload}
                      accept="image/*"
                      style={{ display: "none" }}
                    />
                    <button
                      type="button"
                      className="btn-upload-avatar"
                      onClick={() => fileInputRef.current?.click()}
                    >
                      <FaUpload /> <span>Fazer Upload de Foto</span>
                    </button>

                    {formData.fotoDePerfil && (
                      <button
                        type="button"
                        className="btn-remove-avatar"
                        onClick={handleRemovePhoto}
                      >
                        <FaTrashAlt /> <span>Remover Foto</span>
                      </button>
                    )}
                  </div>

                  <div className="input-with-icon url-avatar-input">
                    <FaImage />
                    <input
                      type="url"
                      name="fotoDePerfil"
                      value={formData.fotoDePerfil}
                      onChange={handleChange}
                      placeholder="Ou insira a URL direta da imagem (https://...)"
                    />
                  </div>
                  {formData.fotoDePerfil && (
                    <span className="input-hint">
                      Foto configurada com sucesso. Salve para confirmar.
                    </span>
                  )}
                </div>
              </div>
            </div>

            {/* Card 2: Informações Pessoais & Bio */}
            <div className="config-card">
              <div className="config-card-header">
                <FaUser className="section-icon" />
                <div>
                  <h3>Informações Pessoais & Bio</h3>
                  <p>Seu nome de exibição, bio pública e e-mail de acesso</p>
                </div>
              </div>

              <div className="config-form-group">
                <label htmlFor="nomeUsuario">Nome de Usuário</label>
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

              <div className="config-form-group">
                <label htmlFor="bio">Bio do Jogador</label>
                <div className="textarea-with-icon">
                  <FaAlignLeft className="textarea-icon" />
                  <textarea
                    id="bio"
                    name="bio"
                    value={formData.bio}
                    onChange={handleChange}
                    maxLength={300}
                    rows={3}
                    placeholder="Conte à comunidade sobre seus jogos favoritos, estilos que mais joga ou curiosidades gamers..."
                  />
                </div>
                <div className="char-counter">
                  {(formData.bio?.length || 0)} / 300 caracteres
                </div>
              </div>

              <div className="config-form-group">
                <label htmlFor="email">Endereço de E-mail</label>
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

            {/* Card 3: Alteração de Senha & Segurança */}
            <div className="config-card">
              <div className="config-card-header">
                <FaShieldAlt className="section-icon" />
                <div>
                  <h3>Segurança & Senha</h3>
                  <p>Altere sua senha de acesso e confirme sua identidade</p>
                </div>
              </div>

              <div className="config-form-group">
                <label htmlFor="novaSenha">Nova Senha</label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="novaSenha"
                    name="novaSenha"
                    value={formData.novaSenha}
                    onChange={handleChange}
                    placeholder="Deixe em branco para manter sua senha atual"
                  />
                </div>
                <span className="input-hint">Requisitos: Mínimo 6 caracteres, com 1 letra maiúscula e 1 número.</span>
              </div>

              <div className="config-form-group">
                <label htmlFor="confirmarNovaSenha">Confirmar Nova Senha</label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="confirmarNovaSenha"
                    name="confirmarNovaSenha"
                    value={formData.confirmarNovaSenha}
                    onChange={handleChange}
                    placeholder="Digite novamente a nova senha"
                  />
                </div>
              </div>

              <div className="config-divider"></div>

              <div className="config-form-group current-password-group">
                <label htmlFor="senhaAtual">
                  Senha Atual <span>(Obrigatória para confirmar qualquer alteração)</span>
                </label>
                <div className="input-with-icon">
                  <FaLock />
                  <input
                    type="password"
                    id="senhaAtual"
                    name="senhaAtual"
                    value={formData.senhaAtual}
                    onChange={handleChange}
                    placeholder="Digite sua senha atual para autorizar"
                    required
                  />
                </div>
              </div>
            </div>

            {/* Barra de Ações */}
            <div className="config-submit-bar">
              <Link to={`/perfil/${user?.id}`} className="btn-config-cancel">
                Cancelar
              </Link>
              <button type="submit" className="btn-config-save" disabled={submitting}>
                <FaSave /> <span>{submitting ? "Salvando Alterações..." : "Salvar Alterações"}</span>
              </button>
            </div>
          </form>
        )}
      </main>
    </div>
  );
};

export default ConfiguracoesConta;
