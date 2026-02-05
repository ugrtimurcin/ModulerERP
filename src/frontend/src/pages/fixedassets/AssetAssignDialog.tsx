import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface Employee {
    id: string;
    firstName: string;
    lastName: string;
}

interface AssetAssignDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    assetId: string;
}

const API_BASE = '/api/fixedassets';

export function AssetAssignDialog({ open, onClose, assetId }: AssetAssignDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [employees, setEmployees] = useState<Employee[]>([]);

    const [form, setForm] = useState({
        employeeId: '',
        locationId: '',
        startValue: 0,
        condition: '',
    });

    useEffect(() => {
        if (open) {
            fetch('/api/hr/employees', { cache: 'no-store' })
                .then(res => res.json())
                .then(data => setEmployees(data))
                .catch(() => { });
            setForm({ employeeId: '', locationId: '', startValue: 0, condition: '' });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!form.employeeId) {
            toast.error(t('common.fillRequiredFields'));
            return;
        }

        setIsSubmitting(true);
        try {
            const res = await fetch(`${API_BASE}/assign`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    assetId,
                    employeeId: form.employeeId || null,
                    locationId: form.locationId || null,
                    startValue: form.startValue,
                    condition: form.condition || null,
                }),
            });

            if (res.ok) {
                toast.success(t('fixedAssets.assetAssigned'));
                onClose(true);
            } else {
                toast.error(t('common.error'));
            }
        } catch { toast.error(t('common.error')); }
        finally { setIsSubmitting(false); }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('fixedAssets.assignAsset')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.employee')} *</label>
                        <select
                            value={form.employeeId}
                            onChange={(e) => setForm({ ...form, employeeId: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            required
                        >
                            <option value="">{t('common.select')}</option>
                            {employees.map((emp) => (
                                <option key={emp.id} value={emp.id}>{emp.firstName} {emp.lastName}</option>
                            ))}
                        </select>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.currentMeter')}</label>
                        <input
                            type="number"
                            step="0.01"
                            value={form.startValue}
                            onChange={(e) => setForm({ ...form, startValue: parseFloat(e.target.value) || 0 })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.condition')}</label>
                        <select
                            value={form.condition}
                            onChange={(e) => setForm({ ...form, condition: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value="">{t('common.select')}</option>
                            <option value="excellent">{t('fixedAssets.conditionExcellent')}</option>
                            <option value="good">{t('fixedAssets.conditionGood')}</option>
                            <option value="fair">{t('fixedAssets.conditionFair')}</option>
                            <option value="poor">{t('fixedAssets.conditionPoor')}</option>
                        </select>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('fixedAssets.assign')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
