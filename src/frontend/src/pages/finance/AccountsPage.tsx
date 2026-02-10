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
    { value: 1, label: '1' },
    { value: 2, label: '2' },
    { value: 3, label: '3' },
    { value: 4, label: '4' },
    { value: 5, label: '5' },
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
            toast.error(t('common.error'), t('finance.accounts.loadFailed', 'Failed to load accounts'));
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
        // Type is just an int in DTO, so we rely on that.
        // If coming from label finding: const typeValue = ACCOUNTS_TYPES.find(t => t.label === account.type)?.value || 1;
        // But better if DTO has int type or we trust it matches. 
        // Assuming backend sends string representation or int? DTO says string type.
        // If string, we might need parsing. Let's assume it sends "Asset" etc or we need to map back.
        // Actually the DTO `type: string` in line 14 suggests it comes as string name.
        // Let's safe find it.
        // Update: The backend likely sends the Enum name.
        // We will just map it if possible or keep as is.
        // For editing, we need the int value.
        // Let's assume backend returns "Asset" string. We need to find the ID.
        // Helper to find ID from string (English based usually or Enum name)
        let typeValue = 1;
        const typeStr = account.type;
        if (typeStr === "Asset" || typeStr === "Varlık") typeValue = 1;
        else if (typeStr === "Liability" || typeStr === "Yükümlülük") typeValue = 2;
        else if (typeStr === "Equity" || typeStr === "Özkaynak") typeValue = 3;
        else if (typeStr === "Revenue" || typeStr === "Gelir") typeValue = 4;
        else if (typeStr === "Expense" || typeStr === "Gider") typeValue = 5;

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
                toast.success(t('common.success'), isEditing ? t('finance.accounts.updated', 'Account updated') : t('finance.accounts.created', 'Account created'));
                setIsDialogOpen(false);
                loadAccounts();
            } else {
                toast.error(t('common.error'), res.error || t('common.operationFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.unexpectedError'));
        }
    };

    // Columns
    const columns: Column<AccountDto>[] = [
        {
            key: 'code',
            header: t('finance.accounts.code'),
            render: (row) => <span className="font-mono font-medium">{row.code}</span>
        },
        {
            key: 'name',
            header: t('finance.accounts.name'),
            render: (row) => (
                <div className="flex flex-col">
                    <span className="font-medium">{row.name}</span>
                    {row.description && <span className="text-xs text-gray-500">{row.description}</span>}
                </div>
            )
        },
        {
            key: 'type',
            header: t('finance.accounts.type'),
            render: (row) => {
                // Try to map back if it comes as string "Asset" etc. 
                // Or just translate whatever comes if it matches keys?
                // Safer: Map string to int then translate.
                // If row.type is "Asset", we want t('finance.accounts.types.1')
                // Quick map:
                const map: Record<string, string> = { "Asset": "1", "Liability": "2", "Equity": "3", "Revenue": "4", "Expense": "5" };
                const key = map[row.type] || "1";
                return <span className="inline-block px-2 py-1 text-xs rounded-full bg-gray-100 dark:bg-gray-800">{t(`finance.accounts.types.${key}`)}</span>
            }
        },
        {
            key: 'parentAccountName',
            header: t('finance.accounts.parent'),
        },
        {
            key: 'isHeader',
            header: t('finance.accounts.isHeader'),
            render: (row) => row.isHeader ? t('common.yes') : t('common.no')
        }
    ];

    // Filter potential parents (exclude self and children in future)
    const parentOptions = accounts.filter(a => a.id !== formData.id);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('finance.accounts.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.accounts.subtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('finance.accounts.create')}
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
                        title={t('common.edit')}
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
                            <h2 className="text-xl font-bold">{isEditing ? t('finance.accounts.edit') : t('finance.accounts.create')}</h2>
                            <button onClick={() => setIsDialogOpen(false)} className="text-gray-500 hover:text-gray-700">
                                &times;
                            </button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">{t('finance.accounts.code')}</label>
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
                                    <label className="block text-sm font-medium mb-1">{t('finance.accounts.type')}</label>
                                    <select
                                        className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                        value={formData.type}
                                        disabled={isEditing} // Often changing type is restricted
                                        onChange={e => setFormData({ ...formData, type: parseInt(e.target.value) })}
                                    >
                                        {ACCOUNTS_TYPES.map(tOption => (
                                            <option key={tOption.value} value={tOption.value}>{t(`finance.accounts.types.${tOption.label}`)}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">{t('finance.accounts.name')}</label>
                                <input
                                    type="text"
                                    required
                                    className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                    value={formData.name}
                                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">{t('finance.accounts.parent')}</label>
                                <select
                                    className="w-full p-2 border rounded-md dark:bg-gray-900 dark:border-gray-700"
                                    value={formData.parentAccountId}
                                    onChange={e => setFormData({ ...formData, parentAccountId: e.target.value })}
                                >
                                    <option value="">{t('common.none', '(None)')}</option>
                                    {parentOptions.map(a => (
                                        <option key={a.id} value={a.id}>{a.code} - {a.name}</option>
                                    ))}
                                </select>
                            </div>

                            <div>
                                <label className="block text-sm font-medium mb-1">{t('finance.accounts.description')}</label>
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
                                <label htmlFor="isHeader" className="text-sm">{t('finance.accounts.isHeader')}</label>
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
                                    {isEditing ? t('common.update') : t('common.create')}
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
