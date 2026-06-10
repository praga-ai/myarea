import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  ScrollView,
  TouchableOpacity,
  Alert,
  FlatList,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { ApiClient, Ward, Part, Area, Street } from '../services/ApiClient';

interface Questionnaire {
  questionnaireId: number;
  questionText: string;
  questionType: string;
  displayOrder: number;
  options: QuestionnaireOption[];
}

interface QuestionnaireOption {
  optionId: number;
  optionText: string;
  optionValue: string;
}

export default function SurveyScreen() {
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  // Location dropdowns
  const [wards, setWards] = useState<Ward[]>([]);
  const [parts, setParts] = useState<Part[]>([]);
  const [areas, setAreas] = useState<Area[]>([]);
  const [streets, setStreets] = useState<Street[]>([]);

  // Selected locations
  const [selectedWardId, setSelectedWardId] = useState<number | null>(null);
  const [selectedPartId, setSelectedPartId] = useState<number | null>(null);
  const [selectedAreaId, setSelectedAreaId] = useState<number | null>(null);
  const [selectedStreetId, setSelectedStreetId] = useState<number | null>(null);

  // Questionnaires
  const [questionnaires, setQuestionnaires] = useState<Questionnaire[]>([]);
  const [responses, setResponses] = useState<{ [key: number]: string }>({});
  const [savedCount, setSavedCount] = useState(0);

  const [error, setError] = useState<string | null>(null);

  // Load initial data
  useEffect(() => {
    loadInitialData();
  }, []);

  // Load cascading dropdowns
  useEffect(() => {
    if (selectedWardId) {
      loadParts(selectedWardId);
      setSelectedPartId(null);
      setAreas([]);
      setStreets([]);
      setSelectedAreaId(null);
      setSelectedStreetId(null);
    }
  }, [selectedWardId]);

  useEffect(() => {
    if (selectedPartId) {
      loadAreas(selectedPartId);
      setSelectedAreaId(null);
      setStreets([]);
      setSelectedStreetId(null);
    }
  }, [selectedPartId]);

  useEffect(() => {
    if (selectedAreaId) {
      loadStreets(selectedAreaId);
      setSelectedStreetId(null);
    }
  }, [selectedAreaId]);

  const loadInitialData = async () => {
    try {
      setError(null);
      const wardsData = await ApiClient.getWards();
      setWards(wardsData);

      const questionnairesData = await ApiClient.getQuestionnaires();
      setQuestionnaires(questionnairesData);

      setLoading(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
      setLoading(false);
    }
  };

  const loadParts = async (wardId: number) => {
    try {
      const data = await ApiClient.getParts(wardId);
      setParts(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load parts');
    }
  };

  const loadAreas = async (partId: number) => {
    try {
      const data = await ApiClient.getAreas(partId);
      setAreas(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load areas');
    }
  };

  const loadStreets = async (areaId: number) => {
    try {
      const data = await ApiClient.getStreets(areaId);
      setStreets(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load streets');
    }
  };

  const handleResponseChange = (questionnaireId: number, value: string) => {
    setResponses((prev) => ({
      ...prev,
      [questionnaireId]: value,
    }));
  };

  const handleSaveLocally = async () => {
    if (!selectedWardId || !selectedPartId || !selectedAreaId || !selectedStreetId) {
      Alert.alert('Error', 'Please select all location fields');
      return;
    }

    if (Object.keys(responses).length === 0) {
      Alert.alert('Error', 'Please answer at least one question');
      return;
    }

    try {
      const surveyData = {
        wardId: selectedWardId,
        partId: selectedPartId,
        areaId: selectedAreaId,
        streetId: selectedStreetId,
        responses,
        timestamp: new Date().toISOString(),
      };

      // Save to local storage
      const savedSurveys = await AsyncStorage.getItem('savedSurveys');
      const surveys = savedSurveys ? JSON.parse(savedSurveys) : [];
      surveys.push(surveyData);
      await AsyncStorage.setItem('savedSurveys', JSON.stringify(surveys));

      setSavedCount((prev) => prev + 1);
      Alert.alert('Saved', 'Survey saved locally. You can submit it later.');
    } catch (err) {
      Alert.alert('Error', 'Failed to save survey locally');
    }
  };

  const handleSubmit = async () => {
    if (!selectedWardId || !selectedPartId || !selectedAreaId || !selectedStreetId) {
      Alert.alert('Error', 'Please select all location fields');
      return;
    }

    if (Object.keys(responses).length === 0) {
      Alert.alert('Error', 'Please answer at least one question');
      return;
    }

    setSubmitting(true);
    try {
      await ApiClient.createSurvey({
        wardId: selectedWardId,
        partId: selectedPartId,
        areaId: selectedAreaId,
        streetId: selectedStreetId,
        responses,
      });

      Alert.alert('Success', 'Survey submitted successfully!');
      clearForm();
    } catch (err) {
      Alert.alert('Error', err instanceof Error ? err.message : 'Failed to submit survey');
    } finally {
      setSubmitting(false);
    }
  };

  const clearForm = () => {
    setSelectedWardId(null);
    setSelectedPartId(null);
    setSelectedAreaId(null);
    setSelectedStreetId(null);
    setParts([]);
    setAreas([]);
    setStreets([]);
    setResponses({});
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
        <Text style={styles.title}>Survey Form</Text>
        {savedCount > 0 && <Text style={styles.savedBadge}>Saved: {savedCount}</Text>}
      </View>

      {error && (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      )}

      <ScrollView style={styles.scrollView} contentContainerStyle={styles.scrollContent}>
        {/* Location Selection Section */}
        <View style={styles.sectionContainer}>
          <Text style={styles.sectionTitle}>📍 Location</Text>

          <View style={styles.fieldContainer}>
            <Text style={styles.label}>Ward *</Text>
            <View style={styles.pickerContainer}>
              <Picker selectedValue={selectedWardId} onValueChange={setSelectedWardId}>
                <Picker.Item label="-- Select Ward --" value={null} />
                {wards.map((ward) => (
                  <Picker.Item key={ward.wardId} label={ward.wardName} value={ward.wardId} />
                ))}
              </Picker>
            </View>
          </View>

          <View style={styles.fieldContainer}>
            <Text style={styles.label}>Part *</Text>
            <View style={styles.pickerContainer}>
              <Picker
                selectedValue={selectedPartId}
                onValueChange={setSelectedPartId}
                enabled={!!selectedWardId && parts.length > 0}
              >
                <Picker.Item label="-- Select Part --" value={null} />
                {parts.map((part) => (
                  <Picker.Item key={part.partId} label={part.partNumber} value={part.partId} />
                ))}
              </Picker>
            </View>
          </View>

          <View style={styles.fieldContainer}>
            <Text style={styles.label}>Area *</Text>
            <View style={styles.pickerContainer}>
              <Picker
                selectedValue={selectedAreaId}
                onValueChange={setSelectedAreaId}
                enabled={!!selectedPartId && areas.length > 0}
              >
                <Picker.Item label="-- Select Area --" value={null} />
                {areas.map((area) => (
                  <Picker.Item key={area.areaId} label={area.areaName} value={area.areaId} />
                ))}
              </Picker>
            </View>
          </View>

          <View style={styles.fieldContainer}>
            <Text style={styles.label}>Street *</Text>
            <View style={styles.pickerContainer}>
              <Picker
                selectedValue={selectedStreetId}
                onValueChange={setSelectedStreetId}
                enabled={!!selectedAreaId && streets.length > 0}
              >
                <Picker.Item label="-- Select Street --" value={null} />
                {streets.map((street) => (
                  <Picker.Item
                    key={street.streetId}
                    label={street.streetName}
                    value={street.streetId}
                  />
                ))}
              </Picker>
            </View>
          </View>
        </View>

        {/* Questionnaire Section */}
        <View style={styles.sectionContainer}>
          <Text style={styles.sectionTitle}>📋 Questions ({questionnaires.length})</Text>

          {questionnaires.map((question) => (
            <View key={question.questionnaireId} style={styles.questionContainer}>
              <Text style={styles.questionText}>{question.questionText}</Text>

              {question.options.map((option) => (
                <TouchableOpacity
                  key={option.optionId}
                  style={styles.radioOption}
                  onPress={() =>
                    handleResponseChange(question.questionnaireId, option.optionValue)
                  }
                >
                  <View
                    style={[
                      styles.radioButton,
                      responses[question.questionnaireId] === option.optionValue &&
                        styles.radioButtonSelected,
                    ]}
                  >
                    {responses[question.questionnaireId] === option.optionValue && (
                      <View style={styles.radioButtonDot} />
                    )}
                  </View>
                  <Text style={styles.optionText}>{option.optionText}</Text>
                </TouchableOpacity>
              ))}
            </View>
          ))}
        </View>

        <View style={styles.spacer} />
      </ScrollView>

      {/* Fixed Buttons at Bottom */}
      <View style={styles.buttonContainer}>
        <TouchableOpacity
          style={[styles.button, styles.saveButton]}
          onPress={handleSaveLocally}
          disabled={submitting}
        >
          <Text style={styles.saveButtonText}>💾 Save Locally</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[
            styles.button,
            styles.submitButton,
            submitting && styles.submitButtonDisabled,
          ]}
          onPress={handleSubmit}
          disabled={submitting}
        >
          <Text style={styles.submitButtonText}>
            {submitting ? '⏳ Submitting...' : '✓ Submit'}
          </Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  scrollView: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: 16,
    paddingTop: 16,
  },
  header: {
    padding: 16,
    backgroundColor: '#f5f5f5',
    borderBottomWidth: 1,
    borderBottomColor: '#ddd',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#333',
  },
  savedBadge: {
    fontSize: 12,
    backgroundColor: '#4CAF50',
    color: '#fff',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 4,
  },
  errorContainer: {
    padding: 12,
    backgroundColor: '#ffe6e6',
    borderRadius: 8,
    margin: 16,
  },
  errorText: {
    color: '#d32f2f',
    fontSize: 14,
  },
  sectionContainer: {
    marginBottom: 24,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 16,
    color: '#333',
  },
  fieldContainer: {
    marginBottom: 16,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: '#333',
    marginBottom: 8,
  },
  pickerContainer: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 8,
    backgroundColor: '#f9f9f9',
  },
  picker: {
    height: 50,
  },
  questionContainer: {
    marginBottom: 20,
    paddingBottom: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  questionText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#333',
    marginBottom: 12,
  },
  radioOption: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
    paddingVertical: 8,
  },
  radioButton: {
    width: 20,
    height: 20,
    borderRadius: 10,
    borderWidth: 2,
    borderColor: '#2196F3',
    marginRight: 12,
    justifyContent: 'center',
    alignItems: 'center',
  },
  radioButtonSelected: {
    backgroundColor: '#2196F3',
  },
  radioButtonDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: '#fff',
  },
  optionText: {
    fontSize: 13,
    color: '#555',
    flex: 1,
  },
  spacer: {
    height: 100,
  },
  buttonContainer: {
    flexDirection: 'row',
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 10,
    borderTopWidth: 1,
    borderTopColor: '#eee',
    backgroundColor: '#fff',
  },
  button: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: 'center',
  },
  saveButton: {
    backgroundColor: '#FF9800',
  },
  saveButtonText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
  submitButton: {
    backgroundColor: '#4CAF50',
  },
  submitButtonDisabled: {
    backgroundColor: '#ccc',
  },
  submitButtonText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
});
