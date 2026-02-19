import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2 } from 'lucide-react';
import { api } from '../../services/api';
import { Modal, Button, Input, Select, useToast } from '@/components/ui';

interface InvoiceDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: () => void;
    invoiceId?: string;
}

interface InvoiceItem {
    productId: string;
    description: string;
    quantity: number;
    unitPrice: number;
    discountPercent: number;
    taxPercent: number;
    unitOfMeasureId: string;
}

const InvoiceDialog: React.FC<InvoiceDialogProps> = ({ isOpen, onClose, onSave, invoiceId }) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [loading, setLoading] = useState(false);
    const [partners, setPartners] = useState<any[]>([]);
    const [products, setProducts] = useState<any[]>([]);
    const [currencies, setCurrencies] = useState<any[]>([]);

    // Form State
    const [partnerId, setPartnerId] = useState('');
    const [orderId, setOrderId] = useState<string | null>(null);
    const [currencyId, setCurrencyId] = useState('');
    const [exchangeRate, setExchangeRate] = useState(1);
    const [invoiceDate, setInvoiceDate] = useState('');
    const [dueDate, setDueDate] = useState('');
    const [paymentTerms, setPaymentTerms] = useState('');
    const [shippingAddress, setShippingAddress] = useState('');
    const [billingAddress, setBillingAddress] = useState('');
    const [notes, setNotes] = useState('');
    const [items, setItems] = useState<InvoiceItem[]>([]);

    useEffect(() => {
        if (isOpen) {
            loadDependencies();
            if (invoiceId) {
                loadInvoice(invoiceId);
            } else {
                resetForm();
            }
        }
    }, [isOpen, invoiceId]);

    const loadDependencies = async () => {
        try {
            const [partnersRes, productsRes, currenciesRes] = await Promise.all([
                api.partners.getAll(1, 100, true, undefined),
                api.products.getAll(),
                api.getActiveCurrencies()
            ]);

            if (partnersRes.success && partnersRes.data) setPartners(partnersRes.data.data);
            if (productsRes.success && productsRes.data) setProducts(productsRes.data);
            if (currenciesRes.success && currenciesRes.data) {
                setCurrencies(currenciesRes.data);
                if (!invoiceId && currenciesRes.data.length > 0 && !currencyId) {
                    setCurrencyId(currenciesRes.data[0].id);
                }
            }
        } catch (error) {
            console.error('Failed to load dependencies', error);
        }
    };

    const loadInvoice = async (id: string) => {
        setLoading(true);
        try {
            const res = await api.invoices.getById(id);
            if (res.success && res.data) {
                const o = res.data;
                setPartnerId(o.partnerId);
                setOrderId(o.orderId);
                setCurrencyId(o.currencyId);
                setExchangeRate(o.exchangeRate);
                setInvoiceDate(o.invoiceDate ? o.invoiceDate.split('T')[0] : '');
                setDueDate(o.dueDate ? o.dueDate.split('T')[0] : '');
                setPaymentTerms(o.paymentTerms || '');
                setShippingAddress(o.shippingAddress || '');
                setBillingAddress(o.billingAddress || '');
                setNotes(o.notes || '');

                setItems(o.lines.map((l: any) => ({
                    productId: l.productId,
                    description: l.description,
                    quantity: l.quantity,
                    unitPrice: l.unitPrice,
                    discountPercent: l.discountPercent,
                    taxPercent: l.taxPercent,
                    unitOfMeasureId: l.unitOfMeasureId
                })));
            }
        } catch (error) {
            console.error('Failed to load invoice', error);
        } finally {
            setLoading(false);
        }
    };

    const resetForm = () => {
        setPartnerId('');
        setOrderId(null);
        setInvoiceDate(new Date().toISOString().split('T')[0]);
        setDueDate(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
        setPaymentTerms('');
        setShippingAddress('');
        setBillingAddress('');
        setNotes('');
        setItems([]);
    };

    const handleAddItem = () => {
        setItems([...items, {
            productId: '',
            description: '',
            quantity: 1,
            unitPrice: 0,
            discountPercent: 0,
            taxPercent: 0,
            unitOfMeasureId: ''
        }]);
    };

    const handleRemoveItem = (index: number) => {
        const newItems = [...items];
        newItems.splice(index, 1);
        setItems(newItems);
    };

    const handleItemChange = (index: number, field: keyof InvoiceItem, value: any) => {
        const newItems = [...items];
        const item = { ...newItems[index], [field]: value };

        if (field === 'productId') {
            const product = products.find(p => p.id === value);
            if (product) {
                item.description = product.name;
                item.unitPrice = product.salesPrice;
                item.unitOfMeasureId = product.unitOfMeasureId;
            }
        }

        newItems[index] = item;
        setItems(newItems);
    };

    const calculateTotals = () => {
        let subTotal = 0;
        let discount = 0;
        let tax = 0;

        items.forEach(item => {
            const lineAmount = item.quantity * item.unitPrice;
            const lineDisc = lineAmount * (item.discountPercent / 100);
            const lineSub = lineAmount - lineDisc;
            const lineTax = lineSub * (item.taxPercent / 100);

            subTotal += lineAmount;
            discount += lineDisc;
            tax += lineTax;
        });

        return { subTotal, discount, tax, total: subTotal - discount + tax };
    };

    const handleSave = async () => {
        if (!partnerId || items.length === 0) {
            toast.error(t('common.required'));
            return;
        }

        setLoading(true);
        const dto = {
            type: 0, // SalesInvoice
            partnerId,
            currencyId,
            exchangeRate,
            orderId: orderId || null,
            invoiceDate: invoiceDate || new Date().toISOString(),
            dueDate: dueDate || new Date().toISOString(),
            paymentTerms,
            shippingAddress,
            billingAddress,
            notes,
            lines: items.map(i => ({
                productId: i.productId,
                description: i.description,
                quantity: Number(i.quantity),
                unitOfMeasureId: i.unitOfMeasureId,
                unitPrice: Number(i.unitPrice),
                discountPercent: Number(i.discountPercent),
                taxPercent: Number(i.taxPercent)
            }))
        };

        try {
            let res;
            if (invoiceId) {
                // No update endpoint exists — invoices are immutable once created.
                // Cancel and recreate if needed.
                toast.error(t('common.error'), 'Invoices cannot be edited. Delete and recreate instead.');
                setLoading(false);
                return;
            } else {
                res = await api.invoices.create(dto);
            }

            if (res.success) {
                toast.success(invoiceId ? t('common.updatedSuccess') : t('common.createdSuccess'));
                onSave();
                onClose();
            } else {
                toast.error(t('common.error'), res.error || t('common.error'));
            }
        } catch (error) {
            console.error('Save failed', error);
            toast.error(t('common.error'));
        } finally {
            setLoading(false);
        }
    };

    const totals = calculateTotals();

    const footer = (
        <>
            <Button variant="secondary" onClick={onClose}>
                {t('common.cancel')}
            </Button>
            <Button onClick={handleSave} isLoading={loading}>
                {t('common.save')}
            </Button>
        </>
    );

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={invoiceId ? t('invoices.editInvoice') : t('invoices.createInvoice')}
            size="xl"
            footer={footer}
        >
            <div className="space-y-6">
                {/* Header Fields */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <Select
                        label={t('quotes.partner')}
                        value={partnerId}
                        onChange={(e) => setPartnerId(e.target.value)}
                        options={partners.map(p => ({ value: p.id, label: `${p.code} - ${p.name}` }))}
                        required
                        placeholder={t('common.select')}
                    />
                    <Select
                        label={t('quotes.currency')}
                        value={currencyId}
                        onChange={(e) => setCurrencyId(e.target.value)}
                        options={currencies.map(c => ({ value: c.id, label: c.code }))}
                        required
                    />
                    <Input
                        label={t('invoices.invoiceDate')}
                        type="date"
                        value={invoiceDate}
                        onChange={(e) => setInvoiceDate(e.target.value)}
                    />
                    <Input
                        label={t('invoices.dueDate')}
                        type="date"
                        value={dueDate}
                        onChange={(e) => setDueDate(e.target.value)}
                    />
                    <Input
                        label={t('invoices.paymentTerms')}
                        value={paymentTerms}
                        onChange={(e) => setPaymentTerms(e.target.value)}
                    />
                </div>

                {/* Items Table */}
                <div>
                    <div className="flex justify-between items-center mb-2">
                        <h4 className="font-medium text-gray-900">{t('quotes.items')}</h4>
                        <Button variant="secondary" size="sm" onClick={handleAddItem}>
                            <Plus className="h-4 w-4 mr-2" />
                            {t('common.create')} Item
                        </Button>
                    </div>

                    <div className="overflow-x-auto border rounded-lg">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase w-48">{t('quotes.product')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-20">{t('quotes.quantity')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-24">{t('quotes.unitPrice')}</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-20">{t('quotes.discount')} %</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-20">{t('quotes.tax')} %</th>
                                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase w-28">{t('quotes.lineTotal')}</th>
                                    <th className="px-3 py-2 w-10"></th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {items.map((item, index) => (
                                    <tr key={index}>
                                        <td className="px-3 py-2">
                                            <div className="space-y-1">
                                                <select
                                                    className="block w-full border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
                                                    value={item.productId}
                                                    onChange={(e) => handleItemChange(index, 'productId', e.target.value)}
                                                >
                                                    <option value="">{t('common.select')}</option>
                                                    {products.map(p => (
                                                        <option key={p.id} value={p.id}>{p.sku} - {p.name}</option>
                                                    ))}
                                                </select>
                                                <input
                                                    type="text"
                                                    placeholder="Description"
                                                    className="block w-full border-gray-300 rounded-md text-xs text-gray-500 hidden"
                                                    value={item.description}
                                                    onChange={(e) => handleItemChange(index, 'description', e.target.value)}
                                                />
                                            </div>
                                        </td>
                                        <td className="px-3 py-2">
                                            <input
                                                type="number"
                                                className="block w-full border-gray-300 rounded-md text-sm text-right focus:ring-blue-500 focus:border-blue-500"
                                                value={item.quantity}
                                                onChange={(e) => handleItemChange(index, 'quantity', parseFloat(e.target.value))}
                                            />
                                        </td>
                                        <td className="px-3 py-2">
                                            <input
                                                type="number"
                                                className="block w-full border-gray-300 rounded-md text-sm text-right focus:ring-blue-500 focus:border-blue-500"
                                                value={item.unitPrice}
                                                onChange={(e) => handleItemChange(index, 'unitPrice', parseFloat(e.target.value))}
                                            />
                                        </td>
                                        <td className="px-3 py-2">
                                            <input
                                                type="number"
                                                className="block w-full border-gray-300 rounded-md text-sm text-right focus:ring-blue-500 focus:border-blue-500"
                                                value={item.discountPercent}
                                                onChange={(e) => handleItemChange(index, 'discountPercent', parseFloat(e.target.value))}
                                            />
                                        </td>
                                        <td className="px-3 py-2">
                                            <input
                                                type="number"
                                                className="block w-full border-gray-300 rounded-md text-sm text-right focus:ring-blue-500 focus:border-blue-500"
                                                value={item.taxPercent}
                                                onChange={(e) => handleItemChange(index, 'taxPercent', parseFloat(e.target.value))}
                                            />
                                        </td>
                                        <td className="px-3 py-2 text-right text-sm">
                                            {((item.quantity * item.unitPrice) * (1 - item.discountPercent / 100)).toFixed(2)}
                                        </td>
                                        <td className="px-3 py-2 text-center">
                                            <button onClick={() => handleRemoveItem(index)} className="text-red-600 hover:text-red-900">
                                                <Trash2 className="h-4 w-4" />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Footer / Totals */}
                <div className="flex justify-end pt-4">
                    <div className="w-64 space-y-2">
                        <div className="flex justify-between text-sm">
                            <span className="text-gray-500">{t('quotes.subTotal')}:</span>
                            <span className="font-medium">{totals.subTotal.toFixed(2)}</span>
                        </div>
                        <div className="flex justify-between text-sm">
                            <span className="text-gray-500">{t('quotes.discount')}:</span>
                            <span className="font-medium text-red-600">-{totals.discount.toFixed(2)}</span>
                        </div>
                        <div className="flex justify-between text-sm">
                            <span className="text-gray-500">{t('quotes.tax')}:</span>
                            <span className="font-medium">{totals.tax.toFixed(2)}</span>
                        </div>
                        <div className="flex justify-between text-base font-bold border-t pt-2">
                            <span>{t('quotes.total')}:</span>
                            <span>{totals.total.toFixed(2)}</span>
                        </div>
                    </div>
                </div>

                {/* Notes */}
                <div>
                    <label className="block text-sm font-medium mb-1">{t('quotes.notes')}</label>
                    <textarea
                        className="w-full px-3 py-2 rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        rows={3}
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                    />
                </div>
            </div>
        </Modal>
    );
};

export default InvoiceDialog;
