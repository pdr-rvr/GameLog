import React, { useState } from 'react';
import Background from '../../components/Background/Background';
import FundoForm from '../../components/FundoForm/FundoFormCadastro';
import './TelaCadastro.css';
import { useNavigate } from 'react-router-dom';
import { AuthService } from '../../services/authService';

function TelaCadastro() {
  const [email, setEmail] = useState('');
  const [confEmail, setConfEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confSenha, setConfSenha] = useState('');
  const [nick, setNick] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validações
    if (email !== confEmail) {
      setMessage('Os e-mails não coincidem!');
      return;
    }
    
    if (senha !== confSenha) {
      setMessage('As senhas não coincidem!');
      return;
    }
    
    setLoading(true);
    setMessage('');

    try {
      await AuthService.register(nick, email, senha);
      setMessage('Cadastro realizado com sucesso! Faça login.');
      setTimeout(() => navigate('/login'), 1500);
    } catch (error) {
      setMessage(error.message || 'Erro ao cadastrar. Tente novamente.');
    } finally {
      setLoading(false);
    }
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

export default TelaCadastro;
