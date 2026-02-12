import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

import type { Employee, Department } from '@/types/hr';

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
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        firstName: '',
        lastName: '',
        email: '',
        identityNumber: '',
        citizenshipNumber: '',
        workPermitNumber: '',
        workPermitExpiryDate: '',
        jobTitle: '',
        departmentId: '',
        supervisorId: '',
        currentSalary: 0,
        bankName: '',
        iban: '',
    });

    useEffect(() => {
        if (employee) {
            setForm({
                firstName: employee.firstName,
                lastName: employee.lastName,
                email: employee.email,
                identityNumber: employee.identityNumber,
                citizenshipNumber: employee.citizenshipNumber || '',
                workPermitNumber: employee.workPermitNumber || '',
                workPermitExpiryDate: employee.workPermitExpiryDate || '',
                jobTitle: employee.jobTitle,
                departmentId: employee.departmentId,
                supervisorId: employee.supervisorId || '',
                currentSalary: employee.currentSalary,
                bankName: employee.bankName || '',
                iban: employee.iban || '',
            });
        } else {
            setForm({
                firstName: '',
                lastName: '',
                email: '',
                identityNumber: '',
                citizenshipNumber: '',
                workPermitNumber: '',
                workPermitExpiryDate: '',
                jobTitle: '',
                departmentId: departments[0]?.id || '',
                supervisorId: '',
                currentSalary: 0,
                bankName: '',
                iban: '',
            });
        }
    }, [employee, departments, open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            const payload = {
                ...form,
                supervisorId: form.supervisorId || null,
            };

            if (employee) {
                await api.put(`/hr/employees/${employee.id}`, payload);
                toast.success(t('hr.employeeUpdated'));
            } else {
                await api.post('/hr/employees', payload);
                toast.success(t('hr.employeeCreated'));
            }
            onClose(true);
        } catch (error: any) {
            toast.error(error.message || t('common.error'));
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
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

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.firstName')}</label>
                            <input
                                type="text"
                                value={form.firstName}
                                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.lastName')}</label>
                            <input
                                type="text"
                                value={form.lastName}
                                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('common.email')}</label>
                            <input
                                type="email"
                                value={form.email}
                                onChange={(e) => setForm({ ...form, email: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                    </div>

                    <div className="space-y-4">
                        <h3 className="text-lg font-medium border-b border-[hsl(var(--border))] pb-2">{t('hr.legalInfo')}</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.identityNumber')}</label>
                                <input
                                    type="text"
                                    value={form.identityNumber}
                                    onChange={(e) => setForm({ ...form, identityNumber: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                    required
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.citizenshipNumber')}</label>
                                <input
                                    type="text"
                                    value={form.citizenshipNumber}
                                    onChange={(e) => setForm({ ...form, citizenshipNumber: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.workPermitNumber')}</label>
                                <input
                                    type="text"
                                    value={form.workPermitNumber}
                                    onChange={(e) => setForm({ ...form, workPermitNumber: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.workPermitExpiryDate')}</label>
                                <input
                                    type="date"
                                    value={form.workPermitExpiryDate ? new Date(form.workPermitExpiryDate).toISOString().split('T')[0] : ''}
                                    onChange={(e) => setForm({ ...form, workPermitExpiryDate: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                        </div>
                    </div>

                    <div className="space-y-4">
                        <h3 className="text-lg font-medium border-b border-[hsl(var(--border))] pb-2">{t('hr.bankInfo')}</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.bankName')}</label>
                                <input
                                    type="text"
                                    value={form.bankName}
                                    onChange={(e) => setForm({ ...form, bankName: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('hr.iban')}</label>
                                <input
                                    type="text"
                                    value={form.iban}
                                    onChange={(e) => setForm({ ...form, iban: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.jobTitle')}</label>
                            <input
                                type="text"
                                value={form.jobTitle}
                                onChange={(e) => setForm({ ...form, jobTitle: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.department')}</label>
                            <select
                                value={form.departmentId}
                                onChange={(e) => setForm({ ...form, departmentId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            >
                                <option value="">Select Department</option>
                                {departments.map((dept) => (
                                    <option key={dept.id} value={dept.id}>{dept.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.supervisor')}</label>
                            <select
                                value={form.supervisorId}
                                onChange={(e) => setForm({ ...form, supervisorId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            >
                                <option value="">No Supervisor</option>
                                {employees.filter(e => e.id !== employee?.id).map((emp) => (
                                    <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName}</option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('hr.salary')}</label>
                            <input
                                type="number"
                                value={form.currentSalary}
                                onChange={(e) => setForm({ ...form, currentSalary: parseFloat(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                min="0"
                                step="0.01"
                            />
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
