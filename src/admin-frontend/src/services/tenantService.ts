import api from './api';

export interface Tenant {
    id: string;
    name: string;
    subdomain: string;
    subscriptionPlan?: string;
    isActive: boolean;
    createdAt: string;
}

export interface TenantDetail extends Tenant {
    baseCurrencyId: string;
    timeZone: string;
    subscriptionExpiresAt?: string;
}

export interface CreateTenantRequest {
    name: string;
    subdomain: string;
    adminEmail: string;
    adminPassword: string;
    adminFirstName: string;
    adminLastName: string;
    baseCurrencyId: string;
    subscriptionPlan?: string;
}

export interface UpdateTenantRequest {
    name: string;
    subscriptionPlan?: string;
    subscriptionExpiresAt?: string;
    isActive?: boolean;
}

export interface PagedResult<T> {
    data: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export const tenantService = {
    getAll: async (page = 1, pageSize = 10): Promise<PagedResult<Tenant>> => {
        const response = await api.get<PagedResult<Tenant>>('/tenants', {
            params: { page, pageSize }
        });
        return response.data;
    },

    getById: async (id: string): Promise<TenantDetail> => {
        const response = await api.get<TenantDetail>(`/tenants/${id}`);
        return response.data;
    },

    create: async (data: CreateTenantRequest): Promise<TenantDetail> => {
        const response = await api.post<TenantDetail>('/tenants', data);
        return response.data;
    },

    update: async (id: string, data: UpdateTenantRequest): Promise<TenantDetail> => {
        const response = await api.put<TenantDetail>(`/tenants/${id}`, data);
        return response.data;
    }
};
