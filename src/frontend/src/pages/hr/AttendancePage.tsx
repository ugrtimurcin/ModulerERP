import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Clock, Users, CheckCircle2, XCircle, AlertCircle } from 'lucide-react';
import { DataTable, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';

interface DailyAttendance {
    id: string;
    employeeId: string;
    employeeName: string;
    date: string;
    checkInTime: string | null;
    checkOutTime: string | null;
    totalWorkedMins: number;
    overtimeMins: number;
    status: number;
}

const API_BASE = '/api/hr';

export function AttendancePage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [records, setRecords] = useState<DailyAttendance[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dateFilter, setDateFilter] = useState(new Date().toISOString().split('T')[0]);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/attendance?date=${dateFilter}`, { cache: 'no-store' });
            if (res.ok) setRecords(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [dateFilter, toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const formatTime = (time: string | null) => {
        if (!time) return '—';
        return new Date(time).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    };

    const formatDuration = (mins: number) => {
        const hours = Math.floor(mins / 60);
        const minutes = mins % 60;
        return `${hours}${t('hr.units.h')} ${minutes}${t('hr.units.m')}`;
    };

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'error' | 'default', label: string }> = {
            0: { variant: 'success', label: t('hr.attendanceStatuses.present') },
            1: { variant: 'error', label: t('hr.attendanceStatuses.absent') },
            2: { variant: 'warning', label: t('hr.attendanceStatuses.late') },
            3: { variant: 'default', label: t('hr.attendanceStatuses.leave') },
            4: { variant: 'default', label: t('hr.attendanceStatuses.holiday') },
        };
        const cfg = configs[status] || configs[0];
        return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
    };

    // Stats
    const presentCount = records.filter(r => r.status === 0).length;
    const absentCount = records.filter(r => r.status === 1).length;
    const lateCount = records.filter(r => r.status === 2).length;

    const columns: Column<DailyAttendance>[] = [
        {
            key: 'employeeName',
            header: t('hr.employee'),
            render: (rec) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-indigo-500 flex items-center justify-center text-white text-xs font-bold">
                        {rec.employeeName.split(' ').map(n => n[0]).join('')}
                    </div>
                    <span className="font-medium">{rec.employeeName}</span>
                </div>
            ),
        },
        {
            key: 'checkInTime',
            header: t('hr.checkIn'),
            render: (rec) => (
                <span className={`font-mono text-sm ${rec.checkInTime ? '' : 'text-[hsl(var(--muted-foreground))]'}`}>
                    {formatTime(rec.checkInTime)}
                </span>
            ),
        },
        {
            key: 'checkOutTime',
            header: t('hr.checkOut'),
            render: (rec) => (
                <span className={`font-mono text-sm ${rec.checkOutTime ? '' : 'text-[hsl(var(--muted-foreground))]'}`}>
                    {formatTime(rec.checkOutTime)}
                </span>
            ),
        },
        {
            key: 'totalWorkedMins',
            header: t('hr.workedHours'),
            render: (rec) => (
                <span className="font-mono text-sm">{formatDuration(rec.totalWorkedMins)}</span>
            ),
        },
        {
            key: 'overtimeMins',
            header: t('hr.overtime'),
            render: (rec) => rec.overtimeMins > 0 ? (
                <Badge variant="default">+{formatDuration(rec.overtimeMins)}</Badge>
            ) : <span className="text-[hsl(var(--muted-foreground))]">—</span>,
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (rec) => getStatusBadge(rec.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Clock className="w-6 h-6" />
                        {t('hr.attendance')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.attendanceSubtitle')}</p>
                </div>
                <div className="flex items-center gap-3">
                    <input
                        type="date"
                        value={dateFilter}
                        onChange={(e) => setDateFilter(e.target.value)}
                        className="px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    />
                </div>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                            <Users className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.stats.total')}</p>
                            <p className="text-2xl font-bold">{records.length}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 text-green-600 flex items-center justify-center">
                            <CheckCircle2 className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.attendanceStatuses.present')}</p>
                            <p className="text-2xl font-bold">{presentCount}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-red-100 dark:bg-red-900/30 text-red-600 flex items-center justify-center">
                            <XCircle className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.attendanceStatuses.absent')}</p>
                            <p className="text-2xl font-bold">{absentCount}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-amber-100 dark:bg-amber-900/30 text-amber-600 flex items-center justify-center">
                            <AlertCircle className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.attendanceStatuses.late')}</p>
                            <p className="text-2xl font-bold">{lateCount}</p>
                        </div>
                    </div>
                </div>
            </div>

            <DataTable
                data={records}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
            />
        </div>
    );
}
