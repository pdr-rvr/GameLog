import React, { useState } from 'react';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormLogin';
import './TelaLogin.css';

function TelaLogin() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');

    // Simulação de login
    setTimeout(() => {
      if (email === 'admin@exemplo.com' && senha === '123456') {
        setMessage('Login bem-sucedido! (simulado)');
      } else {
        setMessage('Credenciais inválidas! Tente novamente.');
      }
      setLoading(false);
    }, 1500);
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
          </form>
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
          </div>
        </div>
      </div>
    </div>
  );
}

export default TelaLogin;
