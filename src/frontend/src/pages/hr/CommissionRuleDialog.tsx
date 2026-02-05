import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, useToast } from '@/components/ui';
import { X } from 'lucide-react';

interface CommissionRuleDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
}

export function CommissionRuleDialog({ open, onClose }: CommissionRuleDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isLoading, setIsLoading] = useState(false);

    const [formData, setFormData] = useState({
        role: '',
        minTargetAmount: 0,
        percentage: 0,
        basis: 1
    });

    useEffect(() => {
        if (open) {
            setFormData({ role: '', minTargetAmount: 0, percentage: 0, basis: 1 });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsLoading(true);

        try {
            const res = await fetch('/api/hr/commission-rules', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
            });

            if (res.ok) {
                toast.success(t('hr.commissionRuleCreated'));
                onClose(true);
            } else {
                toast.error(t('common.error'));
            }
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    };

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => onClose(false)} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-md border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('hr.createCommissionRule')}</h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('hr.role')}</label>
                        <input
                            type="text"
                            required
                            placeholder="e.g. Sales Representative"
                            value={formData.role}
                            onChange={(e) => setFormData(prev => ({ ...prev, role: e.target.value }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('hr.minTarget')}</label>
                        <input
                            type="number"
                            required
                            min="0"
                            value={formData.minTargetAmount}
                            onChange={(e) => setFormData(prev => ({ ...prev, minTargetAmount: parseFloat(e.target.value) }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('hr.percentage')}</label>
                        <input
                            type="number"
                            required
                            min="0"
                            max="100"
                            value={formData.percentage}
                            onChange={(e) => setFormData(prev => ({ ...prev, percentage: parseFloat(e.target.value) }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1.5">{t('hr.basis')}</label>
                        <select
                            value={formData.basis}
                            onChange={(e) => setFormData(prev => ({ ...prev, basis: parseInt(e.target.value) }))}
                            className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        >
                            <option value={1}>{t('hr.basisInvoiced')}</option>
                            <option value={2}>{t('hr.basisCollected')}</option>
                            <option value={3}>{t('hr.basisProfit')}</option>
                        </select>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit" disabled={isLoading}>
                            {t('common.create')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
