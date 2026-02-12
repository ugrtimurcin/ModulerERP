import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

interface Partner {
    id: string;
    name: string;
}

interface AssetMaintenanceDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    assetId: string;
}

export function AssetMaintenanceDialog({ open, onClose, assetId }: AssetMaintenanceDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suppliers, setSuppliers] = useState<Partner[]>([]);

    const [form, setForm] = useState({
        supplierId: '',
        serviceDate: new Date().toISOString().split('T')[0],
        cost: 0,
        description: '',
        nextServiceDate: '',
        nextServiceMeter: '',
    });

    useEffect(() => {
        if (open) {
            api.get<{ data: Partner[] }>('/partners?type=supplier')
                .then(res => setSuppliers(res.data || (res as any)))
                .catch(() => { });

            setForm({
                supplierId: '',
                serviceDate: new Date().toISOString().split('T')[0],
                cost: 0,
                description: '',
                nextServiceDate: '',
                nextServiceMeter: '',
            });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.supplierId || !form.description) {
            toast.error(t('common.fillRequiredFields'));
            return;
        }

        setIsSubmitting(true);
        try {
            await api.post('/fixedassets/maintenance', {
                assetId,
                supplierId: form.supplierId,
                serviceDate: form.serviceDate,
                cost: form.cost,
                description: form.description,
                incidentId: null,
                nextServiceDate: form.nextServiceDate || null,
                nextServiceMeter: form.nextServiceMeter ? parseFloat(form.nextServiceMeter) : null,
            });

            toast.success(t('fixedAssets.maintenanceRecorded'));
            onClose(true);
        } catch {
            toast.error(t('common.error'));
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('fixedAssets.recordMaintenance')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4 max-h-[70vh] overflow-y-auto">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.supplier')} *</label>
                            <select
                                value={form.supplierId}
                                onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            >
                                <option value="">{t('common.select')}</option>
                                {suppliers.map((s) => (
                                    <option key={s.id} value={s.id}>{s.name}</option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.serviceDate')}</label>
                            <input
                                type="date"
                                value={form.serviceDate}
                                onChange={(e) => setForm({ ...form, serviceDate: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.cost')}</label>
                        <input
                            type="number"
                            step="0.01"
                            value={form.cost}
                            onChange={(e) => setForm({ ...form, cost: parseFloat(e.target.value) || 0 })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('common.description')} *</label>
                        <textarea
                            value={form.description}
                            onChange={(e) => setForm({ ...form, description: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            rows={3}
                            required
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.nextService')}</label>
                            <input
                                type="date"
                                value={form.nextServiceDate}
                                onChange={(e) => setForm({ ...form, nextServiceDate: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.nextServiceMeter')}</label>
                            <input
                                type="number"
                                step="0.01"
                                value={form.nextServiceMeter}
                                onChange={(e) => setForm({ ...form, nextServiceMeter: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                placeholder="e.g. 50000"
                            />
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('common.save')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
