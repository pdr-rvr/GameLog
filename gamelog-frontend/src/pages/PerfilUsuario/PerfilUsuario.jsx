import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Navbar from '../../components/Navbar/Navbar';
// import { buscarDetalhesUsuario } from '../../services/UsuarioService'; // Criaremos este serviço
import './PerfilUsuario.css';

const PerfilUsuario = () => {
  const { userId } = useParams(); 
  const [perfil, setPerfil] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPerfil = async () => {
      setLoading(true);
      setError('');
      try {
        setPerfil({
            id: userId,
            nomeUsuario: `Usuário ${userId}`,
            email: `usuario${userId}@example.com`,
            fotoDePerfil: 'https://via.placeholder.com/150/FF0000/FFFFFF?text=P', // Exemplo de imagem
            totalAvaliacoes: 10,
            totalCurtidasRecebidas: 50
        });
      } catch (err) {
        console.error("Erro ao carregar perfil:", err);
        setError("Não foi possível carregar o perfil do usuário.");
      } finally {
        setLoading(false);
      }
    };
    if (userId) {
      fetchPerfil();
    }
  }, [userId]);

  return (
    <div className="page-container">
      <Navbar />
      <div className="content-area" style={{ padding: '20px', textAlign: 'center' }}>
        {loading && <div className="loading">Carregando perfil...</div>}
        {error && <div className="error-message">{error}</div>}
        {perfil && (
          <div className="profile-details">
            <img src={perfil.fotoDePerfil || '/images/default_profile.png'} alt="Foto de Perfil" className="profile-page-avatar" />
            <h2>{perfil.nomeUsuario}</h2>
            <p>Email: {perfil.email}</p>
            <p>Avaliações Publicadas: {perfil.totalAvaliacoes}</p>
            <p>Curtidas Recebidas: {perfil.totalCurtidasRecebidas}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default PerfilUsuario;