import React from 'react';
import { useNavigate } from 'react-router-dom';
import './ErrorPages.css';

export const Unauthorized: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="error-container">
      <div className="error-card">
        <h1 className="error-code">🔐 403</h1>
        <h2>Unauthorized Access</h2>
        <p>You don't have permission to access this page.</p>
        <p className="error-subtitle">Please contact your administrator if you need access to this resource.</p>
        <button
          onClick={() => navigate('/dashboard')}
          className="btn-error"
        >
          Go to Dashboard
        </button>
      </div>
    </div>
  );
};
