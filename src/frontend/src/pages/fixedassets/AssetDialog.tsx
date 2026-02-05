import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface Asset {
    id: string;
    assetCode: string;
    name: string;
    description: string | null;
    categoryId: string;
    acquisitionDate: string;
    acquisitionCost: number;
    salvageValue: number;
    serialNumber: string | null;
    barCode: string | null;
}

interface AssetCategory {
    id: string;
    name: string;
    code: string;
}

interface AssetDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    asset: Asset | null;
    categories: AssetCategory[];
}

const API_BASE = '/api/fixedassets';

export function AssetDialog({ open, onClose, asset, categories }: AssetDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        assetCode: '',
        name: '',
        description: '',
        categoryId: '',
        acquisitionDate: new Date().toISOString().split('T')[0],
        acquisitionCost: 0,
        salvageValue: 0,
        serialNumber: '',
        barCode: '',
    });

    useEffect(() => {
        if (asset) {
            setForm({
                assetCode: asset.assetCode,
                name: asset.name,
                description: asset.description || '',
                categoryId: asset.categoryId,
                acquisitionDate: asset.acquisitionDate?.split('T')[0] || new Date().toISOString().split('T')[0],
                acquisitionCost: asset.acquisitionCost,
                salvageValue: asset.salvageValue,
                serialNumber: asset.serialNumber || '',
                barCode: asset.barCode || '',
            });
        } else {
            setForm({
                assetCode: '',
                name: '',
                description: '',
                categoryId: '',
                acquisitionDate: new Date().toISOString().split('T')[0],
                acquisitionCost: 0,
                salvageValue: 0,
                serialNumber: '',
                barCode: '',
            });
        }
    }, [asset, open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.name || !form.assetCode || !form.categoryId) {
            toast.error(t('common.fillRequiredFields'));
            return;
        }

        setIsSubmitting(true);

        try {
            const url = asset ? `${API_BASE}/assets/${asset.id}` : `${API_BASE}/assets`;
            const method = asset ? 'PUT' : 'POST';

            const res = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    ...form,
                    serialNumber: form.serialNumber || null,
                    barCode: form.barCode || null,
                    description: form.description || null,
                }),
            });

            if (res.ok) {
                toast.success(asset ? t('fixedAssets.assetUpdated') : t('fixedAssets.assetCreated'));
                onClose(true);
            } else {
                toast.error(t('common.error'), await res.text());
            }
        } catch { toast.error(t('common.error')); }
        finally { setIsSubmitting(false); }
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {asset ? t('fixedAssets.editAsset') : t('fixedAssets.createAsset')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4 max-h-[70vh] overflow-y-auto">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.assetCode')} *</label>
                            <input
                                type="text"
                                value={form.assetCode}
                                onChange={(e) => setForm({ ...form, assetCode: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.category')} *</label>
                            <select
                                value={form.categoryId}
                                onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            >
                                <option value="">{t('common.select')}</option>
                                {categories.map((cat) => (
                                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('common.name')} *</label>
                        <input
                            type="text"
                            value={form.name}
                            onChange={(e) => setForm({ ...form, name: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('common.description')}</label>
                        <textarea
                            value={form.description}
                            onChange={(e) => setForm({ ...form, description: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            rows={2}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.acquisitionDate')}</label>
                            <input
                                type="date"
                                value={form.acquisitionDate}
                                onChange={(e) => setForm({ ...form, acquisitionDate: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.acquisitionCost')}</label>
                            <input
                                type="number"
                                step="0.01"
                                value={form.acquisitionCost}
                                onChange={(e) => setForm({ ...form, acquisitionCost: parseFloat(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.salvageValue')}</label>
                            <input
                                type="number"
                                step="0.01"
                                value={form.salvageValue}
                                onChange={(e) => setForm({ ...form, salvageValue: parseFloat(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.serialNumber')}</label>
                            <input
                                type="text"
                                value={form.serialNumber}
                                onChange={(e) => setForm({ ...form, serialNumber: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('fixedAssets.barCode')}</label>
                        <input
                            type="text"
                            value={form.barCode}
                            onChange={(e) => setForm({ ...form, barCode: e.target.value })}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : (asset ? t('common.save') : t('common.create'))}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
