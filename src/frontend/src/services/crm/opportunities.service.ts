import { request, type PagedResult } from '../api';

export const opportunitiesService = {
    getAll: (page = 1, pageSize = 20, stage?: string) => {
        let url = `/crm/opportunities?page=${page}&pageSize=${pageSize}`;
        if (stage) url += `&stage=${stage}`;
        return request<PagedResult<{
            id: string;
            title: string;
            partnerId: string | null;
            partnerName: string | null;
            estimatedValue: number;
            currencyCode: string | null;
            stage: string;
            probability: number;
            weightedValue: number;
            expectedCloseDate: string | null;
            assignedUserId: string | null;
            territoryId: string | null;
            competitorId: string | null;
            lossReasonId: string | null;
            isActive: boolean;
            createdAt: string;
        }>>(url);
    },
    create: (data: any) => request<any>('/crm/opportunities', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) => request<any>(`/crm/opportunities/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/opportunities/${id}`, { method: 'DELETE' }),
    updateStage: (id: string, stage: string) => request<any>(`/crm/opportunities/${id}/stage`, { method: 'PATCH', body: JSON.stringify(stage) }),
};
