import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Login } from './components/Login';
import { Register } from './components/Register';
import { Dashboard } from './components/Dashboard';
import { Users } from './components/Users';
import { MasterData } from './components/MasterData';
import { Unauthorized } from './components/Unauthorized';
import { NotFound } from './components/NotFound';
import './App.css';

function App() {
  return (
    <BrowserRouter basename="/survey-admin">
      <AuthProvider>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Protected Routes */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* Admin Only Routes */}
          <Route
            path="/users"
            element={
              <ProtectedRoute requiredRole="Admin">
                <Users />
              </ProtectedRoute>
            }
          />

          <Route
            path="/master-data"
            element={
              <ProtectedRoute requiredRole="Admin">
                <MasterData />
              </ProtectedRoute>
            }
          />

          {/* Error Routes */}
          <Route path="/unauthorized" element={<Unauthorized />} />
          <Route path="*" element={<NotFound />} />

          {/* Default Redirect */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
