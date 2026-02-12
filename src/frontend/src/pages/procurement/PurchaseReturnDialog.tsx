import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

interface PurchaseReturnDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    ret: any | null; // Placeholder
}

export function PurchaseReturnDialog({ open, onClose }: Omit<PurchaseReturnDialogProps, 'ret'> & { ret: any | null }) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suppliers, setSuppliers] = useState<any[]>([]);
    const [receipts, setReceipts] = useState<any[]>([]);

    const [form, setForm] = useState({
        supplierId: '',
        goodsReceiptId: '',
    });
    // Purchase Return typically selects items from a Goods Receipt to return.
    // Simplified for now to just header creation or simple item entry.

    useEffect(() => {
        if (open) {
            api.get<{ data: { data: { id: string; name: string }[] } }>('/partners?page=1&pageSize=100&isSupplier=true').then(res => {
                setSuppliers(res.data.data);
            }).catch(() => { });
            api.get<any[]>('/procurement/goods-receipts').then((res) => {
                setReceipts(res);
            }).catch(() => { });
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            // Simplified payload - backend expects CreatePurchaseReturnDto which has Items list.
            // We need to implement item selection logic. 
            // Mocking empty items for now to show UI flow or error.
            const payload = {
                ...form,
                items: [] // Empty items will likely fail backend validation, but this serves as UI placeholder
            };
            await api.post('/procurement/returns', payload);
            toast.success(t('procurement.returnCreated'));
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
                        {t('procurement.createReturn')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.supplier')}</label>
                            <select
                                value={form.supplierId}
                                onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">Select Supplier</option>
                                {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">Goods Receipt</label>
                            <select
                                value={form.goodsReceiptId}
                                onChange={(e) => setForm({ ...form, goodsReceiptId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">Select Receipt</option>
                                {receipts.map(r => <option key={r.id} value={r.id}>{r.receiptNumber}</option>)}
                            </select>
                        </div>

                        <div className="p-4 bg-[hsl(var(--muted))] rounded text-sm text-[hsl(var(--muted-foreground))]">
                            Item selection from Receipt {form.goodsReceiptId ? '#' + form.goodsReceiptId.substring(0, 8) : '...'} would appear here.
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
