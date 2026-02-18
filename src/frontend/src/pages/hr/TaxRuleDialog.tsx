import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { taxRuleService } from '@/services/hr/taxRuleService';
import type { TaxRule, CreateTaxRuleDto, UpdateTaxRuleDto } from '@/services/hr/taxRuleService';

const formSchema = z.object({
    name: z.string().min(2, "Name is required"),
    lowerLimit: z.number().min(0, "Lower limit must be non-negative"),
    upperLimit: z.string().optional().nullable(),
    rate: z.number().min(0).max(100, "Rate must be between 0 and 100"),
    order: z.number().int(),
    effectiveFrom: z.string().min(1, "Effective date is required"),
    effectiveTo: z.string().optional().nullable()
});

type FormValues = z.infer<typeof formSchema>;

interface TaxRuleDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    rule: TaxRule | null;
}

export function TaxRuleDialog({ open, onClose, rule }: TaxRuleDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            name: '',
            lowerLimit: 0,
            upperLimit: '',
            rate: 0,
            order: 0,
            effectiveFrom: new Date().toISOString().split('T')[0]
        }
    });

    useEffect(() => {
        if (open) {
            if (rule) {
                reset({
                    name: rule.name,
                    lowerLimit: rule.lowerLimit,
                    upperLimit: rule.upperLimit ? rule.upperLimit.toString() : '',
                    rate: rule.rate * 100, // Convert fraction to percentage
                    order: rule.order,
                    effectiveFrom: rule.effectiveFrom.split('T')[0],
                    effectiveTo: rule.effectiveTo ? rule.effectiveTo.split('T')[0] : null
                });
            } else {
                reset({
                    name: '',
                    lowerLimit: 0,
                    upperLimit: '',
                    rate: 0,
                    order: 0,
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
                lowerLimit: data.lowerLimit,
                upperLimit: data.upperLimit ? Number(data.upperLimit) : null,
                rate: data.rate / 100, // Convert percentage to fraction
                order: data.order,
                effectiveFrom: data.effectiveFrom,
                effectiveTo: data.effectiveTo || null
            };

            if (rule) {
                const updateDto: UpdateTaxRuleDto = {
                    ...commonData
                };
                await taxRuleService.update(rule.id, updateDto);
                toast.success(t('hr.taxRuleUpdated'));
            } else {
                const createDto: CreateTaxRuleDto = {
                    ...commonData
                };
                await taxRuleService.create(createDto);
                toast.success(t('hr.taxRuleCreated'));
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between z-10">
                    <h2 className="text-xl font-semibold">
                        {rule ? t('hr.editTaxRule') : t('hr.createTaxRule')}
                    </h2>
                    <button
                        onClick={() => onClose(false)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit(onSubmit as any)} className="p-6 space-y-4">
                    <div>
                        <label className={labelClass}>{t('common.name')}</label>
                        <input {...register('name')} className={inputClass} placeholder="e.g. Bracket 1" />
                        {errors.name && <span className="text-red-500 text-xs">{errors.name.message}</span>}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.lowerLimit')}</label>
                            <input type="number" step="0.01" {...register('lowerLimit', { valueAsNumber: true })} className={inputClass} />
                            {errors.lowerLimit && <span className="text-red-500 text-xs">{errors.lowerLimit.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('hr.upperLimit')}</label>
                            <input
                                type="number"
                                step="0.01"
                                {...register('upperLimit')}
                                className={inputClass}
                                placeholder={t('common.unlimited')}
                            />
                            {errors.upperLimit && <span className="text-red-500 text-xs">{errors.upperLimit.message}</span>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className={labelClass}>{t('hr.rate')} (%)</label>
                            <input type="number" step="0.01" {...register('rate', { valueAsNumber: true })} className={inputClass} />
                            {errors.rate && <span className="text-red-500 text-xs">{errors.rate.message}</span>}
                        </div>
                        <div>
                            <label className={labelClass}>{t('common.order')}</label>
                            <input type="number" {...register('order', { valueAsNumber: true })} className={inputClass} />
                            {errors.order && <span className="text-red-500 text-xs">{errors.order.message}</span>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
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
