import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ActivityIndicator, FlatList, RefreshControl } from 'react-native';
import { ApiClient, Survey } from '../services/ApiClient';

export default function DashboardScreen() {
  const [surveys, setSurveys] = useState<Survey[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadSurveys = async () => {
    try {
      setError(null);
      const data = await ApiClient.getSurveys();
      setSurveys(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load surveys');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadSurveys();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    loadSurveys();
  };

  const parseResponses = (surveyData: string) => {
    try {
      return JSON.parse(surveyData);
    } catch {
      return {};
    }
  };

  if (loading) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Dashboard</Text>
        <Text style={styles.subtitle}>Total Surveys: {surveys.length}</Text>
      </View>

      {error && (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      )}

      {surveys.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyText}>No surveys yet</Text>
        </View>
      ) : (
        <FlatList
          data={surveys}
          keyExtractor={(item) => item.surveyId.toString()}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
          renderItem={({ item }) => {
            const responses = parseResponses(item.surveyData);
            const responseCount = Object.keys(responses).length;

            return (
              <View style={styles.surveyCard}>
                <Text style={styles.cardText}>📋 Survey ID: {item.surveyId}</Text>
                <Text style={styles.cardText}>📍 Ward ID: {item.wardId} | Part: {item.partId}</Text>
                <Text style={styles.cardText}>Area: {item.areaId} | Street: {item.streetId}</Text>
                <Text style={styles.cardText}>✓ Responses: {responseCount} question(s)</Text>
                <Text style={styles.cardDate}>
                  📅 {new Date(item.createdDate).toLocaleDateString()}{' '}
                  {new Date(item.createdDate).toLocaleTimeString()}
                </Text>
              </View>
            );
          }}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  header: {
    padding: 20,
    backgroundColor: '#f5f5f5',
    borderBottomWidth: 1,
    borderBottomColor: '#ddd',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: '#666',
  },
  errorContainer: {
    padding: 16,
    backgroundColor: '#ffe6e6',
    borderRadius: 8,
    margin: 16,
  },
  errorText: {
    color: '#d32f2f',
    fontSize: 14,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 16,
    color: '#999',
  },
  surveyCard: {
    padding: 16,
    marginHorizontal: 16,
    marginVertical: 8,
    backgroundColor: '#f9f9f9',
    borderRadius: 8,
    borderLeftWidth: 4,
    borderLeftColor: '#2196F3',
  },
  cardText: {
    fontSize: 14,
    marginBottom: 4,
    color: '#333',
  },
  cardDate: {
    fontSize: 12,
    color: '#999',
    marginTop: 8,
  },
});
