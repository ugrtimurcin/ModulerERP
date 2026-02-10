import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Calendar, Lock, Unlock, Plus } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';

// Backend PeriodStatus
const PeriodStatus = {
    Open: 1,
    Closed: 2,
    Locked: 3
} as const;

type PeriodStatus = typeof PeriodStatus[keyof typeof PeriodStatus];

interface FiscalPeriodDto {
    id: string;
    code: string;
    name: string;
    startDate: string;
    endDate: string;
    fiscalYear: number;
    periodNumber: number;
    status: string; // Comes as string form ToString() usually
    isAdjustment: boolean;
}

const FiscalPeriodsPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const [periods, setPeriods] = useState<FiscalPeriodDto[]>([]);
    const [loading, setLoading] = useState(true);

    // Generator State
    const [generateYear, setGenerateYear] = useState(new Date().getFullYear());

    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            const res = await api.finance.fiscalPeriods.getAll();
            if (res.success) {
                setPeriods(res.data || []);
            }
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

    const handleGenerate = async () => {
        try {
            const res = await api.finance.fiscalPeriods.generate(generateYear);
            if (res.success) { // ApiResponse is { success: boolean, data?: T, error?: string }
                toast.success(t('common.success'), t('finance.periods.generated', { year: generateYear }));
                loadData();
            } else {
                toast.error(t('common.error'), res.error || t('finance.periods.generateFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('finance.periods.generateFailed'));
        }
    };

    const handleStatusChange = async (id: string, newStatus: PeriodStatus) => {
        try {
            // Update DTO structure matches backend expectations for Update
            const payload = {
                status: newStatus,
                name: '' // Ignored but required by DTO potentially if not optional
            };
            const res = await api.finance.fiscalPeriods.update(id, payload);
            if (res.success) {
                toast.success(t('common.success'), t('finance.periods.statusUpdated'));
                loadData();
            }
        } catch (error) {
            toast.error(t('common.error'), t('common.updateFailed'));
        }
    }

    const columns: Column<FiscalPeriodDto>[] = [
        { key: 'code', header: t('finance.periods.code'), render: (row) => <span className="font-mono">{row.code}</span> },
        { key: 'name', header: t('finance.periods.name') },
        {
            key: 'startDate',
            header: t('finance.periods.startDate'),
            render: (row) => new Date(row.startDate).toLocaleDateString()
        },
        {
            key: 'endDate',
            header: t('finance.periods.endDate'),
            render: (row) => new Date(row.endDate).toLocaleDateString()
        },
        {
            key: 'status',
            header: t('finance.periods.status'),
            render: (row) => {
                let color = 'bg-gray-100 text-gray-800';
                if (row.status === 'Open') color = 'bg-green-100 text-green-800';
                if (row.status === 'Closed') color = 'bg-yellow-100 text-yellow-800';
                if (row.status === 'Locked') color = 'bg-red-100 text-red-800';

                // Map status string to translated status
                const statusKey = row.status as keyof typeof PeriodStatus | string;
                // If row.status is "Open", we want t('finance.periods.statuses.Open')

                return (
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${color}`}>
                        {t(`finance.periods.statuses.${statusKey}`)}
                    </span>
                );
            }
        },
        {
            key: 'id',
            header: t('common.actions'),
            render: (row) => (
                <div className="flex gap-2">
                    {row.status !== 'Open' && (
                        <button
                            onClick={() => handleStatusChange(row.id, PeriodStatus.Open)}
                            className="p-1 hover:bg-gray-100 rounded text-green-600" title={t('finance.periods.reopen')}
                        >
                            <Unlock className="w-4 h-4" />
                        </button>
                    )}
                    {row.status === 'Open' && (
                        <button
                            onClick={() => handleStatusChange(row.id, PeriodStatus.Closed)}
                            className="p-1 hover:bg-gray-100 rounded text-yellow-600" title={t('finance.periods.close')}
                        >
                            <Calendar className="w-4 h-4" />
                        </button>
                    )}
                    {row.status !== 'Locked' && (
                        <button
                            onClick={() => handleStatusChange(row.id, PeriodStatus.Locked)}
                            className="p-1 hover:bg-gray-100 rounded text-red-600" title={t('finance.periods.lock')}
                        >
                            <Lock className="w-4 h-4" />
                        </button>
                    )}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">{t('finance.periods.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.periods.subtitle')}</p>
                </div>
                <div className="flex gap-2 items-center">
                    <input
                        type="number"
                        value={generateYear}
                        onChange={e => setGenerateYear(parseInt(e.target.value))}
                        className="w-24 p-2 border rounded-md dark:bg-gray-900"
                    />
                    <Button onClick={handleGenerate}>
                        <Plus className="w-4 h-4 mr-2" />
                        {t('finance.periods.generate')} {generateYear}
                    </Button>
                </div>
            </div>

            <DataTable
                data={periods}
                columns={columns}
                keyField="id"
                isLoading={loading}
            />
        </div>
    );
};

export default FiscalPeriodsPage;
