import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X, AlertTriangle } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface Partner {
    id: string;
    name: string;
}

interface AssetDisposeDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    assetId: string;
}

const API_BASE = '/api/fixedassets';

export function AssetDisposeDialog({ open, onClose, assetId }: AssetDisposeDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [partners, setPartners] = useState<Partner[]>([]);

    const [form, setForm] = useState({
        disposalDate: new Date().toISOString().split('T')[0],
        type: 0, // Sale
        saleAmount: '',
        reason: '',
        partnerId: '',
    });

    useEffect(() => {
        if (open) {
            fetch('/api/partners', { cache: 'no-store' })
                .then(res => res.json())
                .then(data => setPartners(data.data || data))
                .catch(() => { });
            setForm({
                disposalDate: new Date().toISOString().split('T')[0],
                type: 0,
                saleAmount: '',
                reason: '',
                partnerId: '',
            });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        setIsSubmitting(true);
        try {
            const res = await fetch(`${API_BASE}/dispose`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    assetId,
                    disposalDate: form.disposalDate,
                    type: form.type,
                    saleAmount: form.saleAmount ? parseFloat(form.saleAmount) : null,
                    reason: form.reason || null,
                    partnerId: form.partnerId || null,
                }),
            });

            if (res.ok) {
                toast.success(t('fixedAssets.assetDisposed'));
                onClose(true);
            } else {
                toast.error(t('common.error'));
            }
        } catch { toast.error(t('common.error')); }
        finally { setIsSubmitting(false); }
    };

    if (!open) return null;

    const isSale = form.type === 0;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold text-red-600 flex items-center gap-2">
                        <AlertTriangle className="w-5 h-5" />
                        {t('fixedAssets.disposeAsset')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div className="p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-900">
                        <p className="text-sm text-red-700 dark:text-red-300">{t('fixedAssets.disposeWarning')}</p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.disposalType')}</label>
                        <select
                            value={form.type}
                            onChange={(e) => setForm({ ...form, type: parseInt(e.target.value) })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value={0}>{t('fixedAssets.disposalTypes.sale')}</option>
                            <option value={1}>{t('fixedAssets.disposalTypes.scrap')}</option>
                            <option value={2}>{t('fixedAssets.disposalTypes.stolen')}</option>
                            <option value={3}>{t('fixedAssets.disposalTypes.donation')}</option>
                        </select>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.disposalDate')}</label>
                        <input
                            type="date"
                            value={form.disposalDate}
                            onChange={(e) => setForm({ ...form, disposalDate: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    {isSale && (
                        <>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('fixedAssets.saleAmount')}</label>
                                <input
                                    type="number"
                                    step="0.01"
                                    value={form.saleAmount}
                                    onChange={(e) => setForm({ ...form, saleAmount: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">{t('fixedAssets.buyer')}</label>
                                <select
                                    value={form.partnerId}
                                    onChange={(e) => setForm({ ...form, partnerId: e.target.value })}
                                    className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                >
                                    <option value="">{t('common.select')}</option>
                                    {partners.map((p) => (
                                        <option key={p.id} value={p.id}>{p.name}</option>
                                    ))}
                                </select>
                            </div>
                        </>
                    )}

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.reason')}</label>
                        <textarea
                            value={form.reason}
                            onChange={(e) => setForm({ ...form, reason: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            rows={2}
                        />
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" variant="danger" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('fixedAssets.dispose')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
