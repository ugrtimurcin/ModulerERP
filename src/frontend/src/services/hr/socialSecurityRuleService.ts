import { request } from '../api';
import type { CitizenshipType, SocialSecurityType } from '../../types/hr';

export interface SocialSecurityRule {
    id: string;
    name: string;
    citizenshipType: CitizenshipType;
    socialSecurityType: SocialSecurityType;
    employeeRate: number;
    employerRate: number;
    providentFundEmployeeRate: number;
    providentFundEmployerRate: number;
    unemploymentInsuranceEmployeeRate: number;
    unemploymentInsuranceEmployerRate: number;
    effectiveFrom: string;
    effectiveTo?: string | null;
}

export interface CreateSocialSecurityRuleDto {
    name: string;
    citizenshipType: CitizenshipType;
    socialSecurityType: SocialSecurityType;
    employeeRate: number;
    employerRate: number;
    providentFundEmployeeRate: number;
    providentFundEmployerRate: number;
    unemploymentInsuranceEmployeeRate: number;
    unemploymentInsuranceEmployerRate: number;
    effectiveFrom: string;
    effectiveTo?: string | null;
}

export interface UpdateSocialSecurityRuleDto extends CreateSocialSecurityRuleDto { }

const BASE_URL = '/hr/social-security-rules';

export const socialSecurityRuleService = {
    getAll: async () => {
        const response = await request<SocialSecurityRule[]>(BASE_URL);
        return response.success && response.data ? response.data : [];
    },

    getById: async (id: string) => {
        const response = await request<SocialSecurityRule>(`${BASE_URL}/${id}`);
        return response.data;
    },

    create: async (dto: CreateSocialSecurityRuleDto) => {
        const response = await request<SocialSecurityRule>(BASE_URL, {
            method: 'POST',
            body: JSON.stringify(dto)
        });
        return response.data;
    },

    update: async (id: string, dto: UpdateSocialSecurityRuleDto) => {
        await request(`${BASE_URL}/${id}`, {
            method: 'PUT',
            body: JSON.stringify(dto)
        });
    },

    delete: async (id: string) => {
        await request(`${BASE_URL}/${id}`, { method: 'DELETE' });
    }
};
