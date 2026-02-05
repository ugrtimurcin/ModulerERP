import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Plus, Trash2 } from 'lucide-react';
import { Button, useToast } from '@/components/ui';
import { api } from '@/services/api';

interface RfqItem {
    id?: string;
    productId: string;
    productName?: string;
    targetQuantity: number;
}

interface Rfq {
    id: string;
    rfqNumber: string;
    title: string;
    deadLine: string;
    items?: RfqItem[];
}

interface RfqDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    rfq: Rfq | null;
}

export function RfqDialog({ open, onClose, rfq }: RfqDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [products, setProducts] = useState<{ id: string; name: string }[]>([]);

    const [form, setForm] = useState({
        title: '',
        deadLine: '',
    });
    const [items, setItems] = useState<RfqItem[]>([]);

    useEffect(() => {
        if (open) {
            api.products.getAll().then(res => {
                if (Array.isArray(res)) setProducts(res);
                else if ((res as any).data) setProducts((res as any).data);
            }).catch(() => { });
        }
    }, [open]);

    useEffect(() => {
        if (rfq) {
            setForm({
                title: rfq.title,
                deadLine: rfq.deadLine ? new Date(rfq.deadLine).toISOString().split('T')[0] : '',
            });
            setItems(rfq.items || []);
        } else {
            setForm({ title: '', deadLine: '' });
            setItems([]);
        }
    }, [rfq, open]);

    const addItem = () => {
        setItems([...items, { productId: '', targetQuantity: 1 }]);
    };

    const removeItem = (index: number) => {
        setItems(items.filter((_, i) => i !== index));
    };

    const updateItem = (index: number, field: keyof RfqItem, value: string | number) => {
        const updated = [...items];
        (updated[index] as any)[field] = value;
        setItems(updated);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (items.length === 0) {
            toast.error('Add at least one item');
            return;
        }
        setIsSubmitting(true);

        try {
            const payload = {
                ...form,
                items: items.map(i => ({
                    productId: i.productId,
                    targetQuantity: i.targetQuantity
                }))
            };

            if (rfq) {
                // Update implementation would go here if backend supports PUT
                // For now assuming create only based on DTOs seen or similar pattern
                toast.info('Update not fully implemented in this demo');
            } else {
                await api.rfqs.create(payload);
                toast.success(t('procurement.rfqCreated'));
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
                        {rfq ? `RFQ: ${rfq.rfqNumber}` : t('procurement.createRfq')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="col-span-2">
                            <label className="block text-sm font-medium mb-1">{t('common.title')}</label>
                            <input
                                type="text"
                                value={form.title}
                                onChange={(e) => setForm({ ...form, title: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                                placeholder="e.g. Q1 Office Supplies"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.deadline')}</label>
                            <input
                                type="date"
                                value={form.deadLine}
                                onChange={(e) => setForm({ ...form, deadLine: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                    </div>

                    {/* Items */}
                    <div>
                        <div className="flex items-center justify-between mb-2">
                            <label className="text-sm font-medium">{t('procurement.items')}</label>
                            <Button type="button" variant="secondary" onClick={addItem}>
                                <Plus className="w-4 h-4" /> {t('common.add')}
                            </Button>
                        </div>
                        <div className="border border-[hsl(var(--border))] rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead className="bg-[hsl(var(--accent))]">
                                    <tr>
                                        <th className="px-3 py-2 text-left text-sm font-medium">{t('procurement.product')}</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-32">{t('procurement.quantity')}</th>
                                        <th className="w-12"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.map((item, i) => (
                                        <tr key={i} className="border-t border-[hsl(var(--border))]">
                                            <td className="px-3 py-2">
                                                <select
                                                    value={item.productId}
                                                    onChange={(e) => updateItem(i, 'productId', e.target.value)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    required
                                                >
                                                    <option value="">{t('common.select')}</option>
                                                    {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                                                </select>
                                            </td>
                                            <td className="px-3 py-2">
                                                <input
                                                    type="number"
                                                    value={item.targetQuantity}
                                                    onChange={(e) => updateItem(i, 'targetQuantity', parseFloat(e.target.value) || 0)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    min="1"
                                                />
                                            </td>
                                            <td className="px-3 py-2">
                                                <button type="button" onClick={() => removeItem(i)} className="p-1 text-red-500 hover:bg-red-100 rounded">
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    {items.length === 0 && (
                                        <tr>
                                            <td colSpan={3} className="px-3 py-8 text-center text-[hsl(var(--muted-foreground))]">
                                                {t('common.noData')}
                                            </td>
                                        </tr>
                                    )}
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
