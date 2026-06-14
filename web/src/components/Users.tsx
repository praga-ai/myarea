import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Users.css';

interface UserItem {
  userId: number;
  email: string;
  fullName: string;
  roleName: string;
  isActive: boolean;
  lastLoginDate: string;
}

export const Users: React.FC = () => {
  const { token, logout, hasRole } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState<UserItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!hasRole('Admin')) {
      navigate('/unauthorized');
      return;
    }
    loadUsers();
  }, [token]);

  const loadUsers = async () => {
    try {
      setError('');
      const response = await fetch('https://func-mobileapp-cs-in.azurewebsites.net/api/auth/users', {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        throw new Error('Failed to load users');
      }

      const data = await response.json();
      setUsers(data.users || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const handleBack = () => {
    navigate('/dashboard');
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="users-container">
      <header className="users-header">
        <div className="header-left">
          <button onClick={handleBack} className="btn-back">
            ← Back
          </button>
          <h1>👥 User Management</h1>
        </div>
        <button onClick={handleLogout} className="btn-logout">
          Logout
        </button>
      </header>

      <div className="users-content">
        <div className="users-card">
          <div className="card-header">
            <h2>All Users</h2>
            <button onClick={loadUsers} className="btn-refresh">
              🔄 Refresh
            </button>
          </div>

          {error && <div className="error-message">{error}</div>}

          {loading ? (
            <div className="loading">Loading users...</div>
          ) : users.length === 0 ? (
            <div className="empty-state">
              <p>No users found</p>
            </div>
          ) : (
            <div className="users-table">
              <table>
                <thead>
                  <tr>
                    <th>Email</th>
                    <th>Full Name</th>
                    <th>Role</th>
                    <th>Status</th>
                    <th>Last Login</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => (
                    <tr key={user.userId} className={!user.isActive ? 'inactive' : ''}>
                      <td className="email">{user.email}</td>
                      <td>{user.fullName}</td>
                      <td>
                        <span className={`role-badge role-${user.roleName.toLowerCase()}`}>
                          {user.roleName}
                        </span>
                      </td>
                      <td>
                        <span className={`status-badge status-${user.isActive ? 'active' : 'inactive'}`}>
                          {user.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td>
                        {user.lastLoginDate && user.lastLoginDate !== '0001-01-01T00:00:00'
                          ? new Date(user.lastLoginDate).toLocaleDateString()
                          : 'Never'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <div className="users-footer">
            <p>Total Users: <strong>{users.length}</strong></p>
            <p>
              Admins: <strong>{users.filter(u => u.roleName === 'Admin').length}</strong> |
              Surveyors: <strong>{users.filter(u => u.roleName === 'Surveyor').length}</strong>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
