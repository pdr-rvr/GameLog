import React, { createContext, useState, useEffect, useContext, useCallback, useMemo } from "react";
import { jwtDecode } from "jwt-decode";
import { AuthService } from "../services/authService";
import { getAccessToken, setAccessToken } from "../services/api";

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [loadingAuth, setLoadingAuth] = useState(true);

    const buildUserFromTokenAndData = useCallback((token, userData = null) => {
        if (!token) return null;
        try {
            const decoded = jwtDecode(token);
            if (decoded.exp * 1000 <= Date.now()) {
                return null;
            }

            const rawId = decoded.sub || 
                          decoded.nameid || 
                          decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
                          userData?.usuarioId || 
                          userData?.id;

            return {
                id: rawId ? String(rawId) : null,
                nomeUsuario: userData?.nomeUsuario || decoded.nomeUsuario || "Usuário",
                email: userData?.email || decoded.email || "",
                fotoDePerfil: userData?.fotoDePerfil || "",
            };
        } catch {
            return null;
        }
    }, []);

    // Carregamento inicial com Silent Refresh via HttpOnly cookie
    const initAuth = useCallback(async () => {
        setLoadingAuth(true);
        try {
            // Se já tiver token em memória válido (ex: testes)
            const currentToken = getAccessToken();
            if (currentToken) {
                const storedUser = AuthService.getCurrentUser();
                const userObj = buildUserFromTokenAndData(currentToken, storedUser);
                if (userObj) {
                    setUser(userObj);
                    setIsAuthenticated(true);
                    setLoadingAuth(false);
                    return;
                }
            }

            // Tenta renovação silenciosa via cookie seguro
            const res = await AuthService.refreshToken();
            if (res?.token) {
                setAccessToken(res.token);
                const userObj = buildUserFromTokenAndData(res.token, res.usuario);
                if (userObj) {
                    setUser(userObj);
                    setIsAuthenticated(true);
                } else {
                    setUser(null);
                    setIsAuthenticated(false);
                }
            } else {
                setUser(null);
                setIsAuthenticated(false);
            }
        } catch {
            setUser(null);
            setIsAuthenticated(false);
        } finally {
            setLoadingAuth(false);
        }
    }, [buildUserFromTokenAndData]);

    const loadUserFromToken = useCallback(async () => {
        await initAuth();
    }, [initAuth]);

    useEffect(() => {
        initAuth();

        const handleUnauthorized = () => {
            setUser(null);
            setIsAuthenticated(false);
            setAccessToken(null);
        };

        window.addEventListener("gamelog-unauthorized", handleUnauthorized);

        return () => {
            window.removeEventListener("gamelog-unauthorized", handleUnauthorized);
        };
    }, [initAuth]);

    const login = useCallback(async (email, senha) => {
        setLoadingAuth(true);
        try {
            const response = await AuthService.login(email, senha);
            if (response?.token) {
                setAccessToken(response.token);
                const userObj = buildUserFromTokenAndData(response.token, response.usuario);
                setUser(userObj);
                setIsAuthenticated(true);
            }
            return response;
        } finally {
            setLoadingAuth(false);
        }
    }, [buildUserFromTokenAndData]);

    const register = useCallback(async (nick, email, senha) => {
        setLoadingAuth(true);
        try {
            const response = await AuthService.register(nick, email, senha);
            return response;
        } finally {
            setLoadingAuth(false);
        }
    }, []);

    const logout = useCallback(async () => {
        await AuthService.logout();
        setUser(null);
        setIsAuthenticated(false);
    }, []);

    const value = useMemo(() => ({
        user,
        isAuthenticated,
        loadingAuth,
        token: getAccessToken(),
        login,
        register,
        logout,
        loadUserFromToken
    }), [user, isAuthenticated, loadingAuth, login, register, logout, loadUserFromToken]);

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
};
