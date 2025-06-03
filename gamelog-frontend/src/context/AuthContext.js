import React, { createContext, useState, useEffect, useContext, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';
import { AuthService } from '../services/authService';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [loadingAuth, setLoadingAuth] = useState(true);
    const loadUserFromToken = useCallback(() => {
        const token = localStorage.getItem('token');
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                if (decodedToken.exp * 1000 > Date.now()) {
                    const storedUser = localStorage.getItem('user');
                    if (storedUser) {
                        setUser(JSON.parse(storedUser));
                    } else {
                        setUser({ id: decodedToken.sub, nomeUsuario: decodedToken.nomeUsuario || 'Usuário' });
                    }
                    setIsAuthenticated(true);
                } else {
                    AuthService.logout();
                    setUser(null);
                    setIsAuthenticated(false);
                }
            } catch (error) {
                console.error("Erro ao decodificar token ou token inválido:", error);
                AuthService.logout();
                setUser(null);
                setIsAuthenticated(false);
            }
        } else {
            setUser(null);
            setIsAuthenticated(false);
        }
        setLoadingAuth(false);
    }, []);

    useEffect(() => {
        loadUserFromToken();

        const handleStorageChange = () => {
            loadUserFromToken();
        };
        window.addEventListener('storage', handleStorageChange);

        return () => {
            window.removeEventListener('storage', handleStorageChange);
        };
    }, [loadUserFromToken]);

    const login = async (email, senha) => {
        setLoadingAuth(true);
        try {
            const response = await AuthService.login(email, senha);
            loadUserFromToken();
            return response;
        } finally {
            setLoadingAuth(false);
        }
    };

    const register = async (nick, email, senha) => {
        setLoadingAuth(true);
        try {
            const response = await AuthService.register(nick, email, senha);
            return response;
        } finally {
            setLoadingAuth(false);
        }
    };

    const logout = () => {
        AuthService.logout();
        setUser(null);
        setIsAuthenticated(false);
    };

    const value = {
        user,
        isAuthenticated,
        loadingAuth,
        login,
        register,
        logout
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};