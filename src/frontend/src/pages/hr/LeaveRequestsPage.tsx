import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Calendar, CheckCircle, XCircle, Clock } from 'lucide-react';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface LeaveRequest {
    id: string;
    employeeId: string;
    employeeName: string;
    leaveType: number;
    startDate: string;
    endDate: string;
    days: number;
    status: number;
    reason: string | null;
    requestedAt: string;
}

const API_BASE = '/api/hr';

export function LeaveRequestsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [requests, setRequests] = useState<LeaveRequest[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/leave-requests`, { cache: 'no-store' });
            if (res.ok) setRequests(await res.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleApprove = async (req: LeaveRequest) => {
        const ok = await dialog.confirm({
            title: 'Approve Leave Request',
            message: `Approve ${req.days} days leave for ${req.employeeName}?`,
            confirmText: 'Approve',
        });
        if (!ok) return;

        try {
            const res = await fetch(`${API_BASE}/leave-requests/${req.id}/approve`, { method: 'PUT', cache: 'no-store' });
            if (res.ok) { toast.success(t('hr.leaveApproved')); loadData(); }
            else toast.error(t('common.error'));
        } catch { toast.error(t('common.error')); }
    };

    const handleReject = async (req: LeaveRequest) => {
        const ok = await dialog.confirm({
            title: 'Reject Leave Request',
            message: `Reject leave request from ${req.employeeName}?`,
            confirmText: 'Reject',
        });
        if (!ok) return;

        try {
            const res = await fetch(`${API_BASE}/leave-requests/${req.id}/reject`, { method: 'PUT', cache: 'no-store' });
            if (res.ok) { toast.success(t('hr.leaveRejected')); loadData(); }
            else toast.error(t('common.error'));
        } catch { toast.error(t('common.error')); }
    };

    const getLeaveTypeBadge = (type: number) => {
        const types: Record<number, string> = {
            0: 'Annual',
            1: 'Sick',
            2: 'Unpaid',
            3: 'Maternity',
            4: 'Paternity',
            5: 'Bereavement',
        };
        return <Badge variant="default">{types[type] || 'Other'}</Badge>;
    };

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'error' | 'default', label: string }> = {
            0: { variant: 'warning', label: 'Pending' },
            1: { variant: 'success', label: 'Approved' },
            2: { variant: 'error', label: 'Rejected' },
            3: { variant: 'default', label: 'Cancelled' },
        };
        const cfg = configs[status] || configs[0];
        return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
    };

    // Stats
    const pendingCount = requests.filter(r => r.status === 0).length;
    const approvedCount = requests.filter(r => r.status === 1).length;

    const columns: Column<LeaveRequest>[] = [
        {
            key: 'employeeName',
            header: t('hr.employee'),
            render: (req) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center text-white text-xs font-bold">
                        {req.employeeName.split(' ').map(n => n[0]).join('')}
                    </div>
                    <span className="font-medium">{req.employeeName}</span>
                </div>
            ),
        },
        {
            key: 'leaveType',
            header: t('hr.leaveType'),
            render: (req) => getLeaveTypeBadge(req.leaveType),
        },
        {
            key: 'startDate',
            header: t('hr.period'),
            render: (req) => (
                <div className="text-sm">
                    <p>{new Date(req.startDate).toLocaleDateString()}</p>
                    <p className="text-[hsl(var(--muted-foreground))]">to {new Date(req.endDate).toLocaleDateString()}</p>
                </div>
            ),
        },
        {
            key: 'days',
            header: t('hr.days'),
            render: (req) => <span className="font-mono font-medium">{req.days}</span>,
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (req) => getStatusBadge(req.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Calendar className="w-6 h-6" />
                        {t('hr.leaveRequests')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.leaveRequestsSubtitle')}</p>
                </div>
                <Button onClick={() => { }}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createLeaveRequest')}
                </Button>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-amber-100 dark:bg-amber-900/30 text-amber-600 flex items-center justify-center">
                            <Clock className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">Pending</p>
                            <p className="text-2xl font-bold">{pendingCount}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 text-green-600 flex items-center justify-center">
                            <CheckCircle className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">Approved</p>
                            <p className="text-2xl font-bold">{approvedCount}</p>
                        </div>
                    </div>
                </div>
                <div className="bg-[hsl(var(--card))] rounded-xl p-4 border border-[hsl(var(--border))]">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                            <Calendar className="w-5 h-5" />
                        </div>
                        <div>
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">Total</p>
                            <p className="text-2xl font-bold">{requests.length}</p>
                        </div>
                    </div>
                </div>
            </div>

            <DataTable
                data={requests}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(req) => req.status === 0 ? (
                    <div className="flex items-center gap-1">
                        <button onClick={() => handleApprove(req)} className="p-2 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 text-green-600" title="Approve">
                            <CheckCircle className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleReject(req)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600" title="Reject">
                            <XCircle className="w-4 h-4" />
                        </button>
                    </div>
                ) : null}
            />
        </div>
    );
}
