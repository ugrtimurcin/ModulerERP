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

// Daily Log Types

export interface DailyLogDto {
    id: string;
    projectId: string;
    date: string; // DateTime
    weatherCondition: string;
    siteManagerNote: string;
    isApproved: boolean;
    approvalDate?: string;
    approvedByUserId?: string;
    resourceUsages: DailyLogResourceUsageDto[];
    materialUsages: DailyLogMaterialUsageDto[];
}

export interface DailyLogResourceUsageDto {
    id: string;
    projectResourceId: string;
    projectTaskId?: string;
    hoursWorked: number;
    description: string;
}

export interface DailyLogMaterialUsageDto {
    id: string;
    productId: string;
    quantity: number;
    unitOfMeasureId: string;
    location: string;
}

export interface CreateDailyLogDto {
    projectId: string;
    date: string;
    weatherCondition: string;
    siteManagerNote: string;
    resourceUsages: CreateResourceUsageDto[];
    materialUsages: CreateMaterialUsageDto[];
}

export interface CreateResourceUsageDto {
    projectResourceId: string;
    projectTaskId?: string;
    hoursWorked: number;
    description: string;
}

export interface CreateMaterialUsageDto {
    productId: string;
    quantity: number;
    unitOfMeasureId: string;
    location: string;
}

// Project Resource Types

export interface ProjectResourceDto {
    id: string;
    projectId: string;
    employeeId?: string;
    employeeName?: string;
    assetId?: string;
    assetName?: string;
    role: string;
    hourlyCost: number;
    currencyId: string;
    // Helper to display generic name
    name?: string;
}

export interface CreateProjectResourceDto {
    projectId: string;
    employeeId?: string;
    assetId?: string;
    role: string;
    hourlyCost: number;
    currencyId: string;
}

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

export const BudgetCategory = {
    Material: 0,
    Labor: 1,
    Subcontractor: 2,
    Expense: 3,
    Equipment: 4,
    Overhead: 5
} as const;
export type BudgetCategory = typeof BudgetCategory[keyof typeof BudgetCategory];

export interface BillOfQuantitiesItemDto {
    id: string;
    projectId: string;
    parentId?: string;
    itemCode: string;
    description: string;
    quantity: number;
    unitOfMeasureId: string;
    contractUnitPrice: number;
    estimatedUnitCost: number;
    totalContractAmount: number;
    totalEstimatedCost: number;
    category: BudgetCategory;
    // For UI hierarchy
    children?: BillOfQuantitiesItemDto[];
}

export interface CreateBoQItemDto {
    projectId: string;
    parentId?: string;
    itemCode: string;
    description: string;
    quantity: number;
    unitOfMeasureId: string;
    contractUnitPrice: number;
    estimatedUnitCost: number;
    category: BudgetCategory;
}

export interface UpdateBoQItemDto {
    itemCode: string;
    description: string;
    quantity: number;
    unitOfMeasureId: string;
    contractUnitPrice: number;
    estimatedUnitCost: number;
    category: BudgetCategory;
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
    // Budgeting V2 (BoQ)
    boQItems: BillOfQuantitiesItemDto[];
    totalContractAmount: number;
    totalEstimatedCost: number;
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
    assignedResources: TaskResourceDto[];
    children: ProjectTaskDto[];
}

export interface TaskResourceDto {
    projectResourceId: string;
    role: string;
    allocationPercent: number;
}

export interface CreateProjectTaskDto {
    projectId: string;
    name: string;
    parentTaskId?: string;
    startDate: string;
    dueDate: string;
    assignedResourceIds: string[];
}

export interface UpdateProjectTaskDto {
    id: string;
    projectId: string;
    name: string;
    parentTaskId?: string;
    startDate: string;
    dueDate: string;
    assignedResourceIds: string[];
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
    periodStart: string;
    periodEnd: string;
    grossWorkAmount: number;
    materialOnSiteAmount: number;
    cumulativeTotalAmount: number;
    previousCumulativeAmount: number;
    periodDeltaAmount: number;
    retentionRate: number;
    retentionAmount: number;
    withholdingTaxRate: number;
    withholdingTaxAmount: number;
    advanceDeductionAmount: number;
    netPayableAmount: number;
    isExpense: boolean;
    status: ProgressPaymentStatus;
    details?: ProgressPaymentDetailDto[];
}

export interface ProgressPaymentDetailDto {
    id: string;
    progressPaymentId: string;
    billOfQuantitiesItemId: string;
    itemCode: string;
    description: string;
    previousCumulativeQuantity: number;
    cumulativeQuantity: number;
    periodQuantity: number;
    unitPrice: number;
    totalAmount: number;
    periodAmount: number;
}

export interface UpdateProgressPaymentDetailDto {
    id: string;
    cumulativeQuantity: number;
}

export interface CreateProgressPaymentDto {
    projectId: string;
    date: string;
    periodStart: string;
    periodEnd: string;
    materialOnSiteAmount: number;
    advanceDeductionAmount: number;
    isExpense: boolean;
}

export interface UpdateProgressPaymentDto {
    date: string;
    periodStart: string;
    periodEnd: string;
    materialOnSiteAmount: number;
    advanceDeductionAmount: number;
    isExpense: boolean;
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

export interface ProjectChangeOrderDto {
    id: string;
    projectId: string;
    orderNo: number;
    title: string;
    description: string;
    amountChange: number;
    timeExtensionDays: number;
    status: number;
    requestDate: string;
    approvalDate: string;
    approverId?: string;
}

export interface CreateChangeOrderDto {
    projectId: string;
    title: string;
    description: string;
    amountChange: number;
    timeExtensionDays: number;
}

// Rate Cards
export interface ResourceRateCardDto {
    id: string;
    projectId?: string;
    employeeId?: string;
    employeeName?: string;
    assetId?: string;
    assetName?: string;
    hourlyRate: number;
    overtimeRate?: number;
    currencyId: string;
    effectiveFrom: string; // ISO Date
    effectiveTo?: string;  // ISO Date
}

export interface CreateResourceRateCardDto {
    projectId?: string;
    employeeId?: string;
    assetId?: string;
    hourlyRate: number;
    overtimeRate?: number;
    currencyId: string;
    effectiveFrom: string;
    effectiveTo?: string;
}

export interface UpdateResourceRateCardDto {
    hourlyRate: number;
    overtimeRate?: number;
    currencyId: string;
    effectiveFrom: string;
    effectiveTo?: string;
}
