import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Clock } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { WorkShiftDialog } from './WorkShiftDialog';

interface WorkShift {
    id: string;
    name: string;
    startTime: string;
    endTime: string;
    breakMinutes: number;
}

const API_BASE = '/api/hr/work-shifts';

export function WorkShiftsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [shifts, setShifts] = useState<WorkShift[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingShift, setEditingShift] = useState<WorkShift | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(API_BASE, { cache: 'no-store' });
            if (res.ok) setShifts(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setEditingShift(null); setDialogOpen(true); };
    const handleEdit = (shift: WorkShift) => { setEditingShift(shift); setDialogOpen(true); };

    const handleDelete = async (shift: WorkShift) => {
        const confirmed = await dialog.danger({
            title: t('hr.deleteWorkShift'),
            message: t('hr.confirmDeleteWorkShift'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                const res = await fetch(`${API_BASE}/${shift.id}`, { method: 'DELETE', cache: 'no-store' });
                if (res.ok) { toast.success(t('hr.workShiftDeleted')); loadData(); }
                else toast.error(t('common.error'));
            } catch { toast.error(t('common.error')); }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const columns: Column<WorkShift>[] = [
        {
            key: 'name',
            header: t('common.name'),
            render: (shift) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 flex items-center justify-center">
                        <Clock className="w-5 h-5" />
                    </div>
                    <span className="font-semibold">{shift.name}</span>
                </div>
            ),
        },
        { key: 'startTime', header: t('hr.startTime') },
        { key: 'endTime', header: t('hr.endTime') },
        {
            key: 'breakMinutes',
            header: t('hr.breakMinutes'),
            render: (s) => <span>{s.breakMinutes} {t('hr.units.min')}</span>
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Clock className="w-6 h-6" />
                        {t('hr.workShifts')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.workShiftsSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createWorkShift')}
                </Button>
            </div>

            <DataTable
                data={shifts}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(shift) => (
                    <div className="flex items-center gap-1">
                        <button onClick={() => handleEdit(shift)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.edit')}>
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleDelete(shift)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600" title={t('common.delete')}>
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <WorkShiftDialog open={dialogOpen} onClose={handleDialogClose} shift={editingShift} />
        </div>
    );
}
