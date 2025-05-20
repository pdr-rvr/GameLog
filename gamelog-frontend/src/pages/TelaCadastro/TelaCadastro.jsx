import React, { useState } from 'react';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormCadastro';
import './TelaCadastro.css';

function TelaLogin() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [nick, setNick] = useState('');

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
    <div className="login-page-cad">
      <Background />

      <div className="login-overlay-cad"></div>

      <FundoForm />

      <div className="form-container-cad">
        <h2 className="login-title-cad">CADASTRO</h2>
        <div className="form-content-cad">
        <div className="form-box-cad">
          <form onSubmit={handleSubmit} className="cadastro-form">

            <div className="form-group-cad">
              <label htmlFor="email">E-MAIL:</label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="form-group-cad">
              <label htmlFor="email">CONFIRME SEU E-MAIL:</label>
              <input
                id="confEmail"
                type="email"
                required
              />
            </div>

            <div className="form-group-cad">
              <label htmlFor="senha">NICK:</label>
              <input
                id="nick"
                type="text"
                value={nick}
                onChange={(e) => setNick(e.target.value)}
                required
              />
            </div>

            <div className="form-group-cad">
              <label htmlFor="senha">SENHA:</label>
              <input
                id="senha"
                type="password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
              />
            </div>

            <div className="form-group-cad">
              <label htmlFor="senha">CONFIRME SUA SENHA:</label>
              <input
                id="confSenha"
                type="password"
                required
              />
            </div>
</form>
<div className="button-container-cad">
            <button
              type="submit"
              className="login-button-cad"
              disabled={loading}
            >
              {loading ? 'CARREGANDO...' : 'CADASTRAR'}
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
