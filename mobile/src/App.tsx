import React, { useEffect, useState } from 'react';
import { View, Text, ActivityIndicator, StyleSheet } from 'react-native';

export default function App() {
  const [status, setStatus] = useState('Loading...');
  const [timestamp, setTimestamp] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch('https://func-mobileapp-cs-dev.azurewebsites.net/api/health')
      .then((r) => r.json())
      .then((data) => {
        setStatus(data.status);
        setTimestamp(data.timestamp);
        setError(null);
      })
      .catch((err) => {
        setStatus('❌ API Error');
        setError(err.message);
      })
      .finally(() => setLoading(false));
  }, []);

  return (
    <View style={styles.container}>
      {loading ? (
        <ActivityIndicator size="large" color="#0000ff" />
      ) : (
        <>
          <Text style={styles.title}>Mobile App</Text>
          <Text style={styles.status}>{status}</Text>
          {timestamp && <Text style={styles.small}>{timestamp}</Text>}
          {error && <Text style={styles.error}>{error}</Text>}
        </>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#f5f5f5',
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    marginBottom: 20,
  },
  status: {
    fontSize: 24,
    fontWeight: '600',
    color: '#333',
    marginBottom: 10,
  },
  small: {
    fontSize: 12,
    color: '#666',
  },
  error: {
    fontSize: 14,
    color: '#d32f2f',
    marginTop: 10,
  },
});
