import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, DollarSign } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';

export interface PaymentDto {
    id: string;
    paymentNumber: string;
    direction: string;
    method: string;
    amount: number;
    paymentDate: string; // ISO String
    partnerId: string;
    partnerName?: string;
    accountId: string;
    status: string;
}

const PAYMENT_DIRECTIONS = [
    { value: 1, label: 'Incoming' },
    { value: 2, label: 'Outgoing' }
];

const PAYMENT_METHODS = [
    { value: 1, label: 'Cash' },
    { value: 2, label: 'Bank Transfer' },
    { value: 3, label: 'Check' },
    { value: 4, label: 'Credit Card' },
    { value: 5, label: 'Other' }
];

const PaymentsPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();

    // State
    const [payments, setPayments] = useState<PaymentDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    // Lookups
    const [accounts, setAccounts] = useState<any[]>([]); // Bank/Cash Accounts
    const [partners, setPartners] = useState<any[]>([]); // Partners

    // Form
    const [formData, setFormData] = useState({
        direction: 1,
        method: 1,
        partnerId: '',
        amount: 0,
        currencyId: '00000000-0000-0000-0000-000000000000', // Default or fetch
        accountId: '',
        paymentDate: new Date().toISOString().split('T')[0],
        referenceNumber: '',
        notes: ''
    });

    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            const [payRes, accRes, partRes] = await Promise.all([
                api.finance.payments.getAll(),
                api.finance.accounts.getAll(),
                (api as any).crm?.partners?.getAll() || Promise.resolve({ success: true, data: [] }) // Temporary safe access until crm added to type
            ]);

            if (payRes.success) setPayments(payRes.data || []);
            if (accRes.success) setAccounts((accRes.data || []).filter((a: any) => a.isBankAccount || a.type === "Asset")); // Simple filter
            if (partRes.success) setPartners((partRes.data as any).data || []); // Pagination result wrap check needed?

        } catch (error) {
            console.error(error);
            toast.error(t('common.error'), t('common.loadFailed'));
        } finally {
            setLoading(false);
        }
    }, [toast, t]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleCreate = () => {
        setFormData({
            direction: 1,
            method: 1,
            partnerId: '',
            amount: 0,
            currencyId: '00000000-0000-0000-0000-000000000000', // To be properly handled with lookup
            accountId: '',
            paymentDate: new Date().toISOString().split('T')[0],
            referenceNumber: '',
            notes: ''
        });
        setIsDialogOpen(true);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const payload = {
                ...formData,
                amount: Number(formData.amount),
                direction: Number(formData.direction),
                method: Number(formData.method),
                currencyId: "00000000-0000-0000-0000-000000000000" // Placeholder
            };

            const res = await api.finance.payments.create(payload);
            if (res.success) {
                toast.success(t('common.success'), t('finance.payments.created', 'Payment recorded'));
                setIsDialogOpen(false);
                loadData();
            } else {
                toast.error(t('common.error'), res.error || t('finance.payments.createFailed', 'Failed to create payment'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.unexpectedError'));
        }
    };

    const columns: Column<PaymentDto>[] = [
        { key: 'paymentNumber', header: t('common.number'), render: (row) => <span className="font-mono">{row.paymentNumber}</span> },
        {
            key: 'paymentDate',
            header: t('finance.payments.date'),
            render: (row) => new Date(row.paymentDate).toLocaleDateString()
        },
        {
            key: 'direction',
            header: t('finance.payments.direction'),
            render: (row) => {
                // Direction comes as string "Incoming" or "Outgoing" from backend usually, or mapped?
                // DTO says string.
                // If it matches keys in en.json (Incoming, Outgoing), we can try to map.
                // The keys in json are "1" and "2".
                // Let's assume the string matches the english label or map it.
                // Or better, use the enum value if available. But here we have strings.
                // Quick translation if string matches 'Incoming' / 'Outgoing'
                const dirMap: Record<string, string> = { "Incoming": "1", "Outgoing": "2" };
                const key = dirMap[row.direction] || "1";
                const label = t(`finance.payments.directions.${key}`);

                return (
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${row.direction === 'Incoming' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {label}
                    </span>
                );
            }
        },
        { key: 'partnerId', header: t('finance.payments.partner'), render: (row) => partners.find(p => p.id === row.partnerId)?.name || t('common.unknown') },
        { key: 'amount', header: t('finance.payments.amount'), render: (row) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'TRY' }).format(row.amount) }, // Hardcoded currency for now
        {
            key: 'method',
            header: t('finance.payments.method'),
            render: (row) => {
                // row.method is likely string "Cash", "Bank Transfer" etc.
                const methodMap: Record<string, string> = {
                    "Cash": "1", "Bank Transfer": "2", "Check": "3", "Credit Card": "4", "Other": "5"
                };
                // Allow reverse lookup or direct if integer? DTO says string.
                const key = methodMap[row.method] || "5";
                return t(`finance.payments.methods.${key}`);
            }
        },
        { key: 'status', header: t('finance.payments.status') }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">{t('finance.payments.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.payments.subtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('finance.payments.create')}
                </Button>
            </div>

            <DataTable
                data={payments}
                columns={columns}
                keyField="id"
                isLoading={loading}
            />

            {isDialogOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
                    <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl w-full max-w-2xl">
                        <div className="p-6 border-b dark:border-gray-700 flex justify-between items-center">
                            <h2 className="text-xl font-bold flex items-center gap-2">
                                <DollarSign className="w-5 h-5 text-green-600" />
                                {t('finance.payments.record')}
                            </h2>
                            <button onClick={() => setIsDialogOpen(false)} className="text-gray-500 hover:text-gray-700">&times;</button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.date')}</label>
                                    <input
                                        type="date" required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.paymentDate}
                                        onChange={e => setFormData({ ...formData, paymentDate: e.target.value })}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.direction')}</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.direction}
                                        onChange={e => setFormData({ ...formData, direction: Number(e.target.value) })}
                                    >
                                        {PAYMENT_DIRECTIONS.map(d => <option key={d.value} value={d.value}>{t(`finance.payments.directions.${d.value}`)}</option>)}
                                    </select>
                                </div>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.partner')}</label>
                                    <select
                                        required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.partnerId}
                                        onChange={e => setFormData({ ...formData, partnerId: e.target.value })}
                                    >
                                        <option value="">{t('common.select')}</option>
                                        {partners.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.amount')}</label>
                                    <input
                                        type="number" step="0.01" required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.amount}
                                        onChange={e => setFormData({ ...formData, amount: Number(e.target.value) })}
                                    />
                                </div>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.method')}</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.method}
                                        onChange={e => setFormData({ ...formData, method: Number(e.target.value) })}
                                    >
                                        {PAYMENT_METHODS.map(m => <option key={m.value} value={m.value}>{t(`finance.payments.methods.${m.value}`)}</option>)}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.payments.account')}</label>
                                    <select
                                        required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.accountId}
                                        onChange={e => setFormData({ ...formData, accountId: e.target.value })}
                                    >
                                        <option value="">{t('common.select')}</option>
                                        {accounts.map(a => <option key={a.id} value={a.id}>{a.code} - {a.name}</option>)}
                                    </select>
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">{t('finance.payments.reference')}</label>
                                <input
                                    type="text"
                                    className="w-full p-2 border rounded-md dark:bg-gray-900"
                                    value={formData.referenceNumber}
                                    onChange={e => setFormData({ ...formData, referenceNumber: e.target.value })}
                                />
                            </div>

                            <div className="flex justify-end gap-3 pt-4">
                                <button
                                    type="button"
                                    onClick={() => setIsDialogOpen(false)}
                                    className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
                                >
                                    {t('common.cancel')}
                                </button>
                                <Button type="submit" disabled={loading}>
                                    {t('finance.payments.record')}
                                </Button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default PaymentsPage;
