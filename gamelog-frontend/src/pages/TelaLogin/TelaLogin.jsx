import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormLogin';
import { loginUsuario } from './actions/TelaLoginActions';
import './TelaLogin.css';

function TelaLogin() {
  const [formData, setFormData] = useState({
    email: '',
    senha: ''
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage({ text: '', type: '' });

    try {
      await loginUsuario({
        email: formData.email,
        senha: formData.senha
      });
      
      setMessage({ text: 'Login bem-sucedido!', type: 'success' });
      setTimeout(() => navigate('/home'), 1000);
    } catch (error) {
      setMessage({ text: error.message || 'Credenciais inválidas! Tente novamente.', type: 'error' });
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
        <h2 className="auth-title">LOGIN</h2>
        <div className="auth-content">
          <div className="auth-box">
            <form onSubmit={handleSubmit} className="auth-form">
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
              
              <div className="button-container">
                <button
                  type="submit"
                  className="auth-button"
                  disabled={loading}
                >
                  {loading ? 'CARREGANDO...' : 'ENTRAR'}
                </button>

                {message.text && (
                  <div className={`auth-message ${message.type}`}>
                    {message.text}
                  </div>
                )}

                <div className="auth-link">
                  Não tem uma conta? <Link to="/cadastro">Cadastre-se</Link>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TelaLogin;