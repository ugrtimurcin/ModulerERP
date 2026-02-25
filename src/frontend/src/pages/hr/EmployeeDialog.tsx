import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X, User, Briefcase, FileText, Scale } from 'lucide-react';
import { Button, useToast, Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui';
import { employeeService } from '@/services/hr/employeeService';
import { CitizenshipType, SocialSecurityType, MaritalStatus } from '@/types/hr';
import type { Employee, Department } from '@/types/hr';

const formSchema = z.object({
    firstName: z.string().min(2, "First name is required"),
    lastName: z.string().min(2, "Last name is required"),
    email: z.string().email("Invalid email address"),
    identityNumber: z.string().min(5, "Identity number is required"),
    citizenship: z.number(),
    socialSecurityType: z.number(),
    sgkRiskProfileId: z.string().optional(),
    workPermitNumber: z.string().optional(),
    workPermitExpiryDate: z.string().optional(),
    passportNumber: z.string().optional(),
    passportExpDate: z.string().optional(),
    healthReportExpDate: z.string().optional(),
    jobTitle: z.string().min(2, "Job title is required"),
    departmentId: z.string().min(1, "Department is required"),
    supervisorId: z.string().optional(),
    currentSalary: z.number().min(0, "Salary must be non-negative"),
    transportAmount: z.number().min(0, "Transport amount must be non-negative"),
    bankName: z.string().optional(),
    iban: z.string().optional(),
    maritalStatus: z.number(),
    isSpouseWorking: z.boolean(),
    childCount: z.number().min(0),
    isPensioner: z.boolean()
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
    const [activeTab, setActiveTab] = useState("personal");

    // Enum helpers
    const citizenshipOptions = Object.entries(CitizenshipType).filter(([key]) => isNaN(Number(key))).map(([key, value]) => ({ label: t(`hr.citizenshipTypes.${key}`), value }));
    const socialSecurityOptions = Object.entries(SocialSecurityType).filter(([key]) => isNaN(Number(key))).map(([key, value]) => ({ label: t(`hr.socialSecurityTypes.${key}`), value }));
    const maritalStatusOptions = Object.entries(MaritalStatus).filter(([key]) => isNaN(Number(key))).map(([key, value]) => ({ label: t(`hr.maritalStatuses.${key}`, key), value }));

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            citizenship: CitizenshipType.TRNC,
            socialSecurityType: SocialSecurityType.Standard,
            maritalStatus: MaritalStatus.Single,
            currentSalary: 0,
            transportAmount: 0,
            childCount: 0,
            isSpouseWorking: false,
            isPensioner: false
        }
    });

    useEffect(() => {
        if (open) {
            setActiveTab("personal");
            if (employee) {
                reset({
                    firstName: employee.firstName,
                    lastName: employee.lastName,
                    email: employee.email,
                    identityNumber: employee.identityNumber,
                    citizenship: employee.citizenship,
                    socialSecurityType: employee.socialSecurityType,
                    sgkRiskProfileId: employee.sgkRiskProfileId || '',
                    workPermitNumber: employee.workPermitNumber || '',
                    workPermitExpiryDate: employee.workPermitExpiryDate ? new Date(employee.workPermitExpiryDate).toISOString().split('T')[0] : '',
                    passportNumber: employee.passportNumber || '',
                    passportExpDate: employee.passportExpDate ? new Date(employee.passportExpDate).toISOString().split('T')[0] : '',
                    healthReportExpDate: employee.healthReportExpDate ? new Date(employee.healthReportExpDate).toISOString().split('T')[0] : '',
                    jobTitle: employee.jobTitle,
                    departmentId: employee.departmentId,
                    supervisorId: employee.supervisorId || '',
                    currentSalary: employee.currentSalary,
                    transportAmount: employee.transportAmount || 0,
                    bankName: employee.bankName || '',
                    iban: employee.iban || '',
                    maritalStatus: employee.maritalStatus ?? MaritalStatus.Single,
                    isSpouseWorking: employee.isSpouseWorking ?? false,
                    childCount: employee.childCount ?? 0,
                    isPensioner: employee.isPensioner ?? false
                });
            } else {
                reset({
                    firstName: '', lastName: '', email: '', identityNumber: '',
                    citizenship: CitizenshipType.TRNC, socialSecurityType: SocialSecurityType.Standard, sgkRiskProfileId: '',
                    workPermitNumber: '', workPermitExpiryDate: '', passportNumber: '', passportExpDate: '', healthReportExpDate: '',
                    jobTitle: '', departmentId: departments[0]?.id || '', supervisorId: '',
                    currentSalary: 0, transportAmount: 0, bankName: '', iban: '',
                    maritalStatus: MaritalStatus.Single, isSpouseWorking: false, childCount: 0, isPensioner: false
                });
            }
        }
    }, [open, employee, departments, reset]);

    const onSubmit = async (data: FormValues) => {
        try {
            const commonData = {
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                identityNumber: data.identityNumber,
                citizenship: data.citizenship as CitizenshipType,
                socialSecurityType: data.socialSecurityType as SocialSecurityType,
                sgkRiskProfileId: data.sgkRiskProfileId || undefined,
                workPermitNumber: data.workPermitNumber || undefined,
                workPermitExpiryDate: data.workPermitExpiryDate || undefined,
                passportNumber: data.passportNumber || undefined,
                passportExpDate: data.passportExpDate || undefined,
                healthReportExpDate: data.healthReportExpDate || undefined,
                jobTitle: data.jobTitle,
                departmentId: data.departmentId,
                supervisorId: data.supervisorId || null,
                currentSalary: data.currentSalary,
                transportAmount: data.transportAmount,
                bankName: data.bankName || undefined,
                iban: data.iban || undefined,
                maritalStatus: data.maritalStatus as MaritalStatus,
                isSpouseWorking: data.isSpouseWorking,
                childCount: data.childCount,
                isPensioner: data.isPensioner
            };

            if (employee) {
                await employeeService.update(employee.id, { ...commonData, status: employee.status });
                toast.success(t('hr.employeeUpdated'));
            } else {
                await employeeService.create(commonData);
                toast.success(t('hr.employeeCreated'));
            }
            onClose(true);
        } catch (error: any) {
            console.error(error);
            toast.error(error.message || t('common.error'));
        }
    };

    if (!open) return null;

    const inputClass = "w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all duration-200";
    const labelClass = "block text-sm font-medium mb-1.5 text-[hsl(var(--foreground))]";
    const checkboxClass = "w-4 h-4 text-indigo-600 bg-gray-100 border-gray-300 rounded focus:ring-indigo-500 dark:focus:ring-indigo-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600";

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/60 backdrop-blur-sm transition-opacity" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col border border-[hsl(var(--border))] overflow-hidden animate-in fade-in zoom-in-95 duration-200">

                {/* Header */}
                <div className="bg-gradient-to-r from-[hsl(var(--muted))] to-[hsl(var(--card))] px-8 py-6 border-b border-[hsl(var(--border))] flex items-center justify-between z-10">
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-indigo-100 dark:bg-indigo-900/40 rounded-lg text-indigo-600 dark:text-indigo-400">
                            <User className="w-6 h-6" />
                        </div>
                        <div>
                            <h2 className="text-2xl font-bold tracking-tight">
                                {employee ? t('hr.editEmployee') : t('hr.createEmployee')}
                            </h2>
                            <p className="text-sm text-[hsl(var(--muted-foreground))] mt-1">
                                {employee ? 'Update the details and compliance records for this employee.' : 'Add a new employee and configure their legal profiles.'}
                            </p>
                        </div>
                    </div>
                    <button
                        onClick={() => onClose(false)}
                        className="p-2 rounded-full hover:bg-[hsl(var(--accent))] text-[hsl(var(--muted-foreground))] hover:text-foreground transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                {/* Body Form */}
                <form id="employee-form" onSubmit={handleSubmit(onSubmit)} className="flex-1 overflow-auto bg-[hsl(var(--background))]">
                    <div className="p-8">
                        <Tabs defaultValue="personal" value={activeTab} onValueChange={setActiveTab} className="w-full">

                            <TabsList className="grid w-full grid-cols-3 mb-8 bg-[hsl(var(--muted)/0.5)] p-1 rounded-xl">
                                <TabsTrigger value="personal" className="rounded-lg py-2.5 data-[state=active]:shadow-sm transition-all duration-200">
                                    <User className="w-4 h-4 mr-2" /> {t('hr.personalInfo', 'Personal details')}
                                </TabsTrigger>
                                <TabsTrigger value="job" className="rounded-lg py-2.5 data-[state=active]:shadow-sm transition-all duration-200">
                                    <Briefcase className="w-4 h-4 mr-2" /> {t('hr.jobFinancialInfo', 'Job & Financial')}
                                </TabsTrigger>
                                <TabsTrigger value="legal" className="rounded-lg py-2.5 data-[state=active]:shadow-sm transition-all duration-200">
                                    <Scale className="w-4 h-4 mr-2" /> {t('hr.legalComplianceInfo', 'Legal & Compliance')}
                                </TabsTrigger>
                            </TabsList>

                            {/* TAB 1: Personal */}
                            <TabsContent value="personal" className="space-y-6 animate-in slide-in-from-bottom-2 duration-300">
                                <div className="grid grid-cols-2 gap-6">
                                    <div className="space-y-4">
                                        <div>
                                            <label className={labelClass}>{t('hr.firstName')}</label>
                                            <input {...register('firstName')} className={inputClass} placeholder="Jane" />
                                            {errors.firstName && <span className="text-red-500 text-xs mt-1">{errors.firstName.message}</span>}
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('common.email')}</label>
                                            <input type="email" {...register('email')} className={inputClass} placeholder="jane@example.com" />
                                            {errors.email && <span className="text-red-500 text-xs mt-1">{errors.email.message}</span>}
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.maritalStatus', 'Marital Status')}</label>
                                            <select {...register('maritalStatus', { valueAsNumber: true })} className={inputClass}>
                                                {maritalStatusOptions.map(opt => <option key={opt.label} value={opt.value}>{opt.label}</option>)}
                                            </select>
                                        </div>
                                    </div>
                                    <div className="space-y-4">
                                        <div>
                                            <label className={labelClass}>{t('hr.lastName')}</label>
                                            <input {...register('lastName')} className={inputClass} placeholder="Doe" />
                                            {errors.lastName && <span className="text-red-500 text-xs mt-1">{errors.lastName.message}</span>}
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.identityNumber')}</label>
                                            <input {...register('identityNumber')} className={inputClass} placeholder="ID/TC/Passport" />
                                            {errors.identityNumber && <span className="text-red-500 text-xs mt-1">{errors.identityNumber.message}</span>}
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.childCount', 'Number of Children')}</label>
                                            <input type="number" {...register('childCount', { valueAsNumber: true })} className={inputClass} />
                                        </div>
                                    </div>
                                </div>
                                <div className="flex items-center gap-8 pt-4 border-t border-[hsl(var(--border))]">
                                    <label className="flex items-center gap-3 cursor-pointer group">
                                        <input type="checkbox" {...register('isSpouseWorking')} className={checkboxClass} />
                                        <span className="text-sm font-medium group-hover:text-indigo-600 transition-colors">{t('hr.isSpouseWorking', 'Spouse is working')}</span>
                                    </label>
                                    <label className="flex items-center gap-3 cursor-pointer group">
                                        <input type="checkbox" {...register('isPensioner')} className={checkboxClass} />
                                        <span className="text-sm font-medium group-hover:text-indigo-600 transition-colors">{t('hr.isPensioner', 'Employee is a pensioner')}</span>
                                    </label>
                                </div>
                            </TabsContent>

                            {/* TAB 2: Job & Financial */}
                            <TabsContent value="job" className="space-y-6 animate-in slide-in-from-bottom-2 duration-300">
                                <div className="grid grid-cols-2 gap-8">
                                    {/* Left Col: Job parameters */}
                                    <div className="space-y-4">
                                        <h3 className="font-semibold text-base mb-4 flex items-center gap-2"><Briefcase className="w-4 h-4 text-indigo-500" /> Employment</h3>
                                        <div>
                                            <label className={labelClass}>{t('hr.jobTitle')}</label>
                                            <input {...register('jobTitle')} className={inputClass} placeholder="e.g. Senior Engineer" />
                                            {errors.jobTitle && <span className="text-red-500 text-xs mt-1">{errors.jobTitle.message}</span>}
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.department')}</label>
                                            <select {...register('departmentId')} className={inputClass}>
                                                <option value="">Select Department</option>
                                                {departments.map((dept) => <option key={dept.id} value={dept.id}>{dept.name}</option>)}
                                            </select>
                                            {errors.departmentId && <span className="text-red-500 text-xs mt-1">{errors.departmentId.message}</span>}
                                        </div>
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
                                    {/* Right Col: Financials */}
                                    <div className="space-y-4">
                                        <h3 className="font-semibold text-base mb-4 flex items-center gap-2"><FileText className="w-4 h-4 text-emerald-500" /> Financials & Banking</h3>
                                        <div className="grid grid-cols-2 gap-4">
                                            <div>
                                                <label className={labelClass}>{t('hr.salary')}</label>
                                                <input type="number" step="0.01" {...register('currentSalary', { valueAsNumber: true })} className={inputClass} />
                                                {errors.currentSalary && <span className="text-red-500 text-xs mt-1">{errors.currentSalary.message}</span>}
                                            </div>
                                            <div>
                                                <label className={labelClass}>{t('hr.transportAmount')}</label>
                                                <input type="number" step="0.01" {...register('transportAmount', { valueAsNumber: true })} className={inputClass} />
                                            </div>
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.bankName')}</label>
                                            <input {...register('bankName')} className={inputClass} placeholder="e.g. Isbank" />
                                        </div>
                                        <div>
                                            <label className={labelClass}>{t('hr.iban')}</label>
                                            <input {...register('iban')} className={inputClass} placeholder="TR..." />
                                        </div>
                                    </div>
                                </div>
                            </TabsContent>

                            {/* TAB 3: Legal & Compliance */}
                            <TabsContent value="legal" className="space-y-6 animate-in slide-in-from-bottom-2 duration-300">
                                <div className="grid grid-cols-2 gap-x-8 gap-y-5">
                                    <div>
                                        <label className={labelClass}>{t('hr.citizenship')}</label>
                                        <select {...register('citizenship', { valueAsNumber: true })} className={inputClass}>
                                            {citizenshipOptions.map(opt => <option key={opt.label} value={opt.value}>{opt.label}</option>)}
                                        </select>
                                    </div>

                                    <div>
                                        <label className={labelClass}>{t('hr.socialSecurityType')}</label>
                                        <select {...register('socialSecurityType', { valueAsNumber: true })} className={inputClass}>
                                            {socialSecurityOptions.map(opt => <option key={opt.label} value={opt.value}>{opt.label}</option>)}
                                        </select>
                                    </div>

                                    <div>
                                        <label className={labelClass}>{t('hr.sgkRiskProfileId', 'SGK Risk Profile')}</label>
                                        <select {...register('sgkRiskProfileId')} className={inputClass}>
                                            <option value="">{t('common.default', 'Default (From Dept)')}</option>
                                            {/* In a real app we'd fetch actual SGK risk profiles here */}
                                        </select>
                                    </div>
                                    <div>
                                        <label className={labelClass}>{t('hr.healthReportExpDate', 'Health Report Expiration')}</label>
                                        <input type="date" {...register('healthReportExpDate')} className={inputClass} />
                                    </div>

                                    <div className="col-span-2 pt-4 border-t border-[hsl(var(--border))]">
                                        <h4 className="text-sm font-semibold mb-4 text-[hsl(var(--muted-foreground))] uppercase tracking-wider">Foreign Worker Documents</h4>
                                        <div className="grid grid-cols-2 gap-x-8 gap-y-4">
                                            <div>
                                                <label className={labelClass}>{t('hr.passportNumber', 'Passport Number')}</label>
                                                <input {...register('passportNumber')} className={inputClass} />
                                            </div>
                                            <div>
                                                <label className={labelClass}>{t('hr.passportExpDate', 'Passport Expiration Date')}</label>
                                                <input type="date" {...register('passportExpDate')} className={inputClass} />
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
                                </div>
                            </TabsContent>

                        </Tabs>
                    </div>
                </form>

                {/* Footer Actions */}
                <div className="bg-[hsl(var(--card))] px-8 py-5 border-t border-[hsl(var(--border))] flex justify-end gap-3 rounded-b-2xl">
                    <Button type="button" variant="outline" className="w-24" onClick={() => onClose(false)}>
                        {t('common.cancel')}
                    </Button>
                    <Button type="submit" form="employee-form" className="w-32 shadow-md bg-indigo-600 hover:bg-indigo-700 text-white transition-all" disabled={isSubmitting}>
                        {isSubmitting ? (
                            <span className="flex items-center gap-2">
                                <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> {t('common.saving')}
                            </span>
                        ) : (
                            employee ? t('common.save') : t('common.create')
                        )}
                    </Button>
                </div>

            </div>
        </div>
    );
}
