import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2, Calendar } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { PublicHolidayDialog } from './PublicHolidayDialog';

interface PublicHoliday {
    id: string;
    date: string;
    name: string;
    isHalfDay: boolean;
}

const API_BASE = '/api/hr/public-holidays';

export function PublicHolidaysPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [holidays, setHolidays] = useState<PublicHoliday[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(API_BASE, { cache: 'no-store' });
            if (res.ok) setHolidays(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setDialogOpen(true); };

    const handleDelete = async (holiday: PublicHoliday) => {
        const confirmed = await dialog.danger({
            title: t('hr.deletePublicHoliday'),
            message: t('hr.confirmDeletePublicHoliday'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                const res = await fetch(`${API_BASE}/${holiday.id}`, { method: 'DELETE', cache: 'no-store' });
                if (res.ok) { toast.success(t('hr.publicHolidayDeleted')); loadData(); }
                else toast.error(t('common.error'));
            } catch { toast.error(t('common.error')); }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const columns: Column<PublicHoliday>[] = [
        {
            key: 'date',
            header: t('common.date'),
            render: (h) => new Date(h.date).toLocaleDateString()
        },
        {
            key: 'name',
            header: t('common.name'),
            render: (h) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-orange-100 dark:bg-orange-900/30 text-orange-600 flex items-center justify-center">
                        <Calendar className="w-5 h-5" />
                    </div>
                    <div>
                        <span className="font-semibold block">{h.name}</span>
                        {h.isHalfDay && <span className="text-xs px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400">Half Day</span>}
                    </div>
                </div>
            ),
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Calendar className="w-6 h-6" />
                        {t('hr.publicHolidays')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.publicHolidaysSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createPublicHoliday')}
                </Button>
            </div>

            <DataTable
                data={holidays}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(holiday) => (
                    <button onClick={() => handleDelete(holiday)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600" title={t('common.delete')}>
                        <Trash2 className="w-4 h-4" />
                    </button>
                )}
            />

            <PublicHolidayDialog open={dialogOpen} onClose={handleDialogClose} />
        </div>
    );
}
