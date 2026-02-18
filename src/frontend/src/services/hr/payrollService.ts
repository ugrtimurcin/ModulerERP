import { request } from '../api';

// Types (could be moved to types/hr.ts later)
export interface PayrollRun {
    id: string;
    period: string; // "YYYY-MM"
    description: string;
    status: number;
    totalAmount: number;
    currencyCode: string; // "TRY", "USD", etc.
    entryCount: number;
    createdAt: string;
}

export interface PayrollEntry {
    id: string;
    employeeId: string;
    employeeName: string;
    baseSalary: number;
    overtimePay: number;
    commissionPay: number;
    bonusPay: number;
    transportAmount: number;
    advanceDeduction: number;
    socialSecurityEmployee: number;
    providentFundEmployee: number;
    unemploymentInsuranceEmployee: number;
    incomeTax: number;
    netPayable: number;
}

export interface RunPayrollDto {
    year: number;
    month: number;
    currencyId?: string;
}

export interface PayrollSummary {
    year: number;
    month: number;
    totalGross: number;
    totalDeductions: number;
    totalNet: number;
    employeeCount: number;
    currencyCode: string;
}

const BASE_URL = '/hr/payroll';

export const payrollService = {
    getByYear: async (year: number) => {
        const response = await request<PayrollRun[]>(`${BASE_URL}?year=${year}`);
        return response.data || [];
    },

    getById: async (id: string) => {
        const response = await request<PayrollRun>(`${BASE_URL}/${id}`);
        return response.data;
    },

    getSlips: async (id: string) => {
        const response = await request<PayrollEntry[]>(`${BASE_URL}/${id}/slips`);
        return response.data || [];
    },

    runPayroll: async (dto: RunPayrollDto) => {
        const response = await request<PayrollRun>(`${BASE_URL}/run`, {
            method: 'POST',
            body: JSON.stringify(dto)
        });
        return response.data;
    },

    getSummary: async (year: number, month: number) => {
        const response = await request<PayrollSummary>(`${BASE_URL}/summary?year=${year}&month=${month}`);
        return response.data;
    }
};
