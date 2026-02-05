import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast } from '@/components/ui'; // Check if Dialog export exists or use native
import type { Column } from '@/components/ui';

// DTOs
export interface AccountDto {
    id: string;
    code: string;
    name: string;
    description?: string;
    type: string;
    parentAccountId?: string;
    parentAccountName?: string;
    isHeader: boolean;
    isActive: boolean;
}

const ACCOUNTS_TYPES = [
    { value: 1, label: 'Asset' },
    { value: 2, label: 'Liability' },
    { value: 3, label: 'Equity' },
    { value: 4, label: 'Revenue' },
    { value: 5, label: 'Expense' },
];

const AccountsPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();

    // State
    const [accounts, setAccounts] = useState<AccountDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    // Form State
    const [formData, setFormData] = useState({
        id: '',
        code: '',
        name: '',
        type: 1,
        description: '',
        parentAccountId: '',
        isHeader: false
    });
    const [isEditing, setIsEditing] = useState(false);

    // Load Data
    const loadAccounts = useCallback(async () => {
        setLoading(true);
        try {
            const res = await api.finance.accounts.getAll();
            if (res.success && res.data) {
                setAccounts(res.data);
            }
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'), 'Failed to load accounts');
        } finally {
            setLoading(false);
        }
    }, [toast, t]);

    useEffect(() => {
        loadAccounts();
    }, [loadAccounts]);

    // Handlers
    const handleCreate = () => {
        setFormData({
            id: '',
            code: '',
            name: '',
            type: 1,
            description: '',
            parentAccountId: '',
            isHeader: false
        });
        setIsEditing(false);
        setIsDialogOpen(true);
    };

    const handleEdit = (account: AccountDto) => {
        const typeValue = ACCOUNTS_TYPES.find(t => t.label === account.type)?.value || 1;
        setFormData({
            id: account.id,
            code: account.code,
            name: account.name,
            type: typeValue,
            description: account.description || '',
            parentAccountId: account.parentAccountId || '',
            isHeader: account.isHeader
        });
        setIsEditing(true);
        setIsDialogOpen(true);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const data = {
                code: formData.code,
                name: formData.name,
                type: formData.type,
                description: formData.description,
                parentAccountId: formData.parentAccountId || null,
                isHeader: formData.isHeader,
                isBankAccount: false, // Default for now
                isActive: true
            };

            let res;
            if (isEditing) {
                res = await api.finance.accounts.update(formData.id, data);
            } else {
                res = await api.finance.accounts.create(data);
            }

            if (res.success) {
                toast.success('Success', `Account ${isEditing ? 'updated' : 'created'}`);
                setIsDialogOpen(false);
                loadAccounts();
            } else {
                toast.error('Error', res.error || 'Operation failed');
            }
        } catch (error) {
            toast.error('Error', 'An unexpected error occurred');
        }
    };

    // Columns
    const columns: Column<AccountDto>[] = [
        {
            key: 'code',
            header: 'Code',
            render: (row) => <span className="font-mono font-medium">{row.code}</span>
        },
        {
            key: 'name',
            header: 'Name',
            render: (row) => (
                <div className="flex flex-col">
                    <span className="font-medium">{row.name}</span>
                    {row.description && <span className="text-xs text-gray-500">{row.description}</span>}
                </div>
            )
        },
        {
            key: 'type',
            header: 'Type',
            render: (row) => <span className="inline-block px-2 py-1 text-xs rounded-full bg-gray-100 dark:bg-gray-800">{row.type}</span>
        },
        {
            key: 'parentAccountName',
            header: 'Parent',
        },
        {
            key: 'isHeader',
            header: 'Header',
            render: (row) => row.isHeader ? 'Yes' : 'No'
        }
    ];

    // Filter potential parents (exclude self and children in future)
    const parentOptions = accounts.filter(a => a.id !== formData.id);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">Chart of Accounts</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">Manage general ledger accounts</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    New Account
                </Button>
            </div>

            <DataTable
                data={accounts}
                columns={columns}
                keyField="id"
                isLoading={loading}
                actions={(row) => (
                    <button
                        onClick={() => handleEdit(row)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                        title="Edit"
                    >
                        <Pencil className="w-4 h-4" />
                    </button>
                )}
            />

            {/* Create/Edit Modal */}
            {isDialogOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
                    <div className="bg-white dark:bg-gray-800 rounded-xl shadow-xl w-full max-w-lg">
                        <div className="p-6 border-b dark:border-gray-700 flex justify-between items-center">
                            <h2 className="text-xl font-bold">{isEditing ? 'Edit Account' : 'New Account'}</h2>
                            <button onClick={() => setIsDialogOpen(false)} className="text-gray-500 hover:text-gray-700">
                                &times;
                            </button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">Code</label>
                                    <input
                                        type="text"
                                        required
                                        disabled={isEditing}
                                        className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                        value={formData.code}
                                        onChange={e => setFormData({ ...formData, code: e.target.value })}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">Type</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                        value={formData.type}
                                        disabled={isEditing} // Often changing type is restricted
                                        onChange={e => setFormData({ ...formData, type: parseInt(e.target.value) })}
                                    >
                                        {ACCOUNTS_TYPES.map(t => (
                                            <option key={t.value} value={t.value}>{t.label}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">Name</label>
                                <input
                                    type="text"
                                    required
                                    className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                    value={formData.name}
                                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">Parent Account</label>
                                <select
                                    className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                    value={formData.parentAccountId}
                                    onChange={e => setFormData({ ...formData, parentAccountId: e.target.value })}
                                >
                                    <option value="">(None)</option>
                                    {parentOptions.map(a => (
                                        <option key={a.id} value={a.id}>{a.code} - {a.name}</option>
                                    ))}
                                </select>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">Description</label>
                                <textarea
                                    className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                    rows={3}
                                    value={formData.description}
                                    onChange={e => setFormData({ ...formData, description: e.target.value })}
                                />
                            </div>

                            <div className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    id="isHeader"
                                    checked={formData.isHeader}
                                    onChange={e => setFormData({ ...formData, isHeader: e.target.checked })}
                                    className="rounded border-gray-300 dark:border-gray-700"
                                />
                                <label htmlFor="isHeader" className="text-sm">Is Header Account (Cannot accept entries)</label>
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
                                    {isEditing ? 'Update' : 'Create'}
                                </Button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default AccountsPage;
