import { request } from '../api';
import type { Employee, CreateEmployeeDto, UpdateEmployeeDto } from '../../types/hr';

const BASE_URL = '/hr/employees';

export const employeeService = {
    getAll: async () => {
        const response = await request<Employee[]>(BASE_URL);
        return response.data || [];
    },

    getById: async (id: string) => {
        const response = await request<Employee>(`${BASE_URL}/${id}`);
        return response.data;
    },

    create: async (dto: CreateEmployeeDto) => {
        const response = await request<Employee>(BASE_URL, {
            method: 'POST',
            body: JSON.stringify(dto)
        });
        return response.data;
    },

    update: async (id: string, dto: UpdateEmployeeDto) => {
        await request(`${BASE_URL}/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        });
    },

    generateQr: async (id: string) => {
        const response = await request<{ token: string }>(`${BASE_URL}/${id}/generate-qr`, {
            method: 'PUT'
        });
        return response.data?.token;
    },

    getLookup: async () => {
        const response = await request<{ id: string, firstName: string, lastName: string, position: string }[]>(`${BASE_URL}/lookup`);
        return response || [];
    }
};
