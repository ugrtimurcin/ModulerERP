import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Calendar, CheckCircle, XCircle, Clock } from 'lucide-react';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/lib/api';
import { LeaveRequestDialog } from './LeaveRequestDialog';

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

export function LeaveRequestsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [requests, setRequests] = useState<LeaveRequest[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [createDialogOpen, setCreateDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await api.get<LeaveRequest[]>('/hr/leave-requests');
            setRequests(data);
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleApprove = async (req: LeaveRequest) => {
        const ok = await dialog.confirm({
            title: t('hr.dialogs.approveLeaveTitle'),
            message: t('hr.dialogs.approveLeaveMsg', { days: req.days, name: req.employeeName }),
            confirmText: t('hr.approve'),
        });
        if (!ok) return;

        try {
            await api.put(`/hr/leave-requests/${req.id}/approve`, {});
            toast.success(t('hr.leaveApproved'));
            loadData();
        } catch {
            toast.error(t('common.error'));
        }
    };

    const handleReject = async (req: LeaveRequest) => {
        const ok = await dialog.confirm({
            title: t('hr.dialogs.rejectLeaveTitle'),
            message: t('hr.dialogs.rejectLeaveMsg', { name: req.employeeName }),
            confirmText: t('hr.reject'),
        });
        if (!ok) return;

        try {
            await api.put(`/hr/leave-requests/${req.id}/reject`, {});
            toast.success(t('hr.leaveRejected'));
            loadData();
        } catch {
            toast.error(t('common.error'));
        }
    };

    const getLeaveTypeBadge = (type: number) => {
        const types: Record<number, string> = {
            0: t('hr.leaveTypes.annual'),
            1: t('hr.leaveTypes.sick'),
            2: t('hr.leaveTypes.unpaid'),
            3: t('hr.leaveTypes.maternity'),
            4: t('hr.leaveTypes.paternity'),
            5: t('hr.leaveTypes.bereavement'),
        };
        return <Badge variant="default">{types[type] || t('common.unknown')}</Badge>;
    };

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'error' | 'default', label: string }> = {
            0: { variant: 'warning', label: t('hr.leaveStatuses.pending') },
            1: { variant: 'success', label: t('hr.leaveStatuses.approved') },
            2: { variant: 'error', label: t('hr.leaveStatuses.rejected') },
            3: { variant: 'default', label: t('hr.leaveStatuses.cancelled') },
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
                    <p className="text-[hsl(var(--muted-foreground))]">{t('common.to')} {new Date(req.endDate).toLocaleDateString()}</p>
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
                <Button onClick={() => setCreateDialogOpen(true)}>
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
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.leaveStatuses.pending')}</p>
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
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.leaveStatuses.approved')}</p>
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
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('hr.stats.total')}</p>
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

            <LeaveRequestDialog
                open={createDialogOpen}
                onClose={(saved) => {
                    setCreateDialogOpen(false);
                    if (saved) loadData();
                }}
            />
        </div>
    );
}
