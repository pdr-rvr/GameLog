import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { fetchUserProfile, updateUserProfile } from './actions/PerfilUsuarioActions';
import './PerfilUsuario.css';

const PerfilUsuario = () => {
  const navigate = useNavigate();
  const { user, loadingAuth, isAuthenticated } = useAuth();

  const [profileImageFile, setProfileImageFile] = useState(null);
  const [profileImageUrl, setProfileImageUrl] = useState('/images/default_profile.png');
  const [nomeUsuario, setNomeUsuario] = useState('');
  const [email, setEmail] = useState('');
  const [senhaAtual, setSenhaAtual] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [pageLoading, setPageLoading] = useState(true);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);

  useEffect(() => {
    const loadUserProfile = async () => {
      setError(null);
      setSuccessMessage(null);
      setPageLoading(true);

      if (loadingAuth) {
        return; 
      }

      const token = localStorage.getItem('token');

      if (!isAuthenticated || !user?.id || !token) {
        setError('Você precisa estar logado para editar seu perfil.');
        setPageLoading(false);
        return;
      }
      
      try {
        const data = await fetchUserProfile(user.id, token);
        setNomeUsuario(data.nomeUsuario || '');
        setEmail(data.email || '');
        setProfileImageUrl(data.fotoDePerfil || '/images/default_profile.png');
        setError(null);
      } catch (err) {
        setError(err.message || 'Ocorreu um erro ao carregar o perfil.');
      } finally {
        setPageLoading(false);
      }
    };

    loadUserProfile();
  }, [loadingAuth, user, isAuthenticated, navigate]);

  const handleImageChange = (e) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setProfileImageFile(file);
      setProfileImageUrl(URL.createObjectURL(file));
    }
  };

  const handleSave = async (e) => {
    e.preventDefault();

    setPageLoading(true);
    setError(null);
    setSuccessMessage(null);

    const token = localStorage.getItem('token');

    if (!token || !user?.id) {
      setError('Sessão expirada ou usuário não identificado. Faça login novamente.');
      setPageLoading(false);
      navigate('/login');
      return;
    }

    const formData = {
      nomeUsuario: nomeUsuario,
      email: email,
    };

    const convertFileToBase64 = (file) => {
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
      });
    };

    if (profileImageFile) {
        try {
            formData.fotoDePerfil = await convertFileToBase64(profileImageFile);
        } catch (fileError) {
            setError("Erro ao processar a imagem do perfil.");
            setPageLoading(false);
            return;
        }
    } else if (profileImageUrl && profileImageUrl !== '/images/default_profile.png') {
        formData.fotoDePerfil = profileImageUrl;
    } else {
        formData.fotoDePerfil = null;
    }

    if (senhaAtual && novaSenha) {
      formData.senhaAtual = senhaAtual;
      formData.novaSenha = novaSenha;
    } else if (senhaAtual || novaSenha) {
        setError('Para mudar a senha, você deve preencher tanto a "Senha Atual" quanto a "Nova Senha".');
        setPageLoading(false);
        return;
    }
    
    try {
      const result = await updateUserProfile(user.id, formData, token);
      setSuccessMessage(result.message || 'Perfil atualizado com sucesso!');
      setSenhaAtual('');
      setNovaSenha('');

      setTimeout(() => {
        navigate('/');
      }, 1500);

    } catch (err) {
      setError(err.message || 'Ocorreu um erro desconhecido ao salvar o perfil.');
    } finally {
      setPageLoading(false);
    }
  };

  const handleCancel = () => {
    navigate(-1);
  };

  if (loadingAuth || pageLoading) {
    return <div className="loading-message">Carregando perfil...</div>;
  }

  if (!isAuthenticated || !user?.id) {
    return <div className="error-message">{error || 'Você não está logado ou sua sessão expirou.'}</div>;
  }

  return (
    <div className="profile-page-container">
      <div className="header-bar">
        <h1 className="page-title">Meu Perfil</h1>
        <div className="action-buttons">
          <button type="button" className="cancel-button" onClick={handleCancel} disabled={pageLoading}>Cancelar</button>
          <button type="submit" className="save-button" onClick={handleSave} disabled={pageLoading}>
            {pageLoading ? 'Salvando...' : 'Salvar'}
          </button>
        </div>
      </div>
      
      {error && <div className="error-message">{error}</div>}
      {successMessage && <div className="success-message">{successMessage}</div>}

      <form className="profile-form" onSubmit={handleSave}>
        <div className="form-content">
          <div className="left-column">
            <div className="form-section image-upload-section">
              <label htmlFor="avatar-upload" className="label">Inserir avatar</label>
              <div className="image-input-group">
                <input
                  type="text"
                  readOnly
                  className="input-field"
                  value={profileImageFile ? profileImageFile.name : (profileImageUrl && profileImageUrl !== '/images/default_profile.png' ? 'Imagem atual' : 'Adicione um avatar')}
                  placeholder="Adicione um avatar"
                  disabled={pageLoading}
                />
                <input
                  type="file"
                  id="avatar-upload"
                  accept="image/*"
                  onChange={handleImageChange}
                  style={{ display: 'none' }}
                />
                <label htmlFor="avatar-upload" className="select-button">
                  SELECIONAR
                </label>
              </div>
            </div>

            <div className="form-section">
              <label htmlFor="nomeUsuario" className="label">Nome de Usuário</label>
              <input
                type="text"
                id="nomeUsuario"
                className="input-field"
                placeholder="Seu nome de usuário"
                value={nomeUsuario}
                onChange={(e) => setNomeUsuario(e.target.value)}
                required
                disabled={pageLoading}
              />
            </div>

            <div className="form-section">
              <label htmlFor="email" className="label">Email</label>
              <input
                type="email"
                id="email"
                className="input-field"
                placeholder="seu.email@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={pageLoading}
              />
            </div>

            <div className="form-section">
              <label htmlFor="senhaAtual" className="label">Senha Atual (para alterar a senha)</label>
              <input
                type="password"
                id="senhaAtual"
                className="input-field"
                placeholder="********"
                value={senhaAtual}
                onChange={(e) => setSenhaAtual(e.target.value)}
                disabled={pageLoading}
              />
            </div>

            <div className="form-section">
              <label htmlFor="novaSenha" className="label">Nova Senha (deixe em branco se não for alterar)</label>
              <input
                type="password"
                id="novaSenha"
                className="input-field"
                placeholder="********"
                value={novaSenha}
                onChange={(e) => setNovaSenha(e.target.value)}
                disabled={pageLoading}
              />
            </div>

          </div>
          
          <div className="right-column">
            <div className="profile-image-preview">
              {profileImageUrl ? (
                <img src={profileImageUrl} alt="Preview do Avatar" className="profile-preview-image" />
              ) : (
                <div className="profile-placeholder-icon-container">
                  <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#ccc" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                    <circle cx="12" cy="7" r="4"></circle>
                    <path d="M2 21v-2a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v2"></path>
                  </svg>
                </div>
              )}
            </div>
          </div>
        </div>
      </form>
    </div>
  );
};

export default PerfilUsuario;