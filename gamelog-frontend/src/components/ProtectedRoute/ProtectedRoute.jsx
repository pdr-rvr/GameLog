import React from 'react';
import { Navigate } from 'react-router-dom';
import { jwtDecode } from 'jwt-decode';
import './ProtectedRoute.css';

const ProtectedRoute = ({ children }) => {
  const token = localStorage.getItem('token');
  
  try {
    if (!token) throw new Error('No token');
    
    const decoded = jwtDecode(token);
    if (Date.now() >= decoded.exp * 1000) {
      throw new Error('Token expired');
    }
    
    return children;
  } catch (error) {
    return <Navigate to="/login" replace />;
  }
};

export default ProtectedRoute;