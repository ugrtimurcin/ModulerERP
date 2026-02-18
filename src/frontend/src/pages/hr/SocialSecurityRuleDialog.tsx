import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { socialSecurityRuleService } from '@/services/hr/socialSecurityRuleService';
import type { SocialSecurityRule, CreateSocialSecurityRuleDto, UpdateSocialSecurityRuleDto } from '@/services/hr/socialSecurityRuleService';
import { CitizenshipType, SocialSecurityType } from '@/types/hr';



const formSchema = z.object({
    name: z.string().min(2, "Name is required"),
    citizenshipType: z.number(),
    socialSecurityType: z.number(),
    employeeRate: z.number().min(0).max(100),
    employerRate: z.number().min(0).max(100),
    providentFundEmployeeRate: z.number().min(0).max(100),
    providentFundEmployerRate: z.number().min(0).max(100),
    unemploymentInsuranceEmployeeRate: z.number().min(0).max(100),
    unemploymentInsuranceEmployerRate: z.number().min(0).max(100),
    effectiveFrom: z.string().min(1, "Effective date is required"),
    effectiveTo: z.string().optional().nullable()
});

type FormValues = z.infer<typeof formSchema>;

interface SocialSecurityRuleDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    rule: SocialSecurityRule | null;
}

export function SocialSecurityRuleDialog({ open, onClose, rule }: SocialSecurityRuleDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();

    // Enum helpers
    const citizenshipOptions = Object.entries(CitizenshipType)
        .filter(([key]) => isNaN(Number(key))) // Filter out numeric keys if enum is mixed
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
            citizenshipType: CitizenshipType.TRNC,
            socialSecurityType: SocialSecurityType.Standard,
            employeeRate: 0,
            employerRate: 0,
            providentFundEmployeeRate: 0,
            providentFundEmployerRate: 0,
            unemploymentInsuranceEmployeeRate: 0,
            unemploymentInsuranceEmployerRate: 0,
            effectiveFrom: new Date().toISOString().split('T')[0]
        }
    });

    useEffect(() => {
        if (open) {
            if (rule) {
                reset({
                    name: rule.name,
                    citizenshipType: rule.citizenshipType,
                    socialSecurityType: rule.socialSecurityType,
                    employeeRate: rule.employeeRate * 100,
                    employerRate: rule.employerRate * 100,
                    providentFundEmployeeRate: rule.providentFundEmployeeRate * 100,
                    providentFundEmployerRate: rule.providentFundEmployerRate * 100,
                    unemploymentInsuranceEmployeeRate: rule.unemploymentInsuranceEmployeeRate * 100,
                    unemploymentInsuranceEmployerRate: rule.unemploymentInsuranceEmployerRate * 100,
                    effectiveFrom: rule.effectiveFrom.split('T')[0],
                    effectiveTo: rule.effectiveTo ? rule.effectiveTo.split('T')[0] : null
                });
            } else {
                reset({
                    name: '',
                    citizenshipType: CitizenshipType.TRNC,
                    socialSecurityType: SocialSecurityType.Standard,
                    employeeRate: 0,
                    employerRate: 0,
                    providentFundEmployeeRate: 0,
                    providentFundEmployerRate: 0,
                    unemploymentInsuranceEmployeeRate: 0,
                    unemploymentInsuranceEmployerRate: 0,
                    effectiveFrom: new Date().toISOString().split('T')[0],
                    effectiveTo: null
                });
            }
        }
    }, [open, rule, reset]);

    const onSubmit = async (data: FormValues) => {
        try {
            const commonData = {
                name: data.name,
                citizenshipType: data.citizenshipType,
                socialSecurityType: data.socialSecurityType,
                employeeRate: data.employeeRate / 100,
                employerRate: data.employerRate / 100,
                providentFundEmployeeRate: data.providentFundEmployeeRate / 100,
                providentFundEmployerRate: data.providentFundEmployerRate / 100,
                unemploymentInsuranceEmployeeRate: data.unemploymentInsuranceEmployeeRate / 100,
                unemploymentInsuranceEmployerRate: data.unemploymentInsuranceEmployerRate / 100,
                effectiveFrom: data.effectiveFrom,
                effectiveTo: data.effectiveTo || null
            };

            if (rule) {
                const updateDto: UpdateSocialSecurityRuleDto = {
                    ...commonData,
                    citizenshipType: commonData.citizenshipType as CitizenshipType,
                    socialSecurityType: commonData.socialSecurityType as SocialSecurityType
                };
                await socialSecurityRuleService.update(rule.id, updateDto);
                toast.success(t('hr.ssRuleUpdated'));
            } else {
                const createDto: CreateSocialSecurityRuleDto = {
                    ...commonData,
                    citizenshipType: commonData.citizenshipType as CitizenshipType,
                    socialSecurityType: commonData.socialSecurityType as SocialSecurityType
                };
                await socialSecurityRuleService.create(createDto);
                toast.success(t('hr.ssRuleCreated'));
            }
            onClose(true);
        } catch (error: any) {
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
                        {rule ? t('hr.editSSRule') : t('hr.createSSRule')}
                    </h2>
                    <button
                        onClick={() => onClose(false)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
                    <div>
                        <label className={labelClass}>{t('common.name')}</label>
                        <input {...register('name')} className={inputClass} placeholder="e.g. Standard Rules 2026" />
                        {errors.name && <span className="text-red-500 text-xs">{errors.name.message}</span>}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.citizenship')}</label>
                            <select {...register('citizenshipType', { valueAsNumber: true })} className={inputClass}>
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
                    </div>

                    <div className="grid grid-cols-2 gap-4 border-t border-[hsl(var(--border))] pt-4">
                        <h3 className="col-span-2 font-medium text-sm text-[hsl(var(--muted-foreground))]">Social Security Rates</h3>
                        <div>
                            <label className={labelClass}>{t('hr.employeeRate')} (%)</label>
                            <input type="number" step="0.01" {...register('employeeRate', { valueAsNumber: true })} className={inputClass} />
                            {errors.employeeRate && <span className="text-red-500 text-xs">{errors.employeeRate.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.employerRate')} (%)</label>
                            <input type="number" step="0.01" {...register('employerRate', { valueAsNumber: true })} className={inputClass} />
                            {errors.employerRate && <span className="text-red-500 text-xs">{errors.employerRate.message}</span>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4 border-t border-[hsl(var(--border))] pt-4">
                        <h3 className="col-span-2 font-medium text-sm text-[hsl(var(--muted-foreground))]">Provident Fund Rates</h3>
                        <div>
                            <label className={labelClass}>{t('hr.pfEmployeeRate')} (%)</label>
                            <input type="number" step="0.01" {...register('providentFundEmployeeRate', { valueAsNumber: true })} className={inputClass} />
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.pfEmployerRate')} (%)</label>
                            <input type="number" step="0.01" {...register('providentFundEmployerRate', { valueAsNumber: true })} className={inputClass} />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4 border-t border-[hsl(var(--border))] pt-4">
                        <h3 className="col-span-2 font-medium text-sm text-[hsl(var(--muted-foreground))]">Unemployment Insurance</h3>
                        <div>
                            <label className={labelClass}>{t('hr.uiEmployeeRate')} (%)</label>
                            <input type="number" step="0.01" {...register('unemploymentInsuranceEmployeeRate', { valueAsNumber: true })} className={inputClass} />
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.uiEmployerRate')} (%)</label>
                            <input type="number" step="0.01" {...register('unemploymentInsuranceEmployerRate', { valueAsNumber: true })} className={inputClass} />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4 pt-4 border-t border-[hsl(var(--border))]">
                        <div>
                            <label className={labelClass}>{t('common.effectiveFrom')}</label>
                            <input type="date" {...register('effectiveFrom')} className={inputClass} />
                            {errors.effectiveFrom && <span className="text-red-500 text-xs">{errors.effectiveFrom.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('common.effectiveTo')}</label>
                            <input type="date" {...register('effectiveTo')} className={inputClass} />
                            {errors.effectiveTo && <span className="text-red-500 text-xs">{errors.effectiveTo.message}</span>}
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : (rule ? t('common.save') : t('common.create'))}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
