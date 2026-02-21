import { request } from '../api';

export interface TaxRule {
    id: string;
    name: string;
    lowerLimit: number;
    upperLimit: number | null;
    rate: number;
    order: number;
    effectiveFrom: string;
    effectiveTo?: string | null;
}

export interface CreateTaxRuleDto {
    name: string;
    lowerLimit: number;
    upperLimit: number | null;
    rate: number;
    order: number;
    effectiveFrom: string;
    effectiveTo?: string | null;
}

export interface UpdateTaxRuleDto extends CreateTaxRuleDto { }

const BASE_URL = '/hr/tax-rules';

export const taxRuleService = {
    getAll: async () => {
        const response = await request<TaxRule[]>(BASE_URL);
        return response.success && response.data ? response.data : [];
    },

    getById: async (id: string) => {
        const response = await request<TaxRule>(`${BASE_URL}/${id}`);
        return response.data;
    },

    create: async (dto: CreateTaxRuleDto) => {
        const response = await request<TaxRule>(BASE_URL, {
            method: 'POST',
            body: JSON.stringify(dto)
        });
        return response.data;
    },

    update: async (id: string, dto: UpdateTaxRuleDto) => {
        await request(`${BASE_URL}/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        });
    },

    delete: async (id: string) => {
        await request(`${BASE_URL}/${id}`, { method: 'DELETE' });
    }
};
