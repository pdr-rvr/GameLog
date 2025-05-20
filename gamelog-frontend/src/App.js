import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/TelaLogin/TelaLogin';
import Cadastro from './pages/TelaCadastro/TelaCadastro.jsx'

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/cadastro" element={<Cadastro />} />
        {/* Você pode adicionar uma rota padrão para redirecionar */}
        <Route path="/" element={<Login />} />
      </Routes>
    </Router>
  );
}

export default App;
