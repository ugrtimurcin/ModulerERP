import { request, type PagedResult } from '../api';

export const contactsService = {
    getAll: (page = 1, pageSize = 20, partnerId?: string) => {
        let url = `/crm/contacts?page=${page}&pageSize=${pageSize}`;
        if (partnerId) url += `&partnerId=${partnerId}`;
        return request<PagedResult<{
            id: string;
            partnerId: string;
            firstName: string;
            lastName: string;
            fullName: string;
            position: string | null;
            email: string | null;
            phone: string | null;
            isPrimary: boolean;
            isActive: boolean;
            createdAt: string;
        }>>(url);
    },
    create: (data: any) => request<any>('/crm/contacts', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) => request<any>(`/crm/contacts/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/contacts/${id}`, { method: 'DELETE' }),
};
