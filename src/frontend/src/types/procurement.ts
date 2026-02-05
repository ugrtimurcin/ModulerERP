export const RfqStatus = {
    Open: 1,
    Closed: 2,
    Awarded: 3
} as const;

export type RfqStatus = typeof RfqStatus[keyof typeof RfqStatus];

export const PurchaseQuoteStatus = {
    Pending: 1,
    Accepted: 2,
    Rejected: 3
} as const;

export type PurchaseQuoteStatus = typeof PurchaseQuoteStatus[keyof typeof PurchaseQuoteStatus];

export const QualityControlStatus = {
    Pending: 0,
    Passed: 1,
    Failed: 2,
    ConditionallyAccepted: 3
} as const;

export type QualityControlStatus = typeof QualityControlStatus[keyof typeof QualityControlStatus];

export const PurchaseReturnStatus = {
    Draft: 0,
    Shipped: 1,
    Completed: 2,
    Cancelled: 3
} as const;

export type PurchaseReturnStatus = typeof PurchaseReturnStatus[keyof typeof PurchaseReturnStatus];

export interface RequestForQuotationItemDto {
    id: string;
    productId: string;
    targetQuantity: number;
}

export interface PurchaseQuoteItemDto {
    id: string;
    productId: string;
    price: number;
    leadTimeDays: number;
}

export interface PurchaseQuoteDto {
    id: string;
    rfqId: string;
    supplierId: string;
    quoteReference: string;
    validUntil: string;
    totalAmount: number;
    isSelected: boolean;
    status: PurchaseQuoteStatus;
    items: PurchaseQuoteItemDto[];
}

export interface RequestForQuotationDto {
    id: string;
    rfqNumber: string;
    title: string;
    deadLine: string;
    status: RfqStatus;
    items: RequestForQuotationItemDto[];
    quotes: PurchaseQuoteDto[];
}

export interface CreateRequestForQuotationItemDto {
    productId: string;
    targetQuantity: number;
}

export interface CreateRequestForQuotationDto {
    title: string;
    deadLine: string;
    items: CreateRequestForQuotationItemDto[];
}

export interface CreatePurchaseQuoteItemDto {
    rfqItemId: string;
    productId: string;
    price: number;
    leadTimeDays: number;
}

export interface CreatePurchaseQuoteDto {
    rfqId: string;
    supplierId: string;
    quoteReference: string;
    validUntil: string;
    totalAmount: number;
    items: CreatePurchaseQuoteItemDto[];
}

export interface QualityControlInspectionDto {
    id: string;
    receiptItemId: string;
    inspectorId: string;
    inspectionDate: string;
    quantityPassed: number;
    quantityRejected: number;
    targetWarehouseId: string;
    rejectionReasonId?: string;
    status: QualityControlStatus;
    notes?: string;
}

export interface CreateQualityControlInspectionDto {
    receiptItemId: string;
    quantityPassed: number;
    quantityRejected: number;
    targetWarehouseId: string;
    rejectionReasonId?: string;
    targetLocationId?: string;
    notes?: string;
}

export interface PurchaseReturnItemDto {
    id: string;
    receiptItemId: string;
    quantity: number;
    reasonId?: string;
}

export interface PurchaseReturnDto {
    id: string;
    returnNumber: string;
    supplierId: string;
    goodsReceiptId: string;
    status: PurchaseReturnStatus;
    items: PurchaseReturnItemDto[];
}

export interface CreatePurchaseReturnItemDto {
    receiptItemId: string;
    quantity: number;
    reasonId?: string;
}

export interface CreatePurchaseReturnDto {
    supplierId: string;
    goodsReceiptId: string;
    items: CreatePurchaseReturnItemDto[];
}
