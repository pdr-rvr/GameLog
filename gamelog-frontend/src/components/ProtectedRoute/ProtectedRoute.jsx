import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { jwtDecode } from 'jwt-decode';

const ProtectedRoute = ({ children }) => {
    const { isAuthenticated, loadingAuth } = useAuth();

    if (loadingAuth) {
        return <div style={{display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', fontSize: '1.5em'}}>Carregando autenticação...</div>;
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    return children;
};

export default ProtectedRoute;