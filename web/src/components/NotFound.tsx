import React from 'react';
import { useNavigate } from 'react-router-dom';
import './ErrorPages.css';

export const NotFound: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="error-container">
      <div className="error-card">
        <h1 className="error-code">⚠️ 404</h1>
        <h2>Page Not Found</h2>
        <p>The page you're looking for doesn't exist.</p>
        <p className="error-subtitle">Please check the URL or navigate using the menu.</p>
        <button
          onClick={() => navigate('/')}
          className="btn-error"
        >
          Go Home
        </button>
      </div>
    </div>
  );
};
