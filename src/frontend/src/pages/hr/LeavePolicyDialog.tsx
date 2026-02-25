import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, Input, Select, useToast } from '@/components/ui';
import { api } from '@/lib/api';

const schema = z.object({
    name: z.string().min(2, 'Required'),
    description: z.string().optional(),
    defaultDays: z.number().min(0, "Must be positive"),
    maxDaysCarryForward: z.number().min(0, "Must be positive"),
    requiresApproval: z.boolean(),
    genderRestriction: z.number()
});

export function LeavePolicyDialog({ open, onClose, policy }: any) {
    const { t } = useTranslation();
    const toast = useToast();

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm({
        resolver: zodResolver(schema),
        defaultValues: {
            defaultDays: 14,
            maxDaysCarryForward: 0,
            requiresApproval: true,
            genderRestriction: 0
        }
    });

    useEffect(() => {
        if (open) {
            if (policy) reset({ ...policy, genderRestriction: policy.genderRestriction ?? 0 });
            else reset({ name: '', description: '', defaultDays: 14, maxDaysCarryForward: 0, requiresApproval: true, genderRestriction: 0 });
        }
    }, [open, policy, reset]);

    const onSubmit = async (data: any) => {
        try {
            if (policy) {
                await api.put(`/hr/leave-policies/${policy.id}`, data);
                toast.success(t('common.saved'));
            } else {
                await api.post('/hr/leave-policies', data);
                toast.success(t('common.created'));
            }
            onClose(true);
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    if (!open) return null;

    const genderOptions = [
        { value: "0", label: "None" },
        { value: "1", label: "Male Only" },
        { value: "2", label: "Female Only" }
    ];

    const checkboxClass = "w-4 h-4 text-indigo-600 bg-gray-100 border-gray-300 rounded focus:ring-indigo-500 transition-colors cursor-pointer mt-1";

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{policy ? 'Edit Leave Policy' : 'New Leave Policy'}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
                    <Input label={t('hr.name')} {...register('name')} error={errors.name?.message as string} />
                    <Input label={t('hr.description')} {...register('description')} />

                    <div className="grid grid-cols-2 gap-4">
                        <Input type="number" step="0.5" label={t('hr.defaultDays', 'Default Days')} {...register('defaultDays', { valueAsNumber: true })} error={errors.defaultDays?.message as string} />
                        <Input type="number" step="0.5" label={t('hr.maxCarryForward', 'Max Carry')} {...register('maxDaysCarryForward', { valueAsNumber: true })} error={errors.maxDaysCarryForward?.message as string} />
                    </div>

                    <Select
                        label={t('hr.genderRestriction', 'Gender Constraint')}
                        options={genderOptions}
                        {...register('genderRestriction', { valueAsNumber: true })}
                        error={errors.genderRestriction?.message as string}
                    />

                    <label className="flex items-center gap-3 cursor-pointer pt-2">
                        <input type="checkbox" {...register('requiresApproval')} className={checkboxClass} />
                        <span className="text-sm font-medium">{t('hr.requiresApproval', 'Requires Approval')}</span>
                    </label>

                    <div className="flex justify-end gap-2 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" isLoading={isSubmitting}>{t('common.save')}</Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
