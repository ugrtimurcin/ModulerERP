import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { ScanLine, MapPin, ArrowRight, ArrowLeft } from 'lucide-react';
import { DataTable, Button, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { AttendanceLogDialog } from './AttendanceLogDialog';
import { api } from '@/lib/api';

interface AttendanceLog {
    id: string;
    employeeName: string;
    transactionTime: string;
    type: number; // 1=CheckIn, 2=CheckOut
    locationId: string | null;
    gpsCoordinates: string | null;
}

export function AttendanceLogsPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [logs, setLogs] = useState<AttendanceLog[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await api.get<AttendanceLog[]>('/hr/attendance/logs');
            setLogs(data);
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setDialogOpen(true); };
    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const columns: Column<AttendanceLog>[] = [
        {
            key: 'employeeName',
            header: t('hr.employee'),
            render: (l) => <span className="font-semibold">{l.employeeName}</span>
        },
        {
            key: 'transactionTime',
            header: t('common.time'),
            render: (l) => new Date(l.transactionTime).toLocaleString()
        },
        {
            key: 'type',
            header: t('common.type'),
            render: (l) => l.type === 1 ? (
                <span className="flex items-center gap-1 text-emerald-600 bg-emerald-100 dark:bg-emerald-900/30 px-2 py-1 rounded text-xs font-medium w-fit">
                    <ArrowRight className="w-3 h-3" /> {t('hr.checkIn')}
                </span>
            ) : (
                <span className="flex items-center gap-1 text-amber-600 bg-amber-100 dark:bg-amber-900/30 px-2 py-1 rounded text-xs font-medium w-fit">
                    <ArrowLeft className="w-3 h-3" /> {t('hr.checkOut')}
                </span>
            )
        },
        {
            key: 'gpsCoordinates',
            header: t('hr.location'),
            render: (l) => l.gpsCoordinates ? (
                <div className="flex items-center gap-1 text-xs text-[hsl(var(--muted-foreground))]">
                    <MapPin className="w-3 h-3" />
                    {l.gpsCoordinates}
                </div>
            ) : <span className="text-[hsl(var(--muted-foreground))] text-xs">-</span>
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <ScanLine className="w-6 h-6" />
                        {t('hr.attendanceLogs')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.attendanceLogsSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <ScanLine className="w-4 h-4" />
                    {t('hr.simulateScan')}
                </Button>
            </div>

            <DataTable
                data={logs}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
            />

            <AttendanceLogDialog open={dialogOpen} onClose={handleDialogClose} />
        </div>
    );
}
