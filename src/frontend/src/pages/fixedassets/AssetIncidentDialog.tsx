import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface AssetIncidentDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    assetId: string;
}

const API_BASE = '/api/fixedassets';

export function AssetIncidentDialog({ open, onClose, assetId }: AssetIncidentDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        incidentDate: new Date().toISOString().split('T')[0],
        description: '',
    });

    useEffect(() => {
        if (open) {
            setForm({
                incidentDate: new Date().toISOString().split('T')[0],
                description: '',
            });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.description) {
            toast.error(t('common.fillRequiredFields'));
            return;
        }

        setIsSubmitting(true);
        try {
            const res = await fetch(`${API_BASE}/incidents`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    assetId,
                    incidentDate: form.incidentDate,
                    description: form.description,
                }),
            });

            if (res.ok) {
                toast.success(t('fixedAssets.incidentReported'));
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
                    <h2 className="text-xl font-semibold">{t('fixedAssets.reportIncident')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.incidentDate')}</label>
                        <input
                            type="date"
                            value={form.incidentDate}
                            onChange={(e) => setForm({ ...form, incidentDate: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('common.description')} *</label>
                        <textarea
                            value={form.description}
                            onChange={(e) => setForm({ ...form, description: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            rows={4}
                            placeholder={t('fixedAssets.incidentDescriptionPlaceholder')}
                            required
                        />
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('fixedAssets.reportIncident')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
