import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { employeeService } from '@/services/hr/employeeService';
import { CitizenshipType, SocialSecurityType } from '@/types/hr';
import type { Employee, Department, CreateEmployeeDto, UpdateEmployeeDto } from '@/types/hr';



const formSchema = z.object({
    firstName: z.string().min(2, "First name is required"),
    lastName: z.string().min(2, "Last name is required"),
    email: z.string().email("Invalid email address"),
    identityNumber: z.string().min(5, "Identity number is required"),
    citizenship: z.number(),
    socialSecurityType: z.number(),
    workPermitNumber: z.string().optional(),
    workPermitExpiryDate: z.string().optional(),
    jobTitle: z.string().min(2, "Job title is required"),
    departmentId: z.string().min(1, "Department is required"),
    supervisorId: z.string().optional(),
    currentSalary: z.number().min(0, "Salary must be non-negative"),
    transportAmount: z.number().min(0, "Transport amount must be non-negative"),
    bankName: z.string().optional(),
    iban: z.string().optional()
});

type FormValues = z.infer<typeof formSchema>;

interface EmployeeDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    employee: Employee | null;
    departments: Department[];
    employees: Employee[];
}

export function EmployeeDialog({ open, onClose, employee, departments, employees }: EmployeeDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();

    // Enum helpers
    const citizenshipOptions = Object.entries(CitizenshipType)
        .filter(([key]) => isNaN(Number(key)))
        .map(([key, value]) => ({
            label: t(`hr.citizenshipTypes.${key}`),
            value: value
        }));

    const socialSecurityOptions = Object.entries(SocialSecurityType)
        .filter(([key]) => isNaN(Number(key)))
        .map(([key, value]) => ({
            label: t(`hr.socialSecurityTypes.${key}`),
            value: value
        }));

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            citizenship: CitizenshipType.TRNC,
            socialSecurityType: SocialSecurityType.Standard,
            currentSalary: 0,
            transportAmount: 0
        }
    });

    useEffect(() => {
        if (open) {
            if (employee) {
                reset({
                    firstName: employee.firstName,
                    lastName: employee.lastName,
                    email: employee.email,
                    identityNumber: employee.identityNumber,
                    citizenship: employee.citizenship,
                    socialSecurityType: employee.socialSecurityType,
                    workPermitNumber: employee.workPermitNumber || '',
                    workPermitExpiryDate: employee.workPermitExpiryDate ? new Date(employee.workPermitExpiryDate).toISOString().split('T')[0] : '',
                    jobTitle: employee.jobTitle,
                    departmentId: employee.departmentId,
                    supervisorId: employee.supervisorId || '',
                    currentSalary: employee.currentSalary,
                    transportAmount: employee.transportAmount || 0,
                    bankName: employee.bankName || '',
                    iban: employee.iban || ''
                });
            } else {
                reset({
                    firstName: '',
                    lastName: '',
                    email: '',
                    identityNumber: '',
                    citizenship: CitizenshipType.TRNC,
                    socialSecurityType: SocialSecurityType.Standard,
                    workPermitNumber: '',
                    workPermitExpiryDate: '',
                    jobTitle: '',
                    departmentId: departments[0]?.id || '',
                    supervisorId: '',
                    currentSalary: 0,
                    transportAmount: 0,
                    bankName: '',
                    iban: ''
                });
            }
        }
    }, [open, employee, departments, reset]);

    const onSubmit = async (data: FormValues) => {
        try {
            // Helper to clean empty strings to undefined/null if needed
            const commonData = {
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                identityNumber: data.identityNumber,
                citizenship: data.citizenship,
                socialSecurityType: data.socialSecurityType,
                workPermitNumber: data.workPermitNumber || undefined,
                workPermitExpiryDate: data.workPermitExpiryDate || undefined,
                jobTitle: data.jobTitle,
                departmentId: data.departmentId,
                supervisorId: data.supervisorId || null,
                currentSalary: data.currentSalary,
                transportAmount: data.transportAmount,
                bankName: data.bankName || undefined,
                iban: data.iban || undefined,
            };

            if (employee) {
                const updateDto: UpdateEmployeeDto = {
                    ...commonData,
                    citizenship: commonData.citizenship as CitizenshipType,
                    socialSecurityType: commonData.socialSecurityType as SocialSecurityType,
                    status: employee.status
                };
                await employeeService.update(employee.id, updateDto);
                toast.success(t('hr.employeeUpdated'));
            } else {
                const createDto: CreateEmployeeDto = {
                    ...commonData,
                    citizenship: commonData.citizenship as CitizenshipType,
                    socialSecurityType: commonData.socialSecurityType as SocialSecurityType,
                };
                await employeeService.create(createDto);
                toast.success(t('hr.employeeCreated'));
            }
            onClose(true);
        } catch (error: any) {
            console.error(error);
            toast.error(error.message || t('common.error'));
        }
    };

    if (!open) return null;

    const inputClass = "w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500";
    const labelClass = "block text-sm font-medium mb-1";

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between z-10">
                    <h2 className="text-xl font-semibold">
                        {employee ? t('hr.editEmployee') : t('hr.createEmployee')}
                    </h2>
                    <button
                        onClick={() => onClose(false)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
                    {/* Personal Info */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.firstName')}</label>
                            <input {...register('firstName')} className={inputClass} />
                            {errors.firstName && <span className="text-red-500 text-xs">{errors.firstName.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.lastName')}</label>
                            <input {...register('lastName')} className={inputClass} />
                            {errors.lastName && <span className="text-red-500 text-xs">{errors.lastName.message}</span>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('common.email')}</label>
                            <input type="email" {...register('email')} className={inputClass} />
                            {errors.email && <span className="text-red-500 text-xs">{errors.email.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.identityNumber')}</label>
                            <input {...register('identityNumber')} className={inputClass} />
                            {errors.identityNumber && <span className="text-red-500 text-xs">{errors.identityNumber.message}</span>}
                        </div>
                    </div>

                    {/* Legal Info */}
                    <div className="space-y-4">
                        <h3 className="text-lg font-medium border-b border-[hsl(var(--border))] pb-2">{t('hr.legalInfo')}</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className={labelClass}>{t('hr.citizenship')}</label>
                                <select {...register('citizenship', { valueAsNumber: true })} className={inputClass}>
                                    {citizenshipOptions.map(opt => (
                                        <option key={opt.label} value={opt.value}>{opt.label}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.socialSecurityType')}</label>
                                <select {...register('socialSecurityType', { valueAsNumber: true })} className={inputClass}>
                                    {socialSecurityOptions.map(opt => (
                                        <option key={opt.label} value={opt.value}>{opt.label}</option>
                                    ))}
                                </select>
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.workPermitNumber')}</label>
                                <input {...register('workPermitNumber')} className={inputClass} />
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.workPermitExpiryDate')}</label>
                                <input type="date" {...register('workPermitExpiryDate')} className={inputClass} />
                            </div>
                        </div>
                    </div>

                    {/* Job Info */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.jobTitle')}</label>
                            <input {...register('jobTitle')} className={inputClass} />
                            {errors.jobTitle && <span className="text-red-500 text-xs">{errors.jobTitle.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.department')}</label>
                            <select {...register('departmentId')} className={inputClass}>
                                <option value="">Select Department</option>
                                {departments.map((dept) => (
                                    <option key={dept.id} value={dept.id}>{dept.name}</option>
                                ))}
                            </select>
                            {errors.departmentId && <span className="text-red-500 text-xs">{errors.departmentId.message}</span>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.supervisor')}</label>
                            <select {...register('supervisorId')} className={inputClass}>
                                <option value="">No Supervisor</option>
                                {employees.filter(e => e.id !== employee?.id).map((emp) => (
                                    <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    {/* Financial Info */}
                    <div className="space-y-4">
                        <h3 className="text-lg font-medium border-b border-[hsl(var(--border))] pb-2">{t('hr.financialInfo')}</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className={labelClass}>{t('hr.salary')}</label>
                                <input type="number" step="0.01" {...register('currentSalary', { valueAsNumber: true })} className={inputClass} />
                                {errors.currentSalary && <span className="text-red-500 text-xs">{errors.currentSalary.message}</span>}
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.transportAmount')}</label>
                                <input type="number" step="0.01" {...register('transportAmount', { valueAsNumber: true })} className={inputClass} />
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.bankName')}</label>
                                <input {...register('bankName')} className={inputClass} />
                            </div>
                            <div>
                                <label className={labelClass}>{t('hr.iban')}</label>
                                <input {...register('iban')} className={inputClass} />
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : (employee ? t('common.save') : t('common.create'))}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
