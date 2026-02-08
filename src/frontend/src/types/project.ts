export const ProjectStatus = {
    Planning: 0,
    Active: 1,
    Suspended: 2,
    Completed: 3,
    Cancelled: 4
} as const;
export type ProjectStatus = typeof ProjectStatus[keyof typeof ProjectStatus];

export const ProjectTaskStatus = {
    NotStarted: 0,
    InProgress: 1,
    Completed: 2,
    OnHold: 3
} as const;
export type ProjectTaskStatus = typeof ProjectTaskStatus[keyof typeof ProjectTaskStatus];

export const ProgressPaymentStatus = {
    Draft: 0,
    Approved: 1,
    Invoiced: 2
} as const;
export type ProgressPaymentStatus = typeof ProgressPaymentStatus[keyof typeof ProgressPaymentStatus];

export const ProjectTransactionType = {
    Material: 0,
    Labor: 1,
    Subcontractor: 2,
    GeneralExpense: 3
} as const;
export type ProjectTransactionType = typeof ProjectTransactionType[keyof typeof ProjectTransactionType];

export interface ProjectBudget {
    totalBudget: number;
    materialBudget: number;
    laborBudget: number;
    subcontractorBudget: number;
    expenseBudget: number;
}

export interface ProjectDto {
    id: string;
    code: string;
    name: string;
    description: string;
    customerId?: string;
    projectManagerId?: string;
    contractCurrencyId: string;
    contractAmount: number;
    startDate: string;
    targetDate?: string;
    actualFinishDate?: string;
    status: ProjectStatus;
    completionPercentage: number;
    budget: ProjectBudget;
}

export interface CreateProjectDto {
    code: string;
    name: string;
    description: string;
    customerId?: string;
    projectManagerId?: string;
    contractCurrencyId: string;
    contractAmount: number;
    startDate: string;
    targetDate?: string;
}

export interface UpdateProjectDto {
    name: string;
    description: string;
    status: ProjectStatus;
    actualFinishDate?: string;
}

export interface ProjectTaskDto {
    id: string;
    projectId: string;
    name: string;
    parentTaskId?: string;
    startDate: string;
    dueDate: string;
    completionPercentage: number;
    status: ProjectTaskStatus;
    assignedEmployeeId?: string;
    assignedSubcontractorId?: string;
    children: ProjectTaskDto[];
}

export interface CreateProjectTaskDto {
    projectId: string;
    name: string;
    parentTaskId?: string;
    startDate: string;
    dueDate: string;
    assignedEmployeeId?: string;
    assignedSubcontractorId?: string;
}

export interface UpdateProjectTaskDto {
    id: string;
    projectId: string;
    name: string;
    parentTaskId?: string;
    startDate: string;
    dueDate: string;
    assignedEmployeeId?: string;
    assignedSubcontractorId?: string;
}

export interface UpdateProjectTaskProgressDto {
    taskId: string;
    completionPercentage: number;
    status: ProjectTaskStatus;
}

export interface ProjectTransactionDto {
    id: string;
    projectId: string;
    projectTaskId?: string;
    sourceModule: string;
    sourceRecordId: string;
    description: string;
    amount: number;
    currencyId: string;
    exchangeRate: number;
    amountReporting: number;
    type: ProjectTransactionType;
    createdAt: string;
}

export interface CreateProjectTransactionDto {
    projectId: string;
    projectTaskId?: string;
    sourceModule?: string;
    sourceRecordId?: string;
    description: string;
    amount: number;
    currencyId: string;
    exchangeRate: number;
    type: ProjectTransactionType;
}

export interface ProgressPaymentDto {
    id: string;
    projectId: string;
    paymentNo: number;
    date: string;
    previousCumulativeAmount: number;
    currentAmount: number;
    retentionRate: number;
    retentionAmount: number;
    netPayableAmount: number;
    status: ProgressPaymentStatus;
}

export interface CreateProgressPaymentDto {
    projectId: string;
    date: string;
    currentAmount: number;
    retentionRate: number;
}

export interface ProjectDocumentDto {
    id: string;
    projectId: string;
    title: string;
    documentType: string;
    fileUrl: string;
    systemFileId?: string;
    description: string;
    uploadedAt: string;
}

export interface CreateProjectDocumentDto {
    projectId: string;
    title: string;
    documentType: string;
    fileUrl: string;
    systemFileId?: string;
    description: string;
}

export interface ProjectCostBreakdownDto {
    type: ProjectTransactionType;
    amount: number;
}

export interface ProjectFinancialSummaryDto {
    projectId: string;
    contractAmount: number;
    totalBilled: number;
    totalCost: number;
    projectedProfit: number;
    currencyCode: string;
    costBreakdown: ProjectCostBreakdownDto[];
}
