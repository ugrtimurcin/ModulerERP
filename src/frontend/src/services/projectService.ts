import { request } from './api';
import type {
    ProjectDto,
    CreateProjectDto,
    UpdateProjectDto,
    ProjectTaskDto,
    CreateProjectTaskDto,
    UpdateProjectTaskDto,
    UpdateProjectTaskProgressDto,
    ProjectTransactionDto,
    CreateProjectTransactionDto,
    ProgressPaymentDto,
    CreateProgressPaymentDto,
    UpdateProgressPaymentDto,
    ProjectDocumentDto,
    CreateProjectDocumentDto,
    ProjectFinancialSummaryDto,
    ProjectChangeOrderDto,
    CreateChangeOrderDto,
    BillOfQuantitiesItemDto,
    CreateBoQItemDto,
    UpdateBoQItemDto,
    UpdateProgressPaymentDetailDto,
    ResourceRateCardDto,
    CreateResourceRateCardDto,
    UpdateResourceRateCardDto
} from '../types/project';

export const projectService = {
    // Projects
    projects: {
        getAll: () => request<Array<ProjectDto>>('/projects'),
        getById: (id: string) => request<ProjectDto>(`/projects/${id}`),
        create: (data: CreateProjectDto) => request<ProjectDto>('/projects', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: UpdateProjectDto) => request<void>(`/projects/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projects/${id}`, { method: 'DELETE' }),

        // Bill of Quantities (BoQ)
        addBoQItem: (id: string, data: CreateBoQItemDto) => request<BillOfQuantitiesItemDto>(`/projects/${id}/boq-items`, { method: 'POST', body: JSON.stringify(data) }),
        updateBoQItem: (id: string, itemId: string, data: UpdateBoQItemDto) => request<void>(`/projects/${id}/boq-items/${itemId}`, { method: 'PUT', body: JSON.stringify(data) }),
        deleteBoQItem: (id: string, itemId: string) => request<void>(`/projects/${id}/boq-items/${itemId}`, { method: 'DELETE' }),
    },

    // Tasks
    tasks: {
        getByProject: (projectId: string) => request<Array<ProjectTaskDto>>(`/projecttasks/project/${projectId}`),
        create: (data: CreateProjectTaskDto) => request<ProjectTaskDto>('/projecttasks', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: UpdateProjectTaskDto) => request<void>(`/projecttasks/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projecttasks/${id}`, { method: 'DELETE' }),
        updateProgress: (id: string, data: UpdateProjectTaskProgressDto) => request<void>(`/projecttasks/${id}/progress`, { method: 'PUT', body: JSON.stringify(data) }),
    },

    // Resources
    resources: {
        getAll: (projectId: string) => request<Array<import('../types/project').ProjectResourceDto>>(`/projects/${projectId}/resources`),
        create: (data: import('../types/project').CreateProjectResourceDto) => request<import('../types/project').ProjectResourceDto>('/projectresources', { method: 'POST', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projectresources/${id}`, { method: 'DELETE' }),
    },

    // Transactions (Costs)
    transactions: {
        getByProject: (projectId: string) => request<Array<ProjectTransactionDto>>(`/projecttransactions/project/${projectId}`),
        create: (data: CreateProjectTransactionDto) => request<ProjectTransactionDto>('/projecttransactions', { method: 'POST', body: JSON.stringify(data) }),
    },

    // Progress Payments (Hakediş)
    payments: {
        getByProject: (projectId: string) => request<Array<ProgressPaymentDto>>(`/projects/${projectId}/payments`),
        create: (data: CreateProgressPaymentDto) => request<ProgressPaymentDto>(`/projects/${data.projectId}/payments`, { method: 'POST', body: JSON.stringify(data) }),
        update: (projectId: string, id: string, data: UpdateProgressPaymentDto) => request<void>(`/projects/${projectId}/payments/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        approve: (projectId: string, id: string) => request<void>(`/projects/${projectId}/payments/${id}/approve`, { method: 'POST' }),
        updateDetail: (projectId: string, id: string, data: UpdateProgressPaymentDetailDto) => request<void>(`/projects/${projectId}/payments/${id}/details`, { method: 'PUT', body: JSON.stringify(data) }),
    },

    // Documents
    documents: {
        getByProject: (projectId: string) => request<Array<ProjectDocumentDto>>(`/projectdocuments/project/${projectId}`),
        create: (data: CreateProjectDocumentDto) => request<ProjectDocumentDto>('/projectdocuments', { method: 'POST', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projectdocuments/${id}`, { method: 'DELETE' }),
    },

    // Financials
    financials: {
        getSummary: (projectId: string) => request<ProjectFinancialSummaryDto>(`/projectfinancial/${projectId}`),
    },

    // Change Orders (Zeyilnames)
    changeOrders: {
        getByProject: (projectId: string) => request<Array<ProjectChangeOrderDto>>(`/projectchangeorders/project/${projectId}`),
        create: (data: CreateChangeOrderDto) => request<ProjectChangeOrderDto>('/projectchangeorders', { method: 'POST', body: JSON.stringify(data) }),
        approve: (id: string) => request<void>(`/projectchangeorders/${id}/approve`, { method: 'POST' }),
        reject: (id: string) => request<void>(`/projectchangeorders/${id}/reject`, { method: 'POST' })
    },

    // Rate Cards
    rateCards: {
        getAll: (projectId?: string) => request<Array<ResourceRateCardDto>>(`/projects/rate-cards${projectId ? `?projectId=${projectId}` : ''}`),
        create: (data: CreateResourceRateCardDto) => request<ResourceRateCardDto>('/projects/rate-cards', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: UpdateResourceRateCardDto) => request<void>(`/projects/rate-cards/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projects/rate-cards/${id}`, { method: 'DELETE' }),
    }
};
