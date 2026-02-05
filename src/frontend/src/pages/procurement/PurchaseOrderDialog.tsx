import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Plus, Trash2 } from 'lucide-react';
import { Button, useToast } from '@/components/ui';

interface PurchaseOrder {
    id: string;
    orderNumber: string;
    supplierId: string;
    status: number;
    totalAmount: number;
    orderDate: string;
    expectedDate: string | null;
    lines?: OrderLine[];
}

interface OrderLine {
    id?: string;
    productId: string;
    productName?: string;
    quantity: number;
    unitPrice: number;
    total?: number;
}

interface PurchaseOrderDialogProps {
    open: boolean;
    onClose: (saved: boolean) => void;
    order: PurchaseOrder | null;
}

const API_BASE = '/api/procurement';

export function PurchaseOrderDialog({ open, onClose, order }: PurchaseOrderDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suppliers, setSuppliers] = useState<{ id: string; name: string }[]>([]);
    const [products, setProducts] = useState<{ id: string; name: string; unitPrice: number }[]>([]);

    const [form, setForm] = useState({
        supplierId: '',
        expectedDate: '',
        notes: '',
    });
    const [lines, setLines] = useState<OrderLine[]>([]);

    useEffect(() => {
        if (open) {
            // Load suppliers and products
            fetch('/api/partners?type=Supplier').then(r => r.json()).then(setSuppliers).catch(() => { });
            fetch('/api/products').then(r => r.json()).then(setProducts).catch(() => { });
        }
    }, [open]);

    useEffect(() => {
        if (order) {
            setForm({
                supplierId: order.supplierId,
                expectedDate: order.expectedDate?.split('T')[0] || '',
                notes: '',
            });
            setLines(order.lines || []);
        } else {
            setForm({ supplierId: '', expectedDate: '', notes: '' });
            setLines([]);
        }
    }, [order, open]);

    const addLine = () => {
        setLines([...lines, { productId: '', quantity: 1, unitPrice: 0 }]);
    };

    const removeLine = (index: number) => {
        setLines(lines.filter((_, i) => i !== index));
    };

    const updateLine = (index: number, field: keyof OrderLine, value: string | number) => {
        const updated = [...lines];
        (updated[index] as any)[field] = value;
        if (field === 'productId') {
            const product = products.find(p => p.id === value);
            if (product) updated[index].unitPrice = product.unitPrice;
        }
        setLines(updated);
    };

    const total = lines.reduce((sum, line) => sum + (line.quantity * line.unitPrice), 0);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (lines.length === 0) {
            toast.error('Add at least one line item');
            return;
        }
        setIsSubmitting(true);

        try {
            const url = order ? `${API_BASE}/purchase-orders/${order.id}` : `${API_BASE}/purchase-orders`;
            const method = order ? 'PUT' : 'POST';

            const res = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...form, lines }),
            });

            if (res.ok) {
                toast.success(order ? t('procurement.poUpdated') : t('procurement.poCreated'));
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
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-auto border border-[hsl(var(--border))]">
                <div className="sticky top-0 bg-[hsl(var(--card))] px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">
                        {order ? `PO: ${order.orderNumber}` : t('procurement.createPO')}
                    </h2>
                    <button onClick={() => onClose(false)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-6">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.supplier')}</label>
                            <select
                                value={form.supplierId}
                                onChange={(e) => setForm({ ...form, supplierId: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            >
                                <option value="">Select Supplier</option>
                                {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('procurement.expectedDate')}</label>
                            <input
                                type="date"
                                value={form.expectedDate}
                                onChange={(e) => setForm({ ...form, expectedDate: e.target.value })}
                                className="w-full px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                    </div>

                    {/* Line Items */}
                    <div>
                        <div className="flex items-center justify-between mb-2">
                            <label className="text-sm font-medium">{t('procurement.lineItems')}</label>
                            <Button type="button" variant="secondary" onClick={addLine}>
                                <Plus className="w-4 h-4" /> Add Item
                            </Button>
                        </div>
                        <div className="border border-[hsl(var(--border))] rounded-lg overflow-hidden">
                            <table className="w-full">
                                <thead className="bg-[hsl(var(--accent))]">
                                    <tr>
                                        <th className="px-3 py-2 text-left text-sm font-medium">Product</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-24">Qty</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-32">Unit Price</th>
                                        <th className="px-3 py-2 text-left text-sm font-medium w-32">Total</th>
                                        <th className="w-12"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {lines.map((line, i) => (
                                        <tr key={i} className="border-t border-[hsl(var(--border))]">
                                            <td className="px-3 py-2">
                                                <select
                                                    value={line.productId}
                                                    onChange={(e) => updateLine(i, 'productId', e.target.value)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    required
                                                >
                                                    <option value="">Select</option>
                                                    {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                                                </select>
                                            </td>
                                            <td className="px-3 py-2">
                                                <input
                                                    type="number"
                                                    value={line.quantity}
                                                    onChange={(e) => updateLine(i, 'quantity', parseFloat(e.target.value) || 0)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    min="0"
                                                    step="0.01"
                                                />
                                            </td>
                                            <td className="px-3 py-2">
                                                <input
                                                    type="number"
                                                    value={line.unitPrice}
                                                    onChange={(e) => updateLine(i, 'unitPrice', parseFloat(e.target.value) || 0)}
                                                    className="w-full px-2 py-1 rounded border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                                                    min="0"
                                                    step="0.01"
                                                />
                                            </td>
                                            <td className="px-3 py-2 font-mono">
                                                {(line.quantity * line.unitPrice).toFixed(2)}
                                            </td>
                                            <td className="px-3 py-2">
                                                <button type="button" onClick={() => removeLine(i)} className="p-1 text-red-500 hover:bg-red-100 rounded">
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    {lines.length === 0 && (
                                        <tr>
                                            <td colSpan={5} className="px-3 py-8 text-center text-[hsl(var(--muted-foreground))]">
                                                No items added. Click "Add Item" to begin.
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                                <tfoot className="bg-[hsl(var(--accent))]">
                                    <tr>
                                        <td colSpan={3} className="px-3 py-2 text-right font-medium">Total:</td>
                                        <td className="px-3 py-2 font-mono font-bold">{total.toFixed(2)}</td>
                                        <td></td>
                                    </tr>
                                </tfoot>
                            </table>
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-[hsl(var(--border))]">
                        <Button type="button" variant="secondary" onClick={() => onClose(false)}>{t('common.cancel')}</Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting ? t('common.saving') : (order ? t('common.save') : t('common.create'))}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
