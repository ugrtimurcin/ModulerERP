import { request, type PagedResult } from '../api';

export const activitiesService = {
    getAll: (page = 1, pageSize = 20, entityId?: string, entityType?: string) => {
        let url = `/crm/activities?page=${page}&pageSize=${pageSize}`;
        if (entityId && entityType === 'Lead') url += `&leadId=${entityId}`;
        else if (entityId && entityType === 'Opportunity') url += `&opportunityId=${entityId}`;
        else if (entityId && entityType === 'BusinessPartner') url += `&partnerId=${entityId}`;
        else if (entityId) url += `&entityType=${entityType}&entityId=${entityId}`; // fallback

        return request<PagedResult<{
            id: string;
            type: number;
            subject: string;
            description: string | null;
            activityDate: string;
            leadId: string | null;
            opportunityId: string | null;
            partnerId: string | null;
            isScheduled: boolean;
            isCompleted: boolean;
            completedAt: string | null;
            createdAt: string;
        }>>(url);
    },
    create: (data: any) => request<any>('/crm/activities', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) => request<any>(`/crm/activities/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/activities/${id}`, { method: 'DELETE' }),
    complete: (id: string) => request<any>(`/crm/activities/${id}/complete`, { method: 'POST' }),
};
