import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import AuthLayout from '../../components/AuthLayout/AuthLayout';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import './TelaLogin.css';

function TelaLogin() {
  const [formData, setFormData] = useState({
    email: '',
    senha: ''
  });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();
  const toast = useToast();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const data = await login(formData.email, formData.senha);
      const nome = data?.usuario?.nomeUsuario || "Gamer";
      toast.success(`Bem-vindo de volta, ${nome}!`);
      navigate('/home');
    } catch (error) {
      toast.error(error.message || 'Credenciais inválidas. Verifique seu e-mail e senha.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout subtitle="Entre para gerenciar seu catálogo, notas e acompanhar a comunidade gamer.">
      <div className="auth-form-card">
        <div className="auth-header">
          <h2>Entrar na Conta</h2>
          <p>Digite seus dados de acesso para continuar</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="email">E-mail</label>
            <input
              id="email"
              name="email"
              type="email"
              placeholder="seuemail@exemplo.com"
              value={formData.email}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="senha">Senha</label>
            <input
              id="senha"
              name="senha"
              type="password"
              placeholder="••••••••"
              value={formData.senha}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <button
            type="submit"
            className="btn-auth-submit"
            disabled={loading}
          >
            {loading ? 'Entrando...' : 'Entrar no GameLog'}
          </button>
        </form>

        <div className="auth-footer-link">
          Não possui uma conta? <Link to="/cadastro">Cadastre-se gratuitamente</Link>
        </div>
      </div>
    </AuthLayout>
  );
}

export default TelaLogin;
