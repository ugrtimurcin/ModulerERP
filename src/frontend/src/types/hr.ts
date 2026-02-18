
// Enums matching backend - Converted to const objects for 'erasableSyntaxOnly' compatibility

export const CitizenshipType = {
    TRNC: 0,
    Turkey: 1,
    Other: 2
} as const;
export type CitizenshipType = typeof CitizenshipType[keyof typeof CitizenshipType];

export const SocialSecurityType = {
    Standard: 0,
    Pensioner: 1,
    Student: 2,
    Exempt: 3,
    Foreigner: 4,
    PartTime: 5
} as const;
export type SocialSecurityType = typeof SocialSecurityType[keyof typeof SocialSecurityType];

export const EmploymentStatus = {
    Active: 0,
    Terminated: 1,
    OnLeave: 2
} as const;
export type EmploymentStatus = typeof EmploymentStatus[keyof typeof EmploymentStatus];

export interface Department {
    id: string;
    name: string;
    description?: string;
    managerId?: string | null;
}

export interface Employee {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    identityNumber: string;
    citizenship: CitizenshipType;
    socialSecurityType: SocialSecurityType;
    workPermitNumber?: string;
    workPermitExpiryDate?: string; // Date string
    jobTitle: string;
    departmentId: string;
    departmentName: string;
    supervisorId?: string;
    supervisorName?: string;
    currentSalary: number;
    transportAmount: number;
    bankName?: string;
    iban?: string;
    status: EmploymentStatus;
    createdAt: string;
    qrToken?: string;
}

export interface CreateEmployeeDto {
    firstName: string;
    lastName: string;
    email: string;
    identityNumber: string;
    citizenship: CitizenshipType;
    socialSecurityType: SocialSecurityType;
    workPermitNumber?: string;
    workPermitExpiryDate?: string | Date;
    jobTitle: string;
    departmentId: string;
    supervisorId?: string | null; // Allow null for clear selection
    currentSalary: number;
    transportAmount: number;
    bankName?: string;
    iban?: string;
    userId?: string;
}

export interface UpdateEmployeeDto {
    firstName: string;
    lastName: string;
    email: string;
    citizenship: CitizenshipType;
    socialSecurityType: SocialSecurityType;
    workPermitNumber?: string;
    workPermitExpiryDate?: string | Date;
    jobTitle: string;
    departmentId: string;
    supervisorId?: string | null;
    currentSalary: number;
    transportAmount: number;
    bankName?: string;
    iban?: string;
    status: EmploymentStatus;
}
