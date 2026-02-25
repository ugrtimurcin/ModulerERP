import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { X } from 'lucide-react';
import { Button, Input, Select, Combobox, useToast } from '@/components/ui';

import { api } from '@/lib/api';
import { employeeService } from '@/services/hr/employeeService';

const schema = z.object({
    employeeId: z.string().min(1, 'Required'),
    leavePolicyId: z.string().min(1, 'Required'),
    startDate: z.string().min(1, 'Required'),
    endDate: z.string().min(1, 'Required'),
    reason: z.string().optional(),
}).refine(data => new Date(data.endDate) >= new Date(data.startDate), {
    message: "End date must be after start date",
    path: ["endDate"]
});

type FormData = z.infer<typeof schema>;

interface Props {
    open: boolean;
    onClose: (saved: boolean) => void;
}

export function LeaveRequestDialog({ open, onClose }: Props) {
    const { t } = useTranslation();
    const toast = useToast();
    const [employees, setEmployees] = useState<{ id: string, firstName: string, lastName: string }[]>([]);
    const [leavePolicies, setLeavePolicies] = useState<{ id: string, name: string }[]>([]);

    const { register, control, handleSubmit, watch, formState: { errors, isSubmitting }, reset } = useForm<FormData>({
        resolver: zodResolver(schema),
        defaultValues: {
            leavePolicyId: ''
        }
    });

    const startDate = watch('startDate');
    const endDate = watch('endDate');

    // Calculate days count using native Date
    const getDaysCount = () => {
        if (!startDate || !endDate) return 0;
        const start = new Date(startDate);
        const end = new Date(endDate);
        const diffTime = Math.abs(end.getTime() - start.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays + 1; // Inclusive
    };

    const daysCount = getDaysCount();

    useEffect(() => {
        if (open) {
            reset();
            loadData();
        }
    }, [open, reset]);

    const loadData = async () => {
        try {
            const [empRes, lpRes] = await Promise.all([
                employeeService.getLookup(),
                api.get('/hr/leave-policies')
            ]);

            const cleanEmp = (Array.isArray(empRes) ? empRes : []).map((e: any) => ({
                id: e.id || e.Id,
                firstName: e.firstName || e.FirstName,
                lastName: e.lastName || e.LastName
            }));
            setEmployees(cleanEmp);

            const policies = (lpRes as any).data || lpRes; // Handle axios vs custom api wrapper
            const cleanLp = (Array.isArray(policies) ? policies : policies.items || []).map((p: any) => ({
                id: p.id || p.Id,
                name: p.name || p.Name
            }));
            setLeavePolicies(cleanLp);

        } catch (error) {
            console.error(error);
            toast.error(t('common.error'));
        }
    };

    const onSubmit = async (data: FormData) => {
        try {
            await api.post('/hr/leave-requests', {
                ...data,
                daysCount: daysCount > 0 ? daysCount : 1
            });
            toast.success(t('hr.leaveRequestCreated'));
            onClose(true);
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'));
        }
    };

    if (!open) return null;

    const leavePolicyOptions = leavePolicies.map(p => ({
        value: p.id,
        label: p.name
    }));

    const employeeOptions = employees.map(e => ({
        value: e.id,
        label: `${e.firstName} ${e.lastName}`
    }));

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('hr.createLeaveRequest')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
                    <Controller
                        name="employeeId"
                        control={control}
                        render={({ field }) => (
                            <Combobox
                                label={t('hr.employee')}
                                options={employeeOptions}
                                value={field.value}
                                onChange={field.onChange}
                                placeholder={t('hr.selectEmployee')}
                                searchPlaceholder={t('common.searchPlaceholder')}
                                error={errors.employeeId?.message}
                            />
                        )}
                    />

                    <Select
                        label={t('hr.leavePolicy', 'Leave Policy')}
                        options={leavePolicyOptions}
                        {...register('leavePolicyId')}
                        error={errors.leavePolicyId?.message}
                    />

                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            type="date"
                            label={t('hr.startDate')}
                            {...register('startDate')}
                            error={errors.startDate?.message}
                        />
                        <Input
                            type="date"
                            label={t('hr.endDate')}
                            {...register('endDate')}
                            error={errors.endDate?.message}
                        />
                    </div>

                    {daysCount > 0 && (
                        <div className="text-sm text-[hsl(var(--muted-foreground))] text-right">
                            {t('hr.totalDays')}: <span className="font-bold">{daysCount}</span>
                        </div>
                    )}

                    <Input
                        label={t('hr.reason')}
                        {...register('reason')}
                        placeholder={t('hr.reasonPlaceholder')}
                        error={errors.reason?.message}
                    />

                    <div className="flex justify-end gap-2 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
