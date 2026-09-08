import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/TelaLogin/TelaLogin';
import Cadastro from './pages/TelaCadastro/TelaCadastro';
import Home from './pages/TelaHome/TelaHome';
import PaginaJogos from './pages/PaginaJogos/PaginaJogos'; 
import TelaJogo from './pages/TelaJogo/TelaJogo';
import PaginaAvaliacoes from './pages/PaginaAvaliacoes/PaginaAvaliacoes';
import PerfilUsuario from './pages/PerfilUsuario/PerfilUsuario';
import ConfiguracoesConta from './pages/ConfiguracoesConta/ConfiguracoesConta';
import MinhasAvaliacoes from './pages/MinhasAvaliacoes/MinhasAvaliacoes';
import EditarAvaliacao from './pages/EditarAvaliacao/EditarAvaliacao';

import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute';
import { AuthProvider } from './context/AuthContext'; 
import { ToastProvider } from './context/ToastContext';

function App() {
  return (
    <Router>
      <ToastProvider>
        <AuthProvider>
          <Routes>
            {/* Rotas de Autenticação (públicas) */}
            <Route path="/login" element={<Login />} />
            <Route path="/cadastro" element={<Cadastro />} />

            {/* Rotas Públicas (acessíveis sem autenticação) */}
            <Route path="/" element={<Home />} /> 
            <Route path="/home" element={<Home />} />
            <Route path="/jogos" element={<PaginaJogos />} />
            <Route path="/jogos/:jogoId" element={<TelaJogo />} />
            <Route path="/avaliacoes" element={<PaginaAvaliacoes />} />
            <Route path="/perfil/:userId" element={<PerfilUsuario />} />

            {/* Rotas Protegidas (exigem autenticação) */}
            <Route 
              path="/configuracoes" 
              element={
                <ProtectedRoute>
                  <ConfiguracoesConta />
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
            <Route 
              path="/avaliacoes/editar/:reviewId" 
              element={
                <ProtectedRoute>
                  <EditarAvaliacao />
                </ProtectedRoute>
              } 
            />
          </Routes>
        </AuthProvider>
      </ToastProvider>
    </Router>
  );
}

export default App;
