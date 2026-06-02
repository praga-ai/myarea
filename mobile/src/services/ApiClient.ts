const BASE_URL = process.env.API_BASE_URL ?? 'https://func-mobileapp-dev.azurewebsites.net/api';

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

export const ApiClient = {
  getHealth: () => request<{ status: string }>('/health'),
};
