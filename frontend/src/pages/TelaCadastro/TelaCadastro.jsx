import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import AuthLayout from '../../components/AuthLayout/AuthLayout';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import './TelaCadastro.css';

function TelaCadastro() {
  const [formData, setFormData] = useState({
    nick: '',
    email: '',
    senha: '',
    confirmarSenha: ''
  });
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { register } = useAuth();
  const toast = useToast();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const validateForm = () => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email.trim())) {
      toast.error('Informe um endereço de e-mail válido.');
      return false;
    }

    if (formData.nick.trim().length < 3) {
      toast.warning('O nome de usuário deve ter pelo menos 3 caracteres.');
      return false;
    }

    if (formData.senha.length < 6) {
      toast.warning('A senha deve ter no mínimo 6 caracteres.');
      return false;
    }

    if (!/[A-Z]/.test(formData.senha)) {
      toast.warning('A senha deve conter pelo menos uma letra maiúscula (A-Z).');
      return false;
    }

    if (!/[0-9]/.test(formData.senha)) {
      toast.warning('A senha deve conter pelo menos um número (0-9).');
      return false;
    }

    if (formData.senha !== formData.confirmarSenha) {
      toast.error('As senhas digitadas não coincidem.');
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) return;

    setLoading(true);

    try {
      await register(formData.nick.trim(), formData.email.trim(), formData.senha);
      toast.success('Conta criada com sucesso! Faça seu login para começar.');
      navigate('/login');
    } catch (error) {
      toast.error(error.message || 'Erro ao cadastrar conta. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout subtitle="Crie sua conta em segundos e comece a registrar seus jogos finalizados.">
      <div className="auth-form-card">
        <div className="auth-header">
          <h2>Criar Conta</h2>
          <p>Preencha os dados abaixo para se juntar ao GameLog</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="nick">Nome de Usuário (Nick)</label>
            <input
              id="nick"
              name="nick"
              type="text"
              placeholder="Ex: PedroGamer"
              value={formData.nick}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

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
              placeholder="Mín. 6 dígitos, 1 maiúscula e 1 número"
              value={formData.senha}
              onChange={handleChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmarSenha">Confirmar Senha</label>
            <input
              id="confirmarSenha"
              name="confirmarSenha"
              type="password"
              placeholder="Repita sua senha"
              value={formData.confirmarSenha}
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
            {loading ? 'Criando Conta...' : 'Cadastrar-se'}
          </button>
        </form>

        <div className="auth-footer-link">
          Já possui uma conta? <Link to="/login">Fazer Login</Link>
        </div>
      </div>
    </AuthLayout>
  );
}

export default TelaCadastro;
