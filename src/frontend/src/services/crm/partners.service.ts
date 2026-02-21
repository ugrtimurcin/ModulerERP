import { request, type PagedResult } from '../api';

export const partnersService = {
    getAll: (page = 1, pageSize = 20, isCustomer?: boolean, isSupplier?: boolean) => {
        let url = `/crm/partners?page=${page}&pageSize=${pageSize}`;
        if (isCustomer !== undefined) url += `&isCustomer=${isCustomer}`;
        if (isSupplier !== undefined) url += `&isSupplier=${isSupplier}`;
        return request<PagedResult<{
            id: string;
            code: string;
            name: string;
            isCustomer: boolean;
            isSupplier: boolean;
            groupId: string | null;
            territoryId: string | null;
            defaultCurrencyId: string | null;
            email: string | null;
            mobilePhone: string | null;
            isActive: boolean;
            createdAt: string;
        }>>(url);
    },
    getById: (id: string) => request<any>(`/crm/partners/${id}`),
    create: (data: any) =>
        request<any>('/crm/partners', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: any) =>
        request<any>(`/crm/partners/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => request<any>(`/crm/partners/${id}`, { method: 'DELETE' }),
};
