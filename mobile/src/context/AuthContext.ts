import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';

export interface User {
  userId: number;
  email: string;
  fullName: string;
  roleName: string;
  isActive: boolean;
  lastLoginDate: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  // Check if token exists on mount
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const storedToken = await AsyncStorage.getItem('authToken');
        const storedUser = await AsyncStorage.getItem('authUser');

        if (storedToken && storedUser) {
          setToken(storedToken);
          setUser(JSON.parse(storedUser));
        }
      } catch (err) {
        console.error('Error checking auth:', err);
      } finally {
        setLoading(false);
      }
    };

    checkAuth();
  }, []);

  const login = async (email: string, password: string) => {
    // Demo surveyor credentials
    const demoUsers: { [key: string]: { password: string; user: User } } = {
      'surveyor@myarea.com': {
        password: 'Surveyor@123',
        user: {
          userId: 2,
          email: 'surveyor@myarea.com',
          fullName: 'Surveyor User',
          roleName: 'Surveyor',
          isActive: true,
          lastLoginDate: new Date().toISOString(),
        },
      },
      'surveyor@survey.com': {
        password: 'Surveyor@123',
        user: {
          userId: 2,
          email: 'surveyor@survey.com',
          fullName: 'Surveyor User',
          roleName: 'Surveyor',
          isActive: true,
          lastLoginDate: new Date().toISOString(),
        },
      },
    };

    // Try API first, fall back to demo credentials
    try {
      const response = await fetch('https://func-mobileapp-cs-in.azurewebsites.net/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        const data = await response.json();
        if (data.user.roleName !== 'Surveyor') {
          throw new Error('This app is for Surveyors only');
        }
        setToken(data.token);
        setUser(data.user);
        await AsyncStorage.setItem('authToken', data.token);
        await AsyncStorage.setItem('authUser', JSON.stringify(data.user));
        return;
      }
    } catch (apiErr) {
      console.log('API not available, using demo credentials');
    }

    // Use demo credentials
    const demoUser = demoUsers[email.toLowerCase()];
    if (demoUser && demoUser.password === password) {
      setToken('demo-token');
      setUser(demoUser.user);
      await AsyncStorage.setItem('authToken', 'demo-token');
      await AsyncStorage.setItem('authUser', JSON.stringify(demoUser.user));
      return;
    }

    throw new Error('Invalid email or password');
  };

  const logout = async () => {
    setUser(null);
    setToken(null);
    await AsyncStorage.removeItem('authToken');
    await AsyncStorage.removeItem('authUser');
  };

  return (
    <AuthContext.Provider value={{
      user,
      token,
      loading,
      login,
      logout,
      isAuthenticated: !!token,
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
