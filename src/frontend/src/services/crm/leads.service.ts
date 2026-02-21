import { request, type PagedResult } from '../api';

export const leadsService = {
    getAll: (page = 1, pageSize = 20, status?: string) => {
        let url = `/crm/leads?page=${page}&pageSize=${pageSize}`;
        if (status) url += `&status=${status}`;
        return request<PagedResult<{
            id: string;
            title: string | null;
            firstName: string;
            lastName: string;
            fullName: string;
            company: string | null;
            email: string | null;
            phone: string | null;
            status: string;
            source: string | null;
            assignedUserId: string | null;
            territoryId: string | null;
            isMarketingConsentGiven: boolean;
            consentDate: string | null;
            consentSource: string | null;
            rejectionReasonId: string | null;
            isActive: boolean;
            createdAt: string;
        }>>(url);
    },
    create: (data: any) => request<any>('/crm/leads', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) => request<any>(`/crm/leads/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/leads/${id}`, { method: 'DELETE' }),
    convert: (id: string) => request<any>(`/crm/leads/${id}/convert`, { method: 'POST' }),
};
