import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { DollarSign, Play, FileText, Calendar, Users } from 'lucide-react';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface PayrollRun {
    id: string;
    periodYear: number;
    periodMonth: number;
    status: number;
    employeeCount: number;
    totalGross: number;
    totalNet: number;
    totalDeductions: number;
    processedAt: string | null;
    currencyCode: string;
}

const API_BASE = '/api/hr';

export function PayrollPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [runs, setRuns] = useState<PayrollRun[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/payroll?year=${selectedYear}`, { cache: 'no-store' });
            if (res.ok) setRuns(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [selectedYear, toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleRunPayroll = async () => {
        const ok = await dialog.confirm({
            title: t('hr.dialogs.runPayrollTitle'),
            message: t('hr.dialogs.runPayrollMsg', { month: getMonthName(new Date().getMonth()), year: new Date().getFullYear() }),
            confirmText: t('hr.runPayroll'),
        });
        if (!ok) return;

        try {
            const res = await fetch(`${API_BASE}/payroll/run`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ year: new Date().getFullYear(), month: new Date().getMonth() + 1 })
            });
            if (res.ok) { toast.success(t('hr.payrollGenerated')); loadData(); }
            else toast.error(await res.text());
        } catch { toast.error(t('common.error')); }
    };

    const getMonthName = (month: number) => {
        // Use browser locale naturally, or force specific locale if needed
        return new Date(2000, month, 1).toLocaleString('default', { month: 'long' });
    };

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'info' | 'default', label: string }> = {
            0: { variant: 'warning', label: t('hr.payrollStatuses.draft') },
            1: { variant: 'info', label: t('hr.payrollStatuses.processing') },
            2: { variant: 'success', label: t('hr.payrollStatuses.completed') },
            3: { variant: 'default', label: t('hr.payrollStatuses.cancelled') },
        };
        const cfg = configs[status] || configs[0];
        return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
    };

    const formatCurrency = (amount: number, code: string = 'TRY') => {
        if (amount === undefined || amount === null) return '-';
        // Try to respect locale for currency formatting
        // If code is TRY, use tr-TR, otherwise en-US or default
        const locale = code === 'TRY' ? 'tr-TR' : 'en-US';
        return new Intl.NumberFormat(locale, { style: 'currency', currency: code }).format(amount);
    };

    const columns: Column<PayrollRun>[] = [
        {
            key: 'periodMonth',
            header: t('hr.period'),
            render: (run) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-green-500 to-emerald-500 flex items-center justify-center text-white">
                        <Calendar className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-semibold">{getMonthName(run.periodMonth - 1)} {run.periodYear}</p>
                        {run.processedAt && (
                            <p className="text-xs text-[hsl(var(--muted-foreground))]">
                                {t('common.processed')}: {new Date(run.processedAt).toLocaleDateString()}
                            </p>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'employeeCount',
            header: t('hr.employees'),
            render: (run) => (
                <div className="flex items-center gap-2">
                    <Users className="w-4 h-4 text-[hsl(var(--muted-foreground))]" />
                    <span className="font-mono">{run.employeeCount}</span>
                </div>
            ),
        },
        {
            key: 'totalGross',
            header: t('hr.grossPay'),
            render: (run) => (
                <span className="font-mono font-medium text-green-600">
                    {formatCurrency(run.totalGross, run.currencyCode)}
                </span>
            ),
        },
        {
            key: 'totalDeductions',
            header: t('hr.deductions'),
            render: (run) => (
                <span className="font-mono text-red-600">
                    {formatCurrency(run.totalDeductions * -1, run.currencyCode)}
                </span>
            ),
        },
        {
            key: 'totalNet',
            header: t('hr.netPay'),
            render: (run) => (
                <span className="font-mono font-bold">
                    {formatCurrency(run.totalNet, run.currencyCode)}
                </span>
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (run) => getStatusBadge(run.status),
        },
    ];

    // Summary stats
    const totalGross = runs.reduce((sum, r) => sum + r.totalGross, 0);
    const totalNet = runs.reduce((sum, r) => sum + r.totalNet, 0);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <DollarSign className="w-6 h-6" />
                        {t('hr.payroll')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.payrollSubtitle')}</p>
                </div>
                <div className="flex items-center gap-3">
                    <select
                        value={selectedYear}
                        onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                        className="px-3 py-2 rounded-lg border border-[hsl(var(--border))] bg-[hsl(var(--background))]"
                    >
                        {[2024, 2025, 2026].map(y => <option key={y} value={y}>{y}</option>)}
                    </select>
                    <Button onClick={handleRunPayroll}>
                        <Play className="w-4 h-4" />
                        {t('hr.runPayroll')}
                    </Button>
                </div>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 text-green-600 flex items-center justify-center">
                            <DollarSign className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.stats.ytdGross')}</p>
                            <p className="text-xl font-bold">{formatCurrency(totalGross)}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                            <FileText className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.stats.ytdNet')}</p>
                            <p className="text-xl font-bold">{formatCurrency(totalNet)}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-purple-100 dark:bg-purple-900/30 text-purple-600 flex items-center justify-center">
                            <Calendar className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.stats.payrollRuns')}</p>
                            <p className="text-xl font-bold">{runs.length}</p>
                        </div>
                    </div>
                </div>
            </div>

            <DataTable
                data={runs}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(_run) => (
                    <div className="flex items-center gap-1">
                        <button onClick={() => { }} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title="View Slips">
                            <FileText className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />
        </div>
    );
}
