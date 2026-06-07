import React, { useEffect, useState } from 'react';
import { View, Text, ActivityIndicator, StyleSheet } from 'react-native';

export default function App() {
  const [message, setMessage] = useState('Loading...');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch('https://func-mobileapp-cs-dev.azurewebsites.net/api/health')
      .then(r => r.json())
      .then(data => {
        setMessage(`✅ ${data.status}`);
      })
      .catch(e => {
        setMessage(`❌ ${e.message}`);
      })
      .finally(() => setLoading(false));
  }, []);

  return (
    <View style={styles.container}>
      {loading ? (
        <ActivityIndicator size="large" />
      ) : (
        <Text style={styles.text}>{message}</Text>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  text: { fontSize: 20, fontWeight: 'bold' },
});
