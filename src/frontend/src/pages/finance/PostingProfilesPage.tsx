import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2, Edit } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast, Modal, Input, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface PostingProfileDto {
    id: string;
    transactionType: number;
    category: string;
    accountId: string;
    isDefault: boolean;
}

interface AccountDto {
    id: string;
    code: string;
    name: string;
}

const TRANSACTION_TYPES = [
    { value: 1, label: 'General Journal' },
    { value: 2, label: 'Sales Invoice' },
    { value: 3, label: 'Purchase Invoice' },
    { value: 4, label: 'Incoming Payment' },
    { value: 5, label: 'Outgoing Payment' },
    { value: 6, label: 'Cheque Deposit' },
    { value: 7, label: 'Cheque Clearance' },
    { value: 8, label: 'Payroll Run' },
    { value: 9, label: 'Depreciation' },
    { value: 10, label: 'Inventory Shipment' },
    { value: 11, label: 'Inventory Receipt' },
    { value: 12, label: 'Exchange Variance' },
];

const PostingProfilesPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const [profiles, setProfiles] = useState<PostingProfileDto[]>([]);
    const [accounts, setAccounts] = useState<AccountDto[]>([]);
    const [loading, setLoading] = useState(true);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [formData, setFormData] = useState({
        transactionType: 2, // Default to SalesInvoice
        category: '',
        accountId: '',
        isDefault: false
    });

    const loadData = async () => {
        setLoading(true);
        try {
            const [profRes, accRes] = await Promise.all([
                api.finance.postingProfiles.getAll(),
                api.finance.accounts.getAll()
            ]);

            if (profRes.success) setProfiles(profRes.data || []);
            if (accRes.success) setAccounts(accRes.data || []);
        } catch (error) {
            toast.error(t('common.error'), t('common.loadFailed'));
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            let res;
            if (editingId) {
                res = await api.finance.postingProfiles.update(editingId, formData);
            } else {
                res = await api.finance.postingProfiles.create(formData);
            }

            if (res?.success) {
                toast.success(t('common.success'), t('common.saved'));
                setIsModalOpen(false);
                loadData();
            } else {
                toast.error(t('common.error'), res?.error || t('common.errorSaving'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.errorSaving'));
        }
    };

    const handleEdit = (profile: PostingProfileDto) => {
        setEditingId(profile.id);
        setFormData({
            transactionType: profile.transactionType,
            category: profile.category || '',
            accountId: profile.accountId,
            isDefault: profile.isDefault
        });
        setIsModalOpen(true);
    };

    const handleDelete = async (id: string) => {
        const confirmed = await dialog.danger({
            title: t('common.delete'),
            message: t('common.confirmDelete'),
            confirmText: t('common.delete'),
            cancelText: t('common.cancel')
        });

        if (!confirmed) return;

        try {
            const res = await api.finance.postingProfiles.delete(id);
            if (res.success) {
                toast.success(t('common.deletedSuccess'), t('common.deleted'));
                loadData();
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.errorDeleting'));
        }
    };

    const getAccountDisplay = (id: string) => {
        const acc = accounts.find(a => a.id === id);
        return acc ? `${acc.code} - ${acc.name}` : id;
    };

    const getTransactionTypeDisplay = (val: number) => {
        return TRANSACTION_TYPES.find(t => t.value === val)?.label || 'Unknown';
    };

    const columns: Column<PostingProfileDto>[] = [
        {
            key: 'transactionType',
            header: t('finance.postingProfiles.transactionType'),
            render: (row) => getTransactionTypeDisplay(row.transactionType)
        },
        { key: 'category', header: t('finance.postingProfiles.category') },
        {
            key: 'accountId',
            header: t('finance.journalEntries.account'),
            render: (row) => getAccountDisplay(row.accountId)
        },
        {
            key: 'isDefault',
            header: t('finance.postingProfiles.isDefault'),
            render: (row) => row.isDefault ? t('common.yes') : t('common.no')
        },
        {
            key: 'id',
            header: t('common.actions'),
            align: 'right',
            width: '120px',
            render: (row) => (
                <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="sm" onClick={() => handleEdit(row)}>
                        <Edit className="w-4 h-4 text-blue-500" />
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => handleDelete(row.id)}>
                        <Trash2 className="w-4 h-4 text-red-500" />
                    </Button>
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">{t('finance.postingProfiles.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.postingProfiles.subtitle')}</p>
                </div>
                <Button onClick={() => {
                    setEditingId(null);
                    setFormData({ transactionType: 2, category: '', accountId: '', isDefault: false });
                    setIsModalOpen(true);
                }}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('finance.postingProfiles.create')}
                </Button>
            </div>

            <DataTable
                data={profiles}
                columns={columns}
                keyField="id"
                isLoading={loading}
            />

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingId ? t('finance.postingProfiles.edit') : t('finance.postingProfiles.create')}
            >
                <form onSubmit={handleSave} className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('finance.postingProfiles.transactionType')}</label>
                        <select
                            className="w-full p-2 border rounded-md dark:bg-gray-800"
                            value={formData.transactionType}
                            onChange={e => setFormData({ ...formData, transactionType: parseInt(e.target.value) })}
                            required
                        >
                            {TRANSACTION_TYPES.map(type => (
                                <option key={type.value} value={type.value}>{type.label}</option>
                            ))}
                        </select>
                    </div>

                    <Input
                        label={t('finance.postingProfiles.category')}
                        value={formData.category}
                        onChange={e => setFormData({ ...formData, category: e.target.value })}
                        placeholder="e.g. Local Suppliers, EU Customers"
                    />

                    <div>
                        <label className="block text-sm font-medium mb-1">{t('finance.journalEntries.account')}</label>
                        <select
                            className="w-full p-2 border rounded-md dark:bg-gray-800"
                            value={formData.accountId}
                            onChange={e => setFormData({ ...formData, accountId: e.target.value })}
                            required
                        >
                            <option value="">{t('common.select')}</option>
                            {accounts.map(acc => (
                                <option key={acc.id} value={acc.id}>{acc.code} - {acc.name}</option>
                            ))}
                        </select>
                    </div>

                    <div className="flex items-center gap-2 mt-4 mb-2">
                        <input
                            type="checkbox"
                            id="isDefault"
                            checked={formData.isDefault}
                            onChange={e => setFormData({ ...formData, isDefault: e.target.checked })}
                            className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                        />
                        <label htmlFor="isDefault" className="text-sm font-medium">
                            {t('finance.postingProfiles.isDefault')}
                        </label>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <Button variant="secondary" type="button" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button type="submit">
                            {t('common.save')}
                        </Button>
                    </div>
                </form>
            </Modal>
        </div>
    );
};

export default PostingProfilesPage;
