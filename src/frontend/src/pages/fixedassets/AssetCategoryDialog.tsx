import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface AssetCategory {
    id: string;
    code: string;
    name: string;
    description: string | null;
    depreciationMethod: number;
    usefulLifeMonths: number;
}

interface AssetCategoryDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    category: AssetCategory | null;
}

const API_BASE = '/api/fixedassets';

export function AssetCategoryDialog({ open, onClose, category }: AssetCategoryDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [form, setForm] = useState({
        code: '',
        name: '',
        description: '',
        depreciationMethod: 0,
        usefulLifeMonths: 60,
    });

    useEffect(() => {
        if (category) {
            setForm({
                code: category.code,
                name: category.name,
                description: category.description || '',
                depreciationMethod: category.depreciationMethod,
                usefulLifeMonths: category.usefulLifeMonths,
            });
        } else {
            setForm({
                code: '',
                name: '',
                description: '',
                depreciationMethod: 0,
                usefulLifeMonths: 60,
            });
        }
    }, [category, open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!form.code || !form.name) {
            toast.error(t('common.fillRequiredFields'));
            return;
        }

        setIsSubmitting(true);

        try {
            const url = category ? `${API_BASE}/categories/${category.id}` : `${API_BASE}/categories`;
            const method = category ? 'PUT' : 'POST';

            const res = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    ...form,
                    description: form.description || null,
                }),
            });

            if (res.ok) {
                toast.success(category ? t('fixedAssets.categoryUpdated') : t('fixedAssets.categoryCreated'));
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {category ? t('fixedAssets.editCategory') : t('fixedAssets.createCategory')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('common.code')} *</label>
                            <input
                                type="text"
                                value={form.code}
                                onChange={(e) => setForm({ ...form, code: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
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
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.depreciationMethod')}</label>
                            <select
                                value={form.depreciationMethod}
                                onChange={(e) => setForm({ ...form, depreciationMethod: parseInt(e.target.value) })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            >
                                <option value={0}>{t('fixedAssets.straightLine')}</option>
                                <option value={1}>{t('fixedAssets.decliningBalance')}</option>
                                <option value={2}>{t('fixedAssets.sumOfYears')}</option>
                                <option value={3}>{t('fixedAssets.unitsOfProduction')}</option>
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('fixedAssets.usefulLife')}</label>
                            <input
                                type="number"
                                value={form.usefulLifeMonths}
                                onChange={(e) => setForm({ ...form, usefulLifeMonths: parseInt(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : (category ? t('common.save') : t('common.create'))}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
