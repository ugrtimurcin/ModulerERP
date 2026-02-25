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
    type: z.number(),
    category: z.number(),
    multiplier: z.number().min(0, "Must be positive"),
    isTaxable: z.boolean(),
    isSgkSubject: z.boolean()
});

export function EarningDeductionTypeDialog({ open, onClose, data }: any) {
    const { t } = useTranslation();
    const toast = useToast();

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm({
        resolver: zodResolver(schema),
        defaultValues: { type: 0, category: 1, multiplier: 1, isTaxable: true, isSgkSubject: true }
    });

    useEffect(() => {
        if (open) {
            if (data) reset(data);
            else reset({ name: '', type: 0, category: 1, multiplier: 1, isTaxable: true, isSgkSubject: true });
        }
    }, [open, data, reset]);

    const onSubmit = async (formData: any) => {
        try {
            if (data) {
                await api.put(`/hr/earning-deduction-types/${data.id}`, formData);
                toast.success(t('common.saved'));
            } else {
                await api.post('/hr/earning-deduction-types', formData);
                toast.success(t('common.created'));
            }
            onClose(true);
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    if (!open) return null;

    const typeOptions = [{ value: "0", label: "Earning" }, { value: "1", label: "Deduction" }];
    const categoryOptions = [
        { value: "0", label: "Basic Salary" }, { value: "1", label: "Bonus" },
        { value: "2", label: "Overtime" }, { value: "3", label: "Tax" }, { value: "4", label: "Penalty" }
    ];
    const checkboxClass = "w-4 h-4 text-indigo-600 bg-gray-100 border-gray-300 rounded focus:ring-indigo-500 mt-1 cursor-pointer";

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{data ? 'Edit Type' : 'New Type'}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]"><X className="w-5 h-5" /></button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
                    <Input label={t('hr.name')} {...register('name')} error={errors.name?.message as string} />

                    <div className="grid grid-cols-2 gap-4">
                        <Select label="Type" options={typeOptions} {...register('type', { valueAsNumber: true })} />
                        <Select label="Category" options={categoryOptions} {...register('category', { valueAsNumber: true })} />
                    </div>

                    <Input type="number" step="0.1" label="Multiplier (e.g. 1.5x for Overtime)" {...register('multiplier', { valueAsNumber: true })} error={errors.multiplier?.message as string} />

                    <div className="flex gap-6 pt-2">
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input type="checkbox" {...register('isTaxable')} className={checkboxClass} />
                            <span className="text-sm font-medium">Is Taxable?</span>
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input type="checkbox" {...register('isSgkSubject')} className={checkboxClass} />
                            <span className="text-sm font-medium">Subject to SGK?</span>
                        </label>
                    </div>

                    <div className="flex justify-end gap-2 pt-4 border-t">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" isLoading={isSubmitting}>{t('common.save')}</Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
