import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/TelaLogin/TelaLogin';
import Cadastro from './pages/TelaCadastro/TelaCadastro';
import Home from './pages/TelaHome/TelaHome';
import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import PaginaJogos from './pages/PaginaJogos/PaginaJogos.js';
import PaginaAvaliacoes from './pages/PaginaAvaliacoes/PaginaAvaliacoes';
import PerfilUsuario from './pages/PerfilUsuario/PerfilUsuario';
import MinhasAvaliacoes from './pages/MinhasAvaliacoes/MinhasAvaliacoes';

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>

          <Route path="/login" element={<Login />} />
          <Route path="/cadastro" element={<Cadastro />} />

          <Route path="/" element={<Home />} /> 
          <Route path="/home" element={<Home />} />
          <Route path="/jogos" element={<PaginaJogos />} />
          <Route path="/avaliacoes" element={<PaginaAvaliacoes />} />

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