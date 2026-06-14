import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Dashboard.css';

interface Survey {
  surveyId: number;
  wardId: number;
  partId: number;
  areaId: number;
  streetId: number;
  surveyData: string;
  createdDate: string;
}

export const Dashboard: React.FC = () => {
  const { user, logout, hasRole, token } = useAuth();
  const navigate = useNavigate();
  const [surveys, setSurveys] = useState<Survey[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadSurveys();
  }, [token]);

  const loadSurveys = async () => {
    try {
      setError('');
      const response = await fetch('https://func-mobileapp-cs-in.azurewebsites.net/api/surveys', {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to load surveys');
      }

      const data = await response.json();
      setSurveys(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load surveys');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const parseResponses = (surveyData: string) => {
    try {
      return JSON.parse(surveyData);
    } catch {
      return {};
    }
  };

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-left">
          <h1>📊 Survey Admin Dashboard</h1>
          <p className="welcome-text">Welcome, <strong>{user?.fullName}</strong> ({user?.roleName})</p>
        </div>
        <div className="header-right">
          <button onClick={handleLogout} className="btn-logout">
            Logout
          </button>
        </div>
      </header>

      <div className="dashboard-content">
        <div className="stats-grid">
          <div className="stat-card">
            <div className="stat-icon">📋</div>
            <div className="stat-info">
              <h3>Total Surveys</h3>
              <p className="stat-value">{surveys.length}</p>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon">👤</div>
            <div className="stat-info">
              <h3>Role</h3>
              <p className="stat-value">{user?.roleName}</p>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon">📅</div>
            <div className="stat-info">
              <h3>Last Login</h3>
              <p className="stat-value">
                {user?.lastLoginDate ? new Date(user.lastLoginDate).toLocaleDateString() : 'N/A'}
              </p>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon">✓</div>
            <div className="stat-info">
              <h3>Status</h3>
              <p className="stat-value">{user?.isActive ? 'Active' : 'Inactive'}</p>
            </div>
          </div>
        </div>

        {hasRole('Admin') && (
          <div className="admin-section">
            <h2>🔐 Admin Functions</h2>
            <div className="admin-buttons">
              <button
                onClick={() => navigate('/users')}
                className="btn-admin"
              >
                👥 Manage Users
              </button>
              <button
                onClick={() => navigate('/master-data')}
                className="btn-admin"
              >
                ⚙️ Master Data
              </button>
            </div>
          </div>
        )}

        <div className="surveys-section">
          <div className="section-header">
            <h2>📋 Recent Surveys</h2>
            <button onClick={loadSurveys} className="btn-refresh">
              🔄 Refresh
            </button>
          </div>

          {error && <div className="error-message">{error}</div>}

          {loading ? (
            <div className="loading">Loading surveys...</div>
          ) : surveys.length === 0 ? (
            <div className="empty-state">
              <p>No surveys yet</p>
            </div>
          ) : (
            <div className="surveys-table">
              <table>
                <thead>
                  <tr>
                    <th>Survey ID</th>
                    <th>Location</th>
                    <th>Responses</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {surveys.map((survey) => {
                    const responses = parseResponses(survey.surveyData);
                    return (
                      <tr key={survey.surveyId}>
                        <td>#{survey.surveyId}</td>
                        <td>
                          Ward {survey.wardId} | Part {survey.partId} | Area {survey.areaId} | Street {survey.streetId}
                        </td>
                        <td>
                          <span className="badge">{Object.keys(responses).length} questions</span>
                        </td>
                        <td>{new Date(survey.createdDate).toLocaleDateString()}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
