import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Eye, Plus, Undo2 } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { CreateJournalEntryDialog } from './components/CreateJournalEntryDialog';

interface JournalEntryDto {
    id: string;
    entryNumber: string;
    description: string;
    entryDate: string;
    status: string;
    totalDebit: number;
    totalCredit: number;
    sourceType?: string;
}

const JournalEntriesPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const [entries, setEntries] = useState<JournalEntryDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [isCreateOpen, setIsCreateOpen] = useState(false);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        setLoading(true);
        try {
            const res = await api.finance.journalEntries.getAll();
            if (res.success) {
                setEntries(res.data || []);
            } else {
                toast.error(t('common.error'), res.error || t('common.loadFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'), "Failed to load journal entries");
        } finally {
            setLoading(false);
        }
    };

    const handleReverse = async (id: string) => {
        const confirmed = await dialog.danger({
            title: t('finance.journalEntries.void', 'Void / Reverse'),
            message: "Are you sure you want to reverse this journal entry? This will create a permanent offsetting entry.",
            confirmText: "Reverse Entry",
            cancelText: t('common.cancel')
        });

        if (!confirmed) return;

        setLoading(true);
        try {
            const res = await api.finance.journalEntries.reverse(id);
            if (res.success) {
                toast.success(t('common.success'), "Entry reversed successfully");
                loadData();
            } else {
                toast.error(t('common.error'), res.error || "Failed to reverse entry");
            }
        } catch (error) {
            toast.error(t('common.error'), "Failed to reverse entry");
        } finally {
            setLoading(false);
        }
    };

    const columns: Column<JournalEntryDto>[] = [
        { key: 'entryNumber', header: t('finance.journalEntries.entryNumber'), width: '120px' },
        {
            key: 'entryDate',
            header: t('finance.journalEntries.entryDate'),
            render: (row) => new Date(row.entryDate).toLocaleDateString(),
            width: '120px'
        },
        { key: 'description', header: t('finance.journalEntries.description') },
        { key: 'sourceType', header: t('finance.journalEntries.source'), width: '100px' },
        {
            key: 'totalDebit',
            header: t('finance.journalEntries.total'),
            align: 'right',
            render: (row) => row.totalDebit.toFixed(2),
            width: '120px'
        },
        { key: 'status', header: t('finance.journalEntries.status'), width: '100px' },
        {
            key: 'id',
            header: t('common.actions'),
            align: 'right',
            width: '120px',
            render: (row) => (
                <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="sm" title={t('common.view')}>
                        <Eye className="w-4 h-4" />
                    </Button>
                    {row.status !== 'Reversed' && row.status !== 'Void' && (
                        <Button variant="ghost" size="sm" onClick={() => handleReverse(row.id)} title={t('finance.journalEntries.void', 'Reverse')}>
                            <Undo2 className="w-4 h-4 text-orange-500" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">{t('finance.journalEntries.title')}</h1>
                    <p className="text-gray-500">{t('finance.journalEntries.subtitle')}</p>
                </div>
                <Button onClick={() => setIsCreateOpen(true)}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('common.create')}
                </Button>
            </div>

            <DataTable
                data={entries}
                columns={columns}
                keyField="id"
                isLoading={loading}
            />

            <CreateJournalEntryDialog
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
                onSuccess={() => {
                    loadData();
                    setIsCreateOpen(false);
                }}
            />
        </div>
    );
};

export default JournalEntriesPage;
