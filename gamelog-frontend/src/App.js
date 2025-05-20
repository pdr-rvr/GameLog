import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/TelaLogin/TelaLogin';
import Cadastro from './pages/TelaCadastro/TelaCadastro.jsx'

function App() {
  return (
    <Router>
      <div className="app-container">
        <Routes>
          {/* Rota padrão direciona para o login */}
          <Route path="/" element={<Login />} />

          {/* Rota explícita para login */}
          <Route path="/login" element={<Login />} />
          <Route path="/cadastro" element={<Cadastro />} />

          {/* Adicione outras rotas conforme criar novas telas */}
          {/* <Route path="/dashboard" element={<Dashboard />} /> */}
          {/* <Route path="/cadastro" element={<Cadastro />} /> */}
        </Routes>
      </div>
    </Router>
  );
}

export default App;
