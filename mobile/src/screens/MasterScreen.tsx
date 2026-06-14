import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  FlatList,
  RefreshControl,
  SectionList,
} from 'react-native';
import { ApiClient, Ward, Part, Area, Street } from '../services/ApiClient';

export default function MasterScreen() {
  const [wards, setWards] = useState<Ward[]>([]);
  const [parts, setParts] = useState<Part[]>([]);
  const [areas, setAreas] = useState<Area[]>([]);
  const [streets, setStreets] = useState<Street[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadMasterData = async () => {
    try {
      setError(null);
      const [wardsData, partsData, areasData, streetsData] = await Promise.all([
        ApiClient.getWards(),
        ApiClient.getParts(1).catch(() => []), // Get parts for ward 1 as sample
        ApiClient.getAreas(1).catch(() => []), // Get areas for part 1 as sample
        ApiClient.getStreets(1).catch(() => []), // Get streets for area 1 as sample
      ]);
      setWards(wardsData);
      setParts(partsData);
      setAreas(areasData);
      setStreets(streetsData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load master data');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadMasterData();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    loadMasterData();
  };

  if (loading) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  const sections = [
    {
      title: `Wards (${wards.length})`,
      data: wards,
      renderItem: (item: Ward) => (
        <View style={styles.itemContainer}>
          <Text style={styles.itemTitle}>{item.wardName}</Text>
          <Text style={styles.itemSubtext}>ID: {item.wardId}</Text>
        </View>
      ),
    },
    {
      title: `Parts (${parts.length})`,
      data: parts,
      renderItem: (item: Part) => (
        <View style={styles.itemContainer}>
          <Text style={styles.itemTitle}>{item.partNumber}</Text>
          <Text style={styles.itemSubtext}>Ward ID: {item.wardId}</Text>
        </View>
      ),
    },
    {
      title: `Areas (${areas.length})`,
      data: areas,
      renderItem: (item: Area) => (
        <View style={styles.itemContainer}>
          <Text style={styles.itemTitle}>{item.areaName}</Text>
          <Text style={styles.itemSubtext}>Part ID: {item.partId}</Text>
        </View>
      ),
    },
    {
      title: `Streets (${streets.length})`,
      data: streets,
      renderItem: (item: Street) => (
        <View style={styles.itemContainer}>
          <Text style={styles.itemTitle}>{item.streetName}</Text>
          <Text style={styles.itemSubtext}>Area ID: {item.areaId}</Text>
        </View>
      ),
    },
  ];

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Master Data</Text>
      </View>

      {error && (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      )}

      <SectionList
        sections={sections}
        keyExtractor={(item: any, index) => item.id?.toString() ?? index.toString()}
        renderItem={({ item, section }: { item: any; section: any }) =>
          section.renderItem(item)
        }
        renderSectionHeader={({ section: { title } }) => (
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>{title}</Text>
          </View>
        )}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        contentContainerStyle={styles.listContainer}
      />
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
    color: '#333',
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
  listContainer: {
    paddingHorizontal: 16,
  },
  sectionHeader: {
    paddingVertical: 12,
    paddingHorizontal: 8,
    backgroundColor: '#2196F3',
    marginTop: 16,
    marginBottom: 8,
    borderRadius: 6,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#fff',
  },
  itemContainer: {
    paddingVertical: 12,
    paddingHorizontal: 12,
    backgroundColor: '#f9f9f9',
    marginBottom: 8,
    borderRadius: 6,
    borderLeftWidth: 3,
    borderLeftColor: '#2196F3',
  },
  itemTitle: {
    fontSize: 14,
    fontWeight: '500',
    color: '#333',
    marginBottom: 4,
  },
  itemSubtext: {
    fontSize: 12,
    color: '#999',
  },
});
