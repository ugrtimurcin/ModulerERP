import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

interface AssetMeterLogDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    assetId: string;
}

export function AssetMeterLogDialog({ open, onClose, assetId }: AssetMeterLogDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        logDate: new Date().toISOString().split('T')[0],
        meterValue: 0,
        source: 0, // Manual
    });

    useEffect(() => {
        if (open) {
            setForm({
                logDate: new Date().toISOString().split('T')[0],
                meterValue: 0,
                source: 0,
            });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        setIsSubmitting(true);
        try {
            await api.post('/fixedassets/log-meter', {
                assetId,
                logDate: form.logDate,
                meterValue: form.meterValue,
                source: form.source,
            });

            toast.success(t('fixedAssets.meterLogged'));
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('fixedAssets.logMeter')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('common.date')}</label>
                        <input
                            type="date"
                            value={form.logDate}
                            onChange={(e) => setForm({ ...form, logDate: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.meterValue')} *</label>
                        <input
                            type="number"
                            step="0.01"
                            value={form.meterValue}
                            onChange={(e) => setForm({ ...form, meterValue: parseFloat(e.target.value) || 0 })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.source')}</label>
                        <select
                            value={form.source}
                            onChange={(e) => setForm({ ...form, source: parseInt(e.target.value) })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value={0}>{t('fixedAssets.meterSource.0')}</option>
                            <option value={1}>{t('fixedAssets.meterSource.1')}</option>
                            <option value={2}>{t('fixedAssets.meterSource.2')}</option>
                            <option value={3}>{t('fixedAssets.meterSource.3')}</option>
                        </select>
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
