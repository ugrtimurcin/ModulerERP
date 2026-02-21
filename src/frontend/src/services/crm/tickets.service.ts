import { request, type PagedResult } from '../api';

export const ticketsService = {
    getAll: (page = 1, pageSize = 20, status?: number, priority?: number) => {
        let url = `/crm/tickets?page=${page}&pageSize=${pageSize}`;
        if (status !== undefined) url += `&status=${status}`;
        if (priority !== undefined) url += `&priority=${priority}`;
        return request<PagedResult<{
            id: string;
            title: string;
            priority: number;
            priorityName: string;
            status: number;
            statusName: string;
            partnerId: string | null;
            partnerName: string | null;
            assignedUserId: string | null;
            createdAt: string;
        }>>(url);
    },
    getById: (id: string) => request<{
        id: string;
        title: string;
        description: string;
        priority: number;
        priorityName: string;
        status: number;
        statusName: string;
        partnerId: string | null;
        partnerName: string | null;
        assignedUserId: string | null;
        resolution: string | null;
        resolvedAt: string | null;
        closedAt: string | null;
        createdAt: string;
    }>(`/crm/tickets/${id}`),
    create: (data: { title: string; description: string; priority?: number; partnerId?: string; assignedUserId?: string }) =>
        request<any>('/crm/tickets', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: { title: string; description: string; priority: number; assignedUserId?: string }) =>
        request<any>(`/crm/tickets/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/tickets/${id}`, { method: 'DELETE' }),
    // Lifecycle actions
    resolve: (id: string, resolution: string) =>
        request<any>(`/crm/tickets/${id}/resolve`, { method: 'POST', body: JSON.stringify({ resolution }) }),
    close: (id: string) => request<any>(`/crm/tickets/${id}/close`, { method: 'POST' }),
    reopen: (id: string) => request<any>(`/crm/tickets/${id}/reopen`, { method: 'POST' }),
    // Messages (nested)
    getMessages: (id: string, includeInternal = true) =>
        request<any[]>(`/crm/tickets/${id}/messages?includeInternal=${includeInternal}`),
    addMessage: (id: string, data: { message: string; isInternal?: boolean }) =>
        request<any>(`/crm/tickets/${id}/messages`, { method: 'POST', body: JSON.stringify(data) }),
};
