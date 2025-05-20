import React, { useState } from 'react';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormLogin';
import './TelaLogin.css';
import { useNavigate } from 'react-router-dom';
import { loginUsuario } from '../TelaLogin/actions/TelaLogin'; // Importe a action que criamos

function TelaLogin() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');

    try {
      // Chamada para a action de login
      await loginUsuario({
        email: email,
        senha: senha,
        nomeUsuario: '', // Pode ser vazio se não for obrigatório no login
        fotoDePerfil: '' // Pode ser vazio se não for obrigatório
      });
      
      setMessage('Login bem-sucedido!');
      // Redireciona após 1 segundo
      setTimeout(() => navigate('/dashboard'), 1000);
    } catch (error) {
      setMessage(error.message || 'Credenciais inválidas! Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <Background />

      <div className="login-overlay"></div>

      <FundoForm />

      <div className="form-container">
        <h2 className="login-title">LOGIN</h2>
        <div className="form-content">
          <div className="form-box">
            <form onSubmit={handleSubmit} className="login-form">
              <div className="form-group">
                <label htmlFor="email">E-MAIL:</label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="senha">SENHA:</label>
                <input
                  id="senha"
                  type="password"
                  value={senha}
                  onChange={(e) => setSenha(e.target.value)}
                  required
                />
              </div>
              
              <div className="button-container">
                <button
                  type="submit"
                  className="login-button"
                  disabled={loading}
                >
                  {loading ? 'CARREGANDO...' : 'ENTRAR'}
                </button>

                {message && (
                  <div className={`login-message ${message.includes('inválidas') ? 'error' : ''}`}>
                    {message}
                  </div>
                )}
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TelaLogin;