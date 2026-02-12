import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import type { Employee } from '@/types/hr';
import { api } from '@/lib/api';

interface ManualAttendanceDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    employees: Employee[];
}

export function ManualAttendanceDialog({ open, onClose, employees }: ManualAttendanceDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        employeeId: '',
        type: 'check-in',
        time: new Date().toISOString().slice(0, 16), // YYYY-MM-DDTHH:mm format for datetime-local
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!form.employeeId) {
            toast.error(t('hr.selectEmployeeRequired'));
            return;
        }

        setIsSubmitting(true);
        try {
            const payload = {
                employeeId: form.employeeId,
                time: new Date(form.time).toISOString(),
            };

            await api.post(`/hr/attendance/${form.type}`, payload);

            toast.success(t('hr.attendanceSaved'));
            onClose(true);
        } catch (error: any) {
            // Error handling is improved in api utility, but we can still catch specific messages
            console.error(error);
            toast.error(t('common.error'));
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md overflow-hidden border border-[hsl(var(--border))]">
                <div className="bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {t('hr.manualEntry')}
                    </h2>
                    <button
                        onClick={() => onClose(false)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('hr.employee')}</label>
                        <select
                            value={form.employeeId}
                            onChange={(e) => setForm({ ...form, employeeId: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            required
                        >
                            <option value="">{t('hr.selectEmployee')}</option>
                            {employees.map((emp) => (
                                <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName}</option>
                            ))}
                        </select>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('hr.actionType')}</label>
                        <div className="grid grid-cols-2 gap-2">
                            <button
                                type="button"
                                onClick={() => setForm({ ...form, type: 'check-in' })}
                                className={`px-4 py-2 rounded-lg border text-center transition-colors ${form.type === 'check-in'
                                    ? 'bg-blue-100 border-blue-500 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
                                    : 'border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))]'
                                    }`}
                            >
                                {t('hr.checkIn')}
                            </button>
                            <button
                                type="button"
                                onClick={() => setForm({ ...form, type: 'check-out' })}
                                className={`px-4 py-2 rounded-lg border text-center transition-colors ${form.type === 'check-out'
                                    ? 'bg-amber-100 border-amber-500 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400'
                                    : 'border-[hsl(var(--border))] hover:bg-[hsl(var(--accent))]'
                                    }`}
                            >
                                {t('hr.checkOut')}
                            </button>
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('hr.time')}</label>
                        <input
                            type="datetime-local"
                            value={form.time}
                            onChange={(e) => setForm({ ...form, time: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            required
                        />
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('common.save')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
