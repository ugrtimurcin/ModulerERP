import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

interface QualityControlDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    qc: any | null; // Placeholder type
}

export function QualityControlDialog({ open, onClose }: Omit<QualityControlDialogProps, 'qc'> & { qc: any | null }) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [receipts, setReceipts] = useState<any[]>([]); // To pick Goods Receipt Item
    const [warehouses, setWarehouses] = useState<any[]>([]);

    const [form, setForm] = useState({
        receiptItemId: '',
        quantityPassed: 0,
        quantityRejected: 0,
        targetWarehouseId: '',
        notes: ''
    });

    useEffect(() => {
        if (open) {
            // Fetch Warehouses
            api.get<any[]>('/warehouses').then(res => {
                setWarehouses(res);
            }).catch(() => { });

            // Fetch Pending Receipts
            api.get<any[]>('/procurement/goods-receipts').then((res) => {
                setReceipts(res);
            }).catch(() => { });
        }
    }, [open]);

    // This is simplified. Ideally we need to select a Receipt Item, not just a Receipt.
    // So UI should be: Select Receipt -> Select Item -> Enter Qty.
    // Or a single dropdown "Pending Inspection Items".

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            await api.post('/procurement/qc', form);
            toast.success(t('procurement.inspectionCreated'));
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-lg overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {t('procurement.createInspection')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">Receipt Item (Simplified)</label>
                            <select
                                value={form.receiptItemId}
                                onChange={(e) => setForm({ ...form, receiptItemId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">Select Receipt Item</option>
                                {/* Mocking items structure as we don't have receipt items easily available without more calls */}
                                {receipts.map(r => (
                                    <option key={r.id} value={r.id}>{r.receiptNumber} (Select Item Mock)</option> // Using receipt ID as mock
                                ))}
                            </select>
                            <p className="text-xs text-[hsl(var(--muted-foreground))] mt-1">
                                * Detailed item selection requires expanding receipt details.
                            </p>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.passed')}</label>
                            <input
                                type="number"
                                value={form.quantityPassed}
                                onChange={(e) => setForm({ ...form, quantityPassed: parseFloat(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                min="0" required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.rejected')}</label>
                            <input
                                type="number"
                                value={form.quantityRejected}
                                onChange={(e) => setForm({ ...form, quantityRejected: parseFloat(e.target.value) || 0 })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                min="0" required
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('nav.warehouses')}</label>
                            <select
                                value={form.targetWarehouseId}
                                onChange={(e) => setForm({ ...form, targetWarehouseId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">Select Target Warehouse</option>
                                {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('common.notes')}</label>
                            <textarea
                                value={form.notes}
                                onChange={(e) => setForm({ ...form, notes: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                rows={3}
                            />
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : t('common.create')}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
