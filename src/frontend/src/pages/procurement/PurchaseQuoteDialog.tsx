import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/lib/api';

interface QuoteItem {
    id?: string;
    rfqItemId: string;
    productId: string;
    productName?: string;
    price: number;
    leadTimeDays: number;
}

interface PurchaseQuote {
    id: string;
    rfqId: string;
    supplierId: string;
    quoteReference: string;
    validUntil: string;
    totalAmount: number;
    items?: QuoteItem[];
}

interface PurchaseQuoteDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    quote: PurchaseQuote | null;
}

export function PurchaseQuoteDialog({ open, onClose, quote }: PurchaseQuoteDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suppliers, setSuppliers] = useState<{ id: string; name: string }[]>([]);
    const [rfqs, setRfqs] = useState<any[]>([]);


    const [form, setForm] = useState({
        rfqId: '',
        supplierId: '',
        quoteReference: '',
        validUntil: '',
    });
    const [items, setItems] = useState<QuoteItem[]>([]);

    useEffect(() => {
        if (open) {
            api.get<{ data: { data: { id: string; name: string }[] } }>('/partners?page=1&pageSize=100&isSupplier=true').then(res => {
                setSuppliers(res.data.data);
            }).catch(() => { });

            api.get<any[]>('/procurement/rfqs').then(res => {
                setRfqs(res);
            }).catch(() => { });
        }
    }, [open]);

    // When RFQ is selected, load its items to prepopulate quote items
    useEffect(() => {
        if (form.rfqId) {
            // In a real app we might fetch the specific RFQ details again to get items
            // optimizing by finding in loaded list if full details available
            const selected = rfqs.find(r => r.id === form.rfqId);
            if (selected && selected.items) {
                // Auto-populate quote items based on RFQ items if new quote
                if (!quote && items.length === 0) {
                    setItems(selected.items.map((ri: any) => ({
                        rfqItemId: ri.id,
                        productId: ri.productId,
                        price: 0,
                        leadTimeDays: 0
                    })));
                }
            }
        }
    }, [form.rfqId, rfqs, quote]);


    useEffect(() => {
        if (quote) {
            setForm({
                rfqId: quote.rfqId,
                supplierId: quote.supplierId,
                quoteReference: quote.quoteReference,
                validUntil: quote.validUntil ? new Date(quote.validUntil).toISOString().split('T')[0] : '',
            });
            setItems(quote.items || []);
        } else {
            setForm({ rfqId: '', supplierId: '', quoteReference: '', validUntil: '' });
            setItems([]);
        }
    }, [quote, open]);

    const updateItem = (index: number, field: keyof QuoteItem, value: string | number) => {
        const updated = [...items];
        (updated[index] as any)[field] = value;
        setItems(updated);
    };

    const totalAmount = items.reduce((sum, item) => sum + item.price, 0);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);

        try {
            const payload = {
                ...form,
                totalAmount: totalAmount,
                items: items.map(i => ({
                    rfqItemId: i.rfqItemId,
                    productId: i.productId,
                    price: i.price,
                    leadTimeDays: i.leadTimeDays
                }))
            };

            if (quote) {
                toast.info('Update not fully implemented');
            } else {
                await api.post('/procurement/quotes', payload);
                toast.success(t('procurement.quoteCreated'));
                onClose(true);
            }
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {quote ? `Quote: ${quote.quoteReference}` : t('procurement.createQuote')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.rfqNumber')}</label>
                            <select
                                value={form.rfqId}
                                onChange={(e) => setForm({ ...form, rfqId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">{t('common.select')}</option>
                                {rfqs.map(r => <option key={r.id} value={r.id}>{r.rfqNumber} - {r.title}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.supplier')}</label>
                            <select
                                value={form.supplierId}
                                onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            >
                                <option value="">{t('common.select')}</option>
                                {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.quoteReference')}</label>
                            <input
                                type="text"
                                value={form.quoteReference}
                                onChange={(e) => setForm({ ...form, quoteReference: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                                placeholder="SUP-Q-2024-001"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.validUntil')}</label>
                            <input
                                type="date"
                                value={form.validUntil}
                                onChange={(e) => setForm({ ...form, validUntil: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                required
                            />
                        </div>
                    </div>

                    {/* Items */}
                    <div>
                        <label className="block text-sm font-medium mb-2">{t('procurement.items')}</label>
                        {items.length === 0 && (
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">Select an RFQ to see items</p>
                        )}
                        <div className="border border-[hsl(var(--border))] rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead className="bg-[hsl(var(--accent))]">
                                    <tr>
                                        <th className="px-3 py-2 text-left text-sm font-medium">{t('procurement.product')}</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-32">{t('common.price')}</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-32">Lead Time (Days)</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.map((item, i) => (
                                        <tr key={i} className="border-t border-[hsl(var(--border))]">
                                            <td className="px-3 py-2 text-sm">
                                                {/* In a real app we would lookup product name from ID */}
                                                Product {item.productId.substring(0, 8)}...
                                            </td>
                                            <td className="px-3 py-2">
                                                <input
                                                    type="number"
                                                    value={item.price}
                                                    onChange={(e) => updateItem(i, 'price', parseFloat(e.target.value) || 0)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    min="0"
                                                    step="0.01"
                                                />
                                            </td>
                                            <td className="px-3 py-2">
                                                <input
                                                    type="number"
                                                    value={item.leadTimeDays}
                                                    onChange={(e) => updateItem(i, 'leadTimeDays', parseInt(e.target.value) || 0)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    min="0"
                                                />
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
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
