const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5056/api';

import { leadsService } from './crm/leads.service';
import { partnersService } from './crm/partners.service';
import { opportunitiesService } from './crm/opportunities.service';
import { contactsService } from './crm/contacts.service';
import { activitiesService } from './crm/activities.service';
import { ticketsService } from './crm/tickets.service';
import { tagsService, saleAgentsService, partnerGroupsService, territoriesService, competitorsService, lossReasonsService, rejectionReasonsService } from './crm/auxiliary.service';


export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    error?: string;
}

export interface PagedResult<T> {
    data: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export async function request<T>(endpoint: string, options?: RequestInit): Promise<ApiResponse<T>> {
    try {
        const token = localStorage.getItem('accessToken');
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
                ...options?.headers,
            },
        });

        if (response.status === 401) {
            // Try to refresh token
            const refreshToken = localStorage.getItem('refreshToken');
            if (refreshToken) {
                const refreshResult = await refreshAccessToken(refreshToken);
                if (refreshResult) {
                    // Retry original request with new token
                    return request(endpoint, options);
                }
            }
            // Clear auth state
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            return { success: false, error: 'Session expired' };
        }

        if (!response.ok) {
            const error = await response.json().catch(() => ({ error: 'Request failed' }));
            return { success: false, error: error.error || `HTTP ${response.status}` };
        }

        const data = await response.json();
        return data;
    } catch (error) {
        return { success: false, error: (error as Error).message };
    }
}

async function refreshAccessToken(refreshToken: string): Promise<boolean> {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken }),
        });

        if (response.ok) {
            const data = await response.json();
            if (data.success && data.data) {
                localStorage.setItem('accessToken', data.data.accessToken);
                localStorage.setItem('refreshToken', data.data.refreshToken);
                return true;
            }
        }
        return false;
    } catch {
        return false;
    }
}

interface LoginResult {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    user: {
        id: string;
        email: string;
        firstName: string;
        lastName: string;
        tenantId: string;
        roles: string[];
    };
}

interface UserInfo {
    id: string;
    email: string;
    tenantId: string;
    roles: string[];
}

export const api = {
    // Auth
    auth: {
        login: (email: string, password: string) =>
            request<LoginResult>('/auth/login', {
                method: 'POST',
                body: JSON.stringify({ email, password }),
            }),
        refresh: (refreshToken: string) =>
            request<LoginResult>('/auth/refresh', {
                method: 'POST',
                body: JSON.stringify({ refreshToken }),
            }),
        logout: (refreshToken: string) =>
            request<void>('/auth/logout', {
                method: 'POST',
                body: JSON.stringify({ refreshToken }),
            }),
        me: () => request<UserInfo>('/auth/me'),
    },

    // System
    health: () => request<{ status: string; database: string; timestamp: string }>('/health'),
    info: () => request<{ name: string; version: string; environment: string; runtime: string }>('/info'),
    summary: () => request<{ currencies: number; languages: number; users: number; roles: number }>('/summary'),
    permissions: () => request<string[]>('/system/permissions'),

    // Currencies
    getCurrencies: () =>
        request<Array<{ id: string; code: string; name: string; symbol: string; precision: number; isActive: boolean }>>(
            '/currencies'
        ),
    getActiveCurrencies: () =>
        request<Array<{ id: string; code: string; name: string; symbol: string }>>('/currencies/active'),

    // Languages
    getLanguages: () =>
        request<Array<{ id: string; code: string; name: string; isRtl: boolean; isActive: boolean }>>('/languages'),
    getActiveLanguages: () =>
        request<Array<{ id: string; code: string; name: string; isRtl: boolean }>>('/languages/active'),

    // Users
    users: {
        getAll: (page = 1, pageSize = 20) =>
            request<PagedResult<{
                id: string;
                email: string;
                firstName: string;
                lastName: string;
                isActive: boolean;
                createdAt: string;
                lastLoginDate: string | null;
            }>>(`/users?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/users/${id}`),
        create: (data: { email: string; password: string; firstName: string; lastName: string }) =>
            request<any>('/users', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: { firstName: string; lastName: string; isActive?: boolean }) =>
            request<any>(`/users/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/users/${id}`, { method: 'DELETE' }),
    },

    // Roles
    roles: {
        getAll: () => request<Array<{ id: string; name: string; description: string | null; isSystemRole: boolean; permissionCount: number }>>('/roles'),
        getById: (id: string) => request<any>(`/roles/${id}`),
        create: (data: { name: string; description?: string }) =>
            request<any>('/roles', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: { name: string; description?: string; permissions?: string[] }) =>
            request<any>(`/roles/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/roles/${id}`, { method: 'DELETE' }),
    },

    // CRM Module Services
    leads: leadsService,
    partners: partnersService,
    opportunities: opportunitiesService,
    contacts: contactsService,
    activities: activitiesService,
    tickets: ticketsService,
    tags: tagsService,
    saleAgents: saleAgentsService,
    partnerGroups: partnerGroupsService,
    territories: territoriesService,
    competitors: competitorsService,
    lossReasons: lossReasonsService,
    rejectionReasons: rejectionReasonsService,

    // Inventory - Product Categories
    productCategories: {
        getAll: () => request<Array<{
            id: string;
            name: string;
            description: string | null;
            parentCategoryId: string | null;
            parentCategoryName: string | null;
            sortOrder: number;
            isActive: boolean;
        }>>('/product-categories'),
        getById: (id: string) => request<any>(`/product-categories/${id}`),
        create: (data: any) => request<any>('/product-categories', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/product-categories/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/product-categories/${id}`, { method: 'DELETE' }),
    },

    // Inventory - Warehouses
    warehouses: {
        getAll: () => request<Array<{
            id: string;
            code: string;
            name: string;
            description: string | null;
            isDefault: boolean;
            branchId: string | null;
            address: string | null;
            isActive: boolean;
        }>>('/warehouses'),
        getById: (id: string) => request<any>(`/warehouses/${id}`),
        create: (data: any) => request<any>('/warehouses', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/warehouses/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/warehouses/${id}`, { method: 'DELETE' }),
        setDefault: (id: string) => request<any>(`/warehouses/${id}/set-default`, { method: 'POST' }),
    },

    // Inventory - Units of Measure
    unitOfMeasures: {
        getAll: () => request<Array<{
            id: string;
            code: string;
            name: string;
            type: number;
            baseUnitId: string | null;
            baseUnitCode: string | null;
            conversionFactor: number;
            isActive: boolean;
        }>>('/unit-of-measures'),
        getById: (id: string) => request<any>(`/unit-of-measures/${id}`),
        create: (data: any) => request<any>('/unit-of-measures', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/unit-of-measures/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/unit-of-measures/${id}`, { method: 'DELETE' }),
        getByType: (type: number) => request<any>(`/unit-of-measures/by-type/${type}`),
    },

    // Inventory - Products
    products: {
        getAll: () => request<Array<{
            id: string;
            sku: string;
            name: string;
            type: number;
            unitOfMeasureId: string;
            unitOfMeasureCode: string;
            categoryId: string | null;
            categoryName: string | null;
            salesPrice: number;
            purchasePrice: number;
            minStockLevel: number;
            reorderLevel: number;
            isActive: boolean;
            stockLevels: any[];
        }>>('/products'),
        getById: (id: string) => request<any>(`/products/${id}`),
        create: (data: any) => request<any>('/products', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/products/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/products/${id}`, { method: 'DELETE' }),

        // Barcodes
        addBarcode: (id: string, data: any) => request<any>(`/products/${id}/barcodes`, { method: 'POST', body: JSON.stringify(data) }),
        removeBarcode: (id: string, barcodeId: string) => request<any>(`/products/${id}/barcodes/${barcodeId}`, { method: 'DELETE' }),
        setPrimaryBarcode: (id: string, barcodeId: string) => request<any>(`/products/${id}/barcodes/${barcodeId}/primary`, { method: 'PUT' }),

        // Prices
        addPrice: (id: string, data: any) => request<any>(`/products/${id}/prices`, { method: 'POST', body: JSON.stringify(data) }),
        removePrice: (id: string, priceId: string) => request<any>(`/products/${id}/prices/${priceId}`, { method: 'DELETE' }),
    },
    productVariants: {
        getByProductId: (productId: string) => request<any>(`/products/${productId}/variants`),
        getById: (id: string) => request<any>(`/variants/${id}`),
        create: (data: any) => request<any>('/variants', {
            method: 'POST',
            body: JSON.stringify(data)
        }),
        update: (id: string, data: any) => request<any>(`/variants/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data)
        }),
        delete: (id: string) => request<any>(`/variants/${id}`, { method: 'DELETE' }),
    },

    // Inventory - Stock Operations
    stock: {
        createMovement: (data: any) => request<any>('/inventory/movements', { method: 'POST', body: JSON.stringify(data) }),
        createTransfer: (data: any) => request<any>('/inventory/transfers', { method: 'POST', body: JSON.stringify(data) }),
        getLevels: (warehouseId?: string, productId?: string) => {
            let url = '/inventory/levels?';
            if (warehouseId) url += `warehouseId=${warehouseId}&`;
            if (productId) url += `productId=${productId}`;
            return request<any[]>(url);
        },
        getMovements: (params: { warehouseId?: string; productId?: string; fromDate?: string; toDate?: string }) => {
            let url = '/inventory/movements?';
            if (params.warehouseId) url += `warehouseId=${params.warehouseId}&`;
            if (params.productId) url += `productId=${params.productId}&`;
            // Endpoint might be /inventory/products/{id}/movements or just /inventory/movements. 
            // InventoryController likely has GetMovements.
            // Let's assume /inventory/movements based on Controller pattern.
            if (params.fromDate) url += `fromDate=${params.fromDate}&`;
            if (params.toDate) url += `toDate=${params.toDate}`;
            return request<any[]>(url);
        },
    },

    // Sales - Quotes
    quotes: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/sales/quotes?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/sales/quotes/${id}`),
        create: (data: any) => request<any>('/sales/quotes', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/sales/quotes/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/sales/quotes/${id}`, { method: 'DELETE' }),
        send: (id: string) => request<any>(`/sales/quotes/${id}/send`, { method: 'POST' }),
        accept: (id: string) => request<any>(`/sales/quotes/${id}/accept`, { method: 'POST' }),
        reject: (id: string) => request<any>(`/sales/quotes/${id}/reject`, { method: 'POST' }),
    },

    // Sales - Orders
    orders: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/sales/orders?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/sales/orders/${id}`),
        create: (data: any) => request<any>('/sales/orders', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) => request<any>(`/sales/orders/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/sales/orders/${id}`, { method: 'DELETE' }),
        confirm: (id: string) => request<any>(`/sales/orders/${id}/confirm`, { method: 'POST' }),
        cancel: (id: string) => request<any>(`/sales/orders/${id}/cancel`, { method: 'POST' }),
    },
    // Sales - Invoices
    invoices: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/sales/invoices?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/sales/invoices/${id}`),
        create: (data: any) => request<any>('/sales/invoices', { method: 'POST', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/sales/invoices/${id}`, { method: 'DELETE' }),
        issue: (id: string) => request<any>(`/sales/invoices/${id}/issue`, { method: 'POST' }),
        recordPayment: (id: string, amount: number) => request<any>(`/sales/invoices/${id}/record-payment`, { method: 'POST', body: JSON.stringify(amount) }),
        cancel: (id: string) => request<any>(`/sales/invoices/${id}/cancel`, { method: 'POST' }),
    },
    // Sales - Shipments
    shipments: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/sales/shipments?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/sales/shipments/${id}`),
        create: (data: any) => request<any>('/sales/shipments', { method: 'POST', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/sales/shipments/${id}`, { method: 'DELETE' }),
        ship: (id: string) => request<any>(`/sales/shipments/${id}/ship`, { method: 'POST' }),
        deliver: (id: string) => request<any>(`/sales/shipments/${id}/deliver`, { method: 'POST' }),
        setWaybill: (id: string, data: any) => request<any>(`/sales/shipments/${id}/waybill`, { method: 'POST', body: JSON.stringify(data) }),
    },

    // Procurement - Goods Receipts
    goodsReceipts: {
        getAll: () => request<any[]>('/procurement/goods-receipts'),
        getById: (id: string) => request<any>(`/procurement/goods-receipts/${id}`),
        create: (data: any) => request<any>('/procurement/goods-receipts', { method: 'POST', body: JSON.stringify(data) }),
    },

    // Finance
    finance: {
        journalEntries: {
            getAll: () => request<Array<any>>('/finance/journal-entries'),
            getById: (id: string) => request<any>(`/finance/journal-entries/${id}`),
            create: (data: any) => request<any>('/finance/journal-entries', { method: 'POST', body: JSON.stringify(data) }),
        },
        accounts: {
            getAll: () => request<Array<any>>('/accounts'), // Note: AccountsController is at root api/accounts not api/finance/accounts unless Route attribute changed
            getById: (id: string) => request<any>(`/accounts/${id}`),
            create: (data: any) => request<any>('/accounts', { method: 'POST', body: JSON.stringify(data) }),
            update: (id: string, data: any) => request<any>(`/accounts/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        },
        payments: {
            getAll: () => request<Array<any>>('/payments'),
            getById: (id: string) => request<any>(`/payments/${id}`),
            create: (data: any) => request<any>('/payments', { method: 'POST', body: JSON.stringify(data) }),
        },
        fiscalPeriods: {
            getAll: () => request<Array<any>>('/fiscalperiods'),
            getById: (id: string) => request<any>(`/fiscalperiods/${id}`),
            create: (data: any) => request<any>('/fiscalperiods', { method: 'POST', body: JSON.stringify(data) }),
            update: (id: string, data: any) => request<any>(`/fiscalperiods/${id}`, { method: 'PUT', body: JSON.stringify(data) }), // Using core Update logic
            generate: (year: number) => request<any>(`/fiscalperiods/generate/${year}`, { method: 'POST' }),
        },
        exchangeRates: {
            getAll: () => request<Array<any>>('/exchangerates'),
            getById: (id: string) => request<any>(`/exchangerates/${id}`),
            create: (data: any) => request<any>('/exchangerates', { method: 'POST', body: JSON.stringify(data) }),
            update: (id: string, data: any) => request<any>(`/exchangerates/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
            delete: (id: string) => request<any>(`/exchangerates/${id}`, { method: 'DELETE' }),
            sync: () => request<any>('/exchangerates/sync', { method: 'POST' }),
            fetchExternal: (date: string, code: string) => request<any>(`/exchangerates/external?date=${date}&code=${code}`),
        },
        cheques: {
            getAll: () => request<Array<any>>('/finance/cheques'),
            getById: (id: string) => request<any>(`/finance/cheques/${id}`),
            create: (data: any) => request<any>('/finance/cheques', { method: 'POST', body: JSON.stringify(data) }),
            updateStatus: (data: any) => request<any>('/finance/cheques/status', { method: 'POST', body: JSON.stringify(data) }),
            getHistory: (id: string) => request<Array<any>>(`/finance/cheques/${id}/history`),
        }
    },

    // HR - Employees
    employees: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/hr/employees?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/hr/employees/${id}`),
        // Add minimal lookups if needed
        getLookup: () => request<Array<{ id: string; firstName: string; lastName: string; position: string }>>('/hr/employees/lookup'),
    },

    // Fixed Assets
    assets: {
        getAll: (page = 1, pageSize = 20) => request<PagedResult<any>>(`/fixedassets/assets?page=${page}&pageSize=${pageSize}`),
        getById: (id: string) => request<any>(`/fixedassets/assets/${id}`),
        getLookup: () => request<Array<{ id: string; name: string; serialNumber: string }>>('/fixedassets/assets/lookup'),
    },

    // Manufacturing - Bill of Materials
    bom: {
        getAll: (page = 1, pageSize = 20, search?: string) =>
            request<PagedResult<any>>(`/billofmaterials?page=${page}&pageSize=${pageSize}${search ? `&search=${search}` : ''}`),
        getById: (id: string) => request<any>(`/billofmaterials/${id}`),
        getByProduct: (productId: string) => request<any[]>(`/billofmaterials/product/${productId}`),
        create: (data: { code: string; name: string; productId: string; quantity: number; type?: number; isDefault?: boolean }) =>
            request<any>('/billofmaterials', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) =>
            request<any>(`/billofmaterials/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/billofmaterials/${id}`, { method: 'DELETE' }),
        // Components
        addComponent: (bomId: string, data: { productId: string; quantity: number }) =>
            request<any>(`/billofmaterials/${bomId}/components`, { method: 'POST', body: JSON.stringify(data) }),
        removeComponent: (componentId: string) =>
            request<any>(`/billofmaterials/components/${componentId}`, { method: 'DELETE' }),
    },

    // Manufacturing - Production Orders
    productionOrders: {
        getAll: (page = 1, pageSize = 20, status?: number) =>
            request<PagedResult<any>>(`/productionorders?page=${page}&pageSize=${pageSize}${status !== undefined ? `&status=${status}` : ''}`),
        getById: (id: string) => request<any>(`/productionorders/${id}`),
        create: (data: { orderNumber: string; bomId: string; productId: string; plannedQuantity: number; warehouseId: string; priority?: number }) =>
            request<any>('/productionorders', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: any) =>
            request<any>(`/productionorders/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<any>(`/productionorders/${id}`, { method: 'DELETE' }),
        // Lifecycle
        plan: (id: string) => request<any>(`/productionorders/${id}/plan`, { method: 'POST' }),
        release: (id: string) => request<any>(`/productionorders/${id}/release`, { method: 'POST' }),
        start: (id: string) => request<any>(`/productionorders/${id}/start`, { method: 'POST' }),
        complete: (id: string) => request<any>(`/productionorders/${id}/complete`, { method: 'POST' }),
        cancel: (id: string) => request<any>(`/productionorders/${id}/cancel`, { method: 'POST' }),
        recordProduction: (id: string, quantity: number) =>
            request<any>(`/productionorders/${id}/record-production`, { method: 'POST', body: JSON.stringify({ quantity }) }),
    },

    // Inventory Extended - Brands
    brands: {
        getAll: () => request<any[]>('/brands'),
        getById: (id: string) => request<any>(`/brands/${id}`),
        create: (data: { code: string; name: string; description?: string; website?: string }) =>
            request<any>('/brands', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: { name: string; description?: string; website?: string; logoUrl?: string }) =>
            request<any>(`/brands/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/brands/${id}`, { method: 'DELETE' }),
    },

    // Unit Conversions
    unitConversions: {
        getAll: (productId?: string) => request<any[]>(`/unitconversions${productId ? `?productId=${productId}` : ''}`),
        create: (data: { fromUomId: string; toUomId: string; conversionFactor: number; productId?: string }) =>
            request<any>('/unitconversions', { method: 'POST', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/unitconversions/${id}`, { method: 'DELETE' }),
        convert: (fromUomId: string, toUomId: string, quantity: number, productId?: string) =>
            request<number>(`/unitconversions/convert?fromUomId=${fromUomId}&toUomId=${toUomId}&quantity=${quantity}${productId ? `&productId=${productId}` : ''}`),
    },

    // Product Serials
    productSerials: {
        getByProduct: (productId: string) => request<any[]>(`/products/${productId}/serials`),
        getBySerialNumber: (serialNumber: string) => request<any>(`/serials/${serialNumber}`),
        create: (productId: string, data: { productId: string; serialNumber: string; warehouseId?: string }) =>
            request<any>(`/products/${productId}/serials`, { method: 'POST', body: JSON.stringify(data) }),
        reserve: (productId: string, id: string) =>
            request<any>(`/products/${productId}/serials/${id}/reserve`, { method: 'POST' }),
    },

    // Product Batches
    productBatches: {
        getByProduct: (productId: string) => request<any[]>(`/products/${productId}/batches`),
        getExpiring: (daysAhead = 30) => request<any[]>(`/batches/expiring?daysAhead=${daysAhead}`),
        create: (productId: string, data: { productId: string; batchNumber: string; warehouseId: string; quantity: number; expiryDate?: string }) =>
            request<any>(`/products/${productId}/batches`, { method: 'POST', body: JSON.stringify(data) }),
        consume: (productId: string, id: string, quantity: number) =>
            request<any>(`/products/${productId}/batches/${id}/consume?quantity=${quantity}`, { method: 'POST' }),
        quarantine: (productId: string, id: string) =>
            request<any>(`/products/${productId}/batches/${id}/quarantine`, { method: 'POST' }),
        release: (productId: string, id: string) =>
            request<any>(`/products/${productId}/batches/${id}/release`, { method: 'POST' }),
    },

    // Product Attributes
    attributes: {
        getAll: () => request<any[]>('/attributes'),
        createDefinition: (data: { code: string; name: string; type: number; sortOrder?: number }) =>
            request<any>('/attributes', { method: 'POST', body: JSON.stringify(data) }),
        addValue: (data: { attributeDefinitionId: string; value: string; code?: string; sortOrder?: number }) =>
            request<any>('/attributes/values', { method: 'POST', body: JSON.stringify(data) }),
        deleteValue: (id: string) => request<void>(`/attributes/values/${id}`, { method: 'DELETE' }),
    },

    // Procurement - RFQs
    rfqs: {
        getAll: () => request<any[]>('/procurement/rfqs'),
        getById: (id: string) => request<any>(`/procurement/rfqs/${id}`),
        create: (data: any) => request<any>('/procurement/rfqs', { method: 'POST', body: JSON.stringify(data) }),
        close: (id: string) => request<any>(`/procurement/rfqs/${id}/close`, { method: 'POST' }),
        award: (id: string) => request<any>(`/procurement/rfqs/${id}/award`, { method: 'POST' }),
    },

    // Procurement - Purchase Quotes
    purchaseQuotes: {
        getAll: () => request<any[]>('/procurement/quotes'), // Assuming this endpoint exists or will be added
        getByRfq: (rfqId: string) => request<any[]>(`/procurement/rfqs/${rfqId}/quotes`),
        create: (data: any) => request<any>('/procurement/quotes', { method: 'POST', body: JSON.stringify(data) }),
        accept: (id: string) => request<any>(`/procurement/quotes/${id}/accept`, { method: 'POST' }),
        reject: (id: string) => request<any>(`/procurement/quotes/${id}/reject`, { method: 'POST' }),
    },

    // Procurement - QC
    qc: {
        getAll: () => request<any[]>('/procurement/qc'),
        create: (data: any) => request<any>('/procurement/qc', { method: 'POST', body: JSON.stringify(data) }),
    },

    // Procurement - Returns
    returns: {
        getAll: () => request<any[]>('/procurement/returns'),
        create: (data: any) => request<any>('/procurement/returns', { method: 'POST', body: JSON.stringify(data) }),
        ship: (id: string) => request<any>(`/procurement/returns/${id}/ship`, { method: 'POST' }),
        complete: (id: string) => request<any>(`/procurement/returns/${id}/complete`, { method: 'POST' }),
    }
};
