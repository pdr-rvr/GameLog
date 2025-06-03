import React from 'react';
import Navbar from '../../components/Navbar/Navbar'; 

const PaginaJogos = () => {
  return (
    <div className="page-container">
      <Navbar />
      <div className="content-area" style={{ padding: '20px', textAlign: 'center' }}>
        <h2>Página de Jogos</h2>
        <p>Aqui você verá uma lista de todos os jogos disponíveis para avaliação.</p>
      </div>
    </div>
  );
};

export default PaginaJogos;