import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, Input, useToast } from '@/components/ui';
import { api } from '@/lib/api';

const schema = z.object({
    name: z.string().min(2, 'Required'),
    description: z.string().optional(),
    employeePremiumRate: z.number().min(0, "Must be positive"),
    employerPremiumRate: z.number().min(0, "Must be positive"),
    unemploymentEmployeeRate: z.number().min(0, "Must be positive"),
    unemploymentEmployerRate: z.number().min(0, "Must be positive"),
    isActive: z.boolean()
});

export function SgkRiskProfileDialog({ open, onClose, data }: any) {
    const { t } = useTranslation();
    const toast = useToast();

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm({
        resolver: zodResolver(schema),
        defaultValues: {
            employeePremiumRate: 9, employerPremiumRate: 11,
            unemploymentEmployeeRate: 1, unemploymentEmployerRate: 2,
            isActive: true
        }
    });

    useEffect(() => {
        if (open) {
            if (data) reset(data);
            else reset({ name: '', description: '', employeePremiumRate: 9, employerPremiumRate: 11, unemploymentEmployeeRate: 1, unemploymentEmployerRate: 2, isActive: true });
        }
    }, [open, data, reset]);

    const onSubmit = async (formData: any) => {
        try {
            if (data) {
                await api.put(`/hr/sgk-risk-profiles/${data.id}`, formData);
                toast.success(t('common.saved'));
            } else {
                await api.post('/hr/sgk-risk-profiles', formData);
                toast.success(t('common.created'));
            }
            onClose(true);
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    if (!open) return null;

    const checkboxClass = "w-4 h-4 text-indigo-600 bg-gray-100 border-gray-300 rounded focus:ring-indigo-500 mt-1 cursor-pointer";

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{data ? 'Edit SGK Risk Profile' : 'New SGK Risk Profile'}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]"><X className="w-5 h-5" /></button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <Input label={t('hr.name')} {...register('name')} error={errors.name?.message as string} className="col-span-2" />
                        <Input label={t('hr.description')} {...register('description')} className="col-span-2" />

                        <div className="space-y-4 border p-4 rounded-lg bg-[hsl(var(--muted)/0.3)]">
                            <h3 className="font-semibold text-sm border-b pb-2">Employee Share (%)</h3>
                            <Input type="number" step="0.5" label="Premium Rate" {...register('employeePremiumRate', { valueAsNumber: true })} error={errors.employeePremiumRate?.message as string} />
                            <Input type="number" step="0.5" label="Unemployment Rate" {...register('unemploymentEmployeeRate', { valueAsNumber: true })} error={errors.unemploymentEmployeeRate?.message as string} />
                        </div>
                        <div className="space-y-4 border p-4 rounded-lg bg-[hsl(var(--muted)/0.3)]">
                            <h3 className="font-semibold text-sm border-b pb-2">Employer Share (%)</h3>
                            <Input type="number" step="0.5" label="Premium Rate" {...register('employerPremiumRate', { valueAsNumber: true })} error={errors.employerPremiumRate?.message as string} />
                            <Input type="number" step="0.5" label="Unemployment Rate" {...register('unemploymentEmployerRate', { valueAsNumber: true })} error={errors.unemploymentEmployerRate?.message as string} />
                        </div>
                    </div>

                    <label className="flex items-center gap-2 cursor-pointer pt-2">
                        <input type="checkbox" {...register('isActive')} className={checkboxClass} />
                        <span className="text-sm font-medium">Is Active</span>
                    </label>

                    <div className="flex justify-end gap-2 pt-4 border-t">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" isLoading={isSubmitting}>{t('common.save')}</Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
