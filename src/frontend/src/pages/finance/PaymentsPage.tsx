import React, { useEffect, useState, useCallback } from 'react';
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
            toast.error('Error', 'Failed to load data');
        } finally {
            setLoading(false);
        }
    }, [toast]);

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
                toast.success('Success', 'Payment recorded');
                setIsDialogOpen(false);
                loadData();
            } else {
                toast.error('Error', res.error || 'Failed to create payment');
            }
        } catch (error) {
            toast.error('Error', 'Unexpected error');
        }
    };

    const columns: Column<PaymentDto>[] = [
        { key: 'paymentNumber', header: 'Number', render: (row) => <span className="font-mono">{row.paymentNumber}</span> },
        {
            key: 'paymentDate',
            header: 'Date',
            render: (row) => new Date(row.paymentDate).toLocaleDateString()
        },
        {
            key: 'direction',
            header: 'Direction',
            render: (row) => (
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${row.direction === 'Incoming' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {row.direction}
                </span>
            )
        },
        { key: 'partnerId', header: 'Partner', render: (row) => partners.find(p => p.id === row.partnerId)?.name || 'Unknown' },
        { key: 'amount', header: 'Amount', render: (row) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'TRY' }).format(row.amount) }, // Hardcoded currency for now
        { key: 'method', header: 'Method' },
        { key: 'status', header: 'Status' }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">Payments</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">Manage incoming and outgoing payments</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    New Payment
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
                                Record Payment
                            </h2>
                            <button onClick={() => setIsDialogOpen(false)} className="text-gray-500 hover:text-gray-700">&times;</button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">Payment Date</label>
                                    <input
                                        type="date" required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.paymentDate}
                                        onChange={e => setFormData({ ...formData, paymentDate: e.target.value })}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">Direction</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.direction}
                                        onChange={e => setFormData({ ...formData, direction: Number(e.target.value) })}
                                    >
                                        {PAYMENT_DIRECTIONS.map(d => <option key={d.value} value={d.value}>{d.label}</option>)}
                                    </select>
                                </div>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">Partner</label>
                                    <select
                                        required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.partnerId}
                                        onChange={e => setFormData({ ...formData, partnerId: e.target.value })}
                                    >
                                        <option value="">Select Partner</option>
                                        {partners.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">Amount</label>
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
                                    <label className="block text-sm font-medium mb-1">Payment Method</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.method}
                                        onChange={e => setFormData({ ...formData, method: Number(e.target.value) })}
                                    >
                                        {PAYMENT_METHODS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">Bank/Cash Account</label>
                                    <select
                                        required
                                        className="w-full p-2 border rounded-md dark:bg-gray-900"
                                        value={formData.accountId}
                                        onChange={e => setFormData({ ...formData, accountId: e.target.value })}
                                    >
                                        <option value="">Select Account</option>
                                        {accounts.map(a => <option key={a.id} value={a.id}>{a.code} - {a.name}</option>)}
                                    </select>
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">Reference Number</label>
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
                                    Cancel
                                </button>
                                <Button type="submit" disabled={loading}>
                                    Record Payment
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
