import { request } from '../api';

export const tagsService = {
    getAll: (entityType?: string) => {
        let url = '/crm/tags';
        if (entityType) url += `?entityType=${entityType}`;
        return request<{
            id: string;
            name: string;
            colorCode: string;
            entityType: string | null;
        }[]>(url);
    },
    create: (data: { name: string; colorCode?: string; entityType?: string }) =>
        request<any>('/crm/tags', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: { name: string; colorCode: string; entityType?: string }) =>
        request<any>(`/crm/tags/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/tags/${id}`, { method: 'DELETE' }),
};

export const saleAgentsService = {
    create: (data: any) => request<any>('/crm/sale-agents', { method: 'POST', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/sale-agents/${id}`, { method: 'DELETE' }),
};

export const partnerGroupsService = {
    getAll: () => request<any>('/crm/partner-groups'),
    create: (data: any) => request<any>('/crm/partner-groups', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) => request<any>(`/crm/partner-groups/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/partner-groups/${id}`, { method: 'DELETE' }),
};

export const territoriesService = {
    getAll: () => request<any>('/crm/territories'),
};

export const competitorsService = {
    getAll: () => request<any>('/crm/competitors'),
};

export const lossReasonsService = {
    getAll: () => request<any>('/crm/loss-reasons'),
};

export const rejectionReasonsService = {
    getAll: () => request<any>('/crm/rejection-reasons'),
};
