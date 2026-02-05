import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Check, X, DollarSign, Wallet } from 'lucide-react';
import { DataTable, Button, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { AdvanceRequestDialog } from './AdvanceRequestDialog';

interface AdvanceRequest {
    id: string;
    employeeId: string;
    employeeName: string;
    requestDate: string;
    amount: number;
    status: number;
    statusName: string;
    description: string | null;
}

interface Employee {
    id: string;
    firstName: string;
    lastName: string;
}

const API_BASE = '/api/hr/advance-requests';

export function AdvanceRequestsPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [requests, setRequests] = useState<AdvanceRequest[]>([]);
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const [reqRes, empRes] = await Promise.all([
                fetch(API_BASE, { cache: 'no-store' }),
                fetch('/api/hr/employees', { cache: 'no-store' })
            ]);

            if (reqRes.ok) setRequests(await reqRes.json());
            if (empRes.ok) setEmployees(await empRes.json());
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setDialogOpen(true); };

    const handleAction = async (id: string, action: 'approve' | 'reject' | 'pay') => {
        try {
            const url = action === 'pay'
                ? `${API_BASE}/${id}/paid` // Match backend endpoint: /paid
                : `${API_BASE}/${id}/${action}`;

            const res = await fetch(url, { method: 'PUT', cache: 'no-store' });
            if (res.ok) {
                toast.success(
                    action === 'approve' ? t('hr.advanceApproved') :
                        action === 'reject' ? t('hr.advanceRejected') :
                            t('hr.advancePaid')
                );
                loadData();
            } else {
                toast.error(t('common.error'));
            }
        } catch { toast.error(t('common.error')); }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const getStatusBadge = (status: string) => {
        switch (status) {
            case 'Pending': return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400';
            case 'Approved': return 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400';
            case 'Paid': return 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
            case 'Rejected': return 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
            case 'Deducted': return 'bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400';
            default: return 'bg-gray-100';
        }
    };

    const columns: Column<AdvanceRequest>[] = [
        {
            key: 'employeeName',
            header: t('hr.employees'),
            render: (r) => (
                <div className="font-medium">{r.employeeName}</div>
            ),
        },
        {
            key: 'requestDate',
            header: t('hr.requestDate'),
            render: (r) => new Date(r.requestDate).toLocaleDateString()
        },
        {
            key: 'amount',
            header: t('hr.amount'),
            render: (r) => (
                <span className="font-mono font-semibold">
                    {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(r.amount)}
                </span>
            )
        },
        {
            key: 'statusName',
            header: t('common.status'),
            render: (r) => (
                <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getStatusBadge(r.statusName)}`}>
                    {r.statusName}
                </span>
            )
        },
        { key: 'description', header: t('common.description') }
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Wallet className="w-6 h-6" />
                        {t('hr.advanceRequests')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.advanceRequestsSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createAdvanceRequest')}
                </Button>
            </div>

            <DataTable
                data={requests}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(r) => (
                    <div className="flex items-center gap-1">
                        {r.statusName === 'Pending' && (
                            <>
                                <button onClick={() => handleAction(r.id, 'approve')} className="p-2 rounded-lg hover:bg-green-100 text-green-600" title={t('hr.approve')}>
                                    <Check className="w-4 h-4" />
                                </button>
                                <button onClick={() => handleAction(r.id, 'reject')} className="p-2 rounded-lg hover:bg-red-100 text-red-600" title={t('hr.reject')}>
                                    <X className="w-4 h-4" />
                                </button>
                            </>
                        )}
                        {r.statusName === 'Approved' && (
                            <button onClick={() => handleAction(r.id, 'pay')} className="p-2 rounded-lg hover:bg-blue-100 text-blue-600" title={t('hr.markPaid')}>
                                <DollarSign className="w-4 h-4" />
                            </button>
                        )}
                    </div>
                )}
            />

            <AdvanceRequestDialog open={dialogOpen} onClose={handleDialogClose} employees={employees} />
        </div>
    );
}
