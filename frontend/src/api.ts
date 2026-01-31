const API_URL = 'http://localhost:5133/api';

export interface Service {
  id: number;
  name: string;
  durationMinutes: number;
  price: number;
}

export interface Appointment {
  id: number;
  appointmentDate: string;
  clientName: string;
  phoneNumber: string;
  email: string;
  status: number;
  services: Service[];
}

export interface CreateAppointment {
  appointmentDate: string;
  clientName: string;
  phoneNumber: string;
  email: string;
  serviceIds: number[];
}

let authToken: string | null = localStorage.getItem('token');

export const setAuthToken = (token: string | null) => {
  authToken = token;
  if (token) {
    localStorage.setItem('token', token);
  } else {
    localStorage.removeItem('token');
  }
};

const getHeaders = () => {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
  };
  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`;
  }
  return headers;
};

export const api = {
  async register(email: string, password: string) {
    const response = await fetch(`${API_URL}/Auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Registration failed');
    }
    return response.text();
  },

  async login(email: string, password: string): Promise<string> {
    const response = await fetch(`${API_URL}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    if (!response.ok) {
      throw new Error('Login failed');
    }
    const data = await response.json();
    return data.token;
  },

  async getServices(): Promise<Service[]> {
    const response = await fetch(`${API_URL}/Services`);
    if (!response.ok) throw new Error('Failed to fetch services');
    return response.json();
  },

  async createService(service: Omit<Service, 'id'>): Promise<Service> {
    const response = await fetch(`${API_URL}/Services`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify(service),
    });
    if (!response.ok) throw new Error('Failed to create service');
    return response.json();
  },

  async deleteService(id: number): Promise<void> {
    const response = await fetch(`${API_URL}/Services/${id}`, {
      method: 'DELETE',
      headers: getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete service');
  },

  async createAppointment(appointment: CreateAppointment): Promise<Appointment> {
    const response = await fetch(`${API_URL}/Appointments`, {
      method: 'POST',
      headers: getHeaders(),
      body: JSON.stringify(appointment),
    });
    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to create appointment');
    }
    return response.json();
  },

  async getMyAppointments(): Promise<Appointment[]> {
    const response = await fetch(`${API_URL}/Appointments/me`, {
      headers: getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch appointments');
    return response.json();
  },

  async getAllAppointments(): Promise<Appointment[]> {
    const response = await fetch(`${API_URL}/Appointments`, {
      headers: getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch appointments');
    return response.json();
  },

  async updateAppointmentStatus(id: number, status: number): Promise<void> {
    const response = await fetch(`${API_URL}/Appointments/${id}/status`, {
      method: 'PUT',
      headers: getHeaders(),
      body: JSON.stringify({ status }),
    });
    if (!response.ok) throw new Error('Failed to update appointment status');
  },

  async cancelAppointment(id: number): Promise<void> {
    const response = await fetch(`${API_URL}/Appointments/${id}/cancel`, {
      method: 'PUT',
      headers: getHeaders(),
    });
    if (!response.ok) throw new Error('Failed to cancel appointment');
  },
};
