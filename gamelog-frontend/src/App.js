import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/TelaLogin/TelaLogin';
import Cadastro from './pages/TelaCadastro/TelaCadastro';
import Home from './pages/TelaHome/TelaHome';
import PaginaJogos from './pages/PaginaJogos/PaginaJogos'; 
import TelaJogo from './pages/TelaJogo/TelaJogo';
import PaginaAvaliacoes from './pages/PaginaAvaliacoes/PaginaAvaliacoes';
import PerfilUsuario from './pages/PerfilUsuario/PerfilUsuario';
import MinhasAvaliacoes from './pages/MinhasAvaliacoes/MinhasAvaliacoes';

import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute';
import { AuthProvider } from './context/AuthContext'; 

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          {/* Rotas de Autenticação (públicas) */}
          <Route path="/login" element={<Login />} />
          <Route path="/cadastro" element={<Cadastro />} />

          {/* Rotas Públicas (acessíveis sem autenticação) */}
          <Route path="/" element={<Home />} /> 
          <Route path="/home" element={<Home />} />
          <Route path="/jogos" element={<PaginaJogos />} /> {/* Rota da lista de jogos */}
          <Route path="/jogos/:jogoId" element={<TelaJogo />} /> {/* Rota para detalhes do jogo */}
          <Route path="/avaliacoes" element={<PaginaAvaliacoes />} />

          {/* Rotas Protegidas (exigem autenticação) */}
          <Route 
            path="/perfil/:userId" 
            element={
              <ProtectedRoute>
                <PerfilUsuario />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/minhas-avaliacoes" 
            element={
              <ProtectedRoute>
                <MinhasAvaliacoes />
              </ProtectedRoute>
            } 
          />
        </Routes>
      </AuthProvider>
    </Router>
  );
}

export default App;