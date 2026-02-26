import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2, Edit } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast, Modal, Input, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface TaxProfileDto {
    id: string;
    name: string;
    description: string;
    vatRate: number;
    withholdingRate: number;
    stampDutyRate: number;
    vatAccountId?: string;
    withholdingAccountId?: string;
    stampDutyAccountId?: string;
    isActive: boolean;
}

interface AccountDto {
    id: string;
    code: string;
    name: string;
}

const AdvancedTaxProfilesPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const [profiles, setProfiles] = useState<TaxProfileDto[]>([]);
    const [accounts, setAccounts] = useState<AccountDto[]>([]);
    const [loading, setLoading] = useState(true);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        vatRate: 0,
        withholdingRate: 0,
        stampDutyRate: 0,
        vatAccountId: '',
        withholdingAccountId: '',
        stampDutyAccountId: '',
        isActive: true
    });

    const loadData = async () => {
        setLoading(true);
        try {
            const [profRes, accRes] = await Promise.all([
                api.finance.taxProfiles.getAll(),
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
            const dto = {
                ...formData,
                vatAccountId: formData.vatAccountId || null,
                withholdingAccountId: formData.withholdingAccountId || null,
                stampDutyAccountId: formData.stampDutyAccountId || null
            };

            if (editingId) {
                res = await api.finance.taxProfiles.update(editingId, dto);
            } else {
                res = await api.finance.taxProfiles.create(dto);
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

    const handleEdit = (profile: TaxProfileDto) => {
        setEditingId(profile.id);
        setFormData({
            name: profile.name,
            description: profile.description || '',
            vatRate: profile.vatRate,
            withholdingRate: profile.withholdingRate,
            stampDutyRate: profile.stampDutyRate,
            vatAccountId: profile.vatAccountId || '',
            withholdingAccountId: profile.withholdingAccountId || '',
            stampDutyAccountId: profile.stampDutyAccountId || '',
            isActive: profile.isActive
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
            const res = await api.finance.taxProfiles.delete(id);
            if (res.success) {
                toast.success(t('common.deletedSuccess'), t('common.deleted'));
                loadData();
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.errorDeleting'));
        }
    };

    const columns: Column<TaxProfileDto>[] = [
        { key: 'name', header: t('finance.taxProfiles.name', 'Profile Name') },
        {
            key: 'vatRate',
            header: t('finance.taxProfiles.vat', 'KDV (%)'),
            align: 'right',
            render: (row) => row.vatRate + '%'
        },
        {
            key: 'withholdingRate',
            header: t('finance.taxProfiles.withholding', 'Stopaj (%)'),
            align: 'right',
            render: (row) => row.withholdingRate + '%'
        },
        {
            key: 'stampDutyRate',
            header: t('finance.taxProfiles.stampDuty', 'Damga Vergisi (%)'),
            align: 'right',
            render: (row) => row.stampDutyRate + '%'
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (row) => row.isActive ? (
                <span className="px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs font-medium">
                    {t('common.active')}
                </span>
            ) : (
                <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded-full text-xs font-medium">
                    {t('common.inactive')}
                </span>
            )
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
                    <h1 className="text-2xl font-bold">{t('finance.taxProfiles.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.taxProfiles.subtitle')}</p>
                </div>
                <Button onClick={() => {
                    setEditingId(null);
                    setFormData({
                        name: '',
                        description: '',
                        vatRate: 0,
                        withholdingRate: 0,
                        stampDutyRate: 0,
                        vatAccountId: '',
                        withholdingAccountId: '',
                        stampDutyAccountId: '',
                        isActive: true
                    });
                    setIsModalOpen(true);
                }}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('finance.taxProfiles.create')}
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
                title={editingId ? t('finance.taxProfiles.edit') : t('finance.taxProfiles.create')}
                size="lg"
            >
                <form onSubmit={handleSave} className="space-y-4">
                    <Input
                        label={t('common.name')}
                        value={formData.name}
                        onChange={e => setFormData({ ...formData, name: e.target.value })}
                        required
                    />

                    <Input
                        label={t('common.description')}
                        value={formData.description}
                        onChange={e => setFormData({ ...formData, description: e.target.value })}
                    />

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-md bg-gray-50">
                        <div className="font-bold text-gray-700 md:col-span-2 border-b pb-2">KDV (VAT)</div>
                        <Input
                            label={t('finance.taxProfiles.vat', 'Rate (%)')}
                            type="number"
                            step="0.01"
                            value={formData.vatRate}
                            onChange={e => setFormData({ ...formData, vatRate: parseFloat(e.target.value) || 0 })}
                        />
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('finance.taxProfiles.vatAccount', 'Account')}</label>
                            <select
                                className="w-full p-2 border rounded-md dark:bg-gray-800"
                                value={formData.vatAccountId}
                                onChange={e => setFormData({ ...formData, vatAccountId: e.target.value })}
                            >
                                <option value="">{t('common.none')}</option>
                                {accounts.map(acc => (
                                    <option key={acc.id} value={acc.id}>{acc.code} - {acc.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-md bg-gray-50">
                        <div className="font-bold text-gray-700 md:col-span-2 border-b pb-2">Stopaj (Withholding)</div>
                        <Input
                            label={t('finance.taxProfiles.withholding', 'Rate (%)')}
                            type="number"
                            step="0.01"
                            value={formData.withholdingRate}
                            onChange={e => setFormData({ ...formData, withholdingRate: parseFloat(e.target.value) || 0 })}
                        />
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('finance.taxProfiles.withholdingAccount', 'Account')}</label>
                            <select
                                className="w-full p-2 border rounded-md dark:bg-gray-800"
                                value={formData.withholdingAccountId}
                                onChange={e => setFormData({ ...formData, withholdingAccountId: e.target.value })}
                            >
                                <option value="">{t('common.none')}</option>
                                {accounts.map(acc => (
                                    <option key={acc.id} value={acc.id}>{acc.code} - {acc.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border p-4 rounded-md bg-gray-50">
                        <div className="font-bold text-gray-700 md:col-span-2 border-b pb-2">Damga Vergisi (Stamp Duty)</div>
                        <Input
                            label={t('finance.taxProfiles.stampDuty', 'Rate (%)')}
                            type="number"
                            step="0.01"
                            value={formData.stampDutyRate}
                            onChange={e => setFormData({ ...formData, stampDutyRate: parseFloat(e.target.value) || 0 })}
                        />
                        <div>
                            <label className="block text-sm font-medium mb-1">{t('finance.taxProfiles.stampDutyAccount', 'Account')}</label>
                            <select
                                className="w-full p-2 border rounded-md dark:bg-gray-800"
                                value={formData.stampDutyAccountId}
                                onChange={e => setFormData({ ...formData, stampDutyAccountId: e.target.value })}
                            >
                                <option value="">{t('common.none')}</option>
                                {accounts.map(acc => (
                                    <option key={acc.id} value={acc.id}>{acc.code} - {acc.name}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="flex items-center gap-2 mt-4 mb-2">
                        <input
                            type="checkbox"
                            id="isActive"
                            checked={formData.isActive}
                            onChange={e => setFormData({ ...formData, isActive: e.target.checked })}
                            className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                        />
                        <label htmlFor="isActive" className="text-sm font-medium">
                            {t('common.active')}
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

export default AdvancedTaxProfilesPage;
