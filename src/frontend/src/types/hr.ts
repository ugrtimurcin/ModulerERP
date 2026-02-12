export interface Employee {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    identityNumber: string;
    citizenshipNumber?: string;
    workPermitNumber?: string;
    workPermitExpiryDate?: string;
    jobTitle: string;
    departmentId: string;
    departmentName: string;
    supervisorId: string | null;
    supervisorName: string | null;
    currentSalary: number;
    bankName?: string;
    iban?: string;
    status: number;
    createdAt: string;
}

export interface Department {
    id: string;
    name: string;
    description?: string;
    managerId?: string | null;
}
