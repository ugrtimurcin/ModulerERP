import { request } from './api';
import type {
    ProjectDto,
    CreateProjectDto,
    UpdateProjectDto,
    ProjectBudget,
    ProjectTaskDto,
    CreateProjectTaskDto,
    UpdateProjectTaskProgressDto,
    ProjectTransactionDto,
    CreateProjectTransactionDto,
    ProgressPaymentDto,
    CreateProgressPaymentDto,
    ProjectDocumentDto,
    CreateProjectDocumentDto,
    ProjectFinancialSummaryDto
} from '../types/project';

export const projectService = {
    // Projects
    projects: {
        getAll: () => request<Array<ProjectDto>>('/projects'),
        getById: (id: string) => request<ProjectDto>(`/projects/${id}`),
        create: (data: CreateProjectDto) => request<ProjectDto>('/projects', { method: 'POST', body: JSON.stringify(data) }),
        update: (id: string, data: UpdateProjectDto) => request<void>(`/projects/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
        updateBudget: (id: string, data: ProjectBudget) => request<void>(`/projects/${id}/budget`, { method: 'PUT', body: JSON.stringify(data) }),
        delete: (id: string) => request<void>(`/projects/${id}`, { method: 'DELETE' }),
    },

    // Tasks
    tasks: {
        getByProject: (projectId: string) => request<Array<ProjectTaskDto>>(`/projecttasks/project/${projectId}`),
        create: (data: CreateProjectTaskDto) => request<ProjectTaskDto>('/projecttasks', { method: 'POST', body: JSON.stringify(data) }),
        updateProgress: (id: string, data: UpdateProjectTaskProgressDto) => request<void>(`/projecttasks/${id}/progress`, { method: 'PUT', body: JSON.stringify(data) }),
    },

    // Transactions (Costs)
    transactions: {
        getByProject: (projectId: string) => request<Array<ProjectTransactionDto>>(`/projecttransactions/project/${projectId}`),
        create: (data: CreateProjectTransactionDto) => request<ProjectTransactionDto>('/projecttransactions', { method: 'POST', body: JSON.stringify(data) }),
    },

    // Progress Payments (Hakediş)
    payments: {
        getByProject: (projectId: string) => request<Array<ProgressPaymentDto>>(`/progresspayments/project/${projectId}`),
        create: (data: CreateProgressPaymentDto) => request<ProgressPaymentDto>('/progresspayments', { method: 'POST', body: JSON.stringify(data) }),
        approve: (id: string) => request<void>(`/progresspayments/${id}/approve`, { method: 'POST' }),
    },

    // Documents
    documents: {
        getByProject: (projectId: string) => request<Array<ProjectDocumentDto>>(`/projectdocuments/project/${projectId}`),
        create: (data: CreateProjectDocumentDto) => request<ProjectDocumentDto>('/projectdocuments', { method: 'POST', body: JSON.stringify(data) }),
    },

    // Financials
    financials: {
        getSummary: (projectId: string) => request<ProjectFinancialSummaryDto>(`/projectfinancial/${projectId}`),
    }
};
