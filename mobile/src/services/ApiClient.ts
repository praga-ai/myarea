const BASE_URL = 'https://func-mobileapp-cs-in.azurewebsites.net/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    throw new Error(`HTTP ${res.status}: ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

export interface Ward {
  wardId: number;
  wardName: string;
  createdDate: string;
}

export interface Part {
  partId: number;
  partNumber: string;
  wardId: number;
  createdDate: string;
}

export interface Area {
  areaId: number;
  areaName: string;
  partId: number;
  createdDate: string;
}

export interface Street {
  streetId: number;
  streetName: string;
  areaId: number;
  createdDate: string;
}

export interface QuestionnaireOption {
  optionId: number;
  optionText: string;
  optionValue: string;
}

export interface Questionnaire {
  questionnaireId: number;
  questionText: string;
  questionType: string;
  displayOrder: number;
  options: QuestionnaireOption[];
}

export interface Survey {
  surveyId: number;
  wardId: number;
  partId: number;
  areaId: number;
  streetId: number;
  surveyData: string; // JSON format
  createdDate: string;
}

export interface CreateSurveyRequest {
  wardId: number;
  partId: number;
  areaId: number;
  streetId: number;
  responses: { [key: number]: string }; // QuestionnaireId -> SelectedValue
}

export const ApiClient = {
  getHealth: () => request<{ status: string }>('/health'),

  // Location endpoints
  getWards: () => request<Ward[]>('/wards'),
  getParts: (wardId: number) => request<Part[]>(`/parts/${wardId}`),
  getAreas: (partId: number) => request<Area[]>(`/areas/${partId}`),
  getStreets: (areaId: number) => request<Street[]>(`/streets/${areaId}`),

  // Questionnaire endpoints
  getQuestionnaires: () => request<Questionnaire[]>('/questionnaires'),

  // Survey endpoints
  getSurveys: () => request<Survey[]>('/surveys'),
  createSurvey: (data: CreateSurveyRequest) =>
    request<{ surveyId: number; message: string }>('/survey', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
};