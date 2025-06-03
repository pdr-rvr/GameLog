import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormCadastro';
import { useAuth } from '../../context/AuthContext';
import './TelaCadastro.css';

function TelaCadastro() {
  const [formData, setFormData] = useState({
    nick: '',
    email: '',
    confEmail: '',
    senha: '',
    confSenha: ''
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });
  const navigate = useNavigate();
  const { register } = useAuth();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (formData.email !== formData.confEmail) {
      setMessage({ text: 'Os e-mails não coincidem!', type: 'error' });
      return;
    }
    
    if (formData.senha !== formData.confSenha) {
      setMessage({ text: 'As senhas não coincidem!', type: 'error' });
      return;
    }
    
    setLoading(true);
    setMessage({ text: '', type: '' });

    try {
      await register(formData.nick, formData.email, formData.senha);
      
      setMessage({ 
        text: 'Cadastro realizado com sucesso! Redirecionando para login...', 
        type: 'success' 
      });
      setTimeout(() => navigate('/login'), 1500);
    } catch (error) {
      setMessage({ text: error.message || 'Erro ao cadastrar. Tente novamente.', type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <Background />
      <div className="auth-overlay"></div>
      <FundoForm />

      <div className="auth-container">
        <h2 className="auth-title">CADASTRO</h2>
        <div className="auth-content">
          <div className="auth-box">
            <form onSubmit={handleSubmit} className="auth-form">
              <div className="form-group">
                <label htmlFor="nick">NICK:</label>
                <input
                  id="nick"
                  name="nick"
                  type="text"
                  value={formData.nick}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="email">E-MAIL:</label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="confEmail">CONFIRME SEU E-MAIL:</label>
                <input
                  id="confEmail"
                  name="confEmail"
                  type="email"
                  value={formData.confEmail}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="senha">SENHA:</label>
                <input
                  id="senha"
                  name="senha"
                  type="password"
                  value={formData.senha}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="confSenha">CONFIRME SUA SENHA:</label>
                <input
                  id="confSenha"
                  name="confSenha"
                  type="password"
                  value={formData.confSenha}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="button-container">
                <button
                  type="submit"
                  className="auth-button"
                  disabled={loading}
                >
                  {loading ? 'CARREGANDO...' : 'CADASTRAR'}
                </button>

                {message.text && (
                  <div className={`auth-message ${message.type}`}>
                    {message.text}
                  </div>
                )}

                <div className="auth-link">
                  Já tem uma conta? <Link to="/login">Faça login</Link>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TelaCadastro;